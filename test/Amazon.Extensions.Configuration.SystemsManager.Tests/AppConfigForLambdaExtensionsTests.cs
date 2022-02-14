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
using System.Reflection;
using System.Linq;
using Amazon.Extensions.Configuration.SystemsManager.AppConfig;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Amazon.Extensions.Configuration.SystemsManager.Tests
{
    public class AppConfigForLambdaExtensionsTests
    {
        [Fact]
        public void AddAppConfigForLambdaWithProperInputShouldReturnProperConfigurationBuilder()
        {
            var expectedBuilder = new ConfigurationBuilder();
            expectedBuilder.Sources.Add(new Mock<AppConfigConfigurationSource>().Object);
            const string applicationId = "appId";
            const string environmentId = "envId";
            const string configProfileId = "profId";

            var builder = new ConfigurationBuilder();
            builder.AddAppConfigUsingLambdaExtension(applicationId, environmentId, configProfileId);

            var configurationSource = builder.Sources.FirstOrDefault(source => source is AppConfigConfigurationSource) as AppConfigConfigurationSource;
            Assert.NotNull(configurationSource);
            Assert.Equal(applicationId, configurationSource.ApplicationId);
            Assert.Equal(environmentId, configurationSource.EnvironmentId);
            Assert.Equal(configProfileId, configurationSource.ConfigProfileId);

            var property = typeof(AppConfigConfigurationSource).GetProperty("UseLambdaExtension", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(property);
            Assert.True((bool)property.GetValue(configurationSource));
        }

        [Fact]
        public void AddAppConfigForLambdaWithoutApplicationIdShouldThrowException()
        {
            var builder = new ConfigurationBuilder();
            Func<IConfigurationBuilder> func = () => builder.AddAppConfigUsingLambdaExtension(null, "envId", "profileId");

            var ex = Assert.Throws<ArgumentNullException>(func);
            Assert.Contains("applicationId", ex.Message);
        }

        [Fact]
        public void AddAppConfigForLambdaWithoutEnvironmentIdShouldThrowException()
        {
            var builder = new ConfigurationBuilder();
            Func<IConfigurationBuilder> func = () => builder.AddAppConfigUsingLambdaExtension("appId", null, "profileId");

            var ex = Assert.Throws<ArgumentNullException>(func);
            Assert.Contains("environmentId", ex.Message);
        }

        [Fact]
        public void AddAppConfigForLambdaWithoutProfileIdShouldThrowException()
        {
            var builder = new ConfigurationBuilder();
            Func<IConfigurationBuilder> func = () => builder.AddAppConfigUsingLambdaExtension("appId", "envId", null);

            var ex = Assert.Throws<ArgumentNullException>(func);
            Assert.Contains("configProfileId", ex.Message);
        }
    }
}
