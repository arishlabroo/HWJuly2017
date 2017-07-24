using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ConceirgeCommon.Queue
{
    //Generic so that we can get one singleton per T.
    public interface IConceirgeQueueItemCleaner<T>
    {
        bool Clean(ConceirgeQueueItemInfo itemInfo);
    }

    public class ConceirgeQueueItemCleaner<T> : IConceirgeQueueItemCleaner<T>
    {
        private static readonly BlockingCollection<ConceirgeQueueItemInfo> QueueItemsToDelete =
            new BlockingCollection<ConceirgeQueueItemInfo>();

        private static bool _longRunningTaskRunning = false;

        private static readonly object Lock = new object();

        private const int MaxParallelCleaners = 3;
        private const int CleaningBatchSize = 10;

        private readonly IAmazonSQS _sqsClient;
        private readonly ILogger<ConceirgeQueueItemCleaner<T>> _logger;

        public ConceirgeQueueItemCleaner(IAmazonSQS sqsClient, ILogger<ConceirgeQueueItemCleaner<T>> logger)
        {
            _sqsClient = sqsClient;
            _logger = logger;
        }

        public bool Clean(ConceirgeQueueItemInfo itemInfo)
        {
            try
            {
                if (!QueueItemsToDelete.IsAddingCompleted)
                {
                    if (QueueItemsToDelete.TryAdd(itemInfo, TimeSpan.FromMilliseconds(20)))
                    {
                        SetLongRunningState(true);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Queue cleaner is not happy. the info passed was {@itemInfo}", itemInfo);
                //Cleaner never throws, so eat the exception
            }

            _logger.LogCritical("Cleaner is not accepting more items. Why?");
            return false;
        }

        private void SetLongRunningState(bool state)
        {
            if (_longRunningTaskRunning == state) return;

            lock (Lock)
            {
                if (_longRunningTaskRunning == state) return;

                if (state)
                {
                    Task.Run(() => LongRunningCleaner()).ConfigureAwait(false);
                }
                _longRunningTaskRunning = state;
            }
        }

        private async void LongRunningCleaner()
        {
            while (QueueItemsToDelete.Count > 0)
            {
                try
                {
                    var cancellationToken = new CancellationTokenSource();

                    var tasks = Enumerable.Range(1, MaxParallelCleaners)
                        .Select(x => CleanInBatches(cancellationToken.Token));

                    await Task.WhenAny(tasks);
                    //WhenAny is deliberate.
                    //As soon as one ends, we will signal all others in that parallel batch to end.
                    //This way we always have max parallel cleaners running
                    //For e.g. if we have task A, B and C running, and the queue empties and A returns while B and C are still working on their batch.
                    //Now suddenly a surge comes in and B and C will continue working. so the parallelism went from 3 to 2. Crying Babies :(
                    //Hence we tell everyone to finish their batch and then comeback and then we can start A,B,C all again. Happy Babies :)
                    cancellationToken.Cancel();
                }
                catch (AggregateException ae)
                {
                    foreach (var e in ae.InnerExceptions)
                    {
                        _logger.LogError(e, "Cleaning batch threw");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cleaning batch threw");
                }
            }
            SetLongRunningState(false);
        }

        private async Task CleanInBatches(CancellationToken token)
        {
            while (QueueItemsToDelete.Count > 0 && (!token.IsCancellationRequested))
            {
                var batch = new List<ConceirgeQueueItemInfo>();
                while (
                        batch.Count < CleaningBatchSize &&
                        QueueItemsToDelete.TryTake(out var itemInfo, 50) //ConsumingEnumerable blocks, and throws with token, @##$@$%@
                    )
                {
                    batch.Add(itemInfo);
                }

                await CleanBatchImplementation(batch);
            }
        }

        private async Task CleanBatchImplementation(List<ConceirgeQueueItemInfo> batch)
        {
            if (batch?.Count < 1) return;

            var r = new DeleteMessageBatchRequest
            {
                QueueUrl = batch.First().QueueUrl,
                Entries = batch.Select(x => new DeleteMessageBatchRequestEntry
                {
                    Id = x.MessageId,
                    ReceiptHandle = x.ReceiptHandle
                }).ToList()
            };

            DeleteMessageBatchResponse deleteMessageBatchResponse = default(DeleteMessageBatchResponse);

            try
            {
                deleteMessageBatchResponse = await _sqsClient.DeleteMessageBatchAsync(r);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Batch delete threw an exception");
                //eat it, what else can you do
            }

            var failures = new List<ConceirgeQueueItemInfo>();
            if (deleteMessageBatchResponse?.HttpStatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("Batch delete response was {@deleteMessageBatchResponse}", deleteMessageBatchResponse);
                foreach (var failure in deleteMessageBatchResponse.Failed ?? Enumerable.Empty<BatchResultErrorEntry>())
                {
                    _logger.LogError("A batch failure item appeared. It is @{failure}", failure);

                    failures.Add(batch.FirstOrDefault(b => b.MessageId == failure.Id));//Less than 10 items always, dont worry abt perf.
                }
            }
            else
            {
                _logger.LogError("Batch delete response was {@deleteMessageBatchResponse}", deleteMessageBatchResponse);
                failures.AddRange(batch);
            }

            TryDeletingOnceMore(failures);
        }

        private void TryDeletingOnceMore(List<ConceirgeQueueItemInfo> items)
        {
            if (items?.Count < 1) return;

            foreach (var item in items.Where(i => i != null))
            {
                if (item.AttemptedDelete)
                {
                    _logger.LogError("Already attempted delete once, not going to try again for {@item}", item);
                    continue;
                }

                item.AttemptedDelete = true;

                if (!Clean(item))
                {
                    _logger.LogError("Could not add cleaning retry {@item}");
                }
            }
        }
    }
}