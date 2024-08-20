![.NET on AWS Banner](./logo.png ".NET on AWS")

# AWS .NET Configuration Extension for Systems Manager

[![nuget](https://img.shields.io/nuget/v/Amazon.Extensions.Configuration.SystemsManager.svg)](https://www.nuget.org/packages/Amazon.Extensions.Configuration.SystemsManager/)

[Amazon.Extensions.Configuration.SystemsManager](https://www.nuget.org/packages/Amazon.Extensions.Configuration.SystemsManager/) simplifies using [AWS SSM's](https://aws.amazon.com/systems-manager) [Parameter Store](https://docs.aws.amazon.com/systems-manager/latest/userguide/systems-manager-paramstore.html) and [AppConfig](https://docs.aws.amazon.com/appconfig/latest/userguide/what-is-appconfig.html) as a source for configuration information for .NET Core applications. This project was contributed by [@KenHundley](https://github.com/KenHundley) and [@MichalGorski](https://github.com/mgorski-mg).

The library introduces the following dependencies:

* [AWSSDK.Extensions.NETCore.Setup](https://www.nuget.org/packages/AWSSDK.Extensions.NETCore.Setup/)
* [AWSSDK.SimpleSystemsManagement](https://www.nuget.org/packages/AWSSDK.SimpleSystemsManagement/)
* [AWSSDK.AppConfig](https://www.nuget.org/packages/AWSSDK.AppConfig/)
* [Microsoft.Extensions.Configuration](https://www.nuget.org/packages/Microsoft.Extensions.Configuration)

# Getting Started

Follow the examples below to see how the library can be integrated into your application. This extension adheres to the same practices and conventions of [Configuration in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.1).

## ASP.NET Core Example

One of the common use cases for this library is to pull configuration from Parameter Store. You can easily add this functionality by adding 1 line of code.

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddSystemsManager("/my-application/");
            })
            .UseStartup<Startup>();
}
```
It is also possible to load AWS Secrets Manager secrets from Parameter Store parameters. When retrieving a Secrets Manager secret from Parameter Store, the name must begin with the following reserved path: /aws/reference/secretsmanager/`{Secret-Id}`. Below example demonstrates this use case:
```csharp
public class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddSystemsManager("/aws/reference/secretsmanager/SomeSecret");
            })
            .UseStartup<Startup>();
}
```
For loading secrets, the library will use `JsonParameterProcessor` to load Key/Value pairs stored in the secret. These Key/Value pairs could be retrieved from the `ConfigurationManager` object. For more details, kindly refer [Referencing AWS Secrets Manager secrets from Parameter Store parameters](https://docs.aws.amazon.com/systems-manager/latest/userguide/integration-ps-secretsmanager.html).

Another possibility is to pull configuration from AppConfig. You can easily add this functionality by adding 1 line of code.

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddAppConfig("AppConfigApplicationId", "AppConfigEnvironmentId", "AppConfigConfigurationProfileId", TimeSpan.FromSeconds(20));
            })
            .UseStartup<Startup>();
}
```

## HostBuilder Example

Microsoft introduced [.NET Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-2.1) to de-couple HTTP pipeline from the Web Host API. The Generic Host library allows you to write non-HTTP services using configuration, dependency injection, and logging features. The sample code below shows you how to use the AWS .NET Configuration Extension library:

```csharp
namespace HostBuilderExample
{
    public static async Task Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddSystemsManager("/my-application/");
                config.AddAppConfig("AppConfigApplicationId", "AppConfigEnvironmentId", "AppConfigConfigurationProfileId", TimeSpan.FromSeconds(20));
            })
            .ConfigureServices((sc) => { ... })
            .Build();

        await host.RunAsync();
    }
}
```

## AWS Lambda Example

For improved performance with AppConfig and Lambda it is recommended to use the `AddAppConfigUsingLambdaExtension` method and deploy the Lambda function with the AWS AppConfig Lambda extension. More information including the AppConfig Lambda extension layer arn can be found in the [AWS AppConfig user guide](https://docs.aws.amazon.com/appconfig/latest/userguide/appconfig-integration-lambda-extensions.html).


```csharp
var configurations = new ConfigurationBuilder()
                        .AddSystemsManager("/my-application/")
                        .AddAppConfigUsingLambdaExtension("AppConfigApplicationId", "AppConfigEnvironmentId", "AppConfigConfigurationProfileId")
                        .Build();
```

# Config reloading

The `reloadAfter` parameter on `AddSystemsManager()` and `AddAppConfig()` enables automatic reloading of configuration data from Parameter Store or AppConfig as a background task. When using `AddAppConfigUsingLambdaExtension` reload is automatically configured.

## Config reloading in AWS Lambda

In AWS Lambda, background tasks are paused after processing the AWS Lambda event. This could disrupt the provider from retrieving the latest configuration data from Parameter Store or AWS AppConfig. To ensure the reload is performed within the AWS Lambda event, we recommend calling the extension
method `WaitForSystemsManagerReloadToComplete` from the `IConfiguration` object in the beginning of your AWS Lambda function handler. This method will immediately return unless a reload is currently being performed.

Remember to build `IConfiguration` in the AWS Lambda constructor! It is the only way to cache the configuration and reload it using `reloadAfter` parameter.

```csharp
public class SampleLambda
{
    private readonly IConfiguration _configurations;
    
    public SampleLambda()
    {
        _configurations = new ConfigurationBuilder()
                            .AddSystemsManager("/my-application/")
                            .AddAppConfigForLambda("AppConfigApplicationId", "AppConfigEnvironmentId", "AppConfigConfigurationProfileId", TimeSpan.FromSeconds(20))
                            .Build();
    }

    protected void Invoke()
    {
        _configurations.WaitForSystemsManagerReloadToComplete(TimeSpan.FromSeconds(2));
    }
}
```

# Hierarchical configuration data
Let's assume we want to load configuration per the below class hierarchy:
```csharp
public class DemoConfig
{
    public string TestItem { get; set; }
    public DemoSubConfig SubConfig { get; set; }
}

public class DemoSubConfig
{
    public string SubItem { get; set; }
}
```
In System Manager parameter store, these hierarchical values could be represented with below names (notice the use of `/` delimiter):
| Name    | Type |
| :-------- | :------- |
| /my-application/Config/TestItem  | String    |
| /my-application/Config/SubConfig/SubItem | String     |

Using `WebApplicationBuilder` as an example, the above configuration hierarchy could be loaded using below code:
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddSystemsManager($"/my-application/");

builder.Services.Configure<DemoConfig>(builder.Configuration.GetSection("Config"));
```

## Samples

### Custom ParameterProcessor Sample

Example of using a custom `IParameterProcessor` which provides a way to store and retrieve `null` values. Since AWS Parameter Store params are string literals, there is no way to store a `null` value by default.

```csharp
namespace CustomParameterProcessorExample
{
    public class CustomParameterProcessor : DefaultParameterProcessor
    {
        const string NULL_STRING_LITERAL = "NULL";
        
        public override string GetValue(Parameter parameter, string path)
        {
            string value = base.GetValue(parameter, path);
            return value == NULL_STRING_LITERAL ? null : value;
        }
    }
    
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddSystemsManager(config => {
                        config.Path = "/my-application/";
                        config.ParameterProcessor = new CustomParameterProcessor();
                    });
                })
                .UseStartup<Startup>();
    }
}
```

For more complete examples, take a look at sample projects available in [samples directory](https://github.com/aws/aws-dotnet-extensions-configuration/tree/master/samples).

# Configuring Systems Manager Client

This extension is using [AWSSDK.Extensions.NETCore.Setup](https://www.nuget.org/packages/AWSSDK.Extensions.NETCore.Setup/) to get AWSOptions from `Configuration` object and create AWS Systems Manager Client. You can edit and override the configuration by adding AWSOptions to your configuration providers such as appsettings.Development.json. Below is an example of a configuration provider:

```JSON
{
  ...

  "AWS": {
    "Profile": "default",
    "Region": "us-east-1",
    "ResignRetries": true
  }

  ...
}
```

For more information and other configurable options please refer to [Configuring the AWS SDK for .NET with .NET Core](https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/net-dg-config-netcore.html).

# Permissions
## Parameter Store
The AWS credentials used must have access to the `ssm:GetParametersByPath` service operation from AWS System Manager. Below is an example IAM policy for this action.
```JSON
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Sid": "SSMPermissionStatement",
            "Effect": "Allow",
            "Action": "ssm:GetParametersByPath",
            "Resource": "arn:aws:ssm:${Region}:${Account}:parameter/${ParameterNamePrefix}*"
        }
    ]
}
```
The above policy gives user access to get and use parameters which begin with the specified prefix.

For more details, refer [Restricting access to Systems Manager parameters using IAM policies](https://docs.aws.amazon.com/systems-manager/latest/userguide/sysman-paramstore-access.html).

Additionally, for referencing secrets from AWS Secrets Manager from Paramater Store parameters, AWS credentials used must have permissions to access the secret.

## AppConfig
If the application reads configuration values from AWS Systems Manager AppConfig, the AWS credentials used must have access to `appconfig:StartConfigurationSession` and `appconfig:GetLatestConfiguration` service operations. Below is an example IAM policy for this action.
```JSON
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "appconfig:StartConfigurationSession",
        "appconfig:GetLatestConfiguration",
      ],
      "Resource": "arn:${Partition}:appconfig:${Region}:${Account}:application/${ApplicationId}/environment/${EnvironmentId}/configuration/${ConfigurationProfileId}"
    }
  ]
}
```
For more details, refer [Configuring permissions for AWS AppConfig](https://docs.aws.amazon.com/appconfig/latest/userguide/getting-started-with-appconfig-permissions.html) and [Actions, resources, and condition keys for AWS AppConfig](https://docs.aws.amazon.com/service-authorization/latest/reference/list_awsappconfig.html#awsappconfig-GetConfiguration).

# Getting Help

We use the [GitHub issues](https://github.com/aws/aws-dotnet-extensions-configuration/issues) for tracking bugs and feature requests and have limited bandwidth to address them.

If you think you may have found a bug, please open an [issue](https://github.com/aws/aws-dotnet-extensions-configuration/issues/new).

# Contributing

We welcome community contributions and pull requests. See
[CONTRIBUTING.md](./CONTRIBUTING.md) for information on how to set up a development environment and submit code.

# Additional Resources

[AWS .NET GitHub Home Page](https://github.com/aws/dotnet)  
GitHub home for .NET development on AWS. You'll find libraries, tools, and resources to help you build .NET applications and services on AWS.

[AWS Developer Center - Explore .NET on AWS](https://aws.amazon.com/developer/language/net/)  
Find all the .NET code samples, step-by-step guides, videos, blog content, tools, and information about live events that you need in one place.

[AWS Developer Blog - .NET](https://aws.amazon.com/blogs/developer/category/programing-language/dot-net/)  
Come see what .NET developers at AWS are up to!  Learn about new .NET software announcements, guides, and how-to's.

[@dotnetonaws](https://twitter.com/dotnetonaws)
Follow us on twitter!

# License

Libraries in this repository are licensed under the Apache 2.0 License.

See [LICENSE](./LICENSE) and [NOTICE](./NOTICE) for more information.
