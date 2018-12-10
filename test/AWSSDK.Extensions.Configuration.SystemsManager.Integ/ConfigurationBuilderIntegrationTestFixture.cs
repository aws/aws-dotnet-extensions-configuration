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

using Amazon.Extensions.NETCore.Setup;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace AWSSDK.Extensions.Configuration.SystemsManager.Integ
{
    public class IntegTestFixture : IAsyncLifetime
    {
        public const string ParameterPrefix = @"/configuration-extension-testdata/ssm/";
        public AWSOptions AWSOptions { get; private set; }

        public IDictionary<string, string> TestData { get; } = new Dictionary<string, string>
        {
            {"hello", "world"},
            {"hello2", "world2"},
        };

        public async Task InitializeAsync()
        {
            AWSOptions = new AWSOptions();
            AWSOptions.Region = Amazon.RegionEndpoint.USWest2;

            bool success = false;
            using (var client = AWSOptions.CreateServiceClient<IAmazonSimpleSystemsManagement>())
            {
                var tasks = new List<Task>();
                foreach (var kv in TestData)
                {
                    Console.WriteLine($"Adding parameter: ({ParameterPrefix + kv.Key}, {kv.Value})");
                    tasks.Add(client.PutParameterAsync(new PutParameterRequest
                    {
                        Name = ParameterPrefix + kv.Key,
                        Value = kv.Value,
                        Type = ParameterType.String
                    }));
                };

                await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);

                // due to eventual consistency, wait for 5 sec increments for 3 times to verify
                // test data is correctly set before executing tests.
                const int tries = 3;
                for (int i = 0; i < tries; i++)
                {
                    int count = 0;
                    GetParametersByPathResponse response;
                    string nextToken = null;
                    do
                    {
                        response = await client.GetParametersByPathAsync(new GetParametersByPathRequest
                        {
                            Path = ParameterPrefix,
                            NextToken = nextToken
                        }).ConfigureAwait(false);

                        count += response.Parameters.Count;
                        nextToken = response.NextToken;
                    } while (!string.IsNullOrEmpty(nextToken));

                    success = (count == TestData.Count);

                    if (success)
                    {
                        Console.WriteLine("Verified that test data is available.");
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"Waiting on test data to be available. Waiting {count + 1}/{tries}");
                        await Task.Delay(5 * 1000).ConfigureAwait(false);
                    }
                }
            }

            if (!success) throw new Exception("Failed to seed integration test data");
        }

        public async Task DisposeAsync()
        {
            Console.Write($"Delete all test parameters with prefix '{ParameterPrefix}'... ");
            using (var client = AWSOptions.CreateServiceClient<IAmazonSimpleSystemsManagement>())
            {
                GetParametersByPathResponse response;
                string nextToken = null;
                do
                {
                    response =  await client.GetParametersByPathAsync(new GetParametersByPathRequest
                    {
                        Path = ParameterPrefix,
                        NextToken = nextToken
                    }).ConfigureAwait(false);
                    nextToken = response.NextToken;

                    await client.DeleteParametersAsync(new DeleteParametersRequest
                    {
                        Names = response.Parameters.Select(p => p.Name).ToList()
                    }).ConfigureAwait(false);
                } while (!string.IsNullOrEmpty(nextToken));

                // no need to wait for eventual consistency here given we are not running tests back-to-back
            }
            Console.WriteLine("Done");
        }
    }
}
