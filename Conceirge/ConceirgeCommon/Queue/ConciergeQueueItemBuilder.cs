using Microsoft.Extensions.Logging;
using System;

namespace ConceirgeCommon.Queue
{
    public interface IConceirgeQueueItemBuilder<T>
    {
        ConciergeQueueItem<T> Build(ConceirgeQueueItemInfo item);
    }

    //Deserializes the message into a T 
    //and then wraps the T into a disposable conceirge queue item
    public class ConceirgeQueueItemBuilder<T> : IConceirgeQueueItemBuilder<T>
    {
        private readonly IMessageFormatter<T> _formatter;
        private readonly ILogger<ConceirgeQueueItemBuilder<T>> _logger;
        private readonly IConceirgeQueueItemCleaner<T> _cleaner;

        public ConceirgeQueueItemBuilder(
            IMessageFormatter<T> formatter,
            IConceirgeQueueItemCleaner<T> cleaner,
            ILogger<ConceirgeQueueItemBuilder<T>> logger)
        {
            _formatter = formatter;
            _cleaner = cleaner;
            _logger = logger;
        }

        public ConciergeQueueItem<T> Build(ConceirgeQueueItemInfo itemInfo)
        {
            if (string.IsNullOrWhiteSpace(itemInfo?.MessageId))
            {
                throw new ArgumentException("Message id is required to build a Conceirge Queue Item");
            }

            var item = string.IsNullOrWhiteSpace(itemInfo?.Body) ? default(T) : _formatter.DeSerialize(itemInfo.Body);

            return new ConciergeQueueItem<T>(item, itemInfo, _cleaner);
        }
    }
}