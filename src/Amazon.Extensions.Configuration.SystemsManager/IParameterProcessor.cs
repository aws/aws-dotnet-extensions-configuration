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

using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Amazon.Extensions.Configuration.SystemsManager
{
    /// <summary>
    /// Processor responsible for deciding if a <see cref="Parameter"/> should be included and processing the Key
    /// </summary>
    public interface IParameterProcessor
    {
        /// <summary>
        /// Decides if a <see cref="Parameter"/> should be included for processing
        /// </summary>
        /// <param name="parameter"><see cref="Parameter"/> to be processed</param>
        /// <param name="path">Path used when retrieving the <see cref="Parameter"/></param>
        /// <returns>Boolean that determines if the Parameter wil be processed further</returns>
        bool IncludeParameter(Parameter parameter, string path);

        /// <summary>
        /// Converts a parameter to configuration pairs to provide for <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="parameter"><see cref="Parameter"/> to be processed</param>
        /// <param name="path">Path used when retrieving the <see cref="Parameter"/></param>
        /// <returns>The Configuration Pair</returns>
        IEnumerable<KeyValuePair<string,string>> Process(Parameter input, string path);
    }
}
