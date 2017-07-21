using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskCompositionRoot;
using ConceirgeCommon;
using TaskModel;
using TaskService;
using Microsoft.Extensions.Logging;

namespace TaskApi.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly IServiceDependencyFactory _serviceDependencyFactory;
        private readonly ILogger<ValuesController> logger;

        public ValuesController(IServiceDependencyFactory serviceDependencyFactory, ILogger<ValuesController> logger)
        {
            _serviceDependencyFactory = serviceDependencyFactory;
            this.logger = logger;
        }

        // POST api/values
        [HttpGet]
        public async Task<IActionResult> Post()
        {
            return Ok("Hello dink");
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]ServiceRequest<ScheduleTaskDto> request)
        {
            logger.LogError("Honky Ponky");
            try
            {
                var service = _serviceDependencyFactory.Create<IScheduleTaskService>(request);
                await service.Schedule(request.Request);
            }
            catch (Exception e)
            {
                return Ok(e.Message);
            }
            return Ok();
        }
    }
}
