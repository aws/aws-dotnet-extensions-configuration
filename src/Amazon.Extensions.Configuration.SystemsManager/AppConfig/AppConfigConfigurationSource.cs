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
using System.Net.Http;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.Configuration;

namespace Amazon.Extensions.Configuration.SystemsManager.AppConfig
{
    /// <inheritdoc />
    /// <summary>
    /// Represents AWS Systems Manager AppConfig variables as an <see cref="ISystemsManagerConfigurationSource" />.
    /// </summary>
    public class AppConfigConfigurationSource : ISystemsManagerConfigurationSource
    {
        /// <summary>
        /// AppConfig Application Id.
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// AppConfig Environment Id.
        /// </summary>
        public string EnvironmentId { get; set; }

        /// <summary>
        /// AppConfig Configuration Profile Id.
        /// </summary>
        public string ConfigProfileId { get; set; }

        /// <summary>
        /// <see cref="AWSOptions"/> used to create an AWS Systems Manager Client />.
        /// </summary>
        public AWSOptions AwsOptions { get; set; }

        /// <inheritdoc />
        public bool Optional { get; set; }

        /// <inheritdoc />
        public TimeSpan? ReloadAfter { get; set; }

        /// <summary>
        /// Indicates to use configured lambda extension HTTP client to retrieve AppConfig data.
        /// </summary>
        internal bool UseLambdaExtension { get; set; }

        /// <summary>
        /// Used only when integrating with the AWS AppConfig Lambda extension.
        /// 
        /// This property allows customizing the HttpClient connecting to the AWS AppConfig Lambda extension. This is useful
        /// to instrument the HttpClient for telemetry. For example adding the Amazon.XRay.Recorder.Handlers.System.Net.HttpClientXRayTracingHandler handle for AWS X-Ray tracing.
        /// </summary>
        public HttpClient CustomHttpClientForLambdaExtension { get; set; }

        /// <inheritdoc />
        public Action<SystemsManagerExceptionContext> OnLoadException { get; set; }

        /// <inheritdoc />
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new SystemsManagerConfigurationProvider(this);
        }
    }
}
