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

using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.Extensions.Configuration.SystemsManager.AppConfig;
using Amazon.Extensions.NETCore.Setup;
using Xunit;

namespace Amazon.Extensions.Configuration.SystemsManager.Tests
{
    public class AppConfigProcessorTests
    {
        [Fact]
        public async Task AddWrapperNode_WithWrapperNodeName_WrapsConfigurationCorrectly()
        {
            // Arrange
            var source = new AppConfigConfigurationSource
            {
                ApplicationId = "appId",
                EnvironmentId = "envId",
                ConfigProfileId = "profileId",
                WrapperNodeName = "FeatureFlags",
                AwsOptions = new AWSOptions()
            };
            
            var processor = new AppConfigProcessor(source);
            var inputJson = "{\"test-flag-1\":{\"enabled\":false},\"test-flag-2\":{\"enabled\":true}}";
            using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(inputJson));
            using var outputStream = new MemoryStream();
            
            // Act
            await InvokeAddWrapperNode(processor, inputStream, outputStream);
            
            // Assert
            string resultJson;
            using (var reader = new StreamReader(outputStream))
            {
                resultJson = reader.ReadToEnd();
            }
            
            var expectedJson = "{\"FeatureFlags\":{\"test-flag-1\":{\"enabled\":false},\"test-flag-2\":{\"enabled\":true}}}";
            Assert.Equal(expectedJson, resultJson);
        }
        
        // Helper method to invoke the private AddWrapperNode method using reflection
        private async Task InvokeAddWrapperNode(AppConfigProcessor processor, Stream configuration, Stream wrappedConfig)
        {
            var method = typeof(AppConfigProcessor).GetMethod("AddWrapperNodeAsync",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)method.Invoke(processor, new object[] { configuration, wrappedConfig });
        }
    }
}