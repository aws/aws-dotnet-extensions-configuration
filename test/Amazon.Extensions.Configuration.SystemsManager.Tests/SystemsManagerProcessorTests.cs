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
        private readonly List<Parameter> _parameters = new List<Parameter>
        {
            new Parameter {Name = "/start/path/p1/p2-1", Value = "p1:p2-1"},
            new Parameter {Name = "/start/path/p1/p2-2", Value = "p1:p2-2"},
            new Parameter {Name = "/start/path/p1/p2/p3-1", Value = "p1:p2:p3-1"},
            new Parameter {Name = "/start/path/p1/p2/p3-2", Value = "p1:p2:p3-2"}
        };

        private const string Path = "/start/path";

        private readonly Mock<IParameterProcessor> _parameterProcessorMock;

        public SystemsManagerProcessorTests()
        {
            _parameterProcessorMock = new Mock<IParameterProcessor>();
        }

        [Fact]
        public void ProcessParametersTest()
        {
            foreach (var parameter in _parameters)
            {
                _parameterProcessorMock.Setup(processor => processor.IncludeParameter(parameter, Path)).Returns(true);
                _parameterProcessorMock.Setup(processor => processor.GetKey(parameter, Path)).Returns(parameter.Value);
                _parameterProcessorMock.Setup(processor => processor.GetValue(parameter, Path)).Returns(parameter.Value);
            }

            var data = SystemsManagerProcessor.ProcessParameters(_parameters, Path, _parameterProcessorMock.Object);

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
            var data = new Dictionary<string, string> {{"Key", "Value"}};
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

        [Theory]
        [InlineData(null)]
        [InlineData("label")]
        public void CreateLabelFilterTest(string label)
        { 
            var output = SystemsManagerProcessor.CreateLabelFilter(label);

            if (label == null)
            {
                Assert.Null( output);
            }
            else
            {
                Assert.NotNull(output);
                Assert.Single(output);
                Assert.Equal("Label", output.Single().Key);
                Assert.Equal("Equals", output.Single().Option);
                Assert.Single(output.Single().Values);
                Assert.Equal(label, output.Single().Values.Single());
            }
        }
    }
}