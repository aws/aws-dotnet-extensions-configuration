using Amazon;
using Amazon.AppConfig;
using Amazon.AppConfig.Model;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using System.Text.Json;

namespace AppConfigParameterStore;

public class SetupResources
{
    private readonly IAmazonSimpleSystemsManagement _ssmClient;
    private readonly IAmazonAppConfig _appConfigClient;
    private readonly IAmazonIdentityManagementService _iamClient;
    private readonly RegionEndpoint _region;
    private readonly string _parameterPrefix;
    private readonly string _resourcePrefix;

    public SetupResources(RegionEndpoint region)
    {
        _region = region;
        _ssmClient = new AmazonSimpleSystemsManagementClient(region);
        _appConfigClient = new AmazonAppConfigClient(region);
        _iamClient = new AmazonIdentityManagementServiceClient(region);
        
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        _parameterPrefix = $"/appconfig-test-{timestamp}";
        _resourcePrefix = $"AppConfigParameterStoreTest-{timestamp}";
    }

    public async Task<AppConfigResources> SetupAsync()
    {
        Console.WriteLine("🚀 Setting up AWS resources for AppConfig + Parameter Store test...\n");

        // 1. Create SSM Parameters
        Console.WriteLine("📝 Creating SSM Parameters...");
        await CreateParametersAsync();

        // 2. Create IAM Role
        Console.WriteLine("🔐 Creating IAM Role for AppConfig...");
        var roleArn = await CreateIamRoleAsync();

        // 3. Create AppConfig Resources
        Console.WriteLine("⚙️  Creating AppConfig resources...");
        var appConfigResources = await CreateAppConfigResourcesAsync(roleArn);

        Console.WriteLine("✅ All AWS resources created successfully!\n");
        Console.WriteLine($"📍 Parameter Prefix: {_parameterPrefix}");
        Console.WriteLine($"🏷️  Application ID: {appConfigResources.ApplicationId}");
        Console.WriteLine($"🌍 Environment ID: {appConfigResources.EnvironmentId}");
        Console.WriteLine($"📋 Profile ID: {appConfigResources.ConfigProfileId}");
        Console.WriteLine($"🔑 IAM Role: {roleArn}\n");

        return appConfigResources;
    }

    private async Task CreateParametersAsync()
    {
        // Create a single parameter with all configuration as JSON
        var configurationData = new
        {
            StringValue = "parameter-store-string-value",
            IntegerValue = 42,
            DateTimeValue = "2024-07-28T16:30:00Z",
            BooleanValue = true,
            TimeSpanValue = "00:15:30",
            
            // Nested configuration - Database
            Database = new
            {
                ConnectionString = "server=prod-db.example.com;database=appconfig_test;integrated security=true",
                TimeoutSeconds = 120,
                EnableRetries = true
            },
            
            // Nested configuration - API
            Api = new
            {
                BaseUrl = "https://api.example.com/v1",
                TimeoutSeconds = 30,
                ApiKey = "test-api-key-12345"
            },
            
            // Nested configuration - Features
            Features = new
            {
                EnableLogging = true,
                EnableMetrics = false,
                LogLevel = "Debug"
            }
        };

        var jsonConfiguration = JsonSerializer.Serialize(configurationData, new JsonSerializerOptions { WriteIndented = true });

        await _ssmClient.PutParameterAsync(new PutParameterRequest
        {
            Name = _parameterPrefix,
            Value = jsonConfiguration,
            Type = ParameterType.String,
            Description = "AppConfig + Parameter Store test configuration (single JSON parameter)",
            Overwrite = true
        });
        
        Console.WriteLine($"  ✓ {_parameterPrefix}");
        Console.WriteLine($"     Content: {jsonConfiguration.Substring(0, Math.Min(100, jsonConfiguration.Length))}...");
    }

    private async Task<string> CreateIamRoleAsync()
    {
        var roleName = $"{_resourcePrefix}-Role";
        
        // Trust policy allowing AppConfig to assume the role
        var trustPolicy = JsonSerializer.Serialize(new
        {
            Version = "2012-10-17",
            Statement = new[]
            {
                new
                {
                    Effect = "Allow",
                    Principal = new { Service = "appconfig.amazonaws.com" },
                    Action = "sts:AssumeRole"
                }
            }
        });

        try
        {
            // Create the role
            var createRoleResponse = await _iamClient.CreateRoleAsync(new CreateRoleRequest
            {
                RoleName = roleName,
                AssumeRolePolicyDocument = trustPolicy,
                Description = "Role for AppConfig to access Parameter Store"
            });

            // Create inline policy for SSM access
            var ssmPolicy = JsonSerializer.Serialize(new
            {
                Version = "2012-10-17",
                Statement = new[]
                {
                    new
                    {
                        Effect = "Allow",
                        Action = new[] { "ssm:GetParametersByPath", "ssm:GetParameter", "ssm:GetParameters" },
                        Resource = $"arn:aws:ssm:{_region.SystemName}:*:parameter{_parameterPrefix}*"
                    }
                }
            });

            await _iamClient.PutRolePolicyAsync(new PutRolePolicyRequest
            {
                RoleName = roleName,
                PolicyName = "SSMParameterAccess",
                PolicyDocument = ssmPolicy
            });

            // Wait a moment for IAM role to propagate
            await Task.Delay(10000);
            
            Console.WriteLine($"  ✓ Role: {roleName}");
            return createRoleResponse.Role.Arn;
        }
        catch (EntityAlreadyExistsException)
        {
            // Role already exists, get its ARN
            var getRoleResponse = await _iamClient.GetRoleAsync(new GetRoleRequest { RoleName = roleName });
            Console.WriteLine($"  ✓ Role (existing): {roleName}");
            return getRoleResponse.Role.Arn;
        }
    }

    private async Task<AppConfigResources> CreateAppConfigResourcesAsync(string roleArn)
    {
        // Create Application
        var createAppResponse = await _appConfigClient.CreateApplicationAsync(new CreateApplicationRequest
        {
            Name = _resourcePrefix,
            Description = "Test application for AppConfig + Parameter Store integration"
        });
        Console.WriteLine($"  ✓ Application: {_resourcePrefix}");

        // Create Environment
        var createEnvResponse = await _appConfigClient.CreateEnvironmentAsync(new CreateEnvironmentRequest
        {
            ApplicationId = createAppResponse.Id,
            Name = "Test",
            Description = "Test environment"
        });
        Console.WriteLine($"  ✓ Environment: Test");

        // Create Configuration Profile pointing to Parameter Store
        var createProfileResponse = await _appConfigClient.CreateConfigurationProfileAsync(new CreateConfigurationProfileRequest
        {
            ApplicationId = createAppResponse.Id,
            Name = "ParameterStoreProfile",
            Description = "Configuration profile using Parameter Store as source",
            LocationUri = $"ssm-parameter://{_parameterPrefix}",
            Type = "AWS.Freeform",
            RetrievalRoleArn = roleArn
        });
        Console.WriteLine($"  ✓ Configuration Profile: ParameterStoreProfile");

        // Create immediate deployment strategy
        var deploymentStrategyId = await CreateImmediateDeploymentStrategyAsync();

        // Start deployment with immediate strategy
        var startDeploymentResponse = await _appConfigClient.StartDeploymentAsync(new StartDeploymentRequest
        {
            ApplicationId = createAppResponse.Id,
            EnvironmentId = createEnvResponse.Id,
            ConfigurationProfileId = createProfileResponse.Id,
            ConfigurationVersion = "1",
            DeploymentStrategyId = deploymentStrategyId,
            Description = "Initial deployment of Parameter Store configuration"
        });
        Console.WriteLine($"  ✓ Deployment started: {startDeploymentResponse.DeploymentNumber}");

        // Wait for deployment to complete
        await WaitForDeploymentAsync(createAppResponse.Id, createEnvResponse.Id);

        return new AppConfigResources
        {
            ApplicationId = createAppResponse.Id,
            EnvironmentId = createEnvResponse.Id,
            ConfigProfileId = createProfileResponse.Id,
            ParameterPrefix = _parameterPrefix,
            RoleArn = roleArn,
            ResourcePrefix = _resourcePrefix
        };
    }

    private async Task<string> CreateImmediateDeploymentStrategyAsync()
    {
        var strategyName = $"{_resourcePrefix}-ImmediateStrategy";
        
        try
        {
            var createStrategyResponse = await _appConfigClient.CreateDeploymentStrategyAsync(new CreateDeploymentStrategyRequest
            {
                Name = strategyName,
                Description = "Immediate deployment strategy with 0 bake time",
                DeploymentDurationInMinutes = 0, // Deploy immediately
                FinalBakeTimeInMinutes = 0, // No bake time
                GrowthFactor = 100, // Deploy to 100% of targets immediately
                ReplicateTo = ReplicateTo.NONE // Don't replicate to other regions
            });

            Console.WriteLine($"  ✓ Created immediate deployment strategy: {strategyName}");
            return createStrategyResponse.Id;
        }
        catch (ConflictException)
        {
            // Strategy already exists, find it
            var listStrategiesResponse = await _appConfigClient.ListDeploymentStrategiesAsync(new ListDeploymentStrategiesRequest());
            var existingStrategy = listStrategiesResponse.Items.FirstOrDefault(s => s.Name == strategyName);
            
            if (existingStrategy != null)
            {
                Console.WriteLine($"  ✓ Using existing deployment strategy: {strategyName}");
                return existingStrategy.Id;
            }
            
            throw; // Re-throw if we can't find the existing strategy
        }
    }

    private async Task WaitForDeploymentAsync(string applicationId, string environmentId)
    {
        Console.WriteLine("  ⏳ Waiting for deployment to complete...");
        
        for (int i = 0; i < 30; i++) // Wait up to 5 minutes
        {
            var environment = await _appConfigClient.GetEnvironmentAsync(new GetEnvironmentRequest
            {
                ApplicationId = applicationId,
                EnvironmentId = environmentId
            });


            if (environment.State == "ReadyForDeployment")
            {
                Console.WriteLine("  ✅ Deployment completed successfully");
                return;
            }

            if (environment.State == EnvironmentState.ROLLED_BACK)
            {
                throw new InvalidOperationException("Deployment failed and was rolled back");
            }

            await Task.Delay(10000); // Wait 10 seconds
        }

        throw new TimeoutException("Deployment did not complete within the expected time");
    }

    public async Task CleanupAsync(AppConfigResources resources)
    {
        Console.WriteLine("\n🧹 Cleaning up AWS resources...");

        try
        {
            // Delete AppConfig resources
            await _appConfigClient.DeleteEnvironmentAsync(new DeleteEnvironmentRequest
            {
                ApplicationId = resources.ApplicationId,
                EnvironmentId = resources.EnvironmentId
            });
            Console.WriteLine("  ✓ Deleted Environment");

            await _appConfigClient.DeleteConfigurationProfileAsync(new DeleteConfigurationProfileRequest
            {
                ApplicationId = resources.ApplicationId,
                ConfigurationProfileId = resources.ConfigProfileId
            });
            Console.WriteLine("  ✓ Deleted Configuration Profile");

            await _appConfigClient.DeleteApplicationAsync(new DeleteApplicationRequest
            {
                ApplicationId = resources.ApplicationId
            });
            Console.WriteLine("  ✓ Deleted Application");

            // Delete deployment strategy
            var strategyName = $"{resources.ResourcePrefix}-ImmediateStrategy";
            var listStrategiesResponse = await _appConfigClient.ListDeploymentStrategiesAsync(new ListDeploymentStrategiesRequest());
            var strategy = listStrategiesResponse.Items.FirstOrDefault(s => s.Name == strategyName);
            
            if (strategy != null)
            {
                await _appConfigClient.DeleteDeploymentStrategyAsync(new DeleteDeploymentStrategyRequest
                {
                    DeploymentStrategyId = strategy.Id
                });
                Console.WriteLine("  ✓ Deleted Deployment Strategy");
            }

            // Delete IAM role
            var roleName = $"{resources.ResourcePrefix}-Role";
            
            await _iamClient.DeleteRolePolicyAsync(new DeleteRolePolicyRequest
            {
                RoleName = roleName,
                PolicyName = "SSMParameterAccess"
            });

            await _iamClient.DeleteRoleAsync(new DeleteRoleRequest
            {
                RoleName = roleName
            });
            Console.WriteLine("  ✓ Deleted IAM Role");

            // Delete SSM parameter
            await _ssmClient.DeleteParameterAsync(new DeleteParameterRequest
            {
                Name = resources.ParameterPrefix
            });
            Console.WriteLine($"  ✓ Deleted SSM Parameter: {resources.ParameterPrefix}");

            Console.WriteLine("✅ Cleanup completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Cleanup failed: {ex.Message}");
        }
    }
}

public class AppConfigResources
{
    public string ApplicationId { get; set; } = string.Empty;
    public string EnvironmentId { get; set; } = string.Empty;
    public string ConfigProfileId { get; set; } = string.Empty;
    public string ParameterPrefix { get; set; } = string.Empty;
    public string RoleArn { get; set; } = string.Empty;
    public string ResourcePrefix { get; set; } = string.Empty;
}
