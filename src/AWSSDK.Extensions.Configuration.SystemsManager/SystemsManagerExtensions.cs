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

using System;
using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.Extensions.NETCore.Setup;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Extension methods for registering <see cref="SystemsManagerConfigurationProvider"/> with <see cref="IConfigurationBuilder"/>.
    /// </summary>
    public static class SystemsManagerExtensions
    {
        private const string SecretsManagerPath = "/aws/reference/secretsmanager/";
        private const string SecretsManagerExceptionMessage = "Secrets Manager paths are not supported";

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager Parameter Store with a specified path.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="awsOptions"><see cref="AWSOptions"/> used to create an AWS Systems Manager Client connection</param>
        /// <param name="path">The path that variable names must start with. The path will be removed from the variable names.</param>
        /// <param name="optional">Whether the AWS Systems Manager Parameters are optional.</param>
        /// <param name="reloadAfter">Initiate reload after TimeSpan</param>
        /// <param name="onLoadException">Delegate to call on Exception</param>
        /// <exception cref="ArgumentNullException"><see cref="path"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="awsOptions"/> cannot be null</exception>
        /// <exception cref="ArgumentException"><see cref="path"/> does not support Secrets Manager prefix (/aws/reference/secretsmanager/)</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSystemsManager(this IConfigurationBuilder builder, AWSOptions awsOptions, string path, bool optional = false, TimeSpan? reloadAfter = null, Action<SystemsManagerExceptionContext> onLoadException = null)
        {
            if (awsOptions == null) throw new ArgumentNullException(nameof(awsOptions));

            return builder.AddSystemsManager(source =>
            {
                source.AwsOptions = awsOptions;
                source.Path = path;
                source.Optional = optional;
                source.ReloadAfter = reloadAfter;
                source.OnLoadException = onLoadException;
            });
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager Parameter variables with a specified path.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">The path that variable names must start with. The path will be removed from the variable names.</param>
        /// <param name="optional">Whether the AWS Systems Manager Parameters are optional.</param>
        /// <param name="reloadAfter">Initiate reload after TimeSpan</param>
        /// <param name="onLoadException">Delegate to call on Exception</param>
        /// <exception cref="ArgumentNullException"><see cref="path"/> cannot be null</exception>
        /// <exception cref="ArgumentException"><see cref="path"/> does not support Secrets Manager prefix (/aws/reference/secretsmanager/)</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSystemsManager(this IConfigurationBuilder builder, string path, bool optional = false, TimeSpan? reloadAfter = null, Action<SystemsManagerExceptionContext> onLoadException = null)
        {
            var config = builder.Build();
            return builder.AddSystemsManager(config.GetAWSOptions(), path, optional, reloadAfter, onLoadException);
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager Parameter variables with a specified path.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="configureSource">Configures the source.</param>
        /// <exception cref="ArgumentNullException"><see cref="configureSource"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><see cref="configureSource"/>.<see cref="SystemsManagerConfigurationSource.Path"/> cannot be null</exception>
        /// <exception cref="ArgumentException"><see cref="configureSource"/>.<see cref="SystemsManagerConfigurationSource.Path"/> does not support Secrets Manager prefix (/aws/reference/secretsmanager/)</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSystemsManager(this IConfigurationBuilder builder, Action<SystemsManagerConfigurationSource> configureSource)
        {
            if (configureSource == null) throw new ArgumentNullException(nameof(configureSource));

            var source = new SystemsManagerConfigurationSource();
            configureSource(source);

            if (string.IsNullOrWhiteSpace(source.Path)) throw new ArgumentNullException(nameof(source.Path));
            if (source.Path.StartsWith(SecretsManagerPath, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException(SecretsManagerExceptionMessage);
            if (source.AwsOptions != null) return builder.Add(source);
            
            var config = builder.Build();
            source.AwsOptions = config.GetAWSOptions();
            return builder.Add(source);
        }
    }
}