using System;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Lambda.Configuration;

namespace Lambda.Functions
{
    public class ConfigLambda
    {
        public ConfigLambda()
        {
            AWSSDKHandler.RegisterXRayForAllServices();
            ConfigurationReader.Init();
        }

        protected void Invoke()
        {
            ConfigurationReader.EnsureConfigurationIsReloaded();

            var testParameters = ConfigurationReader.GetTestParameters();
            Console.WriteLine($"TestParam: {testParameters.TestParam}");
        }
    }
}
