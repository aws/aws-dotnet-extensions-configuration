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
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Amazon.AppConfigData;
using Amazon.AppConfigData.Model;
using Amazon.Extensions.Configuration.SystemsManager.Internal;

namespace Amazon.Extensions.Configuration.SystemsManager.AppConfig
{
    public class AppConfigProcessor : ISystemsManagerProcessor
    {
        private AppConfigConfigurationSource Source { get; }
        private IDictionary<string, string> LastConfig { get; set; }

        private string PollConfigurationToken { get; set; }
        private DateTime NextAllowedPollTime { get; set; }

        private SemaphoreSlim _lastConfigLock = new SemaphoreSlim(1, 1);
        private const int _lastConfigLockTimeout = 3000;

        private Uri _lambdaExtensionUri;
        private HttpClient _lambdaExtensionClient;

        private IAmazonAppConfigData _appConfigDataClient;

        public AppConfigProcessor(AppConfigConfigurationSource source)
        {
            Source = source;

            if (source.ApplicationId == null) throw new ArgumentNullException(nameof(source.ApplicationId));
            if (source.EnvironmentId == null) throw new ArgumentNullException(nameof(source.EnvironmentId));
            if (source.ConfigProfileId == null) throw new ArgumentNullException(nameof(source.ConfigProfileId));

            // Check to see if the function is being run inside Lambda. If it is not because it is running in a integ test or in the 
            // the .NET Lambda Test Tool the Lambda extension is not available and fallback to using the AppConfig service directly.
            if(Source.UseLambdaExtension && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME")))
            {
                var port = Environment.GetEnvironmentVariable("AWS_APPCONFIG_EXTENSION_HTTP_PORT") ?? "2772";
                _lambdaExtensionUri = new Uri($"http://localhost:{port}/applications/{Source.ApplicationId}/environments/{Source.EnvironmentId}/configurations/{Source.ConfigProfileId}");
                _lambdaExtensionClient = source.CustomHttpClientForLambdaExtension ?? new HttpClient();
            }
            else
            {
                if(source.AwsOptions != null)
                {
                    _appConfigDataClient = source.AwsOptions.CreateServiceClient<IAmazonAppConfigData>();
                }
                else
                {
                    _appConfigDataClient = new AmazonAppConfigDataClient();
                }

                if (_appConfigDataClient is AmazonAppConfigDataClient impl)
                {
                    impl.BeforeRequestEvent += ServiceClientAppender.ServiceClientBeforeRequestEvent;
                }
            }
        }

        public async Task<IDictionary<string, string>> GetDataAsync()
        {

            if(_appConfigDataClient != null)
            {
                return await GetDataFromServiceAsync().ConfigureAwait(false);
            }
            else
            {
                return await GetDataFromLambdaExtensionAsync().ConfigureAwait(false);
            }
        }

        private async Task<IDictionary<string,string>> GetDataFromLambdaExtensionAsync()
        {
            using (var response = await _lambdaExtensionClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, _lambdaExtensionUri)).ConfigureAwait(false))
            using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                LastConfig = ParseConfig(response.Content.Headers.ContentType.ToString(), stream);
            }

            return LastConfig;
        }

        private async Task<IDictionary<string, string>> GetDataFromServiceAsync()
        {
            if(await _lastConfigLock.WaitAsync(_lastConfigLockTimeout).ConfigureAwait(false))
            {
                try
                {
                    if(DateTime.UtcNow < NextAllowedPollTime)
                    {
                        return LastConfig;
                    }

                    if (string.IsNullOrEmpty(PollConfigurationToken))
                    {
                        this.PollConfigurationToken = await GetInitialConfigurationTokenAsync(_appConfigDataClient).ConfigureAwait(false);
                    }

                    var request = new GetLatestConfigurationRequest
                    {
                        ConfigurationToken = PollConfigurationToken
                    };

                    var response = await _appConfigDataClient.GetLatestConfigurationAsync(request).ConfigureAwait(false);
                    PollConfigurationToken = response.NextPollConfigurationToken;
                    NextAllowedPollTime = DateTime.UtcNow.AddSeconds(response.NextPollIntervalInSeconds);

                    // Configuration is empty when the last received config is the latest
                    // so only attempt to parse the AppConfig response when it is not empty
                    if (response.ContentLength > 0)
                    {
                        LastConfig = ParseConfig(response.ContentType, response.Configuration);
                    }
                }
                finally
                {
                    _lastConfigLock.Release();
                }
            }
            else
            {
                return LastConfig;
            }

            return LastConfig;
        }

        private async Task<string> GetInitialConfigurationTokenAsync(IAmazonAppConfigData appConfigClient)
        {
            var request = new StartConfigurationSessionRequest
            {
                ApplicationIdentifier = Source.ApplicationId,
                EnvironmentIdentifier = Source.EnvironmentId,
                ConfigurationProfileIdentifier = Source.ConfigProfileId
            };

            return (await appConfigClient.StartConfigurationSessionAsync(request).ConfigureAwait(false)).InitialConfigurationToken;
        }

        private static IDictionary<string, string> ParseConfig(string contentType, Stream configuration)
        {
            // Content-Type has format "media-type; charset" or "media-type; boundary" (for multipart entities).
            if (contentType != null)
            {
                contentType = contentType.Split(';')[0];
            }

            switch (contentType)
            {
                case "application/json":
                    return JsonConfigurationParser.Parse(configuration);
                default:
                    throw new NotImplementedException($"Not implemented AppConfig type: {contentType}");
            }
        }
    }
}
