/*
 * Copyright 2018 Amazon.com, Inc. or its affiliates. All Rights Reserved.
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
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Amazon.Extensions.Configuration.SystemsManager.Integ
{
    public class ConfigurationBuilderIntegrationTests : IClassFixture<IntegTestFixture>
    {
        private IntegTestFixture fixture;

        public ConfigurationBuilderIntegrationTests(IntegTestFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void TestConfigurationBuilder()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddSystemsManager(IntegTestFixture.ParameterPrefix, fixture.AWSOptions);
            var configurations = configurationBuilder.Build();

            Assert.All(fixture.TestData, (pair) => {
                Assert.Equal(pair.Value, configurations[pair.Key]);
            });
        }

        [Fact]
        public void TestInvalidPrefix()
        {
            var configurationBuilder = new ConfigurationBuilder();
            Exception ex = Assert.Throws<ArgumentException>(() => configurationBuilder.AddSystemsManager(@"/aws/reference/secretsmanager/hello", fixture.AWSOptions));
            Assert.Equal("Secrets Manager paths are not supported", ex.Message);
        }
    }
}
