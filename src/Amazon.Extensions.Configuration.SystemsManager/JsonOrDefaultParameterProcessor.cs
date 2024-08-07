using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;

namespace Amazon.Extensions.Configuration.SystemsManager
{
    public class JsonOrDefaultParameterProcessor : DefaultParameterProcessor
    {
        /// <inheritdoc />
        /// <summary>
        /// Converts AWS SSM Parameters into a dictionary, handling JSON, strings, and StringLists.
        /// Ensures case-insensitive unique keys.
        /// </summary>
        public override IDictionary<string, string> ProcessParameters(IEnumerable<Parameter> parameters, string path)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var parameter in parameters.Where(parameter => IncludeParameter(parameter, path)))
            {
                var prefix = GetKey(parameter, path);

                if (parameter.Type == ParameterType.StringList)
                {
                    ParseStringListParameter(parameter, prefix, result);
                    continue;
                }

                if (!TryParseJsonParameter(parameter, prefix, result))
                {
                    ParseStringParameter(parameter, prefix, result);
                }
            }

            return result;
        }
        
        protected static void ParseStringListParameter(Parameter parameter, string keyPrefix, Dictionary<string, string> result)
        {
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

        protected static void ParseStringParameter(Parameter parameter, string key, Dictionary<string, string> result)
        {
            if (result.ContainsKey(key))
            {
                throw new DuplicateParameterException($"Duplicate parameter '{key}' found. Parameter keys are case-insensitive.");
            }

            result.Add(key, parameter.Value);
        }

        protected static bool TryParseJsonParameter(Parameter parameter, string keyPrefix, Dictionary<string, string> result)
        {
            try
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
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}