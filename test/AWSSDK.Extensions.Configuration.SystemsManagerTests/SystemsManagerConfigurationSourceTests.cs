using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AWSSDK.Extensions.Configuration.SystemsManagerTests
{
    public class SystemsManagerConfigurationSourceTests
    {
        [Fact]
        public void BuildSuccessTest()
        {
            var source = new SystemsManagerConfigurationSource
            {
                AwsOptions = new AWSOptions(),
                Path = "/temp/"
            };
            var builder = new ConfigurationBuilder();

            var result = source.Build(builder);

            Assert.IsType<SystemsManagerConfigurationProvider>(result);
        }
    }
}
