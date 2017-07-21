using Amazon.SQS;
using Amazon.SQS.Model;
using ConceirgeCommon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TaskModel;

namespace Task
{
    public static class ServiceExtensions
    {
        public static IServiceCollection UseTaskQueueMagic(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDefaultAWSOptions(configuration.GetAWSOptions());
            services.AddAWSService<IAmazonSQS>();
            return services;
        }
    }

    public interface IScheduleTaskClient
    {
        Task<ServiceResponse> Schedule(ServiceRequest<ScheduleTaskDto> serviceRequest);
    }

    public interface IReaper<T>
    {
        ConciergeQueueItem<T> Reap();
    }

    public class ScheduleTaskQueueClientThing : BaseQueueThing<ServiceRequest<ScheduleTaskDto>>, IScheduleTaskClient, IReaper<ServiceRequest<ScheduleTaskDto>>
    {
        protected ScheduleTaskQueueClientThing(
            IAmazonSQS sqsClient,
            IJsonMessageFormatter<ServiceRequest<ScheduleTaskDto>> formatter,
            ILogger<ScheduleTaskQueueClientThing> logger)
            : base(sqsClient, formatter, logger)
        { }

        protected override string QueueName => "TestHW2017";

        public Task<ServiceResponse> Schedule(ServiceRequest<ScheduleTaskDto> serviceRequest) => Sow(serviceRequest);
    }



    public abstract class BaseQueueThing<T>
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly IMessageFormatter<T> _formatter;
        private readonly ILogger<BaseQueueThing<T>> _logger;

        protected abstract string QueueName { get; }
        protected virtual int DelaySeconds => 0;


        private string _queueUrl;


        protected BaseQueueThing(
            IAmazonSQS sqsClient, 
            IMessageFormatter<T> formatter,
            ILogger<BaseQueueThing<T>> logger)
        {
            _sqsClient = sqsClient;
            _formatter = formatter;
            _logger = logger;
        }

        public async Task<ServiceResponse> Sow(T message)
        {
            var queueUrl = await GetQueueUrl();

            var messageBody = _formatter.Serialize(message);

            var sendMessageRequest = new SendMessageRequest
            {
                DelaySeconds = DelaySeconds,
                QueueUrl = queueUrl,
                MessageBody = messageBody
            };

            ServiceResponse response = new ServiceResponse();

            try
            {
                var sendMessageResponse = await _sqsClient.SendMessageAsync(sendMessageRequest);
                _logger.LogInformation("For SQS {@sendMessageRequest} the response was {@sendMessageResponse}", sendMessageRequest, sendMessageResponse);
                response.Success = sendMessageResponse.HttpStatusCode == HttpStatusCode.OK;
                response.StatusCode = sendMessageResponse.HttpStatusCode.ToString();
                response.Message = sendMessageResponse.MessageId;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "SQS SendMessageAsync threw an exception for the following request {@request}", sendMessageRequest);
                response.Success = false;
                response.Message = e.Message;
                response.StatusCode = HttpStatusCode.InternalServerError.ToString();
            }

            return response;
        }

        public ConciergeQueueItem<T> Reap()
        {
            throw new System.NotImplementedException();
        }

        protected async ValueTask<string> GetQueueUrl()
        {
            if (string.IsNullOrWhiteSpace(_queueUrl))
            {
                try
                {
                    var response = await _sqsClient.GetQueueUrlAsync(QueueName);
                    _logger.LogInformation("Get queue url response {@response}", response);
                    _queueUrl = response?.QueueUrl;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Attempt to get QueueUrl for {queueName} threw exception", QueueName);
                }
            }
            return _queueUrl;
        }
    }


    public interface IMessageFormatter<T>
    {
        string Serialize(T input);

        T Serialize(string message);
    }

    public interface IJsonMessageFormatter<T> : IMessageFormatter<T> { }
    public interface IProtoMessageFormatter<T> : IMessageFormatter<T> { }

    public class JsonMessageFormatter<T> : IJsonMessageFormatter<T>
    {
        //private static JsonSerializerSettings settings = new JsonSerializerSettings() { };

        public string Serialize(T input)
        {
            return JsonConvert.SerializeObject(input);
        }

        public T Serialize(string message)
        {
            return JsonConvert.DeserializeObject<T>(message);
        }
    }
}