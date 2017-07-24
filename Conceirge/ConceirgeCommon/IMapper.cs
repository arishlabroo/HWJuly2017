using ConceirgeCommon.Queue;
using System;

namespace ConceirgeCommon
{
    public interface IMapper<in TA, TB>
    {
        TB MapNew(TA from);
        void MapExisting(TA from, TB to);
    }

    public class SowResponseToServiceResponseMapper : IMapper<SowResponse, ServiceResponse>
    {
        public ServiceResponse MapNew(SowResponse from)
        {
            if (from == null) return null;
            var to = new ServiceResponse();
            MapExisting(from, to);
            return to;
        }

        public void MapExisting(SowResponse from, ServiceResponse to)
        {
            if (from == null) return;
            if (to == null) throw new ArgumentNullException(nameof(to), $"Need 'to' to map into in {nameof(SowResponseToServiceResponseMapper)}");
            to.StatusCode = from.StatusCode.ToString();
            to.Success = from.StatusCode == System.Net.HttpStatusCode.OK;
            to.Message = string.IsNullOrWhiteSpace(from.MessageId) ? from.Message : from.MessageId;
        }
    }
}