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
using System.Linq;
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
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager Parameter Store by loading specific parameter names.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">The path prefix that will be prepended to each parameter name. The path will be removed from the variable names during processing. Cannot be a Secrets Manager path (/aws/reference/secretsmanager).</param>
        /// <param name="parameterNames">A collection of relative parameter names to load. Names must be relative paths (e.g., "connections/db") and cannot start with "/".</param>
        /// <param name="awsOptions"><see cref="AWSOptions"/> used to create an AWS Systems Manager Client connection</param>
        /// <param name="optional">Whether the AWS Systems Manager Parameters are optional.</param>
        /// <param name="reloadAfter">Initiate reload after TimeSpan</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><paramref name="parameterNames"/> cannot be null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> cannot be a Secrets Manager path (/aws/reference/secretsmanager)</exception>
        /// <exception cref="ArgumentException"><paramref name="parameterNames"/> cannot be empty after filtering</exception>
        /// <exception cref="ArgumentException">Parameter names cannot start with "/"</exception>
        /// <exception cref="ArgumentException">Parameter names cannot be null, empty, or whitespace</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSystemsManager(this IConfigurationBuilder builder, string path, IEnumerable<string> parameterNames, AWSOptions awsOptions, bool optional, TimeSpan reloadAfter)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (parameterNames == null) throw new ArgumentNullException(nameof(parameterNames));

            // Validate that Secrets Manager paths are not used with parameter names
            if (path.StartsWith("/aws/reference/secretsmanager", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Cannot use Secrets Manager path (/aws/reference/secretsmanager) with parameter names. Secrets Manager references must be loaded individually without specifying parameter names.", nameof(path));
            }

            // Filter out null, empty, or whitespace parameter names
            var validNames = new List<string>();
            var validationErrors = new List<string>();
            foreach (var name in parameterNames)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    validationErrors.Add("Parameter name cannot be null, empty, or whitespace.");
                    continue;
                }

                if (name.StartsWith("/", StringComparison.Ordinal))
                {
                    validationErrors.Add($"Parameter name '{name}' must be relative (cannot start with /).");
                    continue;
                }

                validNames.Add(name);
            }

            if (validationErrors.Count > 0)
            {
                throw new ArgumentException("One or more parameter names are invalid: " + string.Join(" ", validationErrors), nameof(parameterNames));
            }

            if (validNames.Count == 0)
            {
                throw new ArgumentException("Parameter names collection cannot be empty", nameof(parameterNames));
            }

            // Remove duplicates
            var uniqueNames = new List<string>();
            var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in validNames)
            {
                if (seenNames.Add(name))
                {
                    uniqueNames.Add(name);
                }
            }

            return builder.AddSystemsManager(configureSource =>
            {
                configureSource.Path = path;
                configureSource.ParameterNames = uniqueNames;
                configureSource.AwsOptions = awsOptions;
                configureSource.Optional = optional;

                if (reloadAfter != TimeSpan.Zero)
                    configureSource.ReloadAfter = reloadAfter;
            });
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager Parameter Store by loading specific parameter names.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">The path prefix that will be prepended to each parameter name. The path will be removed from the variable names during processing. Cannot be a Secrets Manager path (/aws/reference/secretsmanager).</param>
        /// <param name="parameterNames">A collection of relative parameter names to load. Names must be relative paths (e.g., "connections/db") and cannot start with "/".</param>
        /// <param name="awsOptions"><see cref="AWSOptions"/> used to create an AWS Systems Manager Client connection</param>
        /// <param name="optional">Whether the AWS Systems Manager Parameters are optional.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><paramref name="parameterNames"/> cannot be null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> cannot be a Secrets Manager path (/aws/reference/secretsmanager)</exception>
        /// <exception cref="ArgumentException"><paramref name="parameterNames"/> cannot be empty after filtering</exception>
        /// <exception cref="ArgumentException">Parameter names cannot start with "/"</exception>
        /// <exception cref="ArgumentException">Parameter names cannot be null, empty, or whitespace</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSystemsManager(this IConfigurationBuilder builder, string path, IEnumerable<string> parameterNames, AWSOptions awsOptions, bool optional)
        {
            return builder.AddSystemsManager(path, parameterNames, awsOptions, optional, TimeSpan.Zero);
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager Parameter Store by loading specific parameter names.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">The path prefix that will be prepended to each parameter name. The path will be removed from the variable names during processing. Cannot be a Secrets Manager path (/aws/reference/secretsmanager).</param>
        /// <param name="parameterNames">A collection of relative parameter names to load. Names must be relative paths (e.g., "connections/db") and cannot start with "/".</param>
        /// <param name="awsOptions"><see cref="AWSOptions"/> used to create an AWS Systems Manager Client connection</param>
        /// <param name="reloadAfter">Initiate reload after TimeSpan</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><paramref name="parameterNames"/> cannot be null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> cannot be a Secrets Manager path (/aws/reference/secretsmanager)</exception>
        /// <exception cref="ArgumentException"><paramref name="parameterNames"/> cannot be empty after filtering</exception>
        /// <exception cref="ArgumentException">Parameter names cannot start with "/"</exception>
        /// <exception cref="ArgumentException">Parameter names cannot be null, empty, or whitespace</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSystemsManager(this IConfigurationBuilder builder, string path, IEnumerable<string> parameterNames, AWSOptions awsOptions, TimeSpan reloadAfter)
        {
            return builder.AddSystemsManager(path, parameterNames, awsOptions, false, reloadAfter);
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager Parameter Store by loading specific parameter names.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">The path prefix that will be prepended to each parameter name. The path will be removed from the variable names during processing. Cannot be a Secrets Manager path (/aws/reference/secretsmanager).</param>
        /// <param name="parameterNames">A collection of relative parameter names to load. Names must be relative paths (e.g., "connections/db") and cannot start with "/".</param>
        /// <param name="awsOptions"><see cref="AWSOptions"/> used to create an AWS Systems Manager Client connection</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><paramref name="parameterNames"/> cannot be null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> cannot be a Secrets Manager path (/aws/reference/secretsmanager)</exception>
        /// <exception cref="ArgumentException"><paramref name="parameterNames"/> cannot be empty after filtering</exception>
        /// <exception cref="ArgumentException">Parameter names cannot start with "/"</exception>
        /// <exception cref="ArgumentException">Parameter names cannot be null, empty, or whitespace</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSystemsManager(this IConfigurationBuilder builder, string path, IEnumerable<string> parameterNames, AWSOptions awsOptions)
        {
            return builder.AddSystemsManager(path, parameterNames, awsOptions, false, TimeSpan.Zero);
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager Parameter Store by loading specific parameter names.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">The path prefix that will be prepended to each parameter name. The path will be removed from the variable names during processing. Cannot be a Secrets Manager path (/aws/reference/secretsmanager).</param>
        /// <param name="parameterNames">A collection of relative parameter names to load. Names must be relative paths (e.g., "connections/db") and cannot start with "/".</param>
        /// <param name="optional">Whether the AWS Systems Manager Parameters are optional.</param>
        /// <param name="reloadAfter">Initiate reload after TimeSpan</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><paramref name="parameterNames"/> cannot be null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> cannot be a Secrets Manager path (/aws/reference/secretsmanager)</exception>
        /// <exception cref="ArgumentException"><paramref name="parameterNames"/> cannot be empty after filtering</exception>
        /// <exception cref="ArgumentException">Parameter names cannot start with "/"</exception>
        /// <exception cref="ArgumentException">Parameter names cannot be null, empty, or whitespace</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSystemsManager(this IConfigurationBuilder builder, string path, IEnumerable<string> parameterNames, bool optional, TimeSpan reloadAfter)
        {
            return builder.AddSystemsManager(path, parameterNames, null, optional, reloadAfter);
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager Parameter Store by loading specific parameter names.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">The path prefix that will be prepended to each parameter name. The path will be removed from the variable names during processing. Cannot be a Secrets Manager path (/aws/reference/secretsmanager).</param>
        /// <param name="parameterNames">A collection of relative parameter names to load. Names must be relative paths (e.g., "connections/db") and cannot start with "/".</param>
        /// <param name="optional">Whether the AWS Systems Manager Parameters are optional.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><paramref name="parameterNames"/> cannot be null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> cannot be a Secrets Manager path (/aws/reference/secretsmanager)</exception>
        /// <exception cref="ArgumentException"><paramref name="parameterNames"/> cannot be empty after filtering</exception>
        /// <exception cref="ArgumentException">Parameter names cannot start with "/"</exception>
        /// <exception cref="ArgumentException">Parameter names cannot be null, empty, or whitespace</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSystemsManager(this IConfigurationBuilder builder, string path, IEnumerable<string> parameterNames, bool optional)
        {
            return builder.AddSystemsManager(path, parameterNames, null, optional, TimeSpan.Zero);
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager Parameter Store by loading specific parameter names.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">The path prefix that will be prepended to each parameter name. The path will be removed from the variable names during processing. Cannot be a Secrets Manager path (/aws/reference/secretsmanager).</param>
        /// <param name="parameterNames">A collection of relative parameter names to load. Names must be relative paths (e.g., "connections/db") and cannot start with "/".</param>
        /// <param name="reloadAfter">Initiate reload after TimeSpan</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><paramref name="parameterNames"/> cannot be null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> cannot be a Secrets Manager path (/aws/reference/secretsmanager)</exception>
        /// <exception cref="ArgumentException"><paramref name="parameterNames"/> cannot be empty after filtering</exception>
        /// <exception cref="ArgumentException">Parameter names cannot start with "/"</exception>
        /// <exception cref="ArgumentException">Parameter names cannot be null, empty, or whitespace</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSystemsManager(this IConfigurationBuilder builder, string path, IEnumerable<string> parameterNames, TimeSpan reloadAfter)
        {
            return builder.AddSystemsManager(path, parameterNames, null, false, reloadAfter);
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS Systems Manager Parameter Store by loading specific parameter names.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">The path prefix that will be prepended to each parameter name. The path will be removed from the variable names during processing. Cannot be a Secrets Manager path (/aws/reference/secretsmanager).</param>
        /// <param name="parameterNames">A collection of relative parameter names to load. Names must be relative paths (e.g., "connections/db") and cannot start with "/".</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException"><paramref name="parameterNames"/> cannot be null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> cannot be a Secrets Manager path (/aws/reference/secretsmanager)</exception>
        /// <exception cref="ArgumentException"><paramref name="parameterNames"/> cannot be empty after filtering</exception>
        /// <exception cref="ArgumentException">Parameter names cannot start with "/"</exception>
        /// <exception cref="ArgumentException">Parameter names cannot be null, empty, or whitespace</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSystemsManager(this IConfigurationBuilder builder, string path, IEnumerable<string> parameterNames)
        {
            return builder.AddSystemsManager(path, parameterNames, null, false, TimeSpan.Zero);
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