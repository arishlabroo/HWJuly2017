using ConceirgeCommon;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;
using TaskClient;

namespace TaskWorker
{
    public class Startup
    {
        protected string basePath;
        protected string envName;

        public Startup(string basePath, string envName)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{envName}.json", optional: true)
                //.Add(new ZG.RevOC.Config.HashiConfigSource("consul.service.consul", "vault.service.consul", "zg-concierge/pokemon"))
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }
        public IContainer Container { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            //Service.ServiceExtensions.AddService(services);
            services.AddOptions();
            //services.Configure<Config.AppConfig>(Configuration.GetSection("AppConfig"));

            services.UseQueueBasedTaskClient(Configuration);





            return ConfigureIoC(services);
        }

        // use StructureMap for IoC so services in this assembly can be wired up automatically
        protected virtual IServiceProvider ConfigureIoC(IServiceCollection services)
        {
            var container = new Container();
            container.Configure(cf =>
            {
                cf.Scan(x =>
                {
                    x.TheCallingAssembly();
                    ScanAssemblies(x);
                    x.LookForRegistries();
                    x.ConnectImplementationsToTypesClosing(typeof(IMapper<,>));
                });

                cf.AddRegistry<TaskClientServiceRegistry>();

                cf.Populate(services);
            });

            Container = container;
            return container.GetInstance<IServiceProvider>();
        }

        protected virtual void ScanAssemblies(StructureMap.Graph.IAssemblyScanner scanner)
        {
        }
    }
    public class ServiceProvider : IContextClosingDependencyProvider
    {
        private readonly IContainer _container;

        public ServiceProvider(IContainer container)
        {
            _container = container;
        }

        public T Provide<T>(ServiceRequest serviceRequest)
        {
            var nested = _container.GetNestedContainer();
            nested.Configure(cf =>
            {
                cf.ForSingletonOf<IConceirgeContext>().Use(ctx => new ConceirgeContext{
                    ApplicationName = serviceRequest.SomethingId,
                    SomeThingId = serviceRequest.SomethingId,
                    RequestId = serviceRequest.RequestId,
                    CorrelationId = serviceRequest.CorrelationId,
                    EnableLogLevelOverride = serviceRequest.EnableLogLevelOverride,
                    UserId = serviceRequest.UserId
                });
            });
            return nested.TryGetInstance<T>();
        }
    }
}