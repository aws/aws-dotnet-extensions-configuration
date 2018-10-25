![.NET on AWS Banner](./logo.png ".NET on AWS")

# AWS .NET Configuration Extension for Systems Manager
[![AWSSDK.Extensions.Configuration.SystemsManager](https://img.shields.io/nuget/v/AWSSDK.Extensions.Configuration.SystemsManager.svg)](https://www.nuget.org/packages/AWSSDK.Extensions.Configuration.SystemsManager)

<strong>AWSSDK.Extensions.Configuration.SystemsManager</strong> is a library that simplifies using [AWS SSM](https://aws.amazon.com/systems-manager/)'s [Parameter Store](https://docs.aws.amazon.com/systems-manager/latest/userguide/systems-manager-paramstore.html) as a source for configuration information for .NET Core applications.  This project was contributed by [@KenHundley](https://github.com/KenHundley).

The library introduces the following dependencies:

* [AWSSDK.Extensions.NETCore.Setup](https://www.nuget.org/packages/AWSSDK.Extensions.NETCore.Setup/)
* [AWSSDK.SimpleSystemsManagement](https://www.nuget.org/packages/AWSSDK.SimpleSystemsManagement/)
* [Microsoft.Extensions.Configuration](https://www.nuget.org/packages/Microsoft.Extensions.Configuration)

# Getting Started

Follow the examples below to see how the library can be integrated into your application.  This extension adheres to the same practices and conventions of [Configuration in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.1).

## ASP.NET Core Example
The most common use case for this library is for your ASP.NETCore application to pull configuration from Parameter Store.  You can easily add this functionality by adding 1 line of code:

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
            })
            .ConfigureServices((sc) => { ... })
            .Build();

        await host.RunAsync();
    }
}
```

# Getting Help

Please use these community resources for getting help. We use the [GitHub issues](https://github.com/aws/aws-dotnet-extensions-configuration/issues) for tracking bugs and feature requests and have limited bandwidth to address them.

* Ask a question on [StackOverflow](http://stackoverflow.com/) and tag it with `aws` and `.net`
* Come join the AWS .NET community chat on [gitter](https://gitter.im/aws/aws-sdk-net)
* Open a support ticket with [AWS Support](https://console.aws.amazon.com/support/home)
* If it turns out that you may have found a bug, please open an [issue](https://github.com/aws/aws-dotnet-extensions-configuration/issues/new)

# Contributing

We welcome community contributions and pull requests. See
[CONTRIBUTING](./CONTRIBUTING.md) for information on how to set up a development
environment and submit code.

# Additional Resources

[AWS Developer Center - Explore .NET on AWS](https://aws.amazon.com/developer/language/net/)  
Find all the .NET code samples, step-by-step guides, videos, blog content, tools, and information about live events that you need in one place. 

[AWS Developer Blog - .NET](https://aws.amazon.com/blogs/developer/category/programing-language/dot-net/)  
Come see what .NET developers at AWS are up to!  Learn about new .NET software announcements, guides, and how-to's.

[@awsfornet](https://twitter.com/awsfornet)  
Follow us on twitter!

# License

Libraries in this repository are licensed under the Apache 2.0 License. 

See [LICENSE](./LICENSE) and [NOTICE](./NOTICE) for more information.