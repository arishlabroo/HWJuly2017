using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ConceirgeCommon.Queue
{
    public interface IConceirgeQueue<T> : IQueue<T> { };
    public class ConceirgeQueue<T> : IConceirgeQueue<T>
    {
        //We should definately implement a circuit breaker here.

        private readonly ConceirgeQueueInfo _queueInfo;
        private readonly IAmazonSQS _sqsClient;
        private readonly IMessageFormatter<T> _formatter;
        private readonly IConceirgeQueueItemBuilder<T> _queueItemBuilder;
        private readonly ILogger<ConceirgeQueue<T>> _logger;

        private readonly ConcurrentQueue<ConciergeQueueItem<T>> _buffer = new ConcurrentQueue<ConciergeQueueItem<T>>();


        private string _queueUrl;

        public ConceirgeQueue(
            ConceirgeQueueInfo conceirgeQueueInfo,
            IAmazonSQS sqsClient,
            IMessageFormatter<T> formatter,
            IConceirgeQueueItemBuilder<T> queueItemBuilder,
            ILogger<ConceirgeQueue<T>> logger)
        {
            _queueInfo = conceirgeQueueInfo;
            _sqsClient = sqsClient;
            _formatter = formatter;
            _queueItemBuilder = queueItemBuilder;
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_queueInfo?.QueueName))
            {
                throw new InvalidOperationException("Cannot instantiate a Sqs Queue without a queue name");
            }
        }

        public async Task<SowResponse> Sow(T message)
        {
            var queueUrl = await GetQueueUrl();

            var messageBody = _formatter.Serialize(message);

            var sendMessageRequest = new SendMessageRequest
            {
                DelaySeconds = _queueInfo.DelaySeconds,
                QueueUrl = queueUrl,
                MessageBody = messageBody
            };

            SowResponse sowResponse = new SowResponse();

            try
            {
                var sendMessageResponse = await _sqsClient.SendMessageAsync(sendMessageRequest);
                _logger.LogInformation("For SQS {@sendMessageRequest} the response was {@sendMessageResponse}", sendMessageRequest, sendMessageResponse);
                sowResponse.StatusCode = sendMessageResponse.HttpStatusCode;
                sowResponse.MessageId = sendMessageResponse.MessageId;

            }
            catch (Exception e)
            {
                _logger.LogError(e, "SQS SendMessageAsync threw an exception for the following request {@request}", sendMessageRequest);
                sowResponse.Message = e.Message;
                sowResponse.StatusCode = HttpStatusCode.InternalServerError;
            }

            return sowResponse;
        }

        public async Task<ConciergeQueueItem<T>> Reap()
        {
            if (_buffer.IsEmpty) await BuildBuffer();

            _buffer.TryDequeue(out var item);
            return item;
        }

        public async Task<IEnumerable<ConciergeQueueItem<T>>> ReapMany()
        {
            if (_buffer.IsEmpty) await BuildBuffer();
            if (_buffer.IsEmpty) return Enumerable.Empty<ConciergeQueueItem<T>>();

            var items = new List<ConciergeQueueItem<T>>();

            int i = 0;
            while (i < _queueInfo.MaxNumberOfMessages && _buffer.TryDequeue(out var item))
            {
                items.Add(item);
                i++;
            }
            return items;
        }

        private async Task BuildBuffer()
        {
            var queueUrl = await GetQueueUrl();
            if (string.IsNullOrWhiteSpace(queueUrl)) return;

            var recieveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = _queueInfo.MaxNumberOfMessages,
                VisibilityTimeout = _queueInfo.VisibilityTimeoutSeconds,
                WaitTimeSeconds = _queueInfo.WaitTimeSeconds,
                //AttributeNames = attributeNames
            };

            try
            {
                var recieveMessageResponse = await _sqsClient.ReceiveMessageAsync(recieveMessageRequest);
                if ((recieveMessageResponse?.HttpStatusCode == HttpStatusCode.OK) && (recieveMessageResponse.Messages.Count > 0))
                {
                    await Task.WhenAll(recieveMessageResponse.Messages.Select(AddToBuffer));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "SQS ReceiveMessageAsync threw an exception for the following request {@request}", recieveMessageRequest);
            }
        }

        private async Task AddToBuffer(Message message)
        {
            if (string.IsNullOrWhiteSpace(message?.MessageId)) return;
            var queueUrl = await GetQueueUrl();

            var info = new ConceirgeQueueItemInfo
            {
                Body = message.Body,
                QueueUrl = queueUrl,
                ReceiptHandle = message.ReceiptHandle,
                MessageId = message.MessageId,
            };

            _buffer.Enqueue(_queueItemBuilder.Build(info));
        }

        protected async ValueTask<string> GetQueueUrl()
        {
            if (string.IsNullOrWhiteSpace(_queueInfo.QueueName))
            {
                _logger.LogError("You need to think what you are doing with your life");
                throw new ArgumentException("QueueName is required to get queue url", nameof(_queueInfo.QueueName));
            }

            if (string.IsNullOrWhiteSpace(_queueUrl))
            {
                try
                {
                    //Do i need to lock here?
                    //We should circuit break here too.
                    var response = await _sqsClient.GetQueueUrlAsync(_queueInfo.QueueName);
                    _logger.LogInformation("Get queue url response {@response}", response);

                    if (string.IsNullOrWhiteSpace(_queueUrl))
                    {
                        _queueUrl = response?.QueueUrl;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Attempt to get QueueUrl for {queueName} threw exception", _queueInfo.QueueName);
                }
            }

            if (string.IsNullOrWhiteSpace(_queueUrl))
            {
                _logger.LogError("Could not get QueueUrl for {QueueName}", _queueInfo.QueueName);
            }

            return _queueUrl;
        }
    }

    public class ConceirgeQueueInfo
    {
        public ConceirgeQueueInfo(string queueName)
        {
            QueueName = queueName;
        }

        public string QueueName { get; }
        public int DelaySeconds { get; set; } = 0;
        public int WaitTimeSeconds { get; set; } = 5;
        public int VisibilityTimeoutSeconds { get; set; } = 30;
        public int MaxNumberOfMessages { get; set; } = 3;
    }

    public interface IConceirgeQueueFactory
    {
        IConceirgeQueue<T> Create<T>(ConceirgeQueueInfo conceirgeQueueInfo);
    }
}