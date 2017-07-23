using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ConceirgeCommon.Queue
{
    public interface IQueue<T> : IReaper<T>, ISower<T> { }

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