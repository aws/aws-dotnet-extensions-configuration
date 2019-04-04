﻿/*
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
            return parameter.Name.Substring(path.Length).TrimStart('/').Replace("/", KeyDelimiter);
        }

        public virtual string GetValue(Parameter parameter, string path) => parameter.Value;
    }
}