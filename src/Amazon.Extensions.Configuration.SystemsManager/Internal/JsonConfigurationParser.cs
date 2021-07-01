/*
 * Copyright 2018 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at
 * 
 *  http://aws.amazon.com/apache2.0
 * 
 * or in the "license" file accompanying this file. This file is distributed
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Amazon.Extensions.Configuration.SystemsManager.Internal
{
    public class JsonConfigurationParser : IDisposable
    {
        private JsonConfigurationParser() { }

        private readonly IDictionary<string, string> _data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<string> _context = new Stack<string>();
        private string _currentPath;

        private JsonTextReader _reader;

        public static IDictionary<string, string> Parse(Stream input)
        {
            using (var reader = new StreamReader(input))
            using (var parser = new JsonConfigurationParser())
            {
                return parser.ParseInput(reader);    
            }
        }

        public static IDictionary<string, string> Parse(string input)
        {
            using (var reader = new StringReader(input))
            using (var parser = new JsonConfigurationParser())
            {
                return parser.ParseInput(reader);    
            }
        }

        private IDictionary<string, string> ParseInput(TextReader input)
        {
            _data.Clear();
            using (_reader = new JsonTextReader(input) {DateParseHandling = DateParseHandling.None})
            {
                var jsonConfig = JObject.Load(_reader);

                VisitJObject(jsonConfig);

                return _data;
            }
        }
        
        private void VisitJObject(JObject jObject)
        {
            foreach (var property in jObject.Properties())
            {
                EnterContext(property.Name);
                VisitProperty(property);
                ExitContext();
            }
        }

        private void VisitProperty(JProperty property)
        {
            VisitToken(property.Value);
        }

        private void VisitToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    VisitJObject(token.Value<JObject>());
                    break;

                case JTokenType.Array:
                    VisitArray(token.Value<JArray>());
                    break;

                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Bytes:
                case JTokenType.Raw:
                case JTokenType.Null:
                    VisitPrimitive(token.Value<JValue>());
                    break;

                default:
                    throw new FormatException($"Unsupported JSON token '{_reader.TokenType}' was found. SecretId '{_reader.Path}', line {_reader.LineNumber} position {_reader.LinePosition}.");
            }
        }

        private void VisitArray(JArray array)
        {
            for (var index = 0; index < array.Count; index++)
            {
                EnterContext(index.ToString(CultureInfo.InvariantCulture));
                VisitToken(array[index]);
                ExitContext();
            }
        }

        private void VisitPrimitive(JValue data)
        {
            var key = _currentPath;

            if (_data.ContainsKey(key))
            {
                throw new FormatException($"A duplicate key '{key}' was found.");
            }

            _data[key] = data.Value == null ? null : data.ToString(CultureInfo.InvariantCulture);
        }

        private void EnterContext(string context)
        {
            _context.Push(context);
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }

        private void ExitContext()
        {
            _context.Pop();
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ((IDisposable) _reader)?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
