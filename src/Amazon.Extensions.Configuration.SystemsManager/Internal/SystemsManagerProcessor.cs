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

using Amazon.Runtime;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Amazon.Extensions.Configuration.SystemsManager.Internal
{
    public interface ISystemsManagerProcessor
    {
        Task<IDictionary<string, string>> GetDataAsync();
    }

    public class SystemsManagerProcessor : ISystemsManagerProcessor
    {
        private const string SecretsManagerPath = "/aws/reference/secretsmanager/";

        private SystemsManagerConfigurationSource Source { get; }

        public SystemsManagerProcessor(SystemsManagerConfigurationSource source)
        {
            Source = source;
            Source.ParameterProcessor = Source.ParameterProcessor ?? new DefaultParameterProcessor();
        }

        public async Task<IDictionary<string, string>> GetDataAsync()
        {
            return IsSecretsManagerPath(Source.Path)
                ? await GetParameterAsync().ConfigureAwait(false)
                : await GetParametersByPathAsync().ConfigureAwait(false);
        }

        private async Task<IDictionary<string, string>> GetParametersByPathAsync()
        {
            using (var client = Source.AwsOptions.CreateServiceClient<IAmazonSimpleSystemsManagement>())
            {
                if (client is AmazonSimpleSystemsManagementClient impl)
                {
                    impl.BeforeRequestEvent += ServiceClientBeforeRequestEvent;
                }

                var parameters = new List<Parameter>();
                string nextToken = null;
                do
                {
                    var response = await client.GetParametersByPathAsync(new GetParametersByPathRequest { Path = Source.Path, Recursive = true, WithDecryption = true, NextToken = nextToken, ParameterFilters = Source.Filters }).ConfigureAwait(false);
                    nextToken = response.NextToken;
                    parameters.AddRange(response.Parameters);
                } while (!string.IsNullOrEmpty(nextToken));

                return AddPrefix(ProcessParameters(parameters, Source.Path, Source.ParameterProcessor), Source.Prefix);
            }
        }

        private async Task<IDictionary<string, string>> GetParameterAsync()
        {
            using (var client = Source.AwsOptions.CreateServiceClient<IAmazonSimpleSystemsManagement>())
            {
                if (client is AmazonSimpleSystemsManagementClient impl)
                {
                    impl.BeforeRequestEvent += ServiceClientBeforeRequestEvent;
                }

                var response = await client.GetParameterAsync(new GetParameterRequest { Name = Source.Path, WithDecryption = true }).ConfigureAwait(false);

                if (!Source.ParameterProcessor.IncludeParameter(response.Parameter, SecretsManagerPath)) return new Dictionary<string, string>();

                // A secret should only be one value.
                var parameter = Source.ParameterProcessor.Process(response.Parameter, SecretsManagerPath).Single();
                return AddPrefix(JsonConfigurationParser.Parse(parameter.Value), Source.Prefix ?? parameter.Key);

            }
        }

        public static bool IsSecretsManagerPath(string path) => path.StartsWith(SecretsManagerPath, StringComparison.OrdinalIgnoreCase);

        public static IDictionary<string, string> AddPrefix(IDictionary<string, string> input, string prefix)
        {
            return string.IsNullOrEmpty(prefix)
                ? input
                : input.ToDictionary(pair => $"{prefix}{ConfigurationPath.KeyDelimiter}{pair.Key}", pair => pair.Value, StringComparer.OrdinalIgnoreCase);
        }

        public static IDictionary<string, string> ProcessParameters(IEnumerable<Parameter> parameters, string path, IParameterProcessor parameterProcessor)
        {
            return parameters
                .Where(parameter => parameterProcessor.IncludeParameter(parameter, path))
                .SelectMany(parameter => parameterProcessor.Process(parameter,path))
                .ToDictionary(parameter => parameter.Key, parameter => parameter.Value, StringComparer.OrdinalIgnoreCase);
        }

        private const string UserAgentHeader = "User-Agent";
        private static readonly string AssemblyVersion = typeof(SystemsManagerProcessor).GetTypeInfo().Assembly.GetName().Version.ToString();

        private static void ServiceClientBeforeRequestEvent(object sender, RequestEventArgs e)
        {
            if (e is WebServiceRequestEventArgs args && args.Headers.ContainsKey(UserAgentHeader))
            {
                args.Headers[UserAgentHeader] = args.Headers[UserAgentHeader] + " SSMConfigProvider/" + AssemblyVersion;
            }
        }
    }
}