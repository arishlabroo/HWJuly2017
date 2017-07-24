using ConceirgeCommon;
using ConceirgeCommon.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StructureMap;
using StructureMap.AutoFactory;
using StructureMap.Pipeline;
using System.Collections.Generic;
using TaskModel;

namespace TaskClient
{
    public static class ServiceExtensions
    {
        public static IServiceCollection UseQueueBasedTaskClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.UseConceirgeQueue(configuration);

            //services.AddSingleton(typeof(IMapper<SowResponse, ServiceResponse>), typeof(SowResponseToServiceResponseMapper));
            //services.AddSingleton(typeof(ISower<ServiceRequest<ScheduleTaskDto>>), typeof(ScheduleTaskQueue));


            // services.TryAddSingleton<IScheduleTaskClient, ScheduleTaskQueueClient>();

            return services;
        }
    }
    public class TaskClientServiceRegistry : Registry
    {
        public TaskClientServiceRegistry()
        {
            Scan(_ =>
            {
                _.TheCallingAssembly();
                _.AssemblyContainingType<ConceirgeQueueItemInfo>();
                //_.WithDefaultConventions();

                _.ConnectImplementationsToTypesClosing(typeof(IMapper<,>));
                //_.SingleImplementationsOfInterface();
            });

            For<IConceirgeQueue<ServiceRequest<ScheduleTaskDto>>>().Use<ConceirgeQueue<ServiceRequest<ScheduleTaskDto>>>();
            For(typeof(IConceirgeQueueItemBuilder<>)).Use(typeof(ConceirgeQueueItemBuilder<>));
            For(typeof(IConceirgeQueueItemCleaner<>)).Use(typeof(ConceirgeQueueItemCleaner<>));
            For<IScheduleTaskClient>().Use<ScheduleTaskQueueClient>();
            For<IQueue<ServiceRequest<ScheduleTaskDto>>>().Use<ScheduleTaskQueue>();
            Forward<ISower<ServiceRequest<ScheduleTaskDto>>, IQueue<ServiceRequest<ScheduleTaskDto>>>();
            Forward<IReaper<ServiceRequest<ScheduleTaskDto>>, IQueue<ServiceRequest<ScheduleTaskDto>>>();

            For(typeof(IMessageFormatter<>)).Use(typeof(JsonMessageFormatter<>));
            For<IConceirgeQueueFactory>().CreateFactory();
            //ForSingletonOf<IConceirgeQueueFactory>().Use<ConceirgeQueueFactory>();

        }
    }

    public class ConceirgeQueueFactory : IConceirgeQueueFactory
    {
        private readonly IContainer container;

        public ConceirgeQueueFactory(IContainer container)
        {
            this.container = container;
        }
        public IConceirgeQueue<T> Create<T>(ConceirgeQueueInfo queueInfo)
        {
            return container.GetInstance<ConceirgeQueue<T>>(new ExplicitArguments(new Dictionary<string, object>
            {
                {"conceirgeQueueInfo", queueInfo }
            }));
        }
    }
}