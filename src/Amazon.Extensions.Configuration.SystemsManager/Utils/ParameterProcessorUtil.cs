using System.Collections.Generic;
using System.Linq;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;

namespace Amazon.Extensions.Configuration.SystemsManager.Utils
{
    public static class ParameterProcessorUtil
    {
        public static void ParseJsonParameter(Parameter parameter, string keyPrefix, IDictionary<string, string> result)
        {
            foreach (var kv in JsonConfigurationParser.Parse(parameter.Value))
            {
                var key = !string.IsNullOrEmpty(keyPrefix) ? ConfigurationPath.Combine(keyPrefix, kv.Key) : kv.Key;
                if (result.ContainsKey(key))
                {
                    throw new DuplicateParameterException($"Duplicate parameter '{key}' found. Parameter keys are case-insensitive.");
                }

                result.Add(key, kv.Value);
            }
        }

        public static void ParseStringListParameter(Parameter parameter, string keyPrefix, IDictionary<string, string> result)
        {
            // Items in a StringList must be separated by a comma (,).
            // You can't use other punctuation or special characters to escape items in the list.
            // If you have a parameter value that requires a comma, then use the String type.
            // https://docs.aws.amazon.com/systems-manager/latest/userguide/param-create-cli.html#param-create-cli-stringlist
            var configKeyValuePairs = parameter.Value
                .Split(',')
                .Select((value, idx) => new KeyValuePair<string, string>($"{keyPrefix}{ConfigurationPath.KeyDelimiter}{idx}", value));

            foreach (var kv in configKeyValuePairs)
            {
                if (result.ContainsKey(kv.Key))
                {
                    throw new DuplicateParameterException($"Duplicate parameter '{kv.Key}' found. Parameter keys are case-insensitive.");
                }

                result.Add(kv.Key, kv.Value);
            }
        }

        public static void ParseStringParameter(Parameter parameter, string key, IDictionary<string, string> result)
        {
            if (result.ContainsKey(key))
            {
                throw new DuplicateParameterException($"Duplicate parameter '{key}' found. Parameter keys are case-insensitive.");
            }

            result.Add(key, parameter.Value);
        }
    }
}