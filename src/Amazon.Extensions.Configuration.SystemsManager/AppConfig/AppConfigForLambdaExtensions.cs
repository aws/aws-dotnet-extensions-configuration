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
using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.Extensions.Configuration.SystemsManager.AppConfig;
using Amazon.Runtime;
using Amazon.AppConfigData;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Extension methods for registering <see cref="SystemsManagerConfigurationProvider"/> with <see cref="IConfigurationBuilder"/>.
    /// </summary>
    public static class AppConfigForLambdaExtensions
    {
        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager AWS AppConfig using the AWS Lambda Extension.
        /// For more information about using the AppConfig Lambda Extension checkout the <a href="https://docs.aws.amazon.com/appconfig/latest/userguide/appconfig-integration-lambda-extensions.html">AppConfig user guide</a>.
        /// </summary>
        /// <remarks>
        /// The AppConfig Lambda extension reloads configuration data using the interval set by the AWS_APPCONFIG_EXTENSION_POLL_INTERVAL_SECONDS environment variable
        /// or 45 seconds if not set. The .NET configuration provider will refresh at the same interval plus a 5 second buffer for the extension to complete its update
        /// process.
        /// </remarks>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="applicationId">The AppConfig application id.</param>
        /// <param name="environmentId">The AppConfig environment id.</param>
        /// <param name="configProfileId">The AppConfig configuration profile id.</param>
        /// <param name="optional">Whether the AWS Systems Manager AppConfig is optional.</param>
        /// <exception cref="ArgumentNullException"><see cref="applicationId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="environmentId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="configProfileId"/> cannot be null</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddAppConfigUsingLambdaExtension(this IConfigurationBuilder builder, string applicationId, string environmentId, string configProfileId, bool optional)
        {
            if (applicationId == null) throw new ArgumentNullException(nameof(applicationId));
            if (environmentId == null) throw new ArgumentNullException(nameof(environmentId));
            if (configProfileId == null) throw new ArgumentNullException(nameof(configProfileId));

            return builder.AddAppConfigUsingLambdaExtension(ConfigureSource(applicationId, environmentId, configProfileId, optional: optional));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager AWS AppConfig using the AWS Lambda Extension.
        /// For more information about using the AppConfig Lambda Extension checkout the <a href="https://docs.aws.amazon.com/appconfig/latest/userguide/appconfig-integration-lambda-extensions.html">AppConfig user guide</a>.
        /// </summary>
        /// <remarks>
        /// The AppConfig Lambda extension reloads configuration data using the interval set by the AWS_APPCONFIG_EXTENSION_POLL_INTERVAL_SECONDS environment variable
        /// or 45 seconds if not set. The .NET configuration provider will refresh at the same interval plus a 5 second buffer for the extension to complete its update
        /// process.
        /// </remarks>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="applicationId">The AppConfig application id.</param>
        /// <param name="environmentId">The AppConfig environment id.</param>
        /// <param name="configProfileId">The AppConfig configuration profile id.</param>
        /// <exception cref="ArgumentNullException"><see cref="applicationId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="environmentId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="configProfileId"/> cannot be null</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddAppConfigUsingLambdaExtension(this IConfigurationBuilder builder, string applicationId, string environmentId, string configProfileId)
        {
            if (applicationId == null) throw new ArgumentNullException(nameof(applicationId));
            if (environmentId == null) throw new ArgumentNullException(nameof(environmentId));
            if (configProfileId == null) throw new ArgumentNullException(nameof(configProfileId));

            return builder.AddAppConfigUsingLambdaExtension(ConfigureSource(applicationId, environmentId, configProfileId));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager AWS AppConfig using the AWS Lambda Extension.
        /// For more information about using the AppConfig Lambda Extension checkout the <a href="https://docs.aws.amazon.com/appconfig/latest/userguide/appconfig-integration-lambda-extensions.html">AppConfig user guide</a>.
        /// </summary>
        /// <remarks>
        /// The AppConfig Lambda extension reloads configuration data using the interval set by the AWS_APPCONFIG_EXTENSION_POLL_INTERVAL_SECONDS environment variable
        /// or 45 seconds if not set. The .NET configuration provider will refresh at the same interval plus a 5 second buffer for the extension to complete its update
        /// process.
        /// </remarks>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="source">Configuration source.</param>
        /// <exception cref="ArgumentNullException"><see cref="source"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="source"/>.<see cref="AppConfigConfigurationSource.ApplicationId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="source"/>.<see cref="AppConfigConfigurationSource.EnvironmentId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="source"/>.<see cref="AppConfigConfigurationSource.ConfigProfileId"/> cannot be null</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddAppConfigUsingLambdaExtension(this IConfigurationBuilder builder, AppConfigConfigurationSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.ApplicationId == null) throw new ArgumentNullException(nameof(source.ApplicationId));
            if (source.EnvironmentId == null) throw new ArgumentNullException(nameof(source.EnvironmentId));
            if (source.ConfigProfileId == null) throw new ArgumentNullException(nameof(source.ConfigProfileId));

            // Create a specific instance of AmazonAppConfigClient that is configured to make calls to the endpoint setup by the AppConfig Lambda layer.
            source.UseLambdaExtension = true;

            if(!source.ReloadAfter.HasValue)
            {
                // default polling duration is 45 seconds https://docs.aws.amazon.com/appconfig/latest/userguide/appconfig-integration-lambda-extensions.html
                var reloadAfter = 45;

                // Since the user is using Lambda extension which automatically refreshes the data default to the configuration provider defaulting
                // to reload at the rate the extension reloads plus 5 second buffer.
                var reloadAfterStr = Environment.GetEnvironmentVariable("AWS_APPCONFIG_EXTENSION_POLL_INTERVAL_SECONDS");
                if (reloadAfterStr != null && !int.TryParse(reloadAfterStr, out reloadAfter))
                {
                    throw new ArgumentException("Environment variable AWS_APPCONFIG_EXTENSION_POLL_INTERVAL_SECONDS used for computing ReloadAfter is not set to a valid integer");
                }

                reloadAfter += 5;
                source.ReloadAfter = TimeSpan.FromSeconds(reloadAfter);
            }

            return builder.Add(source);
        }

        private static AppConfigConfigurationSource ConfigureSource(
            string applicationId,
            string environmentId,
            string configProfileId,
            bool optional = false
        )
        {
            return new AppConfigConfigurationSource
            {
                ApplicationId = applicationId,
                EnvironmentId = environmentId,
                ConfigProfileId = configProfileId,
                Optional = optional
            };
        }
    }
}
