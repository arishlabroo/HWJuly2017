using System;

namespace ConceirgeCommon
{
    public class ServiceRequest
    {
        public string RequestId { get; set; }
        public string CorrelationId { get; set; }
        public string SomethingId { get; set; }
        public Guid UserId { get; set; }
        public bool EnableLogLevelOverride { get; set; }
    }


    public class ServiceRequest<T> : ServiceRequest
    {
        public T Request { get; set; }

    }

    public class ServiceResponse
    {
        //Maybe we make this the httpstatuscode like aws clients
        public string StatusCode { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
    }

    public class ServiceResponse<T> : ServiceResponse
    {
        public T Response { get; set; }
    }
}