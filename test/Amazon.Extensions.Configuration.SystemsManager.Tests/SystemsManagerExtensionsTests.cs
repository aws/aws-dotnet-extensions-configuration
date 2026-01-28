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
using System.Linq;
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
            {builder => builder.AddSystemsManager(path: null, awsOptions: null), typeof(ArgumentNullException), "path"},
            {builder => builder.AddSystemsManager(path: "/path", awsOptions: null), typeof(ArgumentNullException), "awsOptions"},
            {builder => builder.AddSystemsManager(path: null, awsOptions: new AWSOptions()), typeof(ArgumentNullException), "path"},
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
            var parameterNames = new System.Collections.Generic.List<string> { "param1", "param2", "Param1", "param2", "PARAM1" };

            var result = builder.AddSystemsManager("/path", parameterNames);

            Assert.Equal(builder, result);
            
            // Verify that the configuration source has the correct number of unique parameter names
            var source = builder.Sources[builder.Sources.Count - 1] as SystemsManagerConfigurationSource;
            Assert.NotNull(source);
            Assert.NotNull(source.ParameterNames);
            Assert.Equal(2, source.ParameterNames.Count()); // Should only have "param1" and "param2" (case-insensitive)
            
            // Verify the actual names are preserved (first occurrence wins)
            Assert.Contains("param1", source.ParameterNames);
            Assert.Contains("param2", source.ParameterNames);
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

        [Fact]
        public void AddSystemsManager_WithParameterNames_NullParameterName_ThrowsArgumentException()
        {
            var builder = new ConfigurationBuilder();
            var parameterNames = new System.Collections.Generic.List<string> { "param1", null, "param2" };

            var ex = Assert.Throws<ArgumentException>(() => 
                builder.AddSystemsManager("/path", parameterNames, new AWSOptions(), false, TimeSpan.Zero));
            Assert.Contains("Parameter name cannot be null, empty, or whitespace", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void AddSystemsManager_WithParameterNames_EmptyParameterName_ThrowsArgumentException()
        {
            var builder = new ConfigurationBuilder();
            var parameterNames = new System.Collections.Generic.List<string> { "param1", "", "param2" };

            var ex = Assert.Throws<ArgumentException>(() => 
                builder.AddSystemsManager("/path", parameterNames, new AWSOptions(), false, TimeSpan.Zero));
            Assert.Contains("Parameter name cannot be null, empty, or whitespace", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void AddSystemsManager_WithParameterNames_OnlyWhitespaceNames_ThrowsArgumentException()
        {
            var builder = new ConfigurationBuilder();
            var parameterNames = new System.Collections.Generic.List<string> { "   ", "\t", "\n" };

            var ex = Assert.Throws<ArgumentException>(() => 
                builder.AddSystemsManager("/path", parameterNames, new AWSOptions(), false, TimeSpan.Zero));
            Assert.Contains("Parameter name cannot be null, empty, or whitespace", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void AddSystemsManager_WithParameterNames_SetsPathAndParameterNamesCorrectly()
        {
            var builder = new ConfigurationBuilder();
            var parameterNames = new System.Collections.Generic.List<string> { "param1", "param2", "param3" };
            var path = "/myapp/config";

            builder.AddSystemsManager(path, parameterNames);

            var source = builder.Sources[builder.Sources.Count - 1] as SystemsManagerConfigurationSource;
            Assert.NotNull(source);
            Assert.Equal(path, source.Path);
            Assert.NotNull(source.ParameterNames);
            Assert.Equal(3, source.ParameterNames.Count());
            Assert.Contains("param1", source.ParameterNames);
            Assert.Contains("param2", source.ParameterNames);
            Assert.Contains("param3", source.ParameterNames);
        }

        [Fact]
        public void AddSystemsManager_WithParameterNames_SetsOptionalCorrectly()
        {
            var builder = new ConfigurationBuilder();
            var parameterNames = new System.Collections.Generic.List<string> { "param1" };

            // Test Optional = true
            builder.AddSystemsManager("/path", parameterNames, new AWSOptions(), true);
            var source1 = builder.Sources[builder.Sources.Count - 1] as SystemsManagerConfigurationSource;
            Assert.NotNull(source1);
            Assert.True(source1.Optional);

            // Test Optional = false
            builder.AddSystemsManager("/path", parameterNames, new AWSOptions(), false);
            var source2 = builder.Sources[builder.Sources.Count - 1] as SystemsManagerConfigurationSource;
            Assert.NotNull(source2);
            Assert.False(source2.Optional);
        }

        [Fact]
        public void AddSystemsManager_WithParameterNames_SetsReloadAfterCorrectly()
        {
            var builder = new ConfigurationBuilder();
            var parameterNames = new System.Collections.Generic.List<string> { "param1" };
            var reloadAfter = TimeSpan.FromMinutes(10);

            builder.AddSystemsManager("/path", parameterNames, new AWSOptions(), false, reloadAfter);

            var source = builder.Sources[builder.Sources.Count - 1] as SystemsManagerConfigurationSource;
            Assert.NotNull(source);
            Assert.Equal(reloadAfter, source.ReloadAfter);
        }

        [Fact]
        public void AddSystemsManager_WithParameterNames_SetsAwsOptionsCorrectly()
        {
            var builder = new ConfigurationBuilder();
            var parameterNames = new System.Collections.Generic.List<string> { "param1" };
            var awsOptions = new AWSOptions { Region = Amazon.RegionEndpoint.USWest2 };

            builder.AddSystemsManager("/path", parameterNames, awsOptions);

            var source = builder.Sources[builder.Sources.Count - 1] as SystemsManagerConfigurationSource;
            Assert.NotNull(source);
            Assert.Equal(awsOptions, source.AwsOptions);
            Assert.Equal(Amazon.RegionEndpoint.USWest2, source.AwsOptions.Region);
        }

        [Fact]
        public void AddSystemsManager_WithParameterNames_AllPropertiesSetCorrectly()
        {
            var builder = new ConfigurationBuilder();
            var parameterNames = new System.Collections.Generic.List<string> { "param1", "param2" };
            var path = "/myapp";
            var awsOptions = new AWSOptions { Region = Amazon.RegionEndpoint.EUWest1 };
            var optional = true;
            var reloadAfter = TimeSpan.FromMinutes(15);

            builder.AddSystemsManager(path, parameterNames, awsOptions, optional, reloadAfter);

            var source = builder.Sources[builder.Sources.Count - 1] as SystemsManagerConfigurationSource;
            Assert.NotNull(source);
            Assert.Equal(path, source.Path);
            Assert.Equal(2, source.ParameterNames.Count());
            Assert.Equal(awsOptions, source.AwsOptions);
            Assert.Equal(optional, source.Optional);
            Assert.Equal(reloadAfter, source.ReloadAfter);
        }

        [Fact]
        public void AddSystemsManager_WithParameterNames_ConvenienceOverload_WithoutAwsOptions_UsesDefaultAwsOptions()
        {
            var builder = new ConfigurationBuilder();
            var parameterNames = new System.Collections.Generic.List<string> { "param1" };

            builder.AddSystemsManager("/path", parameterNames);

            var source = builder.Sources[builder.Sources.Count - 1] as SystemsManagerConfigurationSource;
            Assert.NotNull(source);
            Assert.NotNull(source.AwsOptions); // Should have default AWS options
            Assert.Equal("/path", source.Path);
            Assert.Single(source.ParameterNames);
        }

        [Fact]
        public void AddSystemsManager_WithParameterNames_SecretsManagerPath_ThrowsArgumentException()
        {
            var builder = new ConfigurationBuilder();
            var parameterNames = new System.Collections.Generic.List<string> { "param1", "param2" };

            var ex = Assert.Throws<ArgumentException>(() => 
                builder.AddSystemsManager("/aws/reference/secretsmanager/mysecret", parameterNames, new AWSOptions(), false, TimeSpan.Zero));
            Assert.Contains("Cannot use Secrets Manager path", ex.Message, StringComparison.Ordinal);
            Assert.Contains("path", ex.ParamName, StringComparison.Ordinal);
        }

        [Fact]
        public void AddSystemsManager_WithParameterNames_SecretsManagerPathCaseInsensitive_ThrowsArgumentException()
        {
            var builder = new ConfigurationBuilder();
            var parameterNames = new System.Collections.Generic.List<string> { "param1" };

            // Test various case combinations
            var ex1 = Assert.Throws<ArgumentException>(() => 
                builder.AddSystemsManager("/AWS/REFERENCE/SECRETSMANAGER/mysecret", parameterNames));
            Assert.Contains("Cannot use Secrets Manager path", ex1.Message, StringComparison.Ordinal);

            var ex2 = Assert.Throws<ArgumentException>(() => 
                builder.AddSystemsManager("/Aws/Reference/SecretsManager/mysecret", parameterNames));
            Assert.Contains("Cannot use Secrets Manager path", ex2.Message, StringComparison.Ordinal);

            var ex3 = Assert.Throws<ArgumentException>(() => 
                builder.AddSystemsManager("/aws/reference/SECRETSMANAGER/mysecret", parameterNames));
            Assert.Contains("Cannot use Secrets Manager path", ex3.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void AddSystemsManager_WithParameterNames_NonSecretsManagerPath_Succeeds()
        {
            var builder = new ConfigurationBuilder();
            var parameterNames = new System.Collections.Generic.List<string> { "param1", "param2" };

            // These should all succeed
            var result1 = builder.AddSystemsManager("/myapp/config", parameterNames);
            Assert.NotNull(result1);

            var result2 = builder.AddSystemsManager("/aws/myapp", parameterNames);
            Assert.NotNull(result2);

            var result3 = builder.AddSystemsManager("/reference/myapp", parameterNames);
            Assert.NotNull(result3);

            // Path that contains "secretsmanager" but doesn't start with the full Secrets Manager path
            var result4 = builder.AddSystemsManager("/myapp/secretsmanager/config", parameterNames);
            Assert.NotNull(result4);
        }
    }
}
