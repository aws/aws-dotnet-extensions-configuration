using System.Collections.Generic;
using System.Linq;
using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.SimpleSystemsManagement.Model;
using Xunit;

namespace AWSSDK.Extensions.Configuration.SystemsManagerTests
{
    public class DefaultParameterProcessorTests
    {
        private readonly List<Parameter> _parameters = new List<Parameter>
        {
            new Parameter {Name = "/start/path/p1/p2-1", Value = "p1:p2-1"},
            new Parameter {Name = "/start/path/p1/p2-2", Value = "p1:p2-2"},
            new Parameter {Name = "/start/path/p1/p2/p3-1", Value = "p1:p2:p3-1"},
            new Parameter {Name = "/start/path/p1/p2/p3-2", Value = "p1:p2:p3-2"}
        };

        private const string Path = "/start/path";

        [Fact]
        public void GetKeyTest()
        {
            var parameterProcessor = new DefaultParameterProcessor();

            var data = _parameters.Select(parameter => new {Key = parameterProcessor.GetKey(parameter, Path), parameter.Value});
            
            Assert.All(data, item => Assert.Equal(item.Value, item.Key));
        }

        [Fact]
        public void GetValueTest()
        {
            var parameterProcessor = new DefaultParameterProcessor();

            var data = _parameters.Select(parameter => new {ParameterValue = parameter.Value, ProcessorValue = parameterProcessor.GetValue(parameter, Path)});
            
            Assert.All(data, item => Assert.Equal(item.ParameterValue, item.ProcessorValue));
        }

        [Fact]
        public void IncludeParameterTest()
        {
            var parameterProcessor = new DefaultParameterProcessor();

            var data = _parameters.Select(parameter => parameterProcessor.IncludeParameter(parameter, Path));

            Assert.All(data, Assert.True);
        }
    }
}
