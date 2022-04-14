using System.Collections.Generic;
using System.Linq;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Amazon.SimpleSystemsManagement.Model;
using Moq;
using Xunit;

namespace Amazon.Extensions.Configuration.SystemsManager.Tests
{
    public class SystemsManagerProcessorTests
    {
        [Theory]
        [InlineData("/aws/reference/secretsmanager/", true)]
        [InlineData("/not-sm-path/", false)]
        public void IsSecretsManagerPathTest(string path, bool expected)
        {
            Assert.Equal(expected, SystemsManagerProcessor.IsSecretsManagerPath(path));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("prefix")]
        public void AddPrefixTest(string prefix)
        {
            var data = new Dictionary<string, string> { { "Key", "Value" } };
            var output = SystemsManagerProcessor.AddPrefix(data, prefix);

            if (prefix == null)
            {
                Assert.Equal(data, output);
            }
            else
            {
                foreach (var item in output)
                {
                    Assert.StartsWith($"{prefix}:", item.Key);
                }
            }
        }
    }
}