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

using Amazon.Extensions.NETCore.Setup;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Amazon.Extensions.Configuration.SystemsManager.Integ
{
    public class ParameterNameLoadingTestFixture : IDisposable
    {
        public const string ParameterPrefix = @"/configuration-extension-testdata/param-names/";

        public AWSOptions AWSOptions { get; private set; }

        private bool disposed = false;

        // Test data with various parameter types
        public IDictionary<string, (string Value, ParameterType Type)> TestData { get; } = new Dictionary<string, (string, ParameterType)>
        {
            // Regular string parameters
            {"app/database/host", ("localhost", ParameterType.String)},
            {"app/database/port", ("5432", ParameterType.String)},
            {"app/api/key", ("test-api-key-123", ParameterType.String)},
            
            // SecureString parameters for decryption testing
            {"secure/password", ("encrypted-password-value", ParameterType.SecureString)},
            {"secure/token", ("encrypted-token-value", ParameterType.SecureString)},
            
            // Batch testing parameters (12 parameters to test batching with 10-parameter limit)
            {"batch/param1", ("value1", ParameterType.String)},
            {"batch/param2", ("value2", ParameterType.String)},
            {"batch/param3", ("value3", ParameterType.String)},
            {"batch/param4", ("value4", ParameterType.String)},
            {"batch/param5", ("value5", ParameterType.String)},
            {"batch/param6", ("value6", ParameterType.String)},
            {"batch/param7", ("value7", ParameterType.String)},
            {"batch/param8", ("value8", ParameterType.String)},
            {"batch/param9", ("value9", ParameterType.String)},
            {"batch/param10", ("value10", ParameterType.String)},
            {"batch/param11", ("value11", ParameterType.String)},
            {"batch/param12", ("value12", ParameterType.String)},
        };

        public ParameterNameLoadingTestFixture()
        {
            AWSOptions = new AWSOptions();
            AWSOptions.Region = Amazon.RegionEndpoint.USWest2;

            SeedTestData();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                CleanupTestData();
            }
            disposed = true;
        }

        private void SeedTestData()
        {
            bool success = false;
            using (var client = AWSOptions.CreateServiceClient<IAmazonSimpleSystemsManagement>())
            {
                var tasks = new List<Task>();
                foreach (var kv in TestData)
                {
                    var fullName = ParameterPrefix + kv.Key;
                    Console.WriteLine($"Adding parameter: ({fullName}, {kv.Value.Value}, {kv.Value.Type})");
                    tasks.Add(client.PutParameterAsync(new PutParameterRequest
                    {
                        Name = fullName,
                        Value = kv.Value.Value,
                        Type = kv.Value.Type,
                        Overwrite = true
                    }));
                }
                Task.WaitAll(tasks.ToArray());

                // Due to eventual consistency, wait for parameters to be available
                const int tries = 3;
                for (int i = 0; i < tries; i++)
                {
                    // Verify all parameters are accessible using GetParameters API
                    var allParameterNames = TestData.Keys.Select(k => ParameterPrefix + k).ToList();
                    int foundCount = 0;
                    
                    // Batch into groups of 10 for GetParameters API
                    for (int batchStart = 0; batchStart < allParameterNames.Count; batchStart += 10)
                    {
                        var batch = allParameterNames.Skip(batchStart).Take(10).ToList();
                        var response = client.GetParametersAsync(new GetParametersRequest
                        {
                            Names = batch,
                            WithDecryption = true
                        }).Result;
                        
                        foundCount += response.Parameters.Count;
                    }

                    success = (foundCount == TestData.Count);

                    if (success)
                    {
                        Console.WriteLine("Verified that test data is available.");
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"Waiting on test data to be available. Attempt {i + 1}/{tries} (found {foundCount}/{TestData.Count})");
                        Thread.Sleep(5 * 1000);
                    }
                }
            }

            if (!success) throw new Exception("Failed to seed integration test data");
        }

        private void CleanupTestData()
        {
            Console.Write($"Delete all test parameters with prefix '{ParameterPrefix}'... ");
            using (var client = AWSOptions.CreateServiceClient<IAmazonSimpleSystemsManagement>())
            {
                GetParametersByPathResponse response;
                do
                {
                    response = client.GetParametersByPathAsync(new GetParametersByPathRequest
                    {
                        Path = ParameterPrefix
                    }).Result;

                    if (response.Parameters.Any())
                    {
                        client.DeleteParametersAsync(new DeleteParametersRequest
                        {
                            Names = response.Parameters.Select(p => p.Name).ToList()
                        }).Wait();
                    }
                } while (!string.IsNullOrEmpty(response.NextToken));
            }
            Console.WriteLine("Done");
        }
    }
}
