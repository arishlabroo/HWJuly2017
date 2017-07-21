using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskEntity;
using ConceirgeCommon;

namespace TaskDomain
{
    public class ScheduleTaskDomain : IScheduleTaskDomain
    {
        private readonly ILogger<ScheduleTaskDomain> _logger;
        private readonly IConceirgeContext _conceirgeContext;

        public ScheduleTaskDomain(ILogger<ScheduleTaskDomain> logger, IConceirgeContext conceirgeContext)
        {
            _logger = logger;
            _conceirgeContext = conceirgeContext;
        }

        public async Task<int> Schedule(ScheduleTask scheduleTask)
        {
            //DO DATA stuff here.
            _logger.LogError("Hello there my friend from the domain");


            await Task.CompletedTask;
            return 2;
        }
    }
}