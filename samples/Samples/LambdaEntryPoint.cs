using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Samples
{
    public class LambdaEntryPoint : Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    {

        protected override void Init(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var env = context.HostingEnvironment;

                // NOTE: A default AWS SDK configuration has been added to appsettings.Development.json.
                // More Details can be found at: https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/net-dg-config-netcore.html

                // Add systems manager parameter store paths
                config.AddSystemsManager($"/dotnet-aws-samples/systems-manager-sample/common");
                config.AddSystemsManager($"/dotnet-aws-samples/systems-manager-sample/{env.EnvironmentName}", optional: true);
            })
            .UseStartup<Startup>();
        }

    }
}
