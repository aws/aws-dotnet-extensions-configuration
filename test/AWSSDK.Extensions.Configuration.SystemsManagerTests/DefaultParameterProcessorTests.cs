using System.Collections.Generic;
using System.Linq;
using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Amazon.Extensions.NETCore.Setup;
using Amazon.SimpleSystemsManagement.Model;
using Moq;
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

        private readonly string _path = "/start/path";

        [Fact]
        public void NormalizeKeyTest()
        {
            var parameterProcessor = new DefaultParameterProcessor();

            var data = _parameters.Select(parameter => new {Key = parameterProcessor.GetKey(parameter, _path), parameter.Value});
            
            Assert.All(data, item => Assert.Equal(item.Value, item.Key));
        }

        [Fact]
        public void IncludeParameterTest()
        {
            var parameterProcessor = new DefaultParameterProcessor();

            var data = _parameters.Select(parameter => parameterProcessor.IncludeParameter(parameter, _path));

            Assert.All(data, Assert.True);
        }
    }
}
