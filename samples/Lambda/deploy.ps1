dotnet lambda deploy-serverless `
    --configuration Release `
    --stack-name configuration-lambda `
    --s3-bucket [s3-bucket-name] `
    --s3-prefix configuration-sample/lambda/ `
    --template application.yaml;