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

namespace Synapse;

/// <summary>
/// Defines extensions for Avro schemas
/// </summary>
public static class AvroSchemaExtensions
{

    /// <summary>
    /// Converts an Avro schema to a JSON Schema.
    /// </summary>
    /// <param name="schema">The Avro schema to convert.</param>
    /// <returns>A corresponding JSON Schema.</returns>
    /// <exception cref="NotSupportedException">Thrown when the Avro schema type is not supported.</exception>
    public static JsonSchema ToJsonSchema(this Schema schema)
    {
        return schema switch
        {
            RecordSchema recordSchema => ConvertRecordSchema(recordSchema),
            ArraySchema arraySchema => new JsonSchemaBuilder().Type(SchemaValueType.Array).Items(arraySchema.ItemSchema.ToJsonSchema()).Build(),
            MapSchema mapSchema => new JsonSchemaBuilder().Type(SchemaValueType.Object).AdditionalProperties(mapSchema.ValueSchema.ToJsonSchema()).Build(),
            EnumSchema enumSchema => new JsonSchemaBuilder().Type(SchemaValueType.String).Title(enumSchema.Name).Description(enumSchema.Documentation).Enum(enumSchema.Symbols).Build(),
            FixedSchema fixedSchema => new JsonSchemaBuilder().Type(SchemaValueType.String).Title(fixedSchema.Name).Description(fixedSchema.Documentation).Pattern($".{{{fixedSchema.Size}}}").Build(),
            PrimitiveSchema primitiveSchema => new JsonSchemaBuilder().Type(primitiveSchema.Tag.ToJsonSchemaValueType()).Build(),
            UnionSchema unionSchema => new JsonSchemaBuilder().AnyOf(unionSchema.Schemas.Select(s => s.ToJsonSchema())).Build(),
            _ => throw new NotSupportedException($"Unsupported Avro schema type: {schema.GetType().Name}")
        };
    }

    /// <summary>
    /// Converts an Avro record schema to a JSON Schema object.
    /// </summary>
    /// <param name="schema">The Avro record schema to convert.</param>
    /// <returns>A JSON Schema representing the Avro record schema.</returns>
    static JsonSchema ConvertRecordSchema(RecordSchema schema)
    {
        var properties = new Dictionary<string, JsonSchema>();
        var requiredFields = new List<string>();
        foreach (var field in schema.Fields)
        {
            if (field.DefaultValue == null) requiredFields.Add(field.Name);
            properties[field.Name] = field.Schema.ToJsonSchema();
        }
        var builder = new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Title(schema.Name)
            .Description(schema.Documentation)
            .Properties(properties);
        if (requiredFields.Count > 0) builder.Required(requiredFields);
        return builder.Build();
    }

}
