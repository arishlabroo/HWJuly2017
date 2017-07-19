using System;
using ConceirgeCommon;
using StructureMap;

namespace TaskCompositionRoot
{
    public interface IServiceDependencyFactory
    {
        T Create<T>(ServiceRequest serviceRequest);
    }
}