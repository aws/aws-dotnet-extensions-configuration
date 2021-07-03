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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Amazon.Extensions.Configuration.SystemsManager
{
    /// <inheritdoc />
    /// <summary>
    /// An AWS Systems Manager Parameter Store based <see cref="IConfigurationProvider" />.
    /// </summary>
    public class SystemsManagerConfigurationProvider : ConfigurationProvider
    {
        public SystemsManagerConfigurationSource Source { get; }
        private ISystemsManagerProcessor SystemsManagerProcessor { get; }

        private ManualResetEvent ReloadTaskEvent { get; } = new ManualResetEvent(true);

        // Flag is set when parameters have been fetched from SSM before the framework calls Load()
        private bool DataHasBeenPreFetched = false;

        // It is possible that that the pre-fetch is not complete when the framework calls Load()
        // Therefore, lock LoadAsync() so the pre-fetch can complete before Load() continues.
        private static readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance with the specified source.
        /// </summary>
        /// <param name="source">The <see cref="IConfigurationSource"/> used to retrieve values from AWS Systems Manager Parameter Store</param>
        public SystemsManagerConfigurationProvider(SystemsManagerConfigurationSource source) : this(source, new SystemsManagerProcessor(source))
        {
        }


        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance with the specified source.
        /// </summary>
        /// <param name="source">The <see cref="IConfigurationSource"/> used to retrieve values from AWS Systems Manager Parameter Store</param>
        /// <param name="systemsManagerProcessor">The <see cref="ISystemsManagerProcessor"/> used to retrieve values from AWS Systems Manager Parameter Store</param>
        public SystemsManagerConfigurationProvider(SystemsManagerConfigurationSource source, ISystemsManagerProcessor systemsManagerProcessor)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            SystemsManagerProcessor = systemsManagerProcessor ?? throw new ArgumentNullException(nameof(systemsManagerProcessor));

            if (source.AwsOptions == null) throw new ArgumentNullException(nameof(source.AwsOptions));
            if (source.Path == null) throw new ArgumentNullException(nameof(source.Path));

            if (source.ReloadAfter != null)
            {
                ChangeToken.OnChange(() =>
                {
                    var cancellationTokenSource = new CancellationTokenSource(source.ReloadAfter.Value);
                    var cancellationChangeToken = new CancellationChangeToken(cancellationTokenSource.Token);
                    return cancellationChangeToken;
                }, async () =>
                {
                    ReloadTaskEvent.Reset();
                    try
                    {
                        await LoadAsync(true).ConfigureAwait(false);
                    }
                    finally
                    {
                        ReloadTaskEvent.Set();
                    }
                });
            }

            // Pre-fetch Parameter Store values from SSM to improve Lambda cold start times. 
            LoadAsync(false, preFetch: true);
        }

        /// <summary>
        /// If this configuration provider is currently performing a reload of the config data this method will block until
        /// the reload is called.
        /// 
        /// This method is not meant for general use. It is exposed so a Lambda function can wait for the reload to complete
        /// before completing the event causing the Lambda compute environment to be frozen.
        /// </summary>
        public void WaitForReloadToComplete(TimeSpan timeout)
        {
            ReloadTaskEvent.WaitOne(timeout);
        }

        /// <inheritdoc />
        /// <summary>
        /// Loads the AWS Systems Manager Parameters.
        /// </summary>
        public override void Load() => LoadAsync(false).ConfigureAwait(false).GetAwaiter().GetResult();

        // If 1) reload flag is set to true and 2) OnLoadException handler is not set, 
        // all exceptions raised during OnReload() will be ignored.
        private async Task LoadAsync(bool reload, bool preFetch = false)
        {
            // Wait for any already running load operations to complete before starting another. 
            // This can happen if the framework calls Load before the pre-fetch completes.
            await Lock.WaitAsync().ConfigureAwait(false);

            try
            {
                // Check if the Data has been prefetched. If this is a reload,
                // ignore pre-fetched data. We only prefetch during initialization.
                if (DataHasBeenPreFetched && !reload) return;

                var newData = await SystemsManagerProcessor.GetDataAsync().ConfigureAwait(false) ?? new Dictionary<string, string>();

                if (!Data.EquivalentTo(newData))
                {
                    Data = newData;

                    OnReload();
                }
            }
            catch (Exception ex)
            {
                if (Source.Optional) return;

                var ignoreException = reload;
                if (Source.OnLoadException != null)
                {
                    var exceptionContext = new SystemsManagerExceptionContext
                    {
                        Provider = this,
                        Exception = ex,
                        Reload = reload
                    };
                    Source.OnLoadException(exceptionContext);
                    ignoreException = exceptionContext.Ignore;
                }

                if (!ignoreException)
                    throw;
            }
            finally
            {
                // Always reset the flag and release the lock 
                DataHasBeenPreFetched = preFetch;
                Lock.Release();
            }

        }

        [Obsolete("This method has been moved into the internal namespace, and will be removed in a future release. Use ParameterProcessor instead")]
        public static IDictionary<string, string> ProcessParameters(IEnumerable<Parameter> parameters, string path, IParameterProcessor parameterProcessor)
        {
            return parameters
                .Where(parameter => parameterProcessor.IncludeParameter(parameter, path))
                .Select(parameter => new
                {
                    Key = parameterProcessor.GetKey(parameter, path),
                    Value = parameterProcessor.GetValue(parameter, path)
                })
                .ToDictionary(parameter => parameter.Key, parameter => parameter.Value, StringComparer.OrdinalIgnoreCase);
        }
    }
}