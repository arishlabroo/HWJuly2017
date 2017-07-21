using StructureMap;
using TaskService;
using Microsoft.Extensions.DependencyInjection;
using TaskDomain;
using ConceirgeCommon;
using System;

namespace TaskCompositionRoot
{
    public static class ServiceCollectionExtension
    {
        public static IServiceProvider ComposeTaskService(this IServiceCollection services)
        {
            var container = new Container();

            container.Configure(config =>
            {
                // Register stuff in container, using the StructureMap APIs...  
                config.Scan(_ =>
                {
                    _.AssemblyContainingType(typeof(IScheduleTaskService));
                    //  _.SingleImplementationsOfInterface();
                    //  _.WithDefaultConventions();
                    _.ConnectImplementationsToTypesClosing(typeof(IMapper<,>));
                });

                config.For<IScheduleTaskService>().Use<ScheduleTaskService>();
                config.For<IScheduleTaskDomain>().Use<ScheduleTaskDomain>();
                


                config.Populate(services);
            });
            return container.GetInstance<IServiceProvider>();
        }
    }    
}