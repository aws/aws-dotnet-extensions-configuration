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
using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Amazon.Extensions.Configuration.SystemsManager.Tests
{
    public class SystemsManagerExtensionsTests
    {
        [Theory]
        [MemberData(nameof(SourceExtensionData))]
        [MemberData(nameof(WithAWSOptionsExtensionData))]
        [MemberData(nameof(NoAWSOptionsExtensionData))]
        public void AddSystemsManagerInlineTest(Func<IConfigurationBuilder, IConfigurationBuilder> configurationBuilder, Type exceptionType, string exceptionMessage)
        {
            var builder = new ConfigurationBuilder();

            IConfigurationBuilder ExecuteBuilder() => configurationBuilder(builder);

            if (exceptionType != null)
            {
                var ex = Assert.Throws(exceptionType, ExecuteBuilder);
                Assert.Contains(exceptionMessage, ex.Message, StringComparison.Ordinal);
            }
            else
            {
                var result = ExecuteBuilder();
                Assert.Equal(builder, result);
            }
        }

        public static TheoryData<Func<IConfigurationBuilder, IConfigurationBuilder>, Type, string> SourceExtensionData => new TheoryData<Func<IConfigurationBuilder, IConfigurationBuilder>, Type, string>
        {
            {builder => builder.AddSystemsManager(CreateSource(null, null, false, null, null)), typeof(ArgumentNullException), "Path"},
            {builder => builder.AddSystemsManager(CreateSource(null, null, true, null, null)), typeof(ArgumentNullException), "Path"},
            {builder => builder.AddSystemsManager(CreateSource("/path", null, false, null, null)), null, null},
            {builder => builder.AddSystemsManager(CreateSource("/aws/reference/secretsmanager/somevalue", null, false, null, null)), null, null}
        };

        public static TheoryData<Func<IConfigurationBuilder, IConfigurationBuilder>, Type, string> WithAWSOptionsExtensionData => new TheoryData<Func<IConfigurationBuilder, IConfigurationBuilder>, Type, string>
        {
            {builder => builder.AddSystemsManager(null, (AWSOptions)null), typeof(ArgumentNullException), "path"},
            {builder => builder.AddSystemsManager("/path", (AWSOptions)null), typeof(ArgumentNullException), "awsOptions"},
            {builder => builder.AddSystemsManager(null, new AWSOptions()), typeof(ArgumentNullException), "path"},
            {builder => builder.AddSystemsManager("/aws/reference/secretsmanager/somevalue", new AWSOptions()), null, null},
            {builder => builder.AddSystemsManager("/path", new AWSOptions(), true), null, null},
            {builder => builder.AddSystemsManager("/path", new AWSOptions(), false), null, null},
            {builder => builder.AddSystemsManager("/path", new AWSOptions(), TimeSpan.Zero), null, null},
            {builder => builder.AddSystemsManager("/path", new AWSOptions(), TimeSpan.Zero), null, null},
            {builder => builder.AddSystemsManager("/path", new AWSOptions(), true, TimeSpan.Zero), null, null},
            {builder => builder.AddSystemsManager("/path", new AWSOptions(), false, TimeSpan.Zero), null, null}
        };

        public static TheoryData<Func<IConfigurationBuilder, IConfigurationBuilder>, Type, string> NoAWSOptionsExtensionData => new TheoryData<Func<IConfigurationBuilder, IConfigurationBuilder>, Type, string>
        {
            {builder => builder.AddSystemsManager(null as string), typeof(ArgumentNullException), "path"},
            {builder => builder.AddSystemsManager("/path"), null, null},
            {builder => builder.AddSystemsManager("/aws/reference/secretsmanager/somevalue"), null, null},
            {builder => builder.AddSystemsManager("/path", true), null, null},
            {builder => builder.AddSystemsManager("/path", false), null, null},
            {builder => builder.AddSystemsManager("/path", TimeSpan.Zero), null, null},
            {builder => builder.AddSystemsManager("/path", TimeSpan.Zero), null, null},
            {builder => builder.AddSystemsManager("/path", true, TimeSpan.Zero), null, null},
            {builder => builder.AddSystemsManager("/path", false, TimeSpan.Zero), null, null}
        };

        private static Action<SystemsManagerConfigurationSource> CreateSource(string path, AWSOptions awsOptions, bool optional, TimeSpan? reloadAfter, Action<SystemsManagerExceptionContext> onLoadException)
        {
            return source =>
            {
                source.Path = path;
                source.AwsOptions = awsOptions;
                source.Optional = optional;
                source.ReloadAfter = reloadAfter;
                source.OnLoadException = onLoadException;
            };
        }

        [Fact]
        public void AddSystemsManager_WithParameterNames_NullPath_ThrowsArgumentNullException()
        {
            var builder = new ConfigurationBuilder();
            var parameterNames = new System.Collections.Generic.List<string> { "param1", "param2" };

            var ex = Assert.Throws<ArgumentNullException>(() => 
                builder.AddSystemsManager(null, parameterNames, new AWSOptions(), false, TimeSpan.Zero));
            Assert.Contains("path", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void AddSystemsManager_WithParameterNames_NullParameterNames_ThrowsArgumentNullException()
        {
            var builder = new ConfigurationBuilder();

            var ex = Assert.Throws<ArgumentNullException>(() => 
                builder.AddSystemsManager("/path", null, new AWSOptions(), false, TimeSpan.Zero));
            Assert.Contains("parameterNames", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void AddSystemsManager_WithParameterNames_EmptyParameterNames_ThrowsArgumentException()
        {
            var builder = new ConfigurationBuilder();
            var parameterNames = new System.Collections.Generic.List<string>();

            var ex = Assert.Throws<ArgumentException>(() => 
                builder.AddSystemsManager("/path", parameterNames, new AWSOptions(), false, TimeSpan.Zero));
            Assert.Contains("Parameter names collection cannot be empty", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void AddSystemsManager_WithParameterNames_ParameterNameStartsWithSlash_ThrowsArgumentException()
        {
            var builder = new ConfigurationBuilder();
            var parameterNames = new System.Collections.Generic.List<string> { "/param1", "param2" };

            var ex = Assert.Throws<ArgumentException>(() => 
                builder.AddSystemsManager("/path", parameterNames, new AWSOptions(), false, TimeSpan.Zero));
            Assert.Contains("must be relative", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void AddSystemsManager_WithParameterNames_WhitespaceParameterName_ThrowsArgumentException()
        {
            var builder = new ConfigurationBuilder();
            var parameterNames = new System.Collections.Generic.List<string> { "param1", "   ", "param2" };

            var ex = Assert.Throws<ArgumentException>(() => 
                builder.AddSystemsManager("/path", parameterNames, new AWSOptions(), false, TimeSpan.Zero));
            Assert.Contains("Parameter name cannot be null, empty, or whitespace", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void AddSystemsManager_WithParameterNames_ValidParameters_Succeeds()
        {
            var builder = new ConfigurationBuilder();
            var parameterNames = new System.Collections.Generic.List<string> { "param1", "param2" };

            var result = builder.AddSystemsManager("/path", parameterNames, new AWSOptions(), false, TimeSpan.Zero);

            Assert.Equal(builder, result);
        }

        [Fact]
        public void AddSystemsManager_WithParameterNames_RemovesDuplicates()
        {
            var builder = new ConfigurationBuilder();
            var parameterNames = new System.Collections.Generic.List<string> { "param1", "param2", "Param1", "param2" };

            var result = builder.AddSystemsManager("/path", parameterNames);

            Assert.Equal(builder, result);
            // The actual duplicate removal will be verified in the configuration source
        }

        [Fact]
        public void AddSystemsManager_WithParameterNames_ConvenienceOverloads_Succeed()
        {
            var builder = new ConfigurationBuilder();
            var parameterNames = new System.Collections.Generic.List<string> { "param1", "param2" };

            // Test all convenience overloads
            Assert.NotNull(builder.AddSystemsManager("/path", parameterNames));
            Assert.NotNull(builder.AddSystemsManager("/path", parameterNames, new AWSOptions()));
            Assert.NotNull(builder.AddSystemsManager("/path", parameterNames, new AWSOptions(), true));
            Assert.NotNull(builder.AddSystemsManager("/path", parameterNames, new AWSOptions(), TimeSpan.FromMinutes(5)));
            Assert.NotNull(builder.AddSystemsManager("/path", parameterNames, true));
            Assert.NotNull(builder.AddSystemsManager("/path", parameterNames, TimeSpan.FromMinutes(5)));
            Assert.NotNull(builder.AddSystemsManager("/path", parameterNames, true, TimeSpan.FromMinutes(5)));
        }
    }
}
