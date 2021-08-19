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

using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.Configuration;

namespace Amazon.Extensions.Configuration.SystemsManager.Internal
{
    public static class AwsOptionsProvider
    {
        private const string AwsOptionsConfigurationKey = "AWS_CONFIGBUILDER_AWSOPTIONS";
        
        public static AWSOptions GetAwsOptions(IConfigurationBuilder builder)
        {
            if (builder.Properties.TryGetValue(AwsOptionsConfigurationKey, out var value) && value is AWSOptions existingOptions)
            {
                return existingOptions;
            }

            var config = builder.Build();
            var newOptions = config.GetAWSOptions();

            if (builder.Properties.ContainsKey(AwsOptionsConfigurationKey))
            {
                builder.Properties[AwsOptionsConfigurationKey] = newOptions;
            }
            else
            {
                builder.Properties.Add(AwsOptionsConfigurationKey, newOptions);
            }

            return newOptions;
        }
    }
}
