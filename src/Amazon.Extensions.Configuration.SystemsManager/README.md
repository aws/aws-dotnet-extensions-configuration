# AWS .NET Configuration Extension for Systems Manager

[![nuget](https://img.shields.io/nuget/v/Amazon.Extensions.Configuration.SystemsManager.svg)](https://www.nuget.org/packages/Amazon.Extensions.Configuration.SystemsManager/)

[Amazon.Extensions.Configuration.SystemsManager](https://www.nuget.org/packages/Amazon.Extensions.Configuration.SystemsManager/) simplifies using [AWS SSM's](https://aws.amazon.com/systems-manager) [Parameter Store](https://docs.aws.amazon.com/systems-manager/latest/userguide/systems-manager-paramstore.html) and [AppConfig](https://docs.aws.amazon.com/appconfig/latest/userguide/what-is-appconfig.html) as a source for configuration information for .NET applications.

The library introduces the following dependencies:

* [AWSSDK.Extensions.NETCore.Setup](https://www.nuget.org/packages/AWSSDK.Extensions.NETCore.Setup/)
* [AWSSDK.SimpleSystemsManagement](https://www.nuget.org/packages/AWSSDK.SimpleSystemsManagement/)
* [AWSSDK.AppConfig](https://www.nuget.org/packages/AWSSDK.AppConfig/)
* [Microsoft.Extensions.Configuration](https://www.nuget.org/packages/Microsoft.Extensions.Configuration)

## Getting Started

This extension adheres to the same practices and conventions of [Configuration in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/). One of the common use cases is to pull configuration from Parameter Store, which you can add with a single line of code:

```csharp
public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(builder =>
        {
            builder.AddSystemsManager("/my-application/");
        })
        .UseStartup<Startup>();
```

For full documentation, samples, and advanced usage (AppConfig, reloading, custom parameter processors, and more), see the [project README on GitHub](https://github.com/aws/aws-dotnet-extensions-configuration/).

## License

This project is licensed under the Apache-2.0 License.
