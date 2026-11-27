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
using Amazon.Extensions.Configuration.SystemsManager.AppConfig;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Amazon.Extensions.Configuration.SystemsManager.Tests
{
    public class AppConfigExtensionsTests
    {
        [Fact]
        public void AddAppConfigWithProperInputShouldReturnProperConfigurationBuilder()
        {
            var expectedBuilder = new ConfigurationBuilder();
            expectedBuilder.Sources.Add(new Mock<AppConfigConfigurationSource>().Object);
            const string applicationId = "appId";
            const string environmentId = "envId";
            const string configProfileId = "profId";

            var builder = new ConfigurationBuilder();
            builder.AddAppConfig(applicationId, environmentId, configProfileId);

            Assert.Contains(
                builder.Sources,
                source => source is AppConfigConfigurationSource configurationSource
                       && configurationSource.ApplicationId == applicationId
                       && configurationSource.EnvironmentId == environmentId
                       && configurationSource.ConfigProfileId == configProfileId
            );
        }

        [Fact]
        public void AddAppConfigWithoutAwsOptionsShouldThrowException()
        {
            var builder = new ConfigurationBuilder();
            Func<IConfigurationBuilder> func = () => builder.AddAppConfig("appId", "envId", "profileId", awsOptions: null);

            var ex = Assert.Throws<ArgumentNullException>(func);
            Assert.Contains("awsOptions", ex.Message);
        }

        [Fact]
        public void AddAppConfigWithoutApplicationIdShouldThrowException()
        {
            var builder = new ConfigurationBuilder();
            Func<IConfigurationBuilder> func = () => builder.AddAppConfig(null, "envId", "profileId");

            var ex = Assert.Throws<ArgumentNullException>(func);
            Assert.Contains("applicationId", ex.Message);
        }

        [Fact]
        public void AddAppConfigWithoutEnvironmentIdShouldThrowException()
        {
            var builder = new ConfigurationBuilder();
            Func<IConfigurationBuilder> func = () => builder.AddAppConfig("appId", null, "profileId");

            var ex = Assert.Throws<ArgumentNullException>(func);
            Assert.Contains("environmentId", ex.Message);
        }

        [Fact]
        public void AddAppConfigWithoutProfileIdShouldThrowException()
        {
            var builder = new ConfigurationBuilder();
            Func<IConfigurationBuilder> func = () => builder.AddAppConfig("appId", "envId", null);

            var ex = Assert.Throws<ArgumentNullException>(func);
            Assert.Contains("configProfileId", ex.Message);
        }

        [Fact]
        public void AddAppConfigWithSectionShouldSetSectionOnSource()
        {
            var builder = new ConfigurationBuilder();
            builder.AddAppConfig("appId", "envId", "profileId", "MyProfile");

            Assert.Contains(
                builder.Sources,
                source => source is AppConfigConfigurationSource configurationSource
                       && configurationSource.Section == "MyProfile"
            );
        }

        [Fact]
        public void AddAppConfigWithAwsOptionsAndSectionShouldSetSectionOnSource()
        {
            var builder = new ConfigurationBuilder();
            builder.AddAppConfig("appId", "envId", "profileId", new Amazon.Extensions.NETCore.Setup.AWSOptions(), "MyProfile");

            Assert.Contains(
                builder.Sources,
                source => source is AppConfigConfigurationSource configurationSource
                       && configurationSource.Section == "MyProfile"
            );
        }

        [Fact]
        public void AddAppConfigWithSourceWithSectionShouldKeepSection()
        {
            var builder = new ConfigurationBuilder();
            builder.AddAppConfig(new AppConfigConfigurationSource
            {
                ApplicationId = "appId",
                EnvironmentId = "envId",
                ConfigProfileId = "profileId",
                Section = "MyProfile"
            });

            Assert.Contains(
                builder.Sources,
                source => source is AppConfigConfigurationSource configurationSource
                       && configurationSource.Section == "MyProfile"
            );
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(":MyProfile")]
        [InlineData("MyProfile:")]
        public void AddAppConfigWithInvalidSectionShouldThrowException(string section)
        {
            var builder = new ConfigurationBuilder();
            Func<IConfigurationBuilder> func = () => builder.AddAppConfig("appId", "envId", "profileId", section);

            var ex = Assert.Throws<ArgumentException>(func);
            Assert.Contains("section", ex.Message);
        }

        [Fact]
        public void AddAppConfigWithSourceWithInvalidSectionShouldThrowException()
        {
            var builder = new ConfigurationBuilder();
            Func<IConfigurationBuilder> func = () => builder.AddAppConfig(new AppConfigConfigurationSource
            {
                ApplicationId = "appId",
                EnvironmentId = "envId",
                ConfigProfileId = "profileId",
                Section = "MyProfile:"
            });

            var ex = Assert.Throws<ArgumentException>(func);
            Assert.Contains("section", ex.Message);
        }
    }
}
