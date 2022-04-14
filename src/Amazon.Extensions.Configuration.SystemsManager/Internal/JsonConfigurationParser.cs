/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
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
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Amazon.Extensions.Configuration.SystemsManager.Internal
{
    public class JsonConfigurationParser
    {
        private JsonConfigurationParser() { }

        private readonly IDictionary<string, string> _data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<string> _context = new Stack<string>();
        private string _currentPath;

        public static IDictionary<string, string> Parse(Stream input)
        {
            using (var doc = JsonDocument.Parse(input))
            {
                var parser = new JsonConfigurationParser();
                parser.VisitElement(doc.RootElement);
                return parser._data;
            }
        }

        public static IDictionary<string, string> Parse(string input)
        {
            using (var doc = JsonDocument.Parse(input))
            {
                var parser = new JsonConfigurationParser();
                parser.VisitElement(doc.RootElement);
                return parser._data;
            }
        }

        private void VisitElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Undefined:
                    break;
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        EnterContext(property.Name);
                        VisitElement(property.Value);
                        ExitContext();
                    }
                    break;
                case JsonValueKind.Array:
                    VisitArray(element);
                    break;
                case JsonValueKind.String:
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                    VisitPrimitive(element);
                    break;
                case JsonValueKind.Null:
                    VisitNull(element);
                    break;
            }

        }

        private void VisitArray(JsonElement array)
        {
            int index = 0;
            foreach (var item in array.EnumerateArray())
            {
                EnterContext(index.ToString(CultureInfo.InvariantCulture));
                VisitElement(item);
                ExitContext();

                index++;
            }
        }

        private void VisitNull(JsonElement data)
        {
            var key = _currentPath;
            _data[key] = null;
        }

        private void VisitPrimitive(JsonElement data)
        {
            var key = _currentPath;

            if (_data.ContainsKey(key))
            {
                throw new FormatException($"A duplicate key '{key}' was found.");
            }

            _data[key] = data.ToString();
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
    }
}
