using System.Collections.Generic;
using System.Text.Json;
using Amazon.Extensions.Configuration.SystemsManager.Utils;
using Xunit;

namespace Amazon.Extensions.Configuration.SystemsManager.Tests.Utils
{
    public class ParameterProcessorUtilTests
    {
        [Fact]
        public void ParseJsonParameterSuccessfully()
        {
            var result = new Dictionary<string, string>();
            var value = "{\"key\": \"value\"}";
            var keyPrefix = "prefix";

            ParameterProcessorUtil.ParseJsonParameter(keyPrefix, value, result);

            Assert.Single(result);
            Assert.Contains("prefix:key", result.Keys);
            Assert.Equal("value", result["prefix:key"]);
        }

        [Fact]
        public void ParseJsonParameterWithDuplicateKeyThrowsException()
        {
            var result = new Dictionary<string, string> { { "prefix:key", "value" } };
            var value = "{\"key\": \"newvalue\"}";
            var keyPrefix = "prefix";

            Assert.Throws<DuplicateParameterException>(() => ParameterProcessorUtil.ParseJsonParameter(keyPrefix, value, result));
        }

        [Fact]
        public void ParseJsonParameterForInvalidJsonThrowsException()
        {
            var result = new Dictionary<string, string>();
            var value = "invalid json";
            var keyPrefix = "";

            Assert.ThrowsAny<JsonException>(() => ParameterProcessorUtil.ParseJsonParameter(keyPrefix, value, result));
        }

        [Fact]
        public void ParseStringListParameterSuccessfully()
        {
            var result = new Dictionary<string, string>();
            var value = "value1,value2,value3";
            var keyPrefix = "prefix";

            ParameterProcessorUtil.ParseStringListParameter(keyPrefix, value, result);

            Assert.Equal(3, result.Count);
            Assert.Contains("prefix:0", result.Keys);
            Assert.Contains("prefix:1", result.Keys);
            Assert.Contains("prefix:2", result.Keys);
            Assert.Equal("value1", result["prefix:0"]);
            Assert.Equal("value2", result["prefix:1"]);
            Assert.Equal("value3", result["prefix:2"]);
        }

        [Fact]
        public void ParseStringListParameterWithDuplicateKeyThrowsException()
        {
            var result = new Dictionary<string, string> { { "prefix:0", "value" } };
            var value = "value1,value2,value3";
            var keyPrefix = "prefix";

            Assert.Throws<DuplicateParameterException>(() => ParameterProcessorUtil.ParseStringListParameter(keyPrefix, value, result));
        }

        [Fact]
        public void ParseStringParameterSuccessfully()
        {
            var result = new Dictionary<string, string>();
            var value = "stringValue";
            var key = "myKey";

            ParameterProcessorUtil.ParseStringParameter(key, value, result);

            Assert.Single(result);
            Assert.Contains("myKey", result.Keys);
            Assert.Equal("stringValue", result["myKey"]);
        }

        [Fact]
        public void ParseStringParameterWithDuplicateKeyThrowsException()
        {
            var result = new Dictionary<string, string> { { "myKey", "existingValue" } };
            var value = "newValue";
            var key = "myKey";

            Assert.Throws<DuplicateParameterException>(() => ParameterProcessorUtil.ParseStringParameter(key, value, result));
        }
    }
}