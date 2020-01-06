using System.Collections.Generic;
using System.Linq;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Amazon.SimpleSystemsManagement.Model;
using Moq;
using Xunit;

namespace Amazon.Extensions.Configuration.SystemsManager.Tests
{
    public class SystemsManagerProcessorTests
    {
        private readonly Mock<IParameterProcessor> _parameterProcessorMock;

        public SystemsManagerProcessorTests()
        {
            _parameterProcessorMock = new Mock<IParameterProcessor>();
        }

        [Fact]
        public void ProcessParametersTest()
        {
            var parameters = new List<Parameter>
            {
                new Parameter {Name = "/start/path/p1/p2-1", Value = "p1:p2-1"},
                new Parameter {Name = "/start/path/p1/p2-2", Value = "p1:p2-2"},
                new Parameter {Name = "/start/path/p1/p2/p3-1", Value = "p1:p2:p3-1"},
                new Parameter {Name = "/start/path/p1/p2/p3-2", Value = "p1:p2:p3-2"},
            };

            const string path = "/start/path";

            foreach (var parameter in parameters)
            {
                _parameterProcessorMock.Setup(processor => processor.IncludeParameter(parameter, path)).Returns(true);
                _parameterProcessorMock.Setup(processor => processor.Process(parameter, path))
                                       .Returns(new List<KeyValuePair<string, string>> 
                                       { 
                                           { 
                                               new KeyValuePair<string, string>(
                                                   parameter.Value,
                                                   parameter.Value)
                                           } 
                                       });
            }

            var data = SystemsManagerProcessor.ProcessParameters(parameters, path, _parameterProcessorMock.Object);

            Assert.All(data, item => Assert.Equal(item.Value, item.Key));

            _parameterProcessorMock.VerifyAll();
        }

        [Fact]
        public void ProcessParametersRootTest()
        {
            var parameters = new List<Parameter>
            {
                new Parameter {Name = "/p1", Value = "p1"},
                new Parameter {Name = "p2", Value = "p2"},
            };

            const string path = "/";

            foreach (var parameter in parameters)
            {
                _parameterProcessorMock.Setup(processor => processor.IncludeParameter(parameter, path)).Returns(true);
                _parameterProcessorMock.Setup(processor => processor.Process(parameter, path))
                                       .Returns(new List<KeyValuePair<string, string>>
                                       {
                                           {
                                               new KeyValuePair<string, string>(parameter.Value,parameter.Value)
                                           }
                                       }) ;
            }

            var data = SystemsManagerProcessor.ProcessParameters(parameters, path, _parameterProcessorMock.Object);

            Assert.All(data, item => Assert.Equal(item.Value, item.Key));

            _parameterProcessorMock.VerifyAll();
        }

        [Theory]
        [InlineData("/aws/reference/secretsmanager/", true)]
        [InlineData("/not-sm-path/", false)]
        public void IsSecretsManagerPathTest(string path, bool expected)
        {
            Assert.Equal(expected, SystemsManagerProcessor.IsSecretsManagerPath(path));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("prefix")]
        public void AddPrefixTest(string prefix)
        {
            var data = new Dictionary<string, string> { { "Key", "Value" } };
            var output = SystemsManagerProcessor.AddPrefix(data, prefix);

            if (prefix == null)
            {
                Assert.Equal(data, output);
            }
            else
            {
                foreach (var item in output)
                {
                    Assert.StartsWith($"{prefix}:", item.Key);
                }
            }
        }
    }
}