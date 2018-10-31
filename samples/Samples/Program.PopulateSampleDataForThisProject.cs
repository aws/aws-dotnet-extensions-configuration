using System.Threading.Tasks;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace Samples
{
    public static partial class Program
    {
        /// <summary>
        /// This exists only to populate some sample data to be used by this example project
        /// </summary>
        private static async Task PopulateSampleDataForThisProject()
        {
            var awsOptions = new AWSOptions {Region = RegionEndpoint.USEast1};

            var root = $"/dotnet-aws-samples/systems-manager-sample/common";
            var parameters = new[]
            {
                new {Name = "StringValue", Value = "string-value"},
                new {Name = "IntegerValue", Value = "10"},
                new {Name = "DateTimeValue", Value = "2000-01-01"},
                new {Name = "BooleanValue", Value = "True"},
                new {Name = "TimeSpanValue", Value = "00:05:00"},
            };

            using (var client = awsOptions.CreateServiceClient<IAmazonSimpleSystemsManagement>())
            {
                var result = await client.GetParametersByPathAsync(new GetParametersByPathRequest {Path = root, Recursive = true}).ConfigureAwait(false);
                if (result.Parameters.Count == parameters.Length) return;

                foreach (var parameter in parameters)
                {
                    var name = $"{root}/settings/{parameter.Name}";
                    await client.PutParameterAsync(new PutParameterRequest {Name = name, Value = parameter.Value, Type = ParameterType.String, Overwrite = true}).ConfigureAwait(false);
                }
            }
        }
    }
}