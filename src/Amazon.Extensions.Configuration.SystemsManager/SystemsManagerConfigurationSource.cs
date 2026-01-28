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
using Amazon.Extensions.NETCore.Setup;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;

namespace Amazon.Extensions.Configuration.SystemsManager
{
    /// <inheritdoc />
    /// <summary>
    /// Represents AWS Systems Manager Parameter Store variables as an <see cref="ISystemsManagerConfigurationSource" />.
    /// </summary>
    public class SystemsManagerConfigurationSource : ISystemsManagerConfigurationSource
    {
        public SystemsManagerConfigurationSource()
        {
            Filters = new List<ParameterStringFilter>();
        }

        /// <summary>
        /// A Path used to filter parameters.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// <see cref="AWSOptions"/> used to create an AWS Systems Manager Client />.
        /// </summary>
        public AWSOptions AwsOptions { get; set; }

        /// <inheritdoc />
        public bool Optional { get; set; }

        /// <inheritdoc />
        public TimeSpan? ReloadAfter { get; set; }

        /// <summary>
        /// Prepends the supplied Prefix to all result keys
        /// </summary>
        public string Prefix { get; set; }

        /// <inheritdoc />
        public Action<SystemsManagerExceptionContext> OnLoadException { get; set; }

        /// <summary>
        /// Implementation of <see cref="IParameterProcessor"/> used to process <see cref="Parameter"/> results. Defaults to <see cref="DefaultParameterProcessor"/>.
        /// </summary>
        public IParameterProcessor ParameterProcessor { get; set; }

        /// <summary> 
        /// Filters to limit the request results.
        /// You can't filter using the parameter name.
        /// </summary>
        public List<ParameterStringFilter> Filters { get; }

        /// <summary>
        /// A collection of relative parameter names to load from Parameter Store.
        /// When set, the configuration provider will use the GetParameters API to retrieve
        /// specific parameters by combining the <see cref="Path"/> prefix with each relative name.
        /// Parameter names must be relative paths (e.g., "connections/db") and will be combined
        /// with the <see cref="Path"/> to form full parameter names (e.g., "/gamma/connections/db").
        /// If not set, the provider will use GetParametersByPath to load all parameters under <see cref="Path"/>.
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read-only - This property needs to be settable from extension methods
        public List<string> ParameterNames { get; set; }
#pragma warning restore CA2227

        /// <inheritdoc />
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new SystemsManagerConfigurationProvider(this);
        }
    }
}
