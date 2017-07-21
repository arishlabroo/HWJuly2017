using System;

namespace TaskClient
{
    public class ConciergeQueueItem<T> : IDisposable
    {
        T Item { get; set; }
        string MessageId { get; set; }
        string QueueUrl { get; set; }
        bool Retry { get; set; }



        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (!Retry)
                    {

                    }
                    // TODO: dispose managed state (managed objects).
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}