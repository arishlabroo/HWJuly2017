using ConceirgeCommon;
using ConceirgeCommon.Queue;
using TaskModel;

namespace TaskClient
{
    public class ScheduleTaskQueue : ConceirgeQueueClientBase<ServiceRequest<ScheduleTaskDto>>
    {
        public ScheduleTaskQueue(IConceirgeQueueFactory sqsQueueFactory)
            : base(sqsQueueFactory)
        {
        }

        protected override sealed string QueueName => "TestHW2017";
        //Queue name is abstract, hence required. Other params are virtual with some defaults. You can override them here too.
    }
}