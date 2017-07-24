using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskClient;
using ConceirgeCommon;
using TaskModel;

namespace CadencePlaceholderForDemo.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly IScheduleTaskClient _client;

        public ValuesController(IScheduleTaskClient client)
        {
            _client = client;
        }
        // GET api/values
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            await _client.Schedule(new ServiceRequest<ScheduleTaskDto>
            {
                CorrelationId = DateTime.Now.Ticks.ToString(),
                EnableLogLevelOverride = DateTime.Now.Millisecond % 2 == 0,
                RequestId = Guid.NewGuid().ToString(),
                UserId = Guid.NewGuid(),
                SomethingId = "watcha",
                Request = new ScheduleTaskDto
                {
                    BethId = DateTime.Now.Second,
                    When = DateTime.Now.AddDays(DateTime.Now.Second),
                    TaskType = (TaskType)(DateTime.Now.Millisecond % 3)
                }
            });
            return new string[] { "value1", "value2" };
        }
    }
}
