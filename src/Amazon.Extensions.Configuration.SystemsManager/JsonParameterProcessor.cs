/*
 * Copyright 2018 Amazon.com, Inc. or its affiliates. All Rights Reserved.
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
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;

namespace Amazon.Extensions.Configuration.SystemsManager
{
    /// <inheritdoc />
    /// <summary>
    /// Default parameter processor based on Systems Manager's suggested naming convention
    /// </summary>
    public class JsonParameterProcessor : DefaultParameterProcessor
    {
        public override IDictionary<string, string> ProcessParameters(IEnumerable<Parameter> parameters, string path)
        {
            var outputDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (Parameter parameter in parameters.Where(parameter => IncludeParameter(parameter, path)))
            {
                // Get the extra prefix if the path is subset of paramater name.
                string prefix = GetKey(parameter, path);

                var parameterDictionary = JsonConfigurationParser.Parse(parameter.Value);
                foreach (var keyValue in parameterDictionary)
                {
                    string key = (!string.IsNullOrEmpty(prefix) ? ConfigurationPath.Combine(prefix, keyValue.Key) : keyValue.Key);
                    outputDictionary.Add(key, keyValue.Value);
                }
            }

            return outputDictionary;
        }
    }
}