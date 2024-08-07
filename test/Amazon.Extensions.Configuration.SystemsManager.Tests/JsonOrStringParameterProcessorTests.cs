using System.Collections.Generic;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Xunit;

namespace Amazon.Extensions.Configuration.SystemsManager.Tests
{
    public class JsonOrStringParameterProcessorTests
    {
        private readonly IParameterProcessor _parameterProcessor = new JsonOrStringParameterProcessor();

        [Fact]
        public void ParsesJsonParametersSuccessfully()
        {
            var parameters = new List<Parameter>
            {
                new Parameter() { Name = "/test/level1/level2", Type = ParameterType.String, Value = "{\"level1Key\":\"level1value\"}" },
                new Parameter() { Name = "/test/level1", Type = ParameterType.String, Value = "{\"level1Key\" : {\"level2key\" : \"level2value\"}}" }
            };
            var result = _parameterProcessor.ProcessParameters(parameters, "/test");

            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey("level1:level2:level1Key"));
            Assert.True(result.ContainsKey("level1:level1Key:level2key"));
            Assert.Equal("level1value", result["level1:level2:level1Key"]);
            Assert.Equal("level2value", result["level1:level1Key:level2key"]);
        }

        [Fact]
        public void ProcessParameters_ParsesJsonParametersWithoutPrefixSuccessfully()
        {
            var parameters = new List<Parameter>
            {
                new Parameter()
                {
                    Name = "/test/level1/level2", Type = ParameterType.String, Value = "{\"level1Key\":\"level1value\"}"
                },
                new Parameter()
                {
                    Name = "/test/level1", Type = ParameterType.String,
                    Value = "{\"level1Key\" : {\"level2key\" : \"level2value\"}}"
                }
            };
            var result = _parameterProcessor.ProcessParameters(parameters, "");

            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey("test:level1:level2:level1Key"));
            Assert.True(result.ContainsKey("test:level1:level1Key:level2key"));
            Assert.Equal("level1value", result["test:level1:level2:level1Key"]);
            Assert.Equal("level2value", result["test:level1:level1Key:level2key"]);
        }


        [Fact]
        public void ProcessParameters_FallsBackOnString()
        {
            var parameters = new List<Parameter>
            {
                new Parameter() { Name = "/test/stringParam", Type = ParameterType.String, Value = "some string" }
            };
            var result = _parameterProcessor.ProcessParameters(parameters, "/test");

            Assert.Equal(1, result.Count);
            Assert.True(result.ContainsKey("stringParam"));
            Assert.Equal("some string", result["stringParam"]);
        }

        [Fact]
        public void ProcessParameters_ThrowsOnDuplicateParameter()
        {
            var parameters = new List<Parameter>
            {
                new Parameter() { Name = "/test/Duplicate", Type = ParameterType.String, Value = "value1" },
                new Parameter() { Name = "/test/duplicate", Type = ParameterType.String, Value = "value2" }
            };
            var duplicateParameterException = Assert.Throws<DuplicateParameterException>(() => _parameterProcessor.ProcessParameters(parameters, "/test"));
            Assert.Equal("Duplicate parameter 'duplicate' found. Parameter keys are case-insensitive.", duplicateParameterException.Message);
        }

        [Fact]
        public void ProcessParameters_ThrowsOnDuplicateParameterAtMultiLevel()
        {
            var parameters = new List<Parameter>
            {
                new Parameter() { Name = "/test/level1", Type = ParameterType.String, Value = "{\"level1Key\":\"level1value\"}" },
                new Parameter() { Name = "/test/level1/level1key", Type = ParameterType.String, Value = "level1valueOverriden" },
            };

            var duplicateParameterException = Assert.Throws<DuplicateParameterException>(() => _parameterProcessor.ProcessParameters(parameters, "/test"));
            Assert.Equal("Duplicate parameter 'level1:level1key' found. Parameter keys are case-insensitive.", duplicateParameterException.Message);
        }

        [Fact]
        public void ProcessParameters_ThrowsOnDuplicateParameterAtMultilevelForJsonArray()
        {
            var parameters = new List<Parameter>
            {
                new Parameter() { Name = "/test/level1", Type = ParameterType.String, Value = "{\"level1Key\" : [\"level1value1\", \"level1value2\"] }" },
                new Parameter() { Name = "/test/level1/Level1key", Type = ParameterType.StringList, Value = "level1valueOverriden" },
            };

            var duplicateParameterException = Assert.Throws<DuplicateParameterException>(() => _parameterProcessor.ProcessParameters(parameters, "/test"));
            Assert.Equal("Duplicate parameter 'level1:Level1key:0' found. Parameter keys are case-insensitive.", duplicateParameterException.Message);
        }


        [Fact]
        public void ProcessParameters_ProcessesStringListParameters()
        {
            var parameters = new List<Parameter>
            {
                new Parameter() { Name = "/test/stringList", Type = ParameterType.StringList, Value = "value1,value2" }
            };
            var result = _parameterProcessor.ProcessParameters(parameters, "/test");

            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey("stringList:0"));
            Assert.Equal("value1", result["stringList:0"]);
            Assert.True(result.ContainsKey("stringList:1"));
            Assert.Equal("value2", result["stringList:1"]);
        }
    }
}