using Amazon.SQS;
using ConceirgeCommon.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace ConceirgeCommon
{
    public static class ServiceExtensions
    {
        public static IServiceCollection UseConceirgeQueue(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddDefaultAWSOptions(configuration.GetAWSOptions());
            services.AddAWSService<IAmazonSQS>();
            return services;
        }
    }
}