/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Dynamic;

namespace Synapse.Dashboard
{

    /// <summary>
    /// Defines extensions for <see cref="JSchema"/>s
    /// </summary>
    public static class JSchemaExtensions
    {

        /// <summary>
        /// Generates an example value for the <see cref="JSchema"/>
        /// </summary>
        /// <param name="schema">The <see cref="JSchema"/> to generate a new example value for</param>
        /// <returns>A new example value for the <see cref="JSchema"/></returns>
        public static object? GenerateExample(this JSchema schema)
        {
            if (schema.Default != null)
                return schema.Default.ToObject<object>();
            if (schema.ExtensionData.TryGetValue("examples", out var examples)
                && examples is JArray examplesArray)
                return examplesArray.First?.ToObject<object>();
            switch (schema.Type)
            {
                case JSchemaType.Array:
                    var array = new List<object>();
                    var itemSchema = schema.Items.First();
                    if (itemSchema != null)
                        array.Add(itemSchema.GenerateExample()!);
                    return array;
                case JSchemaType.Boolean:
                    return true;
                case JSchemaType.Integer:
                case JSchemaType.Number:
                    var min = schema.Minimum;
                    if (!min.HasValue)
                        min = 0;
                    var max = schema.Maximum;
                    if (!max.HasValue)
                        max = 0;
                    return Random.Shared.Next((int)min.Value, (int)max.Value);
                case JSchemaType.Object:
                    IDictionary<string, object> dyn = new ExpandoObject()!;
                    foreach(var property in schema.Properties)
                    {
                        dyn.Add(property.Key, property.Value.GenerateExample()!);
                    }
                    return dyn;
                case JSchemaType.String:
                    return schema.Title;
                default:
                    return null;
            }
        }

    }

}
