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

using Amazon.Extensions.Configuration.SystemsManager.AppConfig;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Amazon.Extensions.Configuration.SystemsManager.Tests
{
    public class AppConfigConfigurationSourceTests
    {
        [Fact]
        public void BuildShouldReturnSystemsManagerConfigurationProvider()
        {
            var source = new AppConfigConfigurationSource
            {
                ApplicationId = "appId",
                EnvironmentId = "envId",
                ConfigProfileId = "profileId",
                AwsOptions = new AWSOptions()
            };
            var builder = new ConfigurationBuilder();

            var result = source.Build(builder);

            Assert.IsType<SystemsManagerConfigurationProvider>(result);
        }
    }
}
