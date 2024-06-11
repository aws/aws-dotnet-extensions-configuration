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
using System.Threading;
using System.Threading.Tasks;
using Amazon.Extensions.Configuration.SystemsManager.AppConfig;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Amazon.Extensions.Configuration.SystemsManager
{
    /// <inheritdoc />
    /// <summary>
    /// An AWS Systems Manager based <see cref="ConfigurationProvider" />.
    /// </summary>
    public class SystemsManagerConfigurationProvider : ConfigurationProvider
    {
        private ISystemsManagerConfigurationSource Source { get; }
        private ISystemsManagerProcessor SystemsManagerProcessor { get; }

        private ManualResetEvent ReloadTaskEvent { get; } = new ManualResetEvent(true);

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance with the specified source.
        /// </summary>
        /// <param name="source">The <see cref="SystemsManagerConfigurationSource"/> used to retrieve values from AWS Systems Manager Parameter Store</param>
        public SystemsManagerConfigurationProvider(SystemsManagerConfigurationSource source) : this(source, new SystemsManagerProcessor(source))
        {
        }
        
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance with the specified source.
        /// </summary>
        /// <param name="source">The <see cref="AppConfigConfigurationSource"/> used to retrieve values from AWS Systems Manager AppConfig</param>
        public SystemsManagerConfigurationProvider(AppConfigConfigurationSource source) : this(source, new AppConfigProcessor(source))
        {
        }


        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance with the specified source.
        /// </summary>
        /// <param name="source">The <see cref="ISystemsManagerConfigurationSource"/> used to retrieve values from AWS Systems Manager</param>
        /// <param name="systemsManagerProcessor">The <see cref="ISystemsManagerProcessor"/> used to retrieve values from AWS Systems Manager</param>
        public SystemsManagerConfigurationProvider(ISystemsManagerConfigurationSource source, ISystemsManagerProcessor systemsManagerProcessor)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            SystemsManagerProcessor = systemsManagerProcessor ?? throw new ArgumentNullException(nameof(systemsManagerProcessor));

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
        }

        /// <summary>
        /// If this configuration provider is currently performing a reload of the config data this method will block until
        /// the reload is called.
        /// 
        /// This method is not meant for general use. It is exposed so a AWS Lambda function can wait for the reload to complete
        /// before completing the event causing the AWS Lambda compute environment to be frozen.
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
        private async Task LoadAsync(bool reload)
        {
            try
            {
                var newData = await SystemsManagerProcessor.GetDataAsync().ConfigureAwait(false) ?? new Dictionary<string, string>();

                if (!Data.EquivalentTo(newData))
                {
                    Data = newData;

                    OnReload();
                }
            }
            catch (DuplicateParameterException) // Throw duplicate parameter exception irrespective of whether parameter is optional or not.
            {
                throw;
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
        }
    }
}