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

using System.Collections.Generic;
using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Amazon.Extensions.NETCore.Setup;
using Amazon.SimpleSystemsManagement.Model;
using Moq;
using Xunit;

namespace AWSSDK.Extensions.Configuration.SystemsManager.Tests
{
    public class SystemsManagerConfigurationProviderTests
    {
        private readonly List<Parameter> _parameters = new List<Parameter>
        {
            new Parameter {Name = "/start/path/p1/p2-1", Value = "p1:p2-1"},
            new Parameter {Name = "/start/path/p1/p2-2", Value = "p1:p2-2"},
            new Parameter {Name = "/start/path/p1/p2/p3-1", Value = "p1:p2:p3-1"},
            new Parameter {Name = "/start/path/p1/p2/p3-2", Value = "p1:p2:p3-2"}
        };

        private const string Path = "/start/path";

        private readonly SystemsManagerConfigurationProvider _provider;
        private readonly Mock<ISystemsManagerProcessor> _systemsManagerProcessorMock;
        private readonly Mock<IParameterProcessor> _parameterProcessorMock;
        private readonly SystemsManagerConfigurationSource _source;

        public SystemsManagerConfigurationProviderTests()
        {
            _parameterProcessorMock = new Mock<IParameterProcessor>();
            _systemsManagerProcessorMock = new Mock<ISystemsManagerProcessor>();
            _source = new SystemsManagerConfigurationSource
            {
                ParameterProcessor = _parameterProcessorMock.Object,
                AwsOptions = new AWSOptions(), 
                Path = Path
            };
            _provider = new SystemsManagerConfigurationProvider(_source, _systemsManagerProcessorMock.Object);
        }

        [Fact]
        public void ProcessParametersTest()
        {
            foreach (var parameter in _parameters)
            {
                _parameterProcessorMock.Setup(processor => processor.IncludeParameter(parameter, Path)).Returns(true);
                _parameterProcessorMock.Setup(processor => processor.GetKey(parameter, Path)).Returns(parameter.Value);
                _parameterProcessorMock.Setup(processor => processor.GetValue(parameter, Path)).Returns(parameter.Value);
            }

            var data = _provider.ProcessParameters(_parameters, Path);
            
            Assert.All(data, item => Assert.Equal(item.Value, item.Key));

            _parameterProcessorMock.VerifyAll();
        }

        [Fact]
        public void LoadTest()
        {
            _systemsManagerProcessorMock.Setup(p => p.GetParametersByPathAsync(_source.AwsOptions, _source.Path)).ReturnsAsync(_parameters);
            foreach (var parameter in _parameters)
            {
                _parameterProcessorMock.Setup(processor => processor.IncludeParameter(parameter, Path)).Returns(true);
                _parameterProcessorMock.Setup(processor => processor.GetKey(parameter, Path)).Returns(parameter.Value);
            }
            
            _provider.Load();

            foreach (var parameter in _parameters)
            {
                Assert.True(_provider.TryGet(parameter.Value, out _));
            }

            _parameterProcessorMock.VerifyAll();
        }
    }
}
