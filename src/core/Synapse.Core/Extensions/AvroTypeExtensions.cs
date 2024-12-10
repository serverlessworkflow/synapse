// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Avro;
using Json.Schema;
using System.Xml.Linq;

namespace Synapse;

/// <summary>
/// Defines extensions for Avro schema types
/// </summary>
public static class AvroTypeExtensions
{

    /// <summary>
    /// Converts the Avro schema type into a new <see cref="SchemaValueType"/>
    /// </summary>
    /// <param name="type">The extended <see cref="Schema.Type"/></param>
    /// <returns>The converted <see cref="SchemaValueType"/></returns>
    public static SchemaValueType ToJsonSchemaValueType(this Schema.Type type)
    {
        return type switch
        {
            Schema.Type.Null => SchemaValueType.Null,
            Schema.Type.Boolean => SchemaValueType.Boolean,
            Schema.Type.Int => SchemaValueType.Integer,
            Schema.Type.Long => SchemaValueType.Integer,
            Schema.Type.Float => SchemaValueType.Number,
            Schema.Type.Double => SchemaValueType.Number,
            Schema.Type.Bytes => SchemaValueType.String,
            Schema.Type.String => SchemaValueType.String,
            Schema.Type.Record => SchemaValueType.Object,
            Schema.Type.Enumeration => SchemaValueType.String,
            Schema.Type.Array => SchemaValueType.Array,
            Schema.Type.Map => SchemaValueType.Object,
            Schema.Type.Union => SchemaValueType.Null,
            Schema.Type.Fixed => SchemaValueType.String,
            Schema.Type.Error => SchemaValueType.Object,
            Schema.Type.Logical => SchemaValueType.String,
            _ => throw new NotSupportedException($"The specified Avro type '{type}' is not supported")
        };
    }

}
