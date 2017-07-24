using ConceirgeCommon;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;
using System;
using TaskClient;

namespace CadencePlaceholderForDemo
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.UseQueueBasedTaskClient(Configuration);

            var container = new Container();

            container.Configure(config =>
            {
                // Register stuff in container, using the StructureMap APIs...  
                config.Scan(_ =>
                {
                    //_.AssemblyContainingType(typeof(IScheduleTaskService));
                    //  _.SingleImplementationsOfInterface();
                    //  _.WithDefaultConventions();
                    _.ConnectImplementationsToTypesClosing(typeof(IMapper<,>));
                });
                config.AddRegistry<TaskClientServiceRegistry>();
                



                config.Populate(services);
            });
            return container.GetInstance<IServiceProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseMvc();
        }
    }
}
