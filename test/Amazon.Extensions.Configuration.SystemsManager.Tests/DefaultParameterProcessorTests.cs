using System.Collections.Generic;
using Amazon.SimpleSystemsManagement.Model;
using Moq;
using Xunit;

namespace Amazon.Extensions.Configuration.SystemsManager.Tests
{
    public class DefaultParameterProcessorTests
    {
        private readonly IParameterProcessor _parameterProcessor;

        public DefaultParameterProcessorTests()
        {
            _parameterProcessor = new DefaultParameterProcessor();
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

            var data = _parameterProcessor.ProcessParameters(parameters, path);

            Assert.All(data, item => Assert.Equal(item.Value, item.Key));
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

            var data = _parameterProcessor.ProcessParameters(parameters, path);

            Assert.All(data, item => Assert.Equal(item.Value, item.Key));
        }
    }
}