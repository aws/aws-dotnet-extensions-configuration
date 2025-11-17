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

using System.Reflection;
using Amazon.Extensions.Configuration.SystemsManager.AppConfig;
using Amazon.Runtime;

namespace Amazon.Extensions.Configuration.SystemsManager.Internal
{
    public static class ServiceClientAppender
    {
        private static readonly string AssemblyVersion = typeof(AppConfigProcessor).GetTypeInfo().Assembly.GetName().Version.ToString();
        private static readonly string UserAgentSuffix = $"lib/SSMConfigProvider#{AssemblyVersion}";

        public static void ServiceClientBeforeRequestEvent(object sender, RequestEventArgs e)
        {
            WebServiceRequestEventArgs args = e as WebServiceRequestEventArgs;
            if (args != null && args.Request is Amazon.Runtime.Internal.IAmazonWebServiceRequest internalRequest &&
#if NET8_0_OR_GREATER
                !internalRequest.UserAgentDetails.GetCustomUserAgentComponents().Contains(UserAgentSuffix, System.StringComparison.InvariantCulture)
#else
                !internalRequest.UserAgentDetails.GetCustomUserAgentComponents().Contains(UserAgentSuffix)
#endif
                )
            {
                internalRequest.UserAgentDetails.AddUserAgentComponent(UserAgentSuffix);
            }
        }
    }
}
