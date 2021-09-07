dotnet lambda deploy-serverless `
    --configuration Release `
    --region eu-west-1 `
    --stack-name configuration-lambda `
    --s3-bucket [s3-bucket-name] `
    --s3-prefix configuration-sample/lambda/ `
    --template application.yaml;