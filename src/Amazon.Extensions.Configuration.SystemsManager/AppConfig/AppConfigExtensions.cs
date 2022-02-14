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
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Amazon.Extensions.NETCore.Setup;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Extension methods for registering <see cref="SystemsManagerConfigurationProvider"/> with <see cref="IConfigurationBuilder"/>.
    /// </summary>
    public static class AppConfigExtensions
    {
        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager AppConfig.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="applicationId">The AppConfig application id.</param>
        /// <param name="environmentId">The AppConfig environment id.</param>
        /// <param name="configProfileId">The AppConfig configuration profile id.</param>
        /// <param name="awsOptions"><see cref="AWSOptions"/> used to create an AWS Systems Manager AppConfig Client connection</param>
        /// <param name="optional">Whether the AWS Systems Manager AppConfig is optional.</param>
        /// <param name="reloadAfter">Initiate reload after TimeSpan</param>
        /// <exception cref="ArgumentNullException"><see cref="applicationId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="environmentId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="configProfileId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="awsOptions"/> cannot be null</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddAppConfig(this IConfigurationBuilder builder, string applicationId, string environmentId, string configProfileId, AWSOptions awsOptions, bool optional, TimeSpan? reloadAfter)
        {
            if (applicationId == null) throw new ArgumentNullException(nameof(applicationId));
            if (environmentId == null) throw new ArgumentNullException(nameof(environmentId));
            if (configProfileId == null) throw new ArgumentNullException(nameof(configProfileId));
            if (awsOptions == null) throw new ArgumentNullException(nameof(awsOptions));

            return builder.AddAppConfig(ConfigureSource(applicationId, environmentId, configProfileId, awsOptions, optional, reloadAfter));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager AppConfig.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="applicationId">The AppConfig application id.</param>
        /// <param name="environmentId">The AppConfig environment id.</param>
        /// <param name="configProfileId">The AppConfig configuration profile id.</param>
        /// <param name="awsOptions"><see cref="AWSOptions"/> used to create an AWS Systems Manager AppConfig Client connection</param>
        /// <param name="optional">Whether the AWS Systems Manager AppConfig is optional.</param>
        /// <exception cref="ArgumentNullException"><see cref="applicationId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="environmentId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="configProfileId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="awsOptions"/> cannot be null</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddAppConfig(this IConfigurationBuilder builder, string applicationId, string environmentId, string configProfileId, AWSOptions awsOptions, bool optional)
        {
            if (applicationId == null) throw new ArgumentNullException(nameof(applicationId));
            if (environmentId == null) throw new ArgumentNullException(nameof(environmentId));
            if (configProfileId == null) throw new ArgumentNullException(nameof(configProfileId));
            if (awsOptions == null) throw new ArgumentNullException(nameof(awsOptions));

            return builder.AddAppConfig(ConfigureSource(applicationId, environmentId, configProfileId, awsOptions, optional));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager AppConfig.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="applicationId">The AppConfig application id.</param>
        /// <param name="environmentId">The AppConfig environment id.</param>
        /// <param name="configProfileId">The AppConfig configuration profile id.</param>
        /// <param name="awsOptions"><see cref="AWSOptions"/> used to create an AWS Systems Manager AppConfig Client connection</param>
        /// <param name="reloadAfter">Initiate reload after TimeSpan</param>
        /// <exception cref="ArgumentNullException"><see cref="applicationId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="environmentId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="configProfileId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="awsOptions"/> cannot be null</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddAppConfig(this IConfigurationBuilder builder, string applicationId, string environmentId, string configProfileId, AWSOptions awsOptions, TimeSpan? reloadAfter)
        {
            if (applicationId == null) throw new ArgumentNullException(nameof(applicationId));
            if (environmentId == null) throw new ArgumentNullException(nameof(environmentId));
            if (configProfileId == null) throw new ArgumentNullException(nameof(configProfileId));
            if (awsOptions == null) throw new ArgumentNullException(nameof(awsOptions));

            return builder.AddAppConfig(ConfigureSource(applicationId, environmentId, configProfileId, awsOptions, reloadAfter: reloadAfter));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager AppConfig.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="applicationId">The AppConfig application id.</param>
        /// <param name="environmentId">The AppConfig environment id.</param>
        /// <param name="configProfileId">The AppConfig configuration profile id.</param>
        /// <param name="awsOptions"><see cref="AWSOptions"/> used to create an AWS Systems Manager AppConfig Client connection</param>
        /// <exception cref="ArgumentNullException"><see cref="applicationId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="environmentId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="configProfileId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="awsOptions"/> cannot be null</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddAppConfig(this IConfigurationBuilder builder, string applicationId, string environmentId, string configProfileId, AWSOptions awsOptions)
        {
            if (applicationId == null) throw new ArgumentNullException(nameof(applicationId));
            if (environmentId == null) throw new ArgumentNullException(nameof(environmentId));
            if (configProfileId == null) throw new ArgumentNullException(nameof(configProfileId));
            if (awsOptions == null) throw new ArgumentNullException(nameof(awsOptions));

            return builder.AddAppConfig(ConfigureSource(applicationId, environmentId, configProfileId, awsOptions));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager AppConfig.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="applicationId">The AppConfig application id.</param>
        /// <param name="environmentId">The AppConfig environment id.</param>
        /// <param name="configProfileId">The AppConfig configuration profile id.</param>
        /// <param name="optional">Whether the AWS Systems Manager AppConfig is optional.</param>
        /// <param name="reloadAfter">Initiate reload after TimeSpan</param>
        /// <exception cref="ArgumentNullException"><see cref="applicationId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="environmentId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="configProfileId"/> cannot be null</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddAppConfig(this IConfigurationBuilder builder, string applicationId, string environmentId, string configProfileId, bool optional, TimeSpan? reloadAfter)
        {
            if (applicationId == null) throw new ArgumentNullException(nameof(applicationId));
            if (environmentId == null) throw new ArgumentNullException(nameof(environmentId));
            if (configProfileId == null) throw new ArgumentNullException(nameof(configProfileId));

            return builder.AddAppConfig(ConfigureSource(applicationId, environmentId, configProfileId, optional: optional, reloadAfter: reloadAfter));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager AppConfig.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="applicationId">The AppConfig application id.</param>
        /// <param name="environmentId">The AppConfig environment id.</param>
        /// <param name="configProfileId">The AppConfig configuration profile id.</param>
        /// <param name="reloadAfter">Initiate reload after TimeSpan</param>
        /// <exception cref="ArgumentNullException"><see cref="applicationId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="environmentId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="configProfileId"/> cannot be null</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddAppConfig(this IConfigurationBuilder builder, string applicationId, string environmentId, string configProfileId, TimeSpan? reloadAfter)
        {
            if (applicationId == null) throw new ArgumentNullException(nameof(applicationId));
            if (environmentId == null) throw new ArgumentNullException(nameof(environmentId));
            if (configProfileId == null) throw new ArgumentNullException(nameof(configProfileId));

            return builder.AddAppConfig(ConfigureSource(applicationId, environmentId, configProfileId, reloadAfter: reloadAfter));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager AppConfig.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="applicationId">The AppConfig application id.</param>
        /// <param name="environmentId">The AppConfig environment id.</param>
        /// <param name="configProfileId">The AppConfig configuration profile id.</param>
        /// <param name="optional">Whether the AWS Systems Manager AppConfig is optional.</param>
        /// <exception cref="ArgumentNullException"><see cref="applicationId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="environmentId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="configProfileId"/> cannot be null</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddAppConfig(this IConfigurationBuilder builder, string applicationId, string environmentId, string configProfileId, bool optional)
        {
            if (applicationId == null) throw new ArgumentNullException(nameof(applicationId));
            if (environmentId == null) throw new ArgumentNullException(nameof(environmentId));
            if (configProfileId == null) throw new ArgumentNullException(nameof(configProfileId));

            return builder.AddAppConfig(ConfigureSource(applicationId, environmentId, configProfileId, optional: optional));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager AppConfig.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="applicationId">The AppConfig application id.</param>
        /// <param name="environmentId">The AppConfig environment id.</param>
        /// <param name="configProfileId">The AppConfig configuration profile id.</param>
        /// <exception cref="ArgumentNullException"><see cref="applicationId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="environmentId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="configProfileId"/> cannot be null</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddAppConfig(this IConfigurationBuilder builder, string applicationId, string environmentId, string configProfileId)
        {
            if (applicationId == null) throw new ArgumentNullException(nameof(applicationId));
            if (environmentId == null) throw new ArgumentNullException(nameof(environmentId));
            if (configProfileId == null) throw new ArgumentNullException(nameof(configProfileId));

            return builder.AddAppConfig(ConfigureSource(applicationId, environmentId, configProfileId));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager AppConfig.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="source">Configuration source.</param>
        /// <exception cref="ArgumentNullException"><see cref="source"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="source"/>.<see cref="AppConfigConfigurationSource.ApplicationId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="source"/>.<see cref="AppConfigConfigurationSource.EnvironmentId"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="source"/>.<see cref="AppConfigConfigurationSource.ConfigProfileId"/> cannot be null</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddAppConfig(this IConfigurationBuilder builder, AppConfigConfigurationSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.ApplicationId == null) throw new ArgumentNullException(nameof(source.ApplicationId));
            if (source.EnvironmentId == null) throw new ArgumentNullException(nameof(source.EnvironmentId));
            if (source.ConfigProfileId == null) throw new ArgumentNullException(nameof(source.ConfigProfileId));

            if (source.AwsOptions == null)
            {
                source.AwsOptions = AwsOptionsProvider.GetAwsOptions(builder);
            }

            return builder.Add(source);
        }

        private static AppConfigConfigurationSource ConfigureSource(
            string applicationId,
            string environmentId,
            string configProfileId,
            AWSOptions awsOptions = null,
            bool optional = false,
            TimeSpan? reloadAfter = null
        )
        {
            return new AppConfigConfigurationSource
            {
                ApplicationId = applicationId,
                EnvironmentId = environmentId,
                ConfigProfileId = configProfileId,
                AwsOptions = awsOptions,
                Optional = optional,
                ReloadAfter = reloadAfter
            };
        }
    }
}
