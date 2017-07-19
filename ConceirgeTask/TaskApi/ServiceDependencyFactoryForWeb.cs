using ConceirgeCommon;
using StructureMap;
using TaskCompositionRoot;

namespace TaskApi
{
    public class ServiceDependencyFactoryForWeb : IServiceDependencyFactory
    {
        private readonly IContainer _container;

        public ServiceDependencyFactoryForWeb(IContainer container)
        {
            _container = container;
        }

        public T Create<T>(ServiceRequest serviceRequest)
        {
            var nestedContainer = _container.GetNestedContainer();
            nestedContainer.Configure(c =>
            {
                c.For<IConceirgeContext>().Use(_ => new ConceirgeContext
                {




                });
            });
            return nestedContainer.GetInstance<T>();
        }
    }
}