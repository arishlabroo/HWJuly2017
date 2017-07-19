using System;

namespace ConceirgeCommon
{
    public enum Environment
    {
        Dev,
        Staging,
        Test,
        Prod
    }

    public class ConceirgeContext : IConceirgeContext
    {
        public string RequestId { get; set; }
        public string SessionId { get; set; }
        public string ApplicationName { get; set; }
        public bool EnableLogLevelOverride { get; set; }
        public Environment Environment { get; set; }
        public Guid UserId { get; set; }
    }


    public class ServiceRequest
    {
        public string RequestId { get; set; }
        public string SessionId { get; set; }
        public Guid UserId { get; set; }
        public bool EnableLogLevelOverride { get; set; }
    }

    public class ServiceRequest<T> : ServiceRequest
    {
        public T Request { get; set; }

    }
}