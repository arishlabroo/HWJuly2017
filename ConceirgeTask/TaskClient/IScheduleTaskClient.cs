using ConceirgeCommon;
using ConceirgeCommon.Queue;
using System;
using System.Threading.Tasks;
using TaskModel;

namespace TaskClient
{
    public interface IScheduleTaskClient
    {
        Task<ServiceResponse> Schedule(ServiceRequest<ScheduleTaskDto> serviceRequest);
    }

    public class ScheduleTaskQueueClient : IScheduleTaskClient
    {
        private readonly ISower<ServiceRequest<ScheduleTaskDto>> _sower;
        private readonly IMapper<SowResponse, ServiceResponse> _sowResponseMapper;

        public ScheduleTaskQueueClient(ISower<ServiceRequest<ScheduleTaskDto>> sower, IMapper<SowResponse, ServiceResponse> sowResponseMapper)
        {
            _sower = sower;
            _sowResponseMapper = sowResponseMapper;
        }

        public async Task<ServiceResponse> Schedule(ServiceRequest<ScheduleTaskDto> serviceRequest)
        {
            return _sowResponseMapper.MapNew(await _sower.Sow(serviceRequest));
        }
    }

    public class ScheduleTaskHttpClient : IScheduleTaskClient
    {
        public ScheduleTaskHttpClient()
        {
        }

        public Task<ServiceResponse> Schedule(ServiceRequest<ScheduleTaskDto> serviceRequest)
        {
            throw new NotImplementedException();
        }
    }
}