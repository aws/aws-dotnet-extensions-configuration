using System.Collections.Generic;
using System.Linq;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Amazon.Extensions.Configuration.SystemsManager.Utils
{
    public static class ParameterProcessorUtil
    {
        /// <summary>
        /// Parses the SSM parameter as JSON
        /// </summary>
        /// <param name="keyPrefix">prefix to add in configution key</param>
        /// <param name="value">SSM parameter value</param>
        /// <param name="result">append the parsed JSON value into</param>
        /// <exception cref="DuplicateParameterException">SSM parameter key is already present in <paramref name="result"/></exception>
        /// <exception cref="JsonException"><paramref name="value" /> does not represent a valid single JSON value.</exception>
        public static void ParseJsonParameter(string keyPrefix, string value, IDictionary<string, string> result)
        {
            foreach (var kv in JsonConfigurationParser.Parse(value))
            {
                var key = !string.IsNullOrEmpty(keyPrefix) ? ConfigurationPath.Combine(keyPrefix, kv.Key) : kv.Key;
                if (result.ContainsKey(key))
                {
                    throw new DuplicateParameterException($"Duplicate parameter '{key}' found. Parameter keys are case-insensitive.");
                }

                result.Add(key, kv.Value);
            }
        }

        /// <summary>
        /// Parses the StringList SSM parameter as List of String
        /// <br/><br/>
        /// Items in a StringList must be separated by a comma (,).
        /// You can't use other punctuation or special characters to escape items in the list.
        /// If you have a parameter value that requires a comma, then use the String type.
        /// https://docs.aws.amazon.com/systems-manager/latest/userguide/param-create-cli.html#param-create-cli-stringlist
        /// </summary>
        /// <param name="keyPrefix">prefix to add in configution key</param>
        /// <param name="value">SSM parameter</param>
        /// <param name="result">append the parsed string list value into</param>
        /// <exception cref="DuplicateParameterException">SSM parameter key is already present in <paramref name="result"/></exception>
        public static void ParseStringListParameter(string keyPrefix, string value, IDictionary<string, string> result)
        {
            var configKeyValuePairs = value
                .Split(',')
                .Select((eachValue, idx) => new KeyValuePair<string, string>($"{keyPrefix}{ConfigurationPath.KeyDelimiter}{idx}", eachValue));

            foreach (var kv in configKeyValuePairs)
            {
                if (result.ContainsKey(kv.Key))
                {
                    throw new DuplicateParameterException($"Duplicate parameter '{kv.Key}' found. Parameter keys are case-insensitive.");
                }

                result.Add(kv.Key, kv.Value);
            }
        }

        /// <summary>
        /// Parses the SSM parameter as String
        /// </summary>
        /// <param name="key">key to be used for configution key</param>
        /// <param name="value">SSM parameter</param>
        /// <param name="result">append the parsed string value into</param>
        /// <exception cref="DuplicateParameterException">SSM parameter key is already present in <paramref name="result"/></exception>
        public static void ParseStringParameter(string key, string value, IDictionary<string, string> result)
        {
            if (result.ContainsKey(key))
            {
                throw new DuplicateParameterException($"Duplicate parameter '{key}' found. Parameter keys are case-insensitive.");
            }

            result.Add(key, value);
        }
    }
}