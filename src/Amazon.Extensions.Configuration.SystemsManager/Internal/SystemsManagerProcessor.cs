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

using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Amazon.Extensions.Configuration.SystemsManager.Internal
{
    public class SystemsManagerProcessor : ISystemsManagerProcessor
    {
        private const string SecretsManagerPath = "/aws/reference/secretsmanager/";

        private SystemsManagerConfigurationSource Source { get; }

        public SystemsManagerProcessor(SystemsManagerConfigurationSource source)
        {
            if (source.AwsOptions == null) throw new ArgumentNullException(nameof(source.AwsOptions));
            if (source.Path == null) throw new ArgumentNullException(nameof(source.Path));
            
            Source = source;
            Source.ParameterProcessor = Source.ParameterProcessor ??
                (IsSecretsManagerPath(Source.Path)
                    ? new JsonParameterProcessor()
                    : new DefaultParameterProcessor());
        }

        public async Task<IDictionary<string, string>> GetDataAsync()
        {
            if (IsSecretsManagerPath(Source.Path))
            {
                return await GetParameterAsync().ConfigureAwait(false);
            }
            else if (Source.ParameterNames != null && Source.ParameterNames.Any())
            {
                return await GetParametersByNamesAsync().ConfigureAwait(false);
            }
            else
            {
                return await GetParametersByPathAsync().ConfigureAwait(false);
            }
        }

        private async Task<IDictionary<string, string>> GetParametersByPathAsync()
        {
            using (var client = Source.AwsOptions.CreateServiceClient<IAmazonSimpleSystemsManagement>())
            {
                if (client is AmazonSimpleSystemsManagementClient impl)
                {
                    impl.BeforeRequestEvent += ServiceClientAppender.ServiceClientBeforeRequestEvent;
                }

                var parameters = new List<Parameter>();
                string nextToken = null;
                do
                {
                    var response = await client.GetParametersByPathAsync(new GetParametersByPathRequest { Path = Source.Path, Recursive = true, WithDecryption = true, NextToken = nextToken, ParameterFilters = Source.Filters }).ConfigureAwait(false);
                    nextToken = response.NextToken;
                    parameters.AddRange(response.Parameters ?? new List<Parameter>());
                } while (!string.IsNullOrEmpty(nextToken));

                return AddPrefix(Source.ParameterProcessor.ProcessParameters(parameters, Source.Path), Source.Prefix);
            }
        }

        private async Task<IDictionary<string, string>> GetParametersByNamesAsync()
        {
            using (var client = Source.AwsOptions.CreateServiceClient<IAmazonSimpleSystemsManagement>())
            {
                if (client is AmazonSimpleSystemsManagementClient impl)
                {
                    impl.BeforeRequestEvent += ServiceClientAppender.ServiceClientBeforeRequestEvent;
                }

                var allParameters = new List<Parameter>();
                
                // Construct full parameter names by combining path prefix with relative names
                var fullParameterNames = Source.ParameterNames
                    .Select(name => CombinePathAndName(Source.Path, name))
                    .ToList();
                
                // Batch requests into groups of 10 (AWS API limit)
                for (int i = 0; i < fullParameterNames.Count; i += 10)
                {
                    var batch = fullParameterNames.Skip(i).Take(10).ToList();
                    
                    var request = new GetParametersRequest
                    {
                        Names = batch,
                        WithDecryption = true
                    };
                    
                    var response = await client.GetParametersAsync(request).ConfigureAwait(false);
                    
                    // Handle invalid parameters
                    if (response.InvalidParameters != null && response.InvalidParameters.Any() && !Source.Optional)
                    {
                        throw new ParameterNotFoundException(
                            $"The following parameters were not found: {string.Join(", ", response.InvalidParameters)}");
                    }
                    
                    allParameters.AddRange(response.Parameters ?? new List<Parameter>());
                }
                
                // Process parameters using the configured processor
                // Pass the path so it can be stripped, just like GetParametersByPath
                return AddPrefix(Source.ParameterProcessor.ProcessParameters(allParameters, Source.Path), Source.Prefix);
            }
        }

        private async Task<IDictionary<string, string>> GetParameterAsync()
        {
            using (var client = Source.AwsOptions.CreateServiceClient<IAmazonSimpleSystemsManagement>())
            {
                if (client is AmazonSimpleSystemsManagementClient impl)
                {
                    impl.BeforeRequestEvent += ServiceClientAppender.ServiceClientBeforeRequestEvent;
                }

                var response = await client.GetParameterAsync(new GetParameterRequest { Name = Source.Path, WithDecryption = true }).ConfigureAwait(false);

                var prefix = Source.Prefix;
                return AddPrefix(Source.ParameterProcessor.ProcessParameters(new []{response.Parameter}, Source.Path), prefix);
            }
        }

        public static bool IsSecretsManagerPath(string path) => path.StartsWith(SecretsManagerPath, StringComparison.OrdinalIgnoreCase);

        public static IDictionary<string, string> AddPrefix(IDictionary<string, string> input, string prefix)
        {
            return string.IsNullOrEmpty(prefix)
                ? input
                : input.ToDictionary(pair => $"{prefix}{ConfigurationPath.KeyDelimiter}{pair.Key}", pair => pair.Value, StringComparer.OrdinalIgnoreCase);

        }

        private static string CombinePathAndName(string path, string name)
        {
            // Validate that name doesn't start with /
            if (name.StartsWith("/", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    $"Parameter name '{name}' must be relative (cannot start with /)", 
                    nameof(name));
            }
            
            // Ensure path ends with / for proper combination
            if (!path.EndsWith("/", StringComparison.Ordinal))
            {
                path += "/";
            }
            
            // Combine path and name
            string combined = path + name;
            
            // Validate combined name doesn't exceed 2048 characters
            if (combined.Length > 2048)
            {
                throw new ArgumentException(
                    $"Full parameter name exceeds maximum length of 2048 characters: {combined.Length} characters",
                    nameof(name));
            }
            
            return combined;
        }
    }
}