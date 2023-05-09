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
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Amazon.Extensions.NETCore.Setup;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Extension methods for registering <see cref="SystemsManagerConfigurationProvider"/> with <see cref="IConfigurationBuilder"/>.
    /// </summary>
    public static class SystemsManagerExtensions
    {
        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager Parameter Store with a specified path.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="awsOptions"><see cref="AWSOptions"/> used to create an AWS Systems Manager Client connection</param>
        /// <param name="path">The path that variable names must start with. The path will be removed from the variable names.</param>
        /// <param name="optional">Whether the AWS Systems Manager Parameters are optional.</param>
        /// <param name="reloadAfter">Initiate reload after TimeSpan</param>
        /// <exception cref="ArgumentNullException"><see cref="path"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="awsOptions"/> cannot be null</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSystemsManager(this IConfigurationBuilder builder, string path, AWSOptions awsOptions, bool optional, TimeSpan reloadAfter)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (awsOptions == null) throw new ArgumentNullException(nameof(awsOptions));

            return builder.AddSystemsManager(ConfigureSource(path, awsOptions, optional, reloadAfter));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager Parameter Store with a specified path.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="awsOptions"><see cref="AWSOptions"/> used to create an AWS Systems Manager Client connection</param>
        /// <param name="path">The path that variable names must start with. The path will be removed from the variable names.</param>
        /// <param name="optional">Whether the AWS Systems Manager Parameters are optional.</param>
        /// <exception cref="ArgumentNullException"><see cref="path"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="awsOptions"/> cannot be null</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSystemsManager(this IConfigurationBuilder builder, string path, AWSOptions awsOptions, bool optional)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (awsOptions == null) throw new ArgumentNullException(nameof(awsOptions));

            return builder.AddSystemsManager(ConfigureSource(path, awsOptions, optional));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager Parameter Store with a specified path.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="awsOptions"><see cref="AWSOptions"/> used to create an AWS Systems Manager Client connection</param>
        /// <param name="path">The path that variable names must start with. The path will be removed from the variable names.</param>
        /// <param name="reloadAfter">Initiate reload after TimeSpan</param>
        /// <exception cref="ArgumentNullException"><see cref="path"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="awsOptions"/> cannot be null</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSystemsManager(this IConfigurationBuilder builder, string path, AWSOptions awsOptions, TimeSpan reloadAfter)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (awsOptions == null) throw new ArgumentNullException(nameof(awsOptions));

            return builder.AddSystemsManager(ConfigureSource(path, awsOptions, reloadAfter: reloadAfter));
        }
        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager Parameter Store with a specified path.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="awsOptions"><see cref="AWSOptions"/> used to create an AWS Systems Manager Client connection</param>
        /// <param name="path">The path that variable names must start with. The path will be removed from the variable names.</param>
        /// <exception cref="ArgumentNullException"><see cref="path"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="awsOptions"/> cannot be null</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSystemsManager(this IConfigurationBuilder builder, string path, AWSOptions awsOptions)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (awsOptions == null) throw new ArgumentNullException(nameof(awsOptions));

            return builder.AddSystemsManager(ConfigureSource(path, awsOptions));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager Parameter Store with a specified path.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">The path that variable names must start with. The path will be removed from the variable names.</param>
        /// <param name="optional">Whether the AWS Systems Manager Parameters are optional.</param>
        /// <param name="reloadAfter">Initiate reload after TimeSpan</param>
        /// <exception cref="ArgumentNullException"><see cref="path"/> cannot be null</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSystemsManager(this IConfigurationBuilder builder, string path, bool optional, TimeSpan reloadAfter)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            return builder.AddSystemsManager(ConfigureSource(path, null, optional, reloadAfter));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager Parameter Store with a specified path.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">The path that variable names must start with. The path will be removed from the variable names.</param>
        /// <param name="optional">Whether the AWS Systems Manager Parameters are optional.</param>
        /// <exception cref="ArgumentNullException"><see cref="path"/> cannot be null</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSystemsManager(this IConfigurationBuilder builder, string path, bool optional)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            return builder.AddSystemsManager(ConfigureSource(path, null, optional));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager Parameter Store with a specified path.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">The path that variable names must start with. The path will be removed from the variable names.</param>
        /// <param name="reloadAfter">Initiate reload after TimeSpan</param>
        /// <exception cref="ArgumentNullException"><see cref="path"/> cannot be null</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSystemsManager(this IConfigurationBuilder builder, string path, TimeSpan reloadAfter)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            return builder.AddSystemsManager(ConfigureSource(path, null, reloadAfter: reloadAfter));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager Parameter Store with a specified path.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">The path that variable names must start with. The path will be removed from the variable names.</param>
        /// <exception cref="ArgumentNullException"><see cref="path"/> cannot be null</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSystemsManager(this IConfigurationBuilder builder, string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            return builder.AddSystemsManager(ConfigureSource(path, null));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager Parameter variables with a specified path.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="configureSource">Configures the source.</param>
        /// <exception cref="ArgumentNullException"><see cref="configureSource"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="configureSource"/>.<see cref="SystemsManagerConfigurationSource.Path"/> cannot be null</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSystemsManager(this IConfigurationBuilder builder, Action<SystemsManagerConfigurationSource> configureSource)
        {
            if (configureSource == null) throw new ArgumentNullException(nameof(configureSource));

            var source = new SystemsManagerConfigurationSource();
            configureSource(source);

            if (string.IsNullOrWhiteSpace(source.Path)) throw new ArgumentNullException(nameof(source.Path));
            if (source.AwsOptions != null) return builder.Add(source);

            source.AwsOptions = AwsOptionsProvider.GetAwsOptions(builder);
            return builder.Add(source);
        }

        private static Action<SystemsManagerConfigurationSource> ConfigureSource(string path, AWSOptions awsOptions, bool optional = false, TimeSpan? reloadAfter = null)
        {
            return configurationSource => 
            {
                configurationSource.Path = path;
                configurationSource.AwsOptions = awsOptions;
                configurationSource.Optional = optional;
                configurationSource.ReloadAfter = reloadAfter;
            };
        }
    }
}