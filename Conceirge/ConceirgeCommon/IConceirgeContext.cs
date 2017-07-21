using System;

namespace ConceirgeCommon
{
    public interface IConceirgeContext
    {
        string ApplicationName { get; set; }
        bool EnableLogLevelOverride { get; set; }
        Environment Environment { get; set; }
        string RequestId { get; set; }
        string SomeThingId { get; set; }
        Guid UserId { get; set; }
    }
}