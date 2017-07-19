using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskEntity;

namespace TaskDomain
{
    public class ScheduleTaskDomain : IScheduleTaskDomain
    {
        private readonly ILogger<ScheduleTaskDomain> _logger;

        public ScheduleTaskDomain(ILogger<ScheduleTaskDomain> logger)
        {
            _logger = logger;
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