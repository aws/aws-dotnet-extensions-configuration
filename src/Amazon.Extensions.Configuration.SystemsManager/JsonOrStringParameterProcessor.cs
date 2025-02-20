using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using static Amazon.Extensions.Configuration.SystemsManager.Utils.ParameterProcessorUtil;

namespace Amazon.Extensions.Configuration.SystemsManager
{
    /// <inheritdoc />
    /// <summary>
    /// A processor that prioritizes JSON parameters but falls back to string parameters,
    /// in accordance with Systems Manager's suggested naming conventions
    /// </summary>
    public class JsonOrStringParameterProcessor : DefaultParameterProcessor
    {
        public override IDictionary<string, string> ProcessParameters(IEnumerable<Parameter> parameters, string path)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var parameter in parameters.Where(parameter => IncludeParameter(parameter, path)))
            {
                var keyPrefix = GetKey(parameter, path);
                var value = GetValue(parameter, path);

                if (parameter.Type == ParameterType.StringList)
                {
                    ParseStringListParameter(keyPrefix, value, result);
                    continue;
                }

                try
                {
                    ParseJsonParameter(keyPrefix, value, result);
                }
                catch (JsonException)
                {
                    ParseStringParameter(keyPrefix, value, result);
                }
            }

            return result;
        }
    }
}