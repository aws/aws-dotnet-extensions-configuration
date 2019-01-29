using System.Collections.Generic;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Xunit;

namespace Amazon.Extensions.Configuration.SystemsManager.Tests
{
    public class DictionaryExtensionsTests
    {
        [Theory]
        [MemberData(nameof(DictionaryEqualsData))]
        public void TestDictionaryEquals(IDictionary<string, string> first, IDictionary<string, string> second, bool equals)
        {
            Assert.Equal(equals, first.DictionaryEqual(second));
        }

        public static TheoryData<IDictionary<string, string>, IDictionary<string, string>, bool> DictionaryEqualsData => new TheoryData<IDictionary<string, string>, IDictionary<string, string>, bool>
        {
            {new Dictionary<string, string>(), new Dictionary<string, string>(), true},
            {new Dictionary<string, string>(), null, false},
            {new Dictionary<string, string>(), new Dictionary<string, string> {{"a", "a"}}, false},
            {new Dictionary<string, string> {{"a", "a"}}, new Dictionary<string, string> {{"a", "a"}}, true},
            {new Dictionary<string, string> {{"a", "a"}}, new Dictionary<string, string> {{"a", "a"}, {"b", "b"}}, false},
            {new Dictionary<string, string> {{"a", "a"}}, new Dictionary<string, string> {{"b", "b"}}, false},
            {new Dictionary<string, string> {{"a", "a"}}, new Dictionary<string, string> {{"a", "b"}}, false},
            {new Dictionary<string, string> {{"a", "a"}}, new Dictionary<string, string> {{"b", "a"}}, false},
            {new Dictionary<string, string> {{"a", "a"},{"b", "b"}}, new Dictionary<string, string> {{"b", "b"},{"a", "a"}}, true}
        };
    }
}
