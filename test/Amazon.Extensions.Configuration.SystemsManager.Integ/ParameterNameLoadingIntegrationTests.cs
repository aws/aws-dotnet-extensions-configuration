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
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Amazon.Extensions.Configuration.SystemsManager.Integ
{
    /// <summary>
    /// Integration tests for loading parameters by name collection.
    /// 
    /// NOTE: These tests require AWS credentials and will create/delete test parameters in Parameter Store.
    /// The tests use the path prefix: /configuration-extension-testdata/param-names/
    /// 
    /// To run these tests:
    /// 1. Ensure AWS credentials are configured (via environment variables, AWS CLI, or IAM role)
    /// 2. Ensure the credentials have permissions for ssm:PutParameter, ssm:GetParameters, ssm:DeleteParameters
    /// 3. Run: dotnet test --filter "FullyQualifiedName~ParameterNameLoadingIntegrationTests"
    /// </summary>
    public class ParameterNameLoadingIntegrationTests : IClassFixture<ParameterNameLoadingTestFixture>
    {
        private readonly ParameterNameLoadingTestFixture _fixture;

        public ParameterNameLoadingIntegrationTests(ParameterNameLoadingTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void TestLoadParametersByName_WithPathPrefix()
        {
            // Arrange
            var configurationBuilder = new ConfigurationBuilder();
            var parameterNames = new List<string> { "app/database/host", "app/database/port", "app/api/key" };
            
            // Act
            configurationBuilder.AddSystemsManager(
                ParameterNameLoadingTestFixture.ParameterPrefix, 
                parameterNames, 
                _fixture.AWSOptions);
            var configuration = configurationBuilder.Build();

            // Assert
            Assert.Equal("localhost", configuration["app:database:host"]);
            Assert.Equal("5432", configuration["app:database:port"]);
            Assert.Equal("test-api-key-123", configuration["app:api:key"]);
        }

        [Fact]
        public void TestLoadParametersByName_WithDecryption()
        {
            // Arrange
            var configurationBuilder = new ConfigurationBuilder();
            var parameterNames = new List<string> { "secure/password", "secure/token" };
            
            // Act
            configurationBuilder.AddSystemsManager(
                ParameterNameLoadingTestFixture.ParameterPrefix, 
                parameterNames, 
                _fixture.AWSOptions);
            var configuration = configurationBuilder.Build();

            // Assert - SecureString parameters should be decrypted
            Assert.Equal("encrypted-password-value", configuration["secure:password"]);
            Assert.Equal("encrypted-token-value", configuration["secure:token"]);
        }

        [Fact]
        public void TestLoadParametersByName_NonExistentParameter_Optional()
        {
            // Arrange
            var configurationBuilder = new ConfigurationBuilder();
            var parameterNames = new List<string> { "app/database/host", "nonexistent/parameter" };
            
            // Act
            configurationBuilder.AddSystemsManager(
                ParameterNameLoadingTestFixture.ParameterPrefix, 
                parameterNames, 
                _fixture.AWSOptions,
                optional: true);
            var configuration = configurationBuilder.Build();

            // Assert - Should load only the existing parameter
            Assert.Equal("localhost", configuration["app:database:host"]);
            Assert.Null(configuration["nonexistent:parameter"]);
        }

        [Fact]
        public void TestLoadParametersByName_NonExistentParameter_NotOptional()
        {
            // Arrange
            var configurationBuilder = new ConfigurationBuilder();
            var parameterNames = new List<string> { "app/database/host", "nonexistent/parameter" };
            
            // Act & Assert - Should throw ParameterNotFoundException
            configurationBuilder.AddSystemsManager(
                ParameterNameLoadingTestFixture.ParameterPrefix, 
                parameterNames, 
                _fixture.AWSOptions,
                optional: false);
            
            var exception = Assert.Throws<ParameterNotFoundException>(() => configurationBuilder.Build());
            Assert.Contains("nonexistent/parameter", exception.Message);
        }

        [Fact]
        public void TestLoadParametersByName_BatchingBehavior()
        {
            // Arrange - Test with more than 10 parameters to verify batching
            var configurationBuilder = new ConfigurationBuilder();
            var parameterNames = new List<string>
            {
                "batch/param1", "batch/param2", "batch/param3", "batch/param4", "batch/param5",
                "batch/param6", "batch/param7", "batch/param8", "batch/param9", "batch/param10",
                "batch/param11", "batch/param12"
            };
            
            // Act
            configurationBuilder.AddSystemsManager(
                ParameterNameLoadingTestFixture.ParameterPrefix, 
                parameterNames, 
                _fixture.AWSOptions);
            var configuration = configurationBuilder.Build();

            // Assert - All parameters should be loaded despite batching
            Assert.Equal("value1", configuration["batch:param1"]);
            Assert.Equal("value5", configuration["batch:param5"]);
            Assert.Equal("value10", configuration["batch:param10"]);
            Assert.Equal("value11", configuration["batch:param11"]);
            Assert.Equal("value12", configuration["batch:param12"]);
        }

        [Fact]
        public void TestLoadParametersByName_EmptyCollection()
        {
            // Arrange
            var configurationBuilder = new ConfigurationBuilder();
            var parameterNames = new List<string>();
            
            // Act & Assert - Should throw ArgumentException for empty collection
            Assert.Throws<ArgumentException>(() =>
            {
                configurationBuilder.AddSystemsManager(
                    ParameterNameLoadingTestFixture.ParameterPrefix, 
                    parameterNames, 
                    _fixture.AWSOptions);
            });
        }
    }
}
