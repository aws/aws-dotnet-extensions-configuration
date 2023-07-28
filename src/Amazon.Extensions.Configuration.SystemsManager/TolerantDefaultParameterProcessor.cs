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
using Amazon.SimpleSystemsManagement.Model;

namespace Amazon.Extensions.Configuration.SystemsManager
{
    /// <inheritdoc />
    /// <summary>
    /// This implementation will ignore any dublicate parameters that may located in SSM with different CASE (like \Path1\Param1 and \path1\param1 , only first one will be taken).
    /// This is based on Default parameter processor which is based on Systems Manager's suggested naming convention
    /// </summary>
    public class TolerantDefaultParameterProcessor : DefaultParameterProcessor
    {
        public override IDictionary<string, string> ProcessParameters(IEnumerable<Parameter> parameters, string path)
        {
            return parameters
                .Where(parameter => IncludeParameter(parameter, path))
                .Select(parameter => new
                {
                    Key = GetKey(parameter, path),
                    Value = GetValue(parameter, path)
                })                
                .GroupBy(parameter => parameter.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First().Value, StringComparer.OrdinalIgnoreCase);
        }
    }
}