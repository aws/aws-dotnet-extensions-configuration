using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Samples
{
    public static partial class Program
    {
        public static async Task Main(string[] args)
        {
            //populates some sample data to be used by this example project
            // await PopulateSampleDataForThisProject().ConfigureAwait(false);

            var x = CreateWebHostBuilder(args);
            var y = x.Build();
            y.Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                 .ConfigureWebHostDefaults(webBuilder =>
                 {
                     webBuilder.UseStartup<Startup>();
                 }).ConfigureAppConfiguration((hostingContext, config) =>
                 {
                     var env = hostingContext.HostingEnvironment;

                     // NOTE: A default AWS SDK configuration has been added to appsettings.Development.json.
                     // More Details can be found at: https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/net-dg-config-netcore.html

                     // Add systems manager parameter store paths
                     config.AddSystemsManager($"/dotnet-aws-samples/systems-manager-sample/common");
                     config.AddSystemsManager($"/dotnet-aws-samples/systems-manager-sample/{env.EnvironmentName}", optional: true);
                 });

        }
    }
}
