using System;

namespace ConceirgeCommon
{
    public enum Environment
    {
        Development,
        Staging,
        Test,
        Prod
    }

    public class ConceirgeContext : IConceirgeContext
    {
        public string RequestId { get; set; }
        public string SomeThingId { get; set; }
        public string ApplicationName { get; set; }
        public string CorrelationId { get; set; }
        public bool EnableLogLevelOverride { get; set; }
        public Environment Environment { get; set; }
        public Guid UserId { get; set; }
    }
}