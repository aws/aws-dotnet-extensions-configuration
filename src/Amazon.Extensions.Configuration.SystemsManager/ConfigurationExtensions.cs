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
    /// <summary>
    /// This extension is an a different namespace to avoid misuse of this method which should only be called when being used from AWS Lambda.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// This method blocks while any SystemsManagerConfigurationProvider added to IConfiguration are
        /// currently reloading the parameters from Parameter Store.
        /// 
        /// This is generally only needed when the provider is being called from a AWS Lambda function. Without this call
        /// in a AWS Lambda environment there is a potential of the background thread doing the refresh never running successfully.
        /// This can happen because the AWS Lambda compute environment is frozen after the current AWS Lambda event is complete.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="timeout">Maximum time to wait for reload to be completed</param>
        public static void WaitForSystemsManagerReloadToComplete(this IConfiguration configuration, TimeSpan timeout)
        {
            if (configuration is ConfigurationRoot configRoot)
            {
                foreach (var provider in configRoot.Providers)
                {
                    if (provider is SystemsManagerConfigurationProvider ssmProvider)
                    {
                        ssmProvider.WaitForReloadToComplete(timeout);
                    }
                }
            }
        }
    }
}