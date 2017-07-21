using System;
using ConceirgeCommon;

namespace TaskCompositionRoot
{
    public interface IServiceDependencyFactory
    {
        T Create<T>(ServiceRequest serviceRequest);
    }
}