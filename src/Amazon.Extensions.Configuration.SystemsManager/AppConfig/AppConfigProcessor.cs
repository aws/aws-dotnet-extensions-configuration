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
using System.Threading.Tasks;
using Amazon.AppConfig;
using Amazon.AppConfig.Model;
using Amazon.Extensions.Configuration.SystemsManager.Internal;

namespace Amazon.Extensions.Configuration.SystemsManager.AppConfig
{
    public class AppConfigProcessor : ISystemsManagerProcessor
    {
        private AppConfigConfigurationSource Source { get; }

        public AppConfigProcessor(AppConfigConfigurationSource source)
        {
            if (source.ApplicationId == null) throw new ArgumentNullException(nameof(source.ApplicationId));
            if (source.EnvironmentId == null) throw new ArgumentNullException(nameof(source.EnvironmentId));
            if (source.ConfigProfileId == null) throw new ArgumentNullException(nameof(source.ConfigProfileId));
            if (source.AwsOptions == null) throw new ArgumentNullException(nameof(source.AwsOptions));
            
            Source = source;
        }

        public async Task<IDictionary<string, string>> GetDataAsync()
        {
            var request = new GetConfigurationRequest
            {
                Application = Source.ApplicationId,
                Environment = Source.EnvironmentId,
                Configuration = Source.ConfigProfileId,
                ClientId = Guid.NewGuid().ToString()
            };

            using (var client = Source.AwsOptions.CreateServiceClient<IAmazonAppConfig>())
            {
                if (client is AmazonAppConfigClient impl)
                {
                    impl.BeforeRequestEvent += ServiceClientAppender.ServiceClientBeforeRequestEvent;
                }

                var response = await client.GetConfigurationAsync(request).ConfigureAwait(false);

                return JsonConfigurationParser.Parse(response.Content);
            }
        }
    }
}
