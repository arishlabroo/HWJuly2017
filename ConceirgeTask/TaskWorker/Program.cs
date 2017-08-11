using System;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;
using ConceirgeCommon.Queue;
using ConceirgeCommon;
using TaskModel;
using TaskService;

namespace TaskWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            System.AppDomain.CurrentDomain.ProcessExit += (s, e) => 
			{
				Console.WriteLine("Process exiting");				
			};
            Console.WriteLine("Hello World!");

            var environment = "Development";
            var services = new ServiceCollection();
            var startup = new TaskWorker.Startup(Directory.GetCurrentDirectory(), environment);
            startup.ConfigureServices(services);
            //var loggerFactory = startup.ConfigureLoggerFactory(environment);
            //logger = loggerFactory.CreateLogger<Program>();

            //var workerName = configuration.GetSection("workerName").Value;
            var worker = startup.Container.GetInstance<HolaMola>();
            worker.Run().Wait();

        }
    }

    public class HolaMola
    {
        private readonly IReaper<ServiceRequest<ScheduleTaskDto>> _reaper;
        //private IContextClosingDependencyProvider _provider;

        public HolaMola(IReaper<ServiceRequest<ScheduleTaskDto>> reaper
        //, IContextClosingDependencyProvider provider
        )
        {
            _reaper = reaper;
            //_provider = provider;
        }
        public async Task Run()
        {
            if (_reaper == null) Console.WriteLine("Crying Babies");
            using (var queueItem = await _reaper.Reap())
            {
                //Console.WriteLine("Ooiiii");
                Console.WriteLine(queueItem?.Item?.CorrelationId ?? "");
                //var service = _provider.Provide<IScheduleTaskService>(queueItem.Item);
                //await service.Schedule(queueItem.Item.Request);
            }

            //Console.WriteLine(_reaper.GetType().Name);
            //Console.WriteLine("Happy Baby");
            await Task.Delay(10000);
        }
    }
}