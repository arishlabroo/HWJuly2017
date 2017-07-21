using System;
using System.Threading.Tasks;
using ConceirgeCommon;
using Microsoft.Extensions.Logging;
using TaskDomain;
using TaskEntity;
using TaskModel;

namespace TaskService
{
    public class ScheduleTaskService : IScheduleTaskService
    {
        private readonly IScheduleTaskDomain _domain;
        private readonly IMapper<ScheduleTaskDto, ScheduleTask> _dtoMapper;
        private readonly ILogger<ScheduleTaskService> _logger;

        public ScheduleTaskService(
            IScheduleTaskDomain domain,
            IMapper<ScheduleTaskDto, ScheduleTask> dtoMapper,
            ILogger<ScheduleTaskService> logger)
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

    public class ScheduleTaskDtoToScheduleTaskMapper : IMapper<ScheduleTaskDto, ScheduleTask>
    {
        public ScheduleTask MapNew(ScheduleTaskDto from)
        {
            if (from == null) return null;
            var to = new ScheduleTask();
            MapExisting(from, to);
            return to;
        }

        public void MapExisting(ScheduleTaskDto from, ScheduleTask to)
        {
            if (from == null) return;
            if (to == null) throw new ArgumentNullException(nameof(to), $"Need 'to' to map into in {nameof(ScheduleTaskDtoToScheduleTaskMapper)}");
            to.TaskType = from.TaskType.ToString();
            to.When = from.When;
            to.BethId = from.BethId;

        }
    }
}