![.NET on AWS Banner](./logo.png ".NET on AWS")

# AWS .NET Configuration Extension for Systems Manager
[![nuget](https://img.shields.io/nuget/v/Amazon.Extensions.Configuration.SystemsManager.svg)](https://www.nuget.org/packages/Amazon.Extensions.Configuration.SystemsManager/)

[Amazon.Extensions.Configuration.SystemsManager](https://www.nuget.org/packages/Amazon.Extensions.Configuration.SystemsManager/) simplifies using [AWS SSM](https://aws.amazon.com/systems-manager/)'s [Parameter Store](https://docs.aws.amazon.com/systems-manager/latest/userguide/systems-manager-paramstore.html) and [AppConfig](https://docs.aws.amazon.com/appconfig/latest/userguide/what-is-appconfig.html) as a source for configuration information for .NET Core applications.  This project was contributed by [@KenHundley](https://github.com/KenHundley) and [@MichalGorski](https://github.com/mgorski-mg).

The library introduces the following dependencies:

* [AWSSDK.Extensions.NETCore.Setup](https://www.nuget.org/packages/AWSSDK.Extensions.NETCore.Setup/)
* [AWSSDK.SimpleSystemsManagement](https://www.nuget.org/packages/AWSSDK.SimpleSystemsManagement/)
* [AWSSDK.AppConfig](https://www.nuget.org/packages/AWSSDK.AppConfig/)
* [Microsoft.Extensions.Configuration](https://www.nuget.org/packages/Microsoft.Extensions.Configuration)

# Getting Started

Follow the examples below to see how the library can be integrated into your application.  This extension adheres to the same practices and conventions of [Configuration in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.1).

## ASP.NET Core Example
One of the common use cases for this library is to pull configuration from Parameter Store.  You can easily add this functionality by adding 1 line of code:

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

Second possibility is to pull configuration from AppConfig.  You can easily add this functionality by adding 1 line of code:

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
                builder.AddAppConfig("AppConfigApplicationId", "AppConfigEnvironmentId", "AppConfigConfigurationProfileId", , TimeSpan.FromSeconds(20));
            })
            .UseStartup<Startup>();
}
```

## HostBuilder Example
Microsoft introduced [.NET Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-2.1) to de-couple HTTP pipeline from the Web Host API.  The Generic Host library allows you to write non-HTTP services using configuration, dependency injection, and logging features.  The sample code below shows you how to use the the AWS .NET Configuration Extension library:

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

## Samples

### Custom ParameterProcessor Sample
Example of using a custom `IParameterProcessor` which provides a way to store and retreive `null` values. Since AWS Parameter Store params are string literals, there is no way to store a `null` value by default.

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

# Reloading in AWS Lambda

The `reloadAfter` parameter on `AddSystemsManager()` and `AddAppConfig()` enables automatic reloading of configuration data from Parameter Store or AppConfig as a background task.

## Reloading in AWS Lambda

In AWS Lambda, background tasks are paused after processing a Lambda event. This could disrupt the provider from retrieving the latest configuration data from Systems Manager. To ensure the reload is performed within a Lambda event, we recommend calling the extension method `WaitForSystemsManagerReloadToComplete` from the `IConfiguration` object in your Lambda function. This method will immediately return unless a reload is currently being performed. See the example below:

```csharp
using Amazon.Extensions.Configuration.SystemsManager

...

var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.AddSystemsManager(IntegTestFixture.ParameterPrefix, fixture.AWSOptions);
var configurations = configurationBuilder.Build();

...

configurations.WaitForSystemsManagerReloadToComplete(TimeSpan.FromSeconds(5));
```

For AppConfig in Lambda you should use [Lambda Extension](https://docs.aws.amazon.com/appconfig/latest/userguide/appconfig-integration-lambda-extensions.html).

# Configuring Systems Manager Client

This extension is using [AWSSDK.Extensions.NETCore.Setup](https://www.nuget.org/packages/AWSSDK.Extensions.NETCore.Setup/) in order to get AWSOptions from `Configuration` object and create AWS Systems Manager Client. You can edit and override the configuration by adding AWSOptions to your configuration providers such as appsettings.Development.json. Below is an example of a configuration provider:

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
The AWS credentials used must have access to the `ssm:GetParameters` service operation from AWS System Manager. Below is an example IAM policy for this action.
```JSON
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Sid": "SSMPermissionStatement",
            "Effect": "Allow",
            "Action": "ssm:GetParameters",
            "Resource": "arn:aws:ssm:${Region}:${Account}:parameter/${ParameterNamePrefix}*"
        }
    ]
}
```
The above policy gives user access to get and use parameters which begin with the specified prefix.

For more details, refer [Restricting access to Systems Manager parameters using IAM policies](https://docs.aws.amazon.com/systems-manager/latest/userguide/sysman-paramstore-access.html).

## AppConfig
If the application reads configuration values from AWS Systems Manager AppConfig, the AWS credentials used must have access to `appconfig:GetConfiguration` service operation. Below is an example IAM policy for this action.
```JSON
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "appconfig:GetConfiguration"
      ],
      "Resource": [
          "arn:${Partition}:appconfig:${Region}:${Account}:application/${ApplicationId}",
          "arn:${Partition}:appconfig:${Region}:${Account}:application/${ApplicationId}/environment/${EnvironmentId}",
          "arn:${Partition}:appconfig:${Region}:${Account}:application/${ApplicationId}/configurationprofile/${ConfigurationProfileId}"
      ]
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
[CONTRIBUTING.md](./CONTRIBUTING.md) for information on how to set up a development
environment and submit code.

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
