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
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;

namespace Amazon.Extensions.Configuration.SystemsManager
{
    /// <summary>
    /// Processor responsible for deciding if a <see cref="Parameter"/> should be included and processing the Key
    /// </summary>
    public interface IParameterProcessor
    {
        /// <summary>
        /// Process parameters from AWS Parameter Store into a dictionary for <see cref="IConfiguration"/>
        /// </summary>
        /// <param name="parameters">Enumeration of <see cref="Parameter"/>s to be processed</param>
        /// <param name="path">Path used when retrieving the <see cref="Parameter"/></param>
        /// <returns>Configuration values for <see cref="IConfiguration"/></returns>
        IDictionary<string, string> ProcessParameters(IEnumerable<Parameter> parameters, string path);
    }
}
