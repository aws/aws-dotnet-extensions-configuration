using System;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Xunit;

namespace Amazon.Extensions.Configuration.SystemsManager.Tests
{
    public class CombinePathAndNameTests
    {
        [Theory]
        [InlineData("/gamma", "connections/db", "/gamma/connections/db")]
        [InlineData("/gamma/", "connections/db", "/gamma/connections/db")]
        [InlineData("/prod", "api/key", "/prod/api/key")]
        [InlineData("/prod/", "api/key", "/prod/api/key")]
        public void CombinePathAndName_ValidInputs_ReturnsCorrectFullName(string path, string name, string expected)
        {
            // Use reflection to call the private static method
            var method = typeof(SystemsManagerProcessor).GetMethod("CombinePathAndName", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            var result = (string)method.Invoke(null, new object[] { path, name });
            
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CombinePathAndName_NameStartsWithSlash_ThrowsArgumentException()
        {
            var method = typeof(SystemsManagerProcessor).GetMethod("CombinePathAndName", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            var exception = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
                method.Invoke(null, new object[] { "/gamma", "/connections/db" }));
            
            Assert.IsType<ArgumentException>(exception.InnerException);
            Assert.Contains("must be relative", exception.InnerException.Message);
        }

        [Fact]
        public void CombinePathAndName_ExceedsMaxLength_ThrowsArgumentException()
        {
            var method = typeof(SystemsManagerProcessor).GetMethod("CombinePathAndName", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            var longName = new string('a', 2048);
            
            var exception = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
                method.Invoke(null, new object[] { "/gamma", longName }));
            
            Assert.IsType<ArgumentException>(exception.InnerException);
            Assert.Contains("exceeds maximum length", exception.InnerException.Message);
        }
    }
}
