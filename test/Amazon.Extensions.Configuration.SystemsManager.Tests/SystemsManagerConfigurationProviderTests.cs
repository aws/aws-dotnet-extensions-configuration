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

using System.Collections.Generic;
using Amazon.Extensions.Configuration.SystemsManager.AppConfig;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Amazon.Extensions.NETCore.Setup;
using Amazon.SimpleSystemsManagement.Model;
using Moq;
using Xunit;

namespace Amazon.Extensions.Configuration.SystemsManager.Tests
{
    public class SystemsManagerConfigurationProviderTests
    {
        private readonly Mock<ISystemsManagerProcessor> _systemsManagerProcessorMock = new Mock<ISystemsManagerProcessor>();

        [Fact]
        public void LoadForParameterStoreShouldReturnProperParameters()
        {
            var parameters = new List<Parameter>
            {
                new Parameter {Name = "/start/path/p1/p2-1", Value = "p1:p2-1"},
                new Parameter {Name = "/start/path/p1/p2-2", Value = "p1:p2-2"},
                new Parameter {Name = "/start/path/p1/p2/p3-1", Value = "p1:p2:p3-1"},
                new Parameter {Name = "/start/path/p1/p2/p3-2", Value = "p1:p2:p3-2"}
            };
            var parameterProcessorMock = new Mock<IParameterProcessor>();
            var provider = ConfigureParameterStoreConfigurationProvider(parameterProcessorMock, parameters);

            provider.Load();

            foreach (var parameter in parameters)
            {
                Assert.True(provider.TryGet(parameter.Value, out _));
            }

            parameterProcessorMock.VerifyAll();
        }

        [Fact]
        public void LoadForAppConfigShouldReturnProperValues()
        {
            var values = new Dictionary<string, string>
            {
                { "testKey", "testValue" },
                { "testKey2", "testValue2" },
                { "testKey3", "testValue3" },
                { "testKey4", "testValue4" },
                { "testKey5", "testValue5" }
            };
            var provider = ConfigureAppConfigConfigurationProvider(values);

            provider.Load();

            foreach (var parameter in values)
            {
                Assert.True(provider.TryGet(parameter.Key, out _));
            }
        }

        private SystemsManagerConfigurationProvider ConfigureParameterStoreConfigurationProvider(Mock<IParameterProcessor> parameterProcessorMock, IReadOnlyCollection<Parameter> parameters)
        {
            const string path = "/start/path";
            var source = new SystemsManagerConfigurationSource
            {
                ParameterProcessor = parameterProcessorMock.Object,
                AwsOptions = new AWSOptions(),
                Path = path
            };
            var provider = new SystemsManagerConfigurationProvider(source, _systemsManagerProcessorMock.Object);

            var getData = new DefaultParameterProcessor().ProcessParameters(parameters, path);

            _systemsManagerProcessorMock.Setup(p => p.GetDataAsync()).ReturnsAsync(() => getData);

            return provider;
        }

        private SystemsManagerConfigurationProvider ConfigureAppConfigConfigurationProvider(IDictionary<string, string> values)
        {
            var source = new AppConfigConfigurationSource
            {
                ApplicationId = "appId",
                EnvironmentId = "envId",
                ConfigProfileId = "profileId",
                AwsOptions = new AWSOptions()
            };

            _systemsManagerProcessorMock.Setup(p => p.GetDataAsync()).ReturnsAsync(() => values);
            
            var provider = new SystemsManagerConfigurationProvider(source, _systemsManagerProcessorMock.Object);

            return provider;
        }
    }
}
