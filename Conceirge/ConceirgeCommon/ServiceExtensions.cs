using Amazon;
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
            //services.AddAWSService<IAmazonSQS>();
            //var x = new RegionEndpoint()
            services.AddSingleton<IAmazonSQS>((s) => new AmazonSQSClient(new AmazonSQSConfig
            {
                RegionEndpoint = RegionEndpoint.USWest2,
                ServiceURL = "http://localhost:4576",
                UseHttp = true
                
                //RegionEndpoint = RegionEndpoint.USWest2,
                
            }){
                
            });



            return services;
        }
    }
}