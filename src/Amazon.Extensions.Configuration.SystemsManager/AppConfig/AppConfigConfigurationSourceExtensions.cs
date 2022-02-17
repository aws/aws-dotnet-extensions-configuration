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

namespace Amazon.Extensions.Configuration.SystemsManager.AppConfig
{
    public static class AppConfigConfigurationSourceExtensions
    {
        private const string LastAppConfigVersionKeySeparator = "-";

        private static readonly string LastAppConfigVersionKeyTemplate = string.Join(LastAppConfigVersionKeySeparator,
            "AppConfigVersion", "{0}", "{1}", "{2}", "{3}", "{4}");

        public static string GetLastAppConfigVersionKey(this AppConfigConfigurationSource source)
        {
            return source.IncludeLatestConfigVersion ? GetFormattedLastAppConfigVersionKey(source) : string.Empty;
        }

        private static string GetFormattedLastAppConfigVersionKey(AppConfigConfigurationSource source)
        {
            return string.Format(LastAppConfigVersionKeyTemplate, source.AwsOptions.Region.SystemName,
                source.ApplicationId, source.EnvironmentId, source.ConfigProfileId, source.ClientId);
        }
    }
}