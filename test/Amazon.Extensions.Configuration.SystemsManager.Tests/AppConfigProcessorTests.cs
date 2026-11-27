/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at
 * 
 *  http://aws.amazon.com/apache2.0
 * 
 * or in the "license" file accompanying this file. This file is distributed
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Amazon.Extensions.Configuration.SystemsManager.AppConfig;
using Xunit;

namespace Amazon.Extensions.Configuration.SystemsManager.Tests
{
    public class AppConfigProcessorTests
    {
        [Fact]
        public void ParseConfig_ApplicationJson_ParsesSuccessfully()
        {
            var jsonContent = "{\"key1\":\"value1\",\"key2\":\"value2\"}";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));
            
            var result = AppConfigProcessor.ParseConfig("application/json", stream);
            
            Assert.Equal("value1", result["key1"]);
            Assert.Equal("value2", result["key2"]);
        }

        [Fact]
        public void ParseConfig_ApplicationOctetStream_ParsesJsonSuccessfully()
        {
            var jsonContent = "{\"key1\":\"value1\",\"key2\":\"value2\"}";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));
            
            var result = AppConfigProcessor.ParseConfig("application/octet-stream", stream);
            
            Assert.Equal("value1", result["key1"]);
            Assert.Equal("value2", result["key2"]);
        }

        [Fact]
        public void ParseConfig_ApplicationJsonWithCharset_ParsesSuccessfully()
        {
            var jsonContent = "{\"key1\":\"value1\",\"key2\":\"value2\"}";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));
            
            var result = AppConfigProcessor.ParseConfig("application/json; charset=utf-8", stream);
            
            Assert.Equal("value1", result["key1"]);
            Assert.Equal("value2", result["key2"]);
        }

        [Fact]
        public void ParseConfig_UnknownContentType_ParsesJsonSuccessfully()
        {
            var jsonContent = "{\"key1\":\"value1\",\"key2\":\"value2\"}";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));
            
            var result = AppConfigProcessor.ParseConfig("application/unknown", stream);
            
            Assert.Equal("value1", result["key1"]);
            Assert.Equal("value2", result["key2"]);
        }

        [Fact]
        public void ParseConfig_NullContentType_ParsesJsonSuccessfully()
        {
            var jsonContent = "{\"key1\":\"value1\",\"key2\":\"value2\"}";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));
            
            var result = AppConfigProcessor.ParseConfig(null, stream);
            
            Assert.Equal("value1", result["key1"]);
            Assert.Equal("value2", result["key2"]);
        }

        [Fact]
        public void ParseConfig_NestedJson_ParsesSuccessfully()
        {
            var jsonContent = "{\"section1\":{\"key1\":\"value1\",\"key2\":\"value2\"},\"section2\":{\"key3\":\"value3\"}}";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));
            
            var result = AppConfigProcessor.ParseConfig("application/octet-stream", stream);
            
            Assert.Equal("value1", result["section1:key1"]);
            Assert.Equal("value2", result["section1:key2"]);
            Assert.Equal("value3", result["section2:key3"]);
        }

        [Fact]
        public void ParseConfig_InvalidJson_ThrowsInvalidOperationException()
        {
            var invalidJsonContent = "{ invalid json content }";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidJsonContent));
            
            var exception = Assert.Throws<InvalidOperationException>(() => 
                AppConfigProcessor.ParseConfig("application/json", stream));
            
            Assert.Contains("Failed to parse AppConfig content as JSON", exception.Message);
            Assert.Contains("Content-Type was 'application/json'", exception.Message);
        }

        [Fact]
        public void ParseConfig_InvalidJsonWithOctetStream_ThrowsInvalidOperationException()
        {
            var invalidJsonContent = "not json at all";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidJsonContent));
            
            var exception = Assert.Throws<InvalidOperationException>(() => 
                AppConfigProcessor.ParseConfig("application/octet-stream", stream));
            
            Assert.Contains("Failed to parse AppConfig content as JSON", exception.Message);
            Assert.Contains("Content-Type was 'application/octet-stream'", exception.Message);
        }

        [Fact]
        public void ParseConfig_EmptyJson_ReturnsEmptyDictionary()
        {
            var jsonContent = "{}";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));
            
            var result = AppConfigProcessor.ParseConfig("application/json", stream);
            
            Assert.Empty(result);
        }

        [Fact]
        public void ParseConfig_EmptyStream_ThrowsInvalidOperationException()
        {
            using var stream = new MemoryStream();
            
            var exception = Assert.Throws<InvalidOperationException>(() => 
                AppConfigProcessor.ParseConfig("application/json", stream));
            
            Assert.Contains("Failed to parse AppConfig content as JSON", exception.Message);
        }

        [Fact]
        public void ParseConfig_ArrayWithSection_NestsItemsUnderSection()
        {
            // The example from the issue: an array profile should come back as
            // MyProfile:0, MyProfile:1, MyProfile:2 instead of 0, 1, 2 at the root.
            var jsonContent = "[\"Ironman\", \"Hulk\", \"Thor\"]";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));

            var result = AppConfigProcessor.ParseConfig("application/json", stream, "MyProfile");

            Assert.Equal(3, result.Count);
            Assert.Equal("Ironman", result["MyProfile:0"]);
            Assert.Equal("Hulk", result["MyProfile:1"]);
            Assert.Equal("Thor", result["MyProfile:2"]);
        }

        [Fact]
        public void ParseConfig_ObjectWithSection_NestsKeysUnderSection()
        {
            var jsonContent = "{\"key1\":\"value1\",\"key2\":\"value2\"}";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));

            var result = AppConfigProcessor.ParseConfig("application/json", stream, "MyProfile");

            Assert.Equal("value1", result["MyProfile:key1"]);
            Assert.Equal("value2", result["MyProfile:key2"]);
            Assert.DoesNotContain(result, pair => pair.Key == "key1");
        }

        [Fact]
        public void ParseConfig_NestedJsonWithSection_NestsKeysUnderSection()
        {
            var jsonContent = "{\"section1\":{\"key1\":\"value1\"}}";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));

            var result = AppConfigProcessor.ParseConfig("application/json", stream, "MyProfile");

            Assert.Equal("value1", result["MyProfile:section1:key1"]);
        }

        [Fact]
        public void ParseConfig_NestedSection_NestsKeysUnderEachSegment()
        {
            var jsonContent = "{\"key1\":\"value1\"}";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));

            var result = AppConfigProcessor.ParseConfig("application/json", stream, "Parent:Child");

            Assert.Equal("value1", result["Parent:Child:key1"]);
        }

        [Fact]
        public void ParseConfig_NullSection_KeepsContentAtRoot()
        {
            var jsonContent = "{\"key1\":\"value1\"}";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));

            var result = AppConfigProcessor.ParseConfig("application/json", stream, section: null);

            Assert.Equal("value1", result["key1"]);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(":MyProfile")]
        [InlineData("MyProfile:")]
        [InlineData("MyProfile::Nested")]
        public void ParseConfig_InvalidSection_ThrowsArgumentException(string section)
        {
            var jsonContent = "{\"key1\":\"value1\"}";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));

            var exception = Assert.Throws<ArgumentException>(() =>
                AppConfigProcessor.ParseConfig("application/json", stream, section));

            Assert.Contains("section", exception.Message);
        }
    }
}
