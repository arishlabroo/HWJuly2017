using Amazon.SQS;
using ConceirgeCommon;
using Microsoft.Extensions.Logging;
using TaskModel;

namespace TaskClient
{
    public class ScheduleTaskQueue : ConceirgeQueue<ServiceRequest<ScheduleTaskDto>>
    {
        private readonly IMapper<SowResponse, ServiceResponse> _responseMapper;

        protected ScheduleTaskQueue(
            IAmazonSQS sqsClient,
            IJsonMessageFormatter<ServiceRequest<ScheduleTaskDto>> formatter,
            ILogger<ScheduleTaskQueue> logger)
            : base(sqsClient, formatter, logger)
        {
        }

        protected override string QueueName => "TestHW2017";
    }
}