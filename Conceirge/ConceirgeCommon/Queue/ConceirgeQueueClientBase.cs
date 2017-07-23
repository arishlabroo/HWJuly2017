using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConceirgeCommon.Queue
{
    public abstract class ConceirgeQueueClientBase<T> : IQueue<T>
    {
        private readonly IConceirgeQueueFactory _sqsQueueFactory;
        private IQueue<T> _queue;

        protected abstract string QueueName { get; }
        protected virtual int DelaySeconds => 0;
        protected virtual int WaitTimeSeconds => 5;
        protected virtual int VisibilityTimeoutSeconds => 30;
        protected virtual int MaxNumberOfMessages => 3;

        public ConceirgeQueueClientBase(IConceirgeQueueFactory sqsQueueFactory)
        {
            _sqsQueueFactory = sqsQueueFactory;
        }

        public Task<ConciergeQueueItem<T>> Reap()
        {
            return GetQueue().Reap();
        }

        public Task<IEnumerable<ConciergeQueueItem<T>>> ReapMany()
        {
            return GetQueue().ReapMany();
        }

        public Task<SowResponse> Sow(T message)
        {
            return GetQueue().Sow(message);
        }

        private IQueue<T> GetQueue()
        {
            if (_queue == null)
            {
                var info = new ConceirgeQueueInfo(QueueName)
                {
                    DelaySeconds = DelaySeconds,
                    MaxNumberOfMessages = MaxNumberOfMessages,
                    VisibilityTimeoutSeconds = VisibilityTimeoutSeconds,
                    WaitTimeSeconds = WaitTimeSeconds
                };

                _queue = _sqsQueueFactory.Create<T>(info);
            }

            return _queue;
        }
    }
}