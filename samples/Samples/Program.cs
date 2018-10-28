using System.Threading.Tasks;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;

namespace Samples
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    var env = context.HostingEnvironment;

                    //create an AWSOptions to be used when calling the AWS API
                    //https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/net-dg-config-netcore.html
                    //update as required for your environment
                    var awsOptions = new AWSOptions {Profile = "default", Region = RegionEndpoint.USEast1};

                    //populates some sample data to be used by this example project
                    config.PopulateSampleDataForThisProject(context, awsOptions);

                    //add systems manager parameter store paths
                    config.AddSystemsManager($"/dotnet-aws-samples/{env.ApplicationName}/common", awsOptions);
                    config.AddSystemsManager($"/dotnet-aws-samples/{env.ApplicationName}/{env.EnvironmentName}", awsOptions, optional: true);
                })
                .UseStartup<Startup>();
        }

        /// <summary>
        /// This exists only to populate some sample data to be used by this example project
        /// </summary>
        public static void PopulateSampleDataForThisProject(this IConfigurationBuilder builder, WebHostBuilderContext context, AWSOptions awsOptions)
        {
            var env = context.HostingEnvironment;

            var root = $"/dotnet-aws-samples/{env.ApplicationName}/common";
            var parameters = new[]
            {
                new {Name = "StringValue", Value = "string-value"},
                new {Name = "IntegerValue", Value = "10"},
                new {Name = "DateTimeValue", Value = "2000-01-01"},
                new {Name = "BooleanValue", Value = "True"},
                new {Name = "TimeSpanValue", Value = "00:05:00"},
            };

            async Task CreateParameters()
            {
                using (var client = awsOptions.CreateServiceClient<IAmazonSimpleSystemsManagement>())
                {
                    var result = await client.GetParametersByPathAsync(new GetParametersByPathRequest { Path = root, Recursive = true }).ConfigureAwait(false);
                    if (result.Parameters.Any()) return;

                    foreach (var parameter in parameters)
                    {
                        var name = $"{root}/Settings/{parameter.Name}";
                        await client.PutParameterAsync(new PutParameterRequest { Name = name, Value = parameter.Value, Type = ParameterType.String, Overwrite = true }).ConfigureAwait(false);
                    }
                }
            }

            CreateParameters().GetAwaiter().GetResult();
        }
    }
}
