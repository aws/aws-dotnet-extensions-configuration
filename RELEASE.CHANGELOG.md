### Release 2023-02-07
* **Amazon.Extensions.Configuration.SystemsManager (5.0.0)**
   * **Breaking Change:** Fixed issue when parsing JSON SSM parameter values to include the relative SSM parameter name to the JSON property names.

### Release 2023-02-01
* **Amazon.Extensions.Configuration.SystemsManager (4.0.1)**
   * Merged PR [#128](https://github.com/aws/aws-dotnet-extensions-configuration/pull/128) fixed issue parsing AppConfig response when services returns empty response for no changes. Thanks  [Tyler Ohlsen](https://github.com/tylerohlsen)

### Release 2022-06-28
* **Amazon.Extensions.Configuration.SystemsManager (4.0.0)**
   * Merged PR [#99](https://github.com/aws/aws-dotnet-extensions-configuration/pull/99) adding support for using AppConfig Lambda extension. Thanks [mgorski-mg](https://github.com/mgorski-mg)
   * Merged PR [#69](https://github.com/aws/aws-dotnet-extensions-configuration/pull/69) making IParameterProcessor used for customizing data from AWS into config values more genenric. Thanks [warej](https://github.com/warej)   
   * Merged PR [#59](https://github.com/aws/aws-dotnet-extensions-configuration/pull/59) removing dependency on Newtonsoft.JSON and now use System.Text.Json. Thanks [Adilson de Almeida Junior](https://github.com/Adilson)
   * Accessing AppConfig data now uses the AWS AppConfig Data APIs StargConfigurationSession and GetLatestConfiguration
   * **Breaking Change:** The IncludeParameter, GetKey and GetValue methods were removed from IParameterProcessor in favor of the new generic ProcessParameters method allowing more flexibility in implementations.
   * **Breaking Change:** `ClientId` was removed from `AppConfigConfigurationSource`.
   * **Breaking Change:** When using AppConfig IAM permissions for `appconfig:StartConfigurationSession` and `appconfig:GetLatestConfiguration` are now required and `appconfig:GetConfiguration` is not longer required.



### Release 2021-08-18
* **Amazon.Extensions.Configuration.SystemsManager (3.0.0)**
    * Merged PR [#82](https://github.com/aws/aws-dotnet-extensions-configuration/pull/82) Adding [AWS AppConfig](https://docs.aws.amazon.com/appconfig/latest/userguide/what-is-appconfig.html) support. Thanks [Michał Górski](https://github.com/mgorski-mg)

### Release 2021-07-23
* **Amazon.Extensions.Configuration.SystemsManager (2.1.1)**
    * Update AWSSDK.Extensions.NETCore.Setup and AWSSDK.SimpleSystemsManagement package references for SSO credential support. 

### Release 2021-03-30
* **Amazon.Extensions.Configuration.SystemsManager (2.1.0)**
    * Update AWS SDK dependencies to version 3.7

### Release 2020-10-09
* **Amazon.Extensions.Configuration.SystemsManager (2.0.0)**
    * Merged PR [#75](https://github.com/aws/aws-dotnet-extensions-configuration/pull/75) Update AWS SDK dependencies to 3.5. Thanks [Doug Ferris](https://github.com/doug-ferris)
    * Update Newtonsoft.Json to version 12.

### Release 2019-07-23
* **Amazon.Extensions.Configuration.SystemsManager (1.2.0)**
    * Merged PR [#41](https://github.com/aws/aws-dotnet-extensions-configuration/pull/41) Prevents stripping the first char when the Path is "/". Thanks [Ken Hundley](https://github.com/KenHundley)
    * Merged PR [#44](https://github.com/aws/aws-dotnet-extensions-configuration/pull/44) Added `Filters` property to SystemsManagerConfigurationSource. Thanks [Zahy Hwary](https://github.com/zahycs)

### Release 2019-04-09
* **Amazon.Extensions.Configuration.SystemsManager (1.1.1)**
    * Added AWS Secrets Manager support. Thanks [Ken Hundley](https://github.com/KenHundley)
    * Only trigger OnReload when values have changed.
    * Update version of the AWS SDK for .NET to 3.3.100

### Release 2019-01-11
* **Amazon.Extensions.Configuration.SystemsManager (1.0.1)**
  * Made Analysers for development/local only


### Release 2019-12-17
* **Amazon.Extensions.Configuration.SystemsManager (1.0.0)**
  * Initial release
