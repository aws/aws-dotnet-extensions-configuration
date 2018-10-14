using System;
using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AWSSDK.Extensions.Configuration.SystemsManagerTests
{
    public class SystemsManagerExtensionsTests
    {
        [Theory, MemberData(nameof(SourceExtensionData))]
        public void AddSystemsManagerExtensionWithSourceTest(AWSOptions awsOptions, string path, bool optional, TimeSpan? reloadAfter, Action<SystemsManagerExceptionContext> onLoadException, Type exceptionType, string exceptionMessage)
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
                Assert.Equal(exceptionMessage, ex.Message.Replace("\r","", StringComparison.InvariantCultureIgnoreCase));
            }
            else
            {
                var result = ExecuteBuilder();
                Assert.Equal(builder, result);
            }
        }

        [Theory, MemberData(nameof(WithAWSOptionsExtensionData))]
        public void AddSystemsManagerWithAWSOptionsTest(AWSOptions awsOptions, string path, bool optional, TimeSpan? reloadAfter, Action<SystemsManagerExceptionContext> onLoadException, Type exceptionType, string exceptionMessage)
        {
            var builder = new ConfigurationBuilder();

            IConfigurationBuilder ExecuteBuilder() => builder.AddSystemsManager(awsOptions, path, optional, reloadAfter, onLoadException);

            if (exceptionType != null)
            {
                var ex = Assert.Throws(exceptionType, ExecuteBuilder);
                Assert.Equal(exceptionMessage, ex.Message.Replace("\r","", StringComparison.InvariantCultureIgnoreCase));
            }
            else
            {
                var result = ExecuteBuilder();
                Assert.Equal(builder, result);
            }
        }

        [Theory, MemberData(nameof(NoAWSOptionsExtensionData))]
        public void AddSystemsManagerWithNoAWSOptionsTest(string path, bool optional, TimeSpan? reloadAfter, Action<SystemsManagerExceptionContext> onLoadException, Type exceptionType, string exceptionMessage)
        {
            var builder = new ConfigurationBuilder();

            IConfigurationBuilder ExecuteBuilder() => builder.AddSystemsManager(path, optional, reloadAfter, onLoadException);

            if (exceptionType != null)
            {
                var ex = Assert.Throws(exceptionType, ExecuteBuilder);
                Assert.Equal(exceptionMessage, ex.Message.Replace("\r","", StringComparison.InvariantCultureIgnoreCase));
            }
            else
            {
                var result = ExecuteBuilder();
                Assert.Equal(builder, result);
            }
        }

        public static TheoryData<AWSOptions, string, bool, TimeSpan?, Action<SystemsManagerExceptionContext>, Type, string> SourceExtensionData =>
            new TheoryData<AWSOptions, string, bool, TimeSpan?, Action<SystemsManagerExceptionContext>, Type, string>
            {
                {null, null, false, null, null, typeof(ArgumentNullException), "Value cannot be null.\nParameter name: Path"},
                {null, null, true, null, null, typeof(ArgumentNullException), "Value cannot be null.\nParameter name: Path"},
                {null, "/path", false, null, null, null, null},
            };

        public static TheoryData<AWSOptions, string, bool, TimeSpan?, Action<SystemsManagerExceptionContext>, Type, string> WithAWSOptionsExtensionData =>
            new TheoryData<AWSOptions, string, bool, TimeSpan?, Action<SystemsManagerExceptionContext>, Type, string>
            {
                {null, null, false, null, null, typeof(ArgumentNullException), "Value cannot be null.\nParameter name: awsOptions"},
                {null, "/path", false, null, null, typeof(ArgumentNullException), "Value cannot be null.\nParameter name: awsOptions"},
                {new AWSOptions(), null, false, null, null, typeof(ArgumentNullException), "Value cannot be null.\nParameter name: Path"},
                {new AWSOptions(), "/path", false, null, null, null, null},
            };

        public static TheoryData<string, bool, TimeSpan?, Action<SystemsManagerExceptionContext>, Type, string> NoAWSOptionsExtensionData =>
            new TheoryData<string, bool, TimeSpan?, Action<SystemsManagerExceptionContext>, Type, string>
            {
                {null, false, null, null, typeof(ArgumentNullException), "Value cannot be null.\nParameter name: Path"},
                {"/path", false, null, null, null, null},
            };
    }
}
