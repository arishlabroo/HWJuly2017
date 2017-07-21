using ConceirgeCommon;
using Microsoft.AspNetCore.Hosting;
using StructureMap;
using TaskCompositionRoot;
using System;
using Microsoft.AspNetCore.Http;

namespace TaskApi
{
    public class ServiceDependencyFactoryForWeb : IServiceDependencyFactory
    {
        private readonly IContainer _container;
        private readonly IHostingEnvironment _hostingEnvironement;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ServiceDependencyFactoryForWeb(
            IContainer container,
            IHostingEnvironment hostingEnvironement,
            IHttpContextAccessor httpContextAccessor)
        {
            _container = container;
            _hostingEnvironement = hostingEnvironement;
            _httpContextAccessor = httpContextAccessor;
        }

        public T Create<T>(ServiceRequest serviceRequest)
        {
            var nestedContainer = _container.GetNestedContainer();
            var context = new ConceirgeContext
            {
                ApplicationName = "Apple",
                EnableLogLevelOverride = serviceRequest?.EnableLogLevelOverride ?? false,
                Environment = Enum.Parse<ConceirgeCommon.Environment>(_hostingEnvironement.EnvironmentName),
                RequestId = _httpContextAccessor.HttpContext.TraceIdentifier,
                CorrelationId = serviceRequest?.CorrelationId,
                SomeThingId = serviceRequest?.SomethingId
            };
            nestedContainer.Configure(c =>
            {
                c.For<IConceirgeContext>().Use(_ => context);
            });

            return nestedContainer.GetInstance<T>();
        }
    }
}