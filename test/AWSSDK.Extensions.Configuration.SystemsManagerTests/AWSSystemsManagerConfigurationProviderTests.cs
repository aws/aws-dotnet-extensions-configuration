using System.Collections.Generic;
using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Amazon.Extensions.NETCore.Setup;
using Amazon.SimpleSystemsManagement.Model;
using Moq;
using Xunit;

namespace AWSSDK.Extensions.Configuration.SystemsManagerTests
{
    public class AWSSystemsManagerConfigurationProviderTests
    {
        [Fact]
        public void ProcessParametersTest()
        {
            var path = "/start/path";
            var parameters = new List<Parameter>
            {
                new Parameter {Name = "/start/path/p1/p2-1", Value = "p1:p2-1"}, 
                new Parameter {Name = "/start/path/p1/p2-2", Value = "p1:p2-2"}, 
                new Parameter {Name = "/start/path/p1/p2/p3-1", Value = "p1:p2:p3-1"}, 
                new Parameter {Name = "/start/path/p1/p2/p3-2", Value = "p1:p2:p3-2"}
            };
            
            var data = AWSSystemsManagerConfigurationProvider.ProcessParameters(parameters, path);
            
            Assert.All(data, item => Assert.Equal(item.Value, item.Key));
        }

        [Fact]
        public void LoadTest()
        {
            var parameters = new List<Parameter>
            {
                new Parameter {Name = "/start/path/p1/p2-1", Value = "p1:p2-1"}, 
                new Parameter {Name = "/start/path/p1/p2-2", Value = "p1:p2-2"}, 
                new Parameter {Name = "/start/path/p1/p2/p3-1", Value = "p1:p2:p3-1"}, 
                new Parameter {Name = "/start/path/p1/p2/p3-2", Value = "p1:p2:p3-2"}
            };
            var source = new AWSSystemsManagerConfigurationSource
            {
                AwsOptions = new AWSOptions(),
                Path = "/start/path"
            };
            var processor = new Mock<IAWSSystemsManagerProcessor>();
            processor.Setup(p => p.GetParametersByPathAsync(source.AwsOptions, source.Path)).ReturnsAsync(parameters);
            var provider = new AWSSystemsManagerConfigurationProvider(source, processor.Object);
            
            provider.Load();

            foreach (var parameter in parameters)
            {
                Assert.True(provider.TryGet(parameter.Value, out _));
            }
        }
    }
}
