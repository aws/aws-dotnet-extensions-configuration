# AppConfig + Parameter Store Integration Sample

This console application demonstrates the fix for the issue where AppConfig returns `application/octet-stream` content type when using Parameter Store as the configuration source.

## Problem Solved

When AppConfig is configured to use Parameter Store as its configuration source, AWS returns the configuration data with `application/octet-stream` content type instead of `application/json`. This caused the original library to fail parsing, as it only accepted `application/json` content type.

**The fix**: The `ParseConfig` method now attempts JSON parsing regardless of content type, letting the JSON parser determine if the content is valid JSON.

## What This Sample Does

1. **Creates AWS Resources**:
   - SSM Parameters with JSON configuration data
   - IAM Role for AppConfig to access Parameter Store
   - AppConfig Application, Environment, and Configuration Profile
   - Points the Configuration Profile to Parameter Store using `ssm-parameter://` URI

2. **Tests Configuration Loading**:
   - Uses AppConfig to load configuration (which retrieves from Parameter Store)
   - AppConfig returns `application/octet-stream` content type
   - Verifies the fix handles this correctly by parsing the JSON content

3. **Validates Results**:
   - Displays raw configuration values
   - Tests strongly-typed configuration binding
   - Confirms hierarchical configuration works

## Prerequisites

- AWS CLI configured with appropriate credentials
- .NET 8.0 SDK
- AWS permissions for:
  - SSM: `GetParameter`, `GetParametersByPath`, `PutParameter`, `DeleteParameter`
  - AppConfig: Full access for creating applications, environments, profiles, deployments
  - IAM: Create/delete roles and policies

## Usage

```bash
# Navigate to the sample directory
cd samples/AppConfigParameterStore

# Run the sample
dotnet run
```

## Sample Output

```
🧪 AppConfig + Parameter Store Integration Test
==============================================

This test demonstrates the fix for the issue where AppConfig
returns 'application/octet-stream' content type when using
Parameter Store as the configuration source.

🚀 Setting up AWS resources for AppConfig + Parameter Store test...

📝 Creating SSM Parameters...
  ✓ /appconfig-test-20250728163000/StringValue
  ✓ /appconfig-test-20250728163000/IntegerValue
  ✓ /appconfig-test-20250728163000/Database/ConnectionString
  ...

🔐 Creating IAM Role for AppConfig...
  ✓ Role: AppConfigParameterStoreTest-20250728163000-Role

⚙️  Creating AppConfig resources...
  ✓ Application: AppConfigParameterStoreTest-20250728163000
  ✓ Environment: Test
  ✓ Configuration Profile: ParameterStoreProfile
  ✓ Deployment started: 1
  ✅ Deployment completed successfully

✅ All AWS resources created successfully!

🔧 Testing configuration loading...
📡 Loading configuration from AppConfig...

📊 Raw Configuration Values:
=============================
✅ StringValue: parameter-store-string-value
✅ IntegerValue: 42
✅ Database:ConnectionString: server=prod-db.example.com;database=appconfig_test;integrated security=true
...

🔍 Content Type Verification:
==============================
✅ Configuration loaded successfully!
✅ This confirms that the ParseConfig fix is working correctly.
✅ AppConfig returned 'application/octet-stream' content type from Parameter Store,
✅ but the library successfully parsed the JSON content anyway.

✅ All tests passed! The fix is working correctly.

❓ Do you want to keep the AWS resources for further testing? (y/N)
```

## Manual Testing

If you keep the resources, you can:

1. **Modify parameters** in AWS Systems Manager Parameter Store
2. **Wait for refresh** (30-second polling interval)
3. **See changes reflected** in the application

## Cleanup

The application will offer to clean up AWS resources automatically. If you choose to keep them for further testing, you can:

1. Delete resources manually from AWS Console
2. Or run the application again and choose cleanup when prompted

## Key Files

- **Program.cs**: Main application logic and test scenarios
- **SetupResources.cs**: AWS resource creation and cleanup
- **Settings.cs**: Strongly-typed configuration models
- **AppConfigParameterStore.csproj**: Project dependencies

## What This Proves

✅ **Real-world scenario**: Creates actual AWS resources in the problematic configuration  
✅ **Content type issue**: AppConfig returns `application/octet-stream` from Parameter Store  
✅ **Fix validation**: Library successfully parses JSON despite content type  
✅ **End-to-end test**: Full integration from Parameter Store → AppConfig → .NET Configuration  
✅ **Hierarchical config**: Nested configuration structures work correctly  

This sample provides definitive proof that the ParseConfig fix resolves the original issue in real AWS environments.
