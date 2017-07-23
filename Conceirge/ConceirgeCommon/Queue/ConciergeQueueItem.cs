using System;

namespace ConceirgeCommon.Queue
{
    //This is how the world sees an item coming out of the queue. This is disposable to allow "using" pattern. 
    //On dispose ,it calls the cleaner to batch the item up for deleting from the source queue, unless Retry is enabled.
    public class ConciergeQueueItem<T> : IDisposable
    {
        private bool _disposedValue = false; // To detect redundant calls

        private readonly IConciergeQueueItemCleaner<T> _cleaner;

        public bool Retry { get; set; }

        public T Item { get; }

        ConceirgeQueueItemInfo ItemInfo { get; }

        public ConciergeQueueItem(
            T item,
            ConceirgeQueueItemInfo itemInfo,
            IConciergeQueueItemCleaner<T> cleaner)
        {
            Item = item;
            ItemInfo = itemInfo;
            _cleaner = cleaner;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (!Retry)
                    {
                        _cleaner.Clean(ItemInfo);
                    }
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }

    public class ConceirgeQueueItemInfo
    {
        public string Body { get; set; }
        public string QueueUrl { get; set; }
        public string ReceiptHandle { get; set; }
        public string MessageId { get; set; }
    }
}