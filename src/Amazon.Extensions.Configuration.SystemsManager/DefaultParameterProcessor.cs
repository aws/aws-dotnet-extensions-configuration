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
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;

namespace Amazon.Extensions.Configuration.SystemsManager
{
    /// <inheritdoc />
    /// <summary>
    /// Default parameter processor based on Systems Manager's suggested naming convention
    /// </summary>
    public class DefaultParameterProcessor : IParameterProcessor
    {
        /// <inheritdoc cref="ConfigurationPath.KeyDelimiter"/>
        public static readonly string KeyDelimiter = ConfigurationPath.KeyDelimiter;

        public virtual bool IncludeParameter(Parameter parameter, string path) => true;

        public virtual string GetKey(Parameter parameter, string path)
        {
            var name = parameter.Name.StartsWith(path, StringComparison.OrdinalIgnoreCase) 
                ? parameter.Name.Substring(path.Length) 
                : parameter.Name;
#if NETCOREAPP3_1_OR_GREATER
            return name.TrimStart('/').Replace("/", KeyDelimiter, StringComparison.InvariantCulture);
#else
            return name.TrimStart('/').Replace("/", KeyDelimiter);
#endif
        }

        public virtual string GetValue(Parameter parameter, string path) => parameter.Value;

        private IEnumerable<KeyValuePair<string, string>> ParseStringList(Parameter parameter, string path)
        {
            // Items in a StringList must be separated by a comma (,).
            // You can't use other punctuation or special characters to escape items in the list.
            // If you have a parameter value that requires a comma, then use the String type.
            // https://docs.aws.amazon.com/systems-manager/latest/userguide/param-create-cli.html#param-create-cli-stringlist
            return parameter.Value.Split(',').Select((value, idx) =>
                new KeyValuePair<string, string>($"{GetKey(parameter, path)}:{idx}", value));
        }

        public virtual IDictionary<string, string> ProcessParameters(IEnumerable<Parameter> parameters, string path)
        {
            var result = new List<KeyValuePair<string, string>>();
            foreach (var parameter in parameters.Where(parameter => IncludeParameter(parameter, path)))
            {
                if (parameter.Type == ParameterType.StringList)
                {
                    var parameterList = ParseStringList(parameter, path);
                    
                    // Check for duplicate parameter keys.
                    var stringListKeys = parameterList.Select(p => p.Key);
                    var duplicateKeys = result.Where(r => stringListKeys.Contains(r.Key, StringComparer.OrdinalIgnoreCase)).Select(r => r.Key);
                    if (duplicateKeys.Count() > 0)
                    {
                        throw new DuplicateParameterException($"Duplicate parameters '{string.Join(";", duplicateKeys)}' found. Parameter keys are case-insensitive.");
                    }
                    
                    result.AddRange(parameterList);
                }
                else
                {
                    string parameterKey = GetKey(parameter, path);

                    // Check for duplicate parameter key.
                    if (result.Any(r => string.Equals(r.Key, parameterKey, StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new DuplicateParameterException($"Duplicate parameter '{parameterKey}' found. Parameter keys are case-insensitive.");
                    }

                    result.Add(new KeyValuePair<string, string>(parameterKey, GetValue(parameter, path)));
                }
            }

            return result.ToDictionary(parameter => parameter.Key, parameter => parameter.Value,
                StringComparer.OrdinalIgnoreCase);
        }
    }
}