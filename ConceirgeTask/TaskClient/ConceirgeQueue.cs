using Amazon.SQS;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using System;
using System.Net;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace TaskClient
{
    public abstract class ConceirgeQueue<T> : IReaper<T>, ISower<T>
    {
        //We should definately implement a circuit breaker here.

        private readonly IAmazonSQS _sqsClient;
        private readonly IMessageFormatter<T> _formatter;
        private readonly ILogger<ConceirgeQueue<T>> _logger;
        private readonly ConcurrentQueue<ConciergeQueueItem<T>> _buffer = new ConcurrentQueue<ConciergeQueueItem<T>>();

        protected abstract string QueueName { get; }

        protected virtual int DelaySeconds => 0;
        protected virtual int WaitTimeSeconds => 5;
        protected virtual int VisibilityTimeoutSeconds => 30;
        protected virtual int MaxNumberOfMessages => 3;

        private string _queueUrl;

        protected ConceirgeQueue(
            IAmazonSQS sqsClient,
            IMessageFormatter<T> formatter,
           
            ILogger<ConceirgeQueue<T>> logger)
        {
            _sqsClient = sqsClient;
            _formatter = formatter;
            _logger = logger;
        }

        public async Task<SowResponse> Sow(T message)
        {
            var queueUrl = await GetQueueUrl();

            var messageBody = _formatter.Serialize(message);

            var sendMessageRequest = new SendMessageRequest
            {
                DelaySeconds = DelaySeconds,
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
            while (i < MaxNumberOfMessages && _buffer.TryDequeue(out var item))
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
                MaxNumberOfMessages = MaxNumberOfMessages,
                VisibilityTimeout = VisibilityTimeoutSeconds,
                WaitTimeSeconds = WaitTimeSeconds,
                //AttributeNames = attributeNames
            };

            try
            {
                var recieveMessageResponse = await _sqsClient.ReceiveMessageAsync(recieveMessageRequest);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "SQS ReceiveMessageAsync threw an exception for the following request {@request}", recieveMessageRequest);
            }
        }

        protected async ValueTask<string> GetQueueUrl()
        {
            if (string.IsNullOrWhiteSpace(QueueName))
            {
                _logger.LogError("You need to think what you are doing with your life");
                throw new ArgumentException("QueueName is required to get queue url", nameof(QueueName));
            }

            if (string.IsNullOrWhiteSpace(_queueUrl))
            {
                try
                {
                    //Do i need to lock here?
                    //We should circuit break here too.
                    var response = await _sqsClient.GetQueueUrlAsync(QueueName);
                    _logger.LogInformation("Get queue url response {@response}", response);

                    if (string.IsNullOrWhiteSpace(_queueUrl))
                    {
                        _queueUrl = response?.QueueUrl;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Attempt to get QueueUrl for {queueName} threw exception", QueueName);
                }
            }

            if (string.IsNullOrWhiteSpace(_queueUrl))
            {
                _logger.LogError("Could not get QueueUrl for {QueueName}", QueueName);
            }

            return _queueUrl;
        }
    }


    public interface IReaper<T>
    {
        Task<ConciergeQueueItem<T>> Reap();
        Task<IEnumerable<ConciergeQueueItem<T>>> ReapMany();
    }

    public interface ISower<T>
    {
        Task<SowResponse> Sow(T message);
    }

    public class SowResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string MessageId { get; set; }
        public string Message { get; set; }
    }
}