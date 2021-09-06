using System;
using Amazon.Extensions.Configuration.SystemsManager;
using Microsoft.Extensions.Configuration;

namespace Lambda.Configuration
{
    public static class ConfigurationReader
    {
        private static IConfigurationRoot config;

        public static void Init()
        {
            var appConfig = AwsAppConfigConfiguration.GetFromEnvironmentVariables();
            config = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .AddAppConfigForLambda(appConfig.ApplicationId, appConfig.EnvironmentId, appConfig.ConfigProfileId, appConfig.ConfigReloadTime)
                    .Build();
        }

        public static void EnsureConfigurationIsReloaded()
        {
            config.WaitForSystemsManagerReloadToComplete(TimeSpan.FromSeconds(2));
        }

        public static TestParameters GetTestParameters()
        {
            var testParameters = new TestParameters();
            config.GetSection("TestParameters").Bind(testParameters);
            return testParameters;
        }
    }
}
