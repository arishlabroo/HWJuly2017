using System;
using System.Threading.Tasks;
using ConceirgeCommon;
using Microsoft.Extensions.Logging;
using TaskDomain;
using TaskEntity;
using TaskModel;

namespace TaskService
{
    public class ScheduleTaskservice : IScheduleTaskservice
    {
        private readonly IScheduleTaskDomain _domain;
        private readonly IMapper<ScheduleTaskDto, ScheduleTask> _dtoMapper;
        private readonly ILogger<ScheduleTaskservice> _logger;

        public ScheduleTaskservice(
            IScheduleTaskDomain domain,
            IMapper<ScheduleTaskDto, ScheduleTask> dtoMapper,
            ILogger<ScheduleTaskservice> logger)
        {
            _domain = domain;
            _dtoMapper = dtoMapper;
            _logger = logger;
        }

        public async Task Schedule(ScheduleTaskDto dto)
        {
            var task = dto != null ? _dtoMapper.MapNew(dto) : throw new NullReferenceException(nameof(dto));
            var taskId = await _domain.Schedule(task);

        }
    }
}