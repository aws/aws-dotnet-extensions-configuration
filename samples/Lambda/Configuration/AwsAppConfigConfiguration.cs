using System;

namespace Lambda.Configuration
{
    public class AwsAppConfigConfiguration
    {
        public readonly string ApplicationId;
        public readonly string EnvironmentId;
        public readonly string ConfigProfileId;
        public readonly TimeSpan? ConfigReloadTime;

        private AwsAppConfigConfiguration(string applicationId, string environmentId, string configProfileId, TimeSpan? configReloadTime)
        {
            ApplicationId = applicationId;
            EnvironmentId = environmentId;
            ConfigProfileId = configProfileId;
            ConfigReloadTime = configReloadTime;
        }

        public static AwsAppConfigConfiguration GetFromEnvironmentVariables()
        {
            var reloadTimeInSeconds = Environment.GetEnvironmentVariable("AppConfigReloadTimeInSeconds");
            var reloadTime = reloadTimeInSeconds == null ? (TimeSpan?) null : TimeSpan.FromSeconds(int.Parse(reloadTimeInSeconds));

            return new AwsAppConfigConfiguration(
                Environment.GetEnvironmentVariable("AppConfigAppId")!,
                Environment.GetEnvironmentVariable("AppConfigEnvironmentId")!,
                Environment.GetEnvironmentVariable("AppConfigConfigProfileId")!,
                reloadTime
            );
        }
    }
}
