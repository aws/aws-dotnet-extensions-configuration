using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Samples
{
    public static partial class Program
    {
        public static async Task Main(string[] args)
        {
            //populates some sample data to be used by this example project
            await PopulateSampleDataForThisProject().ConfigureAwait(false);

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    var env = context.HostingEnvironment;

                    // Add configuration information for the AWS SDK to use (required before any calls to AddSystemsManager)
                    // NOTE: You may need to adjust these default settings depending on your environment
                    // More Details: https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/net-dg-config-netcore.html
                    config.AddJsonFile("aws.json");

                    // Add systems manager parameter store paths
                    config.AddSystemsManager($"/dotnet-aws-samples/systems-manager-sample/common");
                    config.AddSystemsManager($"/dotnet-aws-samples/systems-manager-sample/{env.EnvironmentName}", optional: true);
                })
                .UseStartup<Startup>();
        }
    }
}
