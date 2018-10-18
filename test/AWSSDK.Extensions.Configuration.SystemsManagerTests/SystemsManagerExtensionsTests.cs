using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.Configuration;
using System;
using Xunit;

namespace AWSSDK.Extensions.Configuration.SystemsManagerTests
{
    public class SystemsManagerExtensionsTests
    {
        [Theory, MemberData(nameof(SourceExtensionData))]
        public void AddSystemsManagerExtensionWithSourceTest(string path, AWSOptions awsOptions, bool optional, TimeSpan? reloadAfter, Action<SystemsManagerExceptionContext> onLoadException, Type exceptionType, string exceptionMessage)
        {
            var builder = new ConfigurationBuilder();

            IConfigurationBuilder ExecuteBuilder() =>
                builder.AddSystemsManager(source =>
                {
                    source.AwsOptions = awsOptions;
                    source.Path = path;
                    source.Optional = optional;
                    source.ReloadAfter = reloadAfter;
                    source.OnLoadException = onLoadException;
                });

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

        [Theory]
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

        public static TheoryData<string, AWSOptions, bool, TimeSpan?, Action<SystemsManagerExceptionContext>, Type, string> SourceExtensionData =>
            new TheoryData<string, AWSOptions, bool, TimeSpan?, Action<SystemsManagerExceptionContext>, Type, string>
            {
                {null, null, false, null, null, typeof(ArgumentNullException), "Parameter name: Path"},
                {null, null, true, null, null, typeof(ArgumentNullException), "Parameter name: Path"},
                {"/path", null, false, null, null, null, null},
                {"/aws/reference/secretsmanager/somevalue", null, false, null, null, typeof(ArgumentException), "Secrets Manager paths are not supported"}
            };

        public static TheoryData<Func<IConfigurationBuilder, IConfigurationBuilder>, Type, string> WithAWSOptionsExtensionData => new TheoryData<Func<IConfigurationBuilder, IConfigurationBuilder>, Type, string>
        {
            {builder => builder.AddSystemsManager(null, null), typeof(ArgumentNullException), "Parameter name: path"},
            {builder => builder.AddSystemsManager("/path", null), typeof(ArgumentNullException), "Parameter name: awsOptions"},
            {builder => builder.AddSystemsManager(null, new AWSOptions()), typeof(ArgumentNullException), "Parameter name: path"},
            {builder => builder.AddSystemsManager("/aws/reference/secretsmanager/somevalue", new AWSOptions()), typeof(ArgumentException), "Secrets Manager paths are not supported"},
            {builder => builder.AddSystemsManager("/path", new AWSOptions(), true), null, null},
            {builder => builder.AddSystemsManager("/path", new AWSOptions(), false), null, null},
            {builder => builder.AddSystemsManager("/path", new AWSOptions(), TimeSpan.Zero), null, null},
            {builder => builder.AddSystemsManager("/path", new AWSOptions(), TimeSpan.Zero), null, null},
            {builder => builder.AddSystemsManager("/path", new AWSOptions(), true, TimeSpan.Zero), null, null},
            {builder => builder.AddSystemsManager("/path", new AWSOptions(), false, TimeSpan.Zero), null, null}
        };

        public static TheoryData<Func<IConfigurationBuilder, IConfigurationBuilder>, Type, string> NoAWSOptionsExtensionData => new TheoryData<Func<IConfigurationBuilder, IConfigurationBuilder>, Type, string>
        {
            {builder => builder.AddSystemsManager(null as string), typeof(ArgumentNullException), "Parameter name: path"},
            {builder => builder.AddSystemsManager("/path"), null, null},
            {builder => builder.AddSystemsManager("/aws/reference/secretsmanager/somevalue"), typeof(ArgumentException), "Secrets Manager paths are not supported"},
            {builder => builder.AddSystemsManager("/path", true), null, null},
            {builder => builder.AddSystemsManager("/path", false), null, null},
            {builder => builder.AddSystemsManager("/path", TimeSpan.Zero), null, null},
            {builder => builder.AddSystemsManager("/path", TimeSpan.Zero), null, null},
            {builder => builder.AddSystemsManager("/path", true, TimeSpan.Zero), null, null},
            {builder => builder.AddSystemsManager("/path", false, TimeSpan.Zero), null, null}
        };
    }
}
