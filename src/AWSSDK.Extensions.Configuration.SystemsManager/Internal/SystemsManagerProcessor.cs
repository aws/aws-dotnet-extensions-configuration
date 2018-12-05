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

using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace Amazon.Extensions.Configuration.SystemsManager.Internal
{
    public interface ISystemsManagerProcessor
    {
        Task<IEnumerable<Parameter>> GetParametersByPathAsync(AWSOptions awsOptions, string path);
    }

    public class SystemsManagerProcessor : ISystemsManagerProcessor
    {
        public async Task<IEnumerable<Parameter>> GetParametersByPathAsync(AWSOptions awsOptions, string path)
        {
            using (var client = awsOptions.CreateServiceClient<IAmazonSimpleSystemsManagement>())
            {
                if(client is AmazonSimpleSystemsManagementClient impl)
                {
                    impl.BeforeRequestEvent += ServiceClientBeforeRequestEvent;
                }

                var parameters = new List<Parameter>();
                string nextToken = null;
                do
                {
                    var response = await client.GetParametersByPathAsync(new GetParametersByPathRequest { Path = path, Recursive = true, WithDecryption = true, NextToken = nextToken }).ConfigureAwait(false);
                    nextToken = response.NextToken;
                    parameters.AddRange(response.Parameters);
                } while (!string.IsNullOrEmpty(nextToken));

                return parameters;
            }
        }

        const string UserAgentHeader = "User-Agent";
        static readonly string _assemblyVersion = typeof(SystemsManagerProcessor).GetTypeInfo().Assembly.GetName().Version.ToString();

        void ServiceClientBeforeRequestEvent(object sender, RequestEventArgs e)
        {
            var args = e as Amazon.Runtime.WebServiceRequestEventArgs;
            if (args == null || !args.Headers.ContainsKey(UserAgentHeader))
                return;


            args.Headers[UserAgentHeader] = args.Headers[UserAgentHeader] + " SSMConfigProvider/" + _assemblyVersion;
        }
    }
}