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
using Microsoft.Extensions.Configuration;

namespace Amazon.Extensions.Configuration.SystemsManager
{
    /// <inheritdoc />
    /// <summary>
    /// Represents AWS Systems Manager variables as an <see cref="ISystemsManagerConfigurationSource" />.
    /// </summary>
    public interface ISystemsManagerConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// Determines if loading configuration data from AWS Systems Manager Parameter Store is optional.
        /// </summary>
        bool Optional { get; set; }

        /// <summary>
        /// Parameters will be reloaded from the AWS Systems Manager Parameter Store after the specified time frame
        /// </summary>
        TimeSpan? ReloadAfter { get; set; }

        /// <summary>
        /// Will be called if an uncaught exception occurs in <see cref="SystemsManagerConfigurationProvider"/>.Load.
        /// </summary>
        Action<SystemsManagerExceptionContext> OnLoadException { get; set; }
    }
}
