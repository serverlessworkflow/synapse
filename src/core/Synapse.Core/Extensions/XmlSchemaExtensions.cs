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

using Json.Schema;
using System.Xml.Schema;

namespace Synapse;

/// <summary>
/// Defines extensions for <see cref="XmlSchema"/>s
/// </summary>
public static class XmlSchemaExtensions
{

    /// <summary>
    /// Converts the specified <see cref="XmlSchema"/> into a new <see cref="JsonSchema"/>
    /// </summary>
    /// <param name="schema">The <see cref="XmlSchema"/> to convert</param>
    /// <returns>A new <see cref="JsonSchema"/></returns>
    public static JsonSchema ToJsonSchema(this XmlSchema schema)
    {
        var builder = new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Title("Converted from XSD");
        foreach (var item in schema.Items)
        {
            if (item is not XmlSchemaElement element) continue;
            builder.Properties(new Dictionary<string, JsonSchema> { { element.Name!, element.ToJsonSchema() } });
        }
        return builder.Build();
    }

    static JsonSchema ToJsonSchema(this XmlSchemaElement element)
    {
        switch (element.ElementSchemaType)
        {
            case XmlSchemaSimpleType simpleType:
                return new JsonSchemaBuilder().Type(simpleType.Datatype.ToJsonSchemaValueType()).Build();
            case XmlSchemaComplexType complexType:
                var builder = new JsonSchemaBuilder().Type(SchemaValueType.Object);
                var properties = new Dictionary<string, JsonSchema>();
                if (complexType.Particle is XmlSchemaSequence sequence)
                {
                    foreach (var item in sequence.Items)
                    {
                        if (item is not XmlSchemaElement childElement) continue;
                        properties[childElement.Name!] = childElement.ToJsonSchema();
                    }
                }
                builder.Properties(properties);
                return builder.Build();
            default:
                throw new NotSupportedException($"The specified XML element type '{element.ElementSchemaType?.Name}' is not supported");
        }
    }

    static SchemaValueType ToJsonSchemaValueType(this XmlSchemaDatatype? dataType)
    {
        if (dataType == null) return SchemaValueType.Null;
        return dataType.TypeCode switch
        {
            XmlTypeCode.String => SchemaValueType.String,
            XmlTypeCode.Int => SchemaValueType.Integer,
            XmlTypeCode.Boolean => SchemaValueType.Boolean,
            XmlTypeCode.Float => SchemaValueType.Number,
            XmlTypeCode.Double => SchemaValueType.Number,
            XmlTypeCode.Date => SchemaValueType.String,
            XmlTypeCode.DateTime => SchemaValueType.String,
            _ => throw new NotSupportedException($"The specified XML type '{dataType}' is not supported")
        };
    }

}