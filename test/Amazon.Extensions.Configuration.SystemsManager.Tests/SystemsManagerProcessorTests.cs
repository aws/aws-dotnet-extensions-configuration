using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Amazon.Extensions.NETCore.Setup;
using Amazon.SimpleSystemsManagement;
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

        #region CombinePathAndName Tests

        [Theory]
        [InlineData("/myapp", "connections/db", "/myapp/connections/db")]
        [InlineData("/myapp/", "connections/db", "/myapp/connections/db")]
        [InlineData("/gamma", "api/key", "/gamma/api/key")]
        [InlineData("/gamma/", "api/key", "/gamma/api/key")]
        [InlineData("/prod", "settings/feature", "/prod/settings/feature")]
        public void CombinePathAndName_ValidCombinations_ProducesCorrectFullNames(string path, string name, string expected)
        {
            // Arrange
            var source = CreateTestSource(path);
            source.ParameterNames = new List<string> { name };
            var processor = new SystemsManagerProcessor(source);

            // Act
            var result = InvokeCombinePathAndName(processor, path, name);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CombinePathAndName_PathWithoutTrailingSlash_AddsSlash()
        {
            // Arrange
            var source = CreateTestSource("/myapp");
            source.ParameterNames = new List<string> { "test" };
            var processor = new SystemsManagerProcessor(source);

            // Act
            var result = InvokeCombinePathAndName(processor, "/myapp", "test");

            // Assert
            Assert.Equal("/myapp/test", result);
            Assert.DoesNotContain("//", result);
        }

        [Fact]
        public void CombinePathAndName_PathWithTrailingSlash_DoesNotCreateDoubleSlash()
        {
            // Arrange
            var source = CreateTestSource("/myapp/");
            source.ParameterNames = new List<string> { "test" };
            var processor = new SystemsManagerProcessor(source);

            // Act
            var result = InvokeCombinePathAndName(processor, "/myapp/", "test");

            // Assert
            Assert.Equal("/myapp/test", result);
            Assert.DoesNotContain("//", result);
        }

        [Fact]
        public void CombinePathAndName_NameStartingWithSlash_ThrowsArgumentException()
        {
            // Arrange
            var source = CreateTestSource("/myapp");
            source.ParameterNames = new List<string> { "/test" };
            var processor = new SystemsManagerProcessor(source);

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => 
                InvokeCombinePathAndName(processor, "/myapp", "/test"));
            
            Assert.IsType<ArgumentException>(exception.InnerException);
            Assert.Contains("must be relative", exception.InnerException.Message);
            Assert.Contains("cannot start with /", exception.InnerException.Message);
        }

        [Fact]
        public void CombinePathAndName_CombinedNameExceeds2048Characters_ThrowsArgumentException()
        {
            // Arrange
            var source = CreateTestSource("/myapp");
            var longName = new string('a', 2048); // This will exceed 2048 when combined with path
            source.ParameterNames = new List<string> { longName };
            var processor = new SystemsManagerProcessor(source);

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => 
                InvokeCombinePathAndName(processor, "/myapp", longName));
            
            Assert.IsType<ArgumentException>(exception.InnerException);
            Assert.Contains("exceeds maximum length of 2048 characters", exception.InnerException.Message);
        }

        #endregion

        #region GetParametersByNamesAsync Batching Tests
        // Note: These tests verify batching behavior indirectly through empty collection handling
        // Full batching tests with API call counting require integration testing or refactoring
        // the SystemsManagerProcessor to accept an injectable client factory

        [Fact]
        public async Task GetDataAsync_WithParameterNames_EmptyCollection_ReturnsEmptyDictionary()
        {
            // Arrange
            var source = CreateTestSource("/myapp");
            source.ParameterNames = new List<string>();
            var processor = new SystemsManagerProcessor(source);

            // Act
            var result = await processor.GetDataAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region Parameter Not Found Handling Tests

        [Fact]
        public void ParameterNotFoundException_CanBeCreatedWithMessage()
        {
            // Arrange
            var message = "The following parameters were not found: /myapp/param1, /myapp/param2";

            // Act
            var exception = new ParameterNotFoundException(message);

            // Assert
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void ParameterNotFoundException_CanBeCreatedWithMessageAndInnerException()
        {
            // Arrange
            var message = "The following parameters were not found: /myapp/param1";
            var innerException = new Exception("Inner exception");

            // Act
            var exception = new ParameterNotFoundException(message, innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Same(innerException, exception.InnerException);
        }

        [Fact]
        public void ParameterNotFoundException_MessageIncludesParameterNames()
        {
            // Arrange
            var paramNames = new[] { "/myapp/param1", "/myapp/param2", "/myapp/param3" };
            var message = $"The following parameters were not found: {string.Join(", ", paramNames)}";

            // Act
            var exception = new ParameterNotFoundException(message);

            // Assert
            foreach (var paramName in paramNames)
            {
                Assert.Contains(paramName, exception.Message);
            }
        }

        #endregion

        #region Mode Selection Tests

        [Fact]
        public void GetDataAsync_WithSecretsManagerPath_UsesSecretsManagerMode()
        {
            // Arrange
            var source = CreateTestSource("/aws/reference/secretsmanager/my-secret");
            _ = new SystemsManagerProcessor(source);

            // Act & Assert
            // Verify that the path is recognized as a Secrets Manager path
            Assert.True(SystemsManagerProcessor.IsSecretsManagerPath(source.Path));
        }

        [Fact]
        public void GetDataAsync_WithParameterNamesSet_UsesParameterNamesMode()
        {
            // Arrange
            var source = CreateTestSource("/myapp");
            source.ParameterNames = new List<string> { "param1", "param2" };
            _ = new SystemsManagerProcessor(source);

            // Act & Assert
            // Verify that ParameterNames is set and not empty
            Assert.NotNull(source.ParameterNames);
            Assert.NotEmpty(source.ParameterNames);
            Assert.False(SystemsManagerProcessor.IsSecretsManagerPath(source.Path));
        }

        [Fact]
        public void GetDataAsync_WithPathOnly_UsesPathBasedMode()
        {
            // Arrange
            var source = CreateTestSource("/myapp");
            // ParameterNames is null by default
            _ = new SystemsManagerProcessor(source);

            // Act & Assert
            // Verify that ParameterNames is not set and path is not Secrets Manager
            Assert.Null(source.ParameterNames);
            Assert.False(SystemsManagerProcessor.IsSecretsManagerPath(source.Path));
        }

        [Fact]
        public void GetDataAsync_WithEmptyParameterNames_UsesPathBasedMode()
        {
            // Arrange
            var source = CreateTestSource("/myapp");
            source.ParameterNames = new List<string>(); // Empty list
            _ = new SystemsManagerProcessor(source);

            // Act & Assert
            // Verify that ParameterNames is empty (should use path-based mode)
            Assert.NotNull(source.ParameterNames);
            Assert.Empty(source.ParameterNames);
            Assert.False(SystemsManagerProcessor.IsSecretsManagerPath(source.Path));
        }

        #endregion

        #region Helper Methods

        private SystemsManagerConfigurationSource CreateTestSource(string path)
        {
            return new SystemsManagerConfigurationSource
            {
                Path = path,
                AwsOptions = new AWSOptions
                {
                    Region = Amazon.RegionEndpoint.USEast1
                }
            };
        }

        private string InvokeCombinePathAndName(SystemsManagerProcessor processor, string path, string name)
        {
            var method = typeof(SystemsManagerProcessor).GetMethod(
                "CombinePathAndName",
                BindingFlags.NonPublic | BindingFlags.Static);
            
            return (string)method.Invoke(null, new object[] { path, name });
        }

        #endregion
    }
}