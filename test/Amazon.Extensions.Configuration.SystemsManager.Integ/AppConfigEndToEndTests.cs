using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Amazon;
using Amazon.AppConfig;
using Amazon.AppConfig.Model;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Amazon.Extensions.Configuration.SystemsManager.Integ
{
    public class AppConfigEndToEndTests
    {
        IAmazonAppConfig _appConfigClient = new AmazonAppConfigClient(RegionEndpoint.USWest2);

        [Fact]
        public async Task RefreshConfiguration()
        {
            var configSettings = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };
            (string applicationId, string environmentId, string configProfileId) = await CreateAppConfigResourcesAsync("RefreshWebAppTest", configSettings);
            try
            {
                var builder = new ConfigurationBuilder()
                                .AddAppConfig(applicationId, environmentId, configProfileId, new AWSOptions {Region = RegionEndpoint.USWest2 }, TimeSpan.FromSeconds(5));

                var configuration = builder.Build();

                Assert.Equal("value1", configuration["key1"]);

                const string newValue = "newValue1";
                configSettings["key1"] = newValue;
                var versionNumber = await CreateNewHostedConfig(applicationId, configProfileId, configSettings);
                await PerformDeploymentAsync(applicationId, environmentId, configProfileId, versionNumber);

                for(int i = 0; i < 10; i++)
                {
                    // Wait for ConfigProvider to perform the reload
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    var value = configuration["key1"];
                    if(string.Equals(newValue, value))
                    {
                        break;
                    }
                }

                Assert.Equal(newValue, configuration["key1"]);
            }
            finally
            {
                await CleanupAppConfigResourcesAsync(applicationId, environmentId, configProfileId);
            }
        }

        [Fact]
        public async Task JsonWithCharsetConfiguration()
        {
            var configSettings = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };
            (string applicationId, string environmentId, string configProfileId) = await CreateAppConfigResourcesAsync("JsonWithCharsetConfiguration", configSettings, "application/json; charset=utf-8");
            try
            {
                var builder = new ConfigurationBuilder()
                                .AddAppConfig(applicationId, environmentId, configProfileId, new AWSOptions { Region = RegionEndpoint.USWest2 }, TimeSpan.FromSeconds(5));

                var configuration = builder.Build();

                Assert.Equal("value1", configuration["key1"]);
            }
            finally
            {
                await CleanupAppConfigResourcesAsync(applicationId, environmentId, configProfileId);
            }
        }

        private async Task CleanupAppConfigResourcesAsync(string applicationId, string environmentId, string configProfileId)
        {
            await _appConfigClient.DeleteEnvironmentAsync(new DeleteEnvironmentRequest {ApplicationId = applicationId, EnvironmentId = environmentId });

            var listHostConfigResponse = await _appConfigClient.ListHostedConfigurationVersionsAsync(new ListHostedConfigurationVersionsRequest 
            {
                ApplicationId = applicationId, 
                ConfigurationProfileId = configProfileId 
            });

            foreach(var item in listHostConfigResponse.Items)
            {
                await _appConfigClient.DeleteHostedConfigurationVersionAsync(new DeleteHostedConfigurationVersionRequest
                {
                    ApplicationId = item.ApplicationId,
                    ConfigurationProfileId = item.ConfigurationProfileId,
                    VersionNumber = item.VersionNumber
                });
            }

            await _appConfigClient.DeleteConfigurationProfileAsync(new DeleteConfigurationProfileRequest
            {
                ApplicationId = applicationId,
                ConfigurationProfileId = configProfileId
            });

            await _appConfigClient.DeleteApplicationAsync(new DeleteApplicationRequest {ApplicationId = applicationId });
        }


        private async Task<(string applicationId, string environmentId, string configProfileId)> CreateAppConfigResourcesAsync(string seedName, IDictionary<string, string> configs, string contentType = "application/json")
        {
            var nameSuffix = DateTime.Now.Ticks;

            var createAppResponse = await _appConfigClient.CreateApplicationAsync(new CreateApplicationRequest { Name = seedName + "-" + nameSuffix });

            var createConfigResponse = await _appConfigClient.CreateConfigurationProfileAsync(new CreateConfigurationProfileRequest
            {
                ApplicationId = createAppResponse.Id,
                Name = seedName + "-" + nameSuffix,
                LocationUri = "hosted"
            });

            var createEnvironmentResponse = await _appConfigClient.CreateEnvironmentAsync(new CreateEnvironmentRequest
            {
                ApplicationId = createAppResponse.Id,
                Name = seedName + "-" + nameSuffix
            });

            var versionNumber = await CreateNewHostedConfig(createAppResponse.Id, createConfigResponse.Id, configs, contentType);
            await PerformDeploymentAsync(createAppResponse.Id, createEnvironmentResponse.Id, createConfigResponse.Id, versionNumber);

            return (createAppResponse.Id, createEnvironmentResponse.Id, createConfigResponse.Id);
        }

        private async Task<string> CreateNewHostedConfig(string applicationId, string configProfileId, IDictionary<string, string> configs, string contentType = "application/json")
        {
            var json = JsonSerializer.Serialize(configs);

            var createHostedresponse = await _appConfigClient.CreateHostedConfigurationVersionAsync(new CreateHostedConfigurationVersionRequest
            {
                ApplicationId = applicationId,
                ConfigurationProfileId = configProfileId,
                ContentType = contentType,
                Content = new MemoryStream(UTF8Encoding.UTF8.GetBytes(json))
            });

            return createHostedresponse.VersionNumber.ToString();
        }

        private async Task PerformDeploymentAsync(string applicationId, string environmentId, string configProfileId, string configVersionNumber, bool waitForDeployment = true)
        {
            var deploymentStrategyId = await GetDeploymentStrategyId();
            var deploymentResponse = await _appConfigClient.StartDeploymentAsync(new StartDeploymentRequest
            {
                ApplicationId = applicationId,
                EnvironmentId = environmentId,
                ConfigurationProfileId = configProfileId,
                ConfigurationVersion = configVersionNumber,
                DeploymentStrategyId = deploymentStrategyId
            });

            if(waitForDeployment)
            {
                await WaitForDeploymentAsync(applicationId, environmentId);
            }
        }

        private async Task WaitForDeploymentAsync(string applicationId, string environmentId)
        {
            var getRequest = new GetEnvironmentRequest {ApplicationId = applicationId, EnvironmentId = environmentId };
            GetEnvironmentResponse getResponse;
            do
            {
                await Task.Delay(2000);
                getResponse = await _appConfigClient.GetEnvironmentAsync(getRequest);
            } while (getResponse.State == EnvironmentState.DEPLOYING);
        }

        private async Task<string> GetDeploymentStrategyId()
        {
            const string integTestDeploymentStrategyName = "IntegTestFast";

            var paginator = _appConfigClient.Paginators.ListDeploymentStrategies(new ListDeploymentStrategiesRequest());
            await foreach(var response in paginator.Responses)
            {
                var strategy = response.Items.FirstOrDefault(x => string.Equals(x.Name, integTestDeploymentStrategyName));

                if (strategy != null)
                {
                    return strategy.Id;
                }
            }

            var createResponse = await _appConfigClient.CreateDeploymentStrategyAsync(new CreateDeploymentStrategyRequest
            {
                Name = integTestDeploymentStrategyName,
                DeploymentDurationInMinutes = 1,
                FinalBakeTimeInMinutes = 0,
                GrowthFactor = 100,
                ReplicateTo = ReplicateTo.NONE
            });

            return createResponse.Id;
        }
    }
}
