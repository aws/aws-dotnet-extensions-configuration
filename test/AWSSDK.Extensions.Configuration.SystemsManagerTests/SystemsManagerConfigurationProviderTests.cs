using System.Collections.Generic;
using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Amazon.Extensions.NETCore.Setup;
using Amazon.SimpleSystemsManagement.Model;
using Moq;
using Xunit;

namespace AWSSDK.Extensions.Configuration.SystemsManagerTests
{
    public class SystemsManagerConfigurationProviderTests
    {
        private readonly List<Parameter> _parameters = new List<Parameter>
        {
            new Parameter {Name = "/start/path/p1/p2-1", Value = "p1:p2-1"},
            new Parameter {Name = "/start/path/p1/p2-2", Value = "p1:p2-2"},
            new Parameter {Name = "/start/path/p1/p2/p3-1", Value = "p1:p2:p3-1"},
            new Parameter {Name = "/start/path/p1/p2/p3-2", Value = "p1:p2:p3-2"}
        };

        private readonly string _path = "/start/path";

        [Fact]
        public void ProcessParametersTest()
        {
            var data = SystemsManagerConfigurationProvider.ProcessParameters(_parameters, _path);
            
            Assert.All(data, item => Assert.Equal(item.Value, item.Key));
        }

        [Fact]
        public void LoadTest()
        {
            var source = new SystemsManagerConfigurationSource
            {
                AwsOptions = new AWSOptions(),
                Path = _path
            };

            var processor = new Mock<ISystemsManagerProcessor>();
            processor.Setup(p => p.GetParametersByPathAsync(source.AwsOptions, source.Path)).ReturnsAsync(_parameters);
            var provider = new SystemsManagerConfigurationProvider(source, processor.Object);
            
            provider.Load();

            foreach (var parameter in _parameters)
            {
                Assert.True(provider.TryGet(parameter.Value, out _));
            }
        }
    }
}
