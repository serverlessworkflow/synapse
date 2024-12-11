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

using Neuroglia;
using Neuroglia.Serialization;
using ServerlessWorkflow.Sdk;
using System.Net;
using Avro.IO;
using Avro;
using Avro.Generic;

namespace Synapse.Core.Infrastructure.Services;

/// <summary>
/// Represents the <see cref="ISchemaHandler"/> implementation used to handle Avro schemas
/// </summary>
/// <param name="externalResourceProvider">The service used to provide external resources</param>
/// <param name="jsonSerializer">The service used to serialize/deserialize data to/from JSON</param>
public class AvroSchemaHandler(IExternalResourceProvider externalResourceProvider, IJsonSerializer jsonSerializer)
    : ISchemaHandler
{

    /// <summary>
    /// Gets the service used to provide external resources
    /// </summary>
    protected IExternalResourceProvider ExternalResourceProvider { get; } = externalResourceProvider;

    /// <summary>
    /// Gets the service used to serialize/deserialize data to/from JSON
    /// </summary>
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;

    /// <inheritdoc/>
    public virtual bool Supports(string format) => format.Equals(SchemaFormat.Avro, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public virtual async Task<IOperationResult> ValidateAsync(object graph, SchemaDefinition schema, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(schema);
        if (!this.Supports(schema.Format)) throw new NotSupportedException($"The specified schema format '{schema.Format}' is not supported in this context");
        Schema avroSchema;
        try
        {
            var json = string.Empty;
            if (schema.Resource == null)
            {
                json = this.JsonSerializer.SerializeToText(schema.Document);
            }
            else
            {
                using var stream = await this.ExternalResourceProvider.ReadAsync(schema.Resource, cancellationToken: cancellationToken).ConfigureAwait(false);
                using var streamReader = new StreamReader(stream);
                json = await streamReader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            }
            avroSchema = Schema.Parse(json);
        }
        catch (Exception ex)
        {
            return new OperationResult((int)HttpStatusCode.BadRequest, null, new Neuroglia.Error()
            {
                Type = ErrorType.Validation,
                Title = ErrorTitle.Validation,
                Status = ErrorStatus.Validation,
                Detail = $"An error occurred while parsing the specified schema: {ex}"
            });
        }
        byte[] avroData;
        try
        {
            using var memoryStream = new MemoryStream();
            var writer = new BinaryEncoder(memoryStream);
            var datumWriter = new GenericDatumWriter<object>(avroSchema);
            var avroObject = ConvertToAvroCompatible(graph, avroSchema);
            datumWriter.Write(avroObject, writer);
            avroData = memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            return new OperationResult((int)HttpStatusCode.BadRequest, null, new Neuroglia.Error()
            {
                Type = ErrorType.Validation,
                Title = ErrorTitle.Validation,
                Status = ErrorStatus.Validation,
                Detail = $"An error occurred while serializing the specified data to Avro: {ex}"
            });
        }
        try
        {
            using var memoryStream = new MemoryStream(avroData);
            var reader = new BinaryDecoder(memoryStream);
            var datumReader = new GenericDatumReader<GenericRecord>(avroSchema, avroSchema);
            datumReader.Read(null!, reader);
            return await Task.FromResult(new OperationResult((int)HttpStatusCode.OK));
        }
        catch (Exception ex)
        {
            return new OperationResult((int)HttpStatusCode.BadRequest, null, new Neuroglia.Error()
            {
                Type = ErrorType.Validation,
                Title = ErrorTitle.Validation,
                Status = ErrorStatus.Validation,
                Detail = ex.Message
            });
        }
    }

    object ConvertToAvroCompatible(object graph, Schema schema)
    {
        return schema switch
        {
            RecordSchema recordSchema => ConvertToGenericRecord(graph, recordSchema),
            ArraySchema arraySchema => ConvertToArray(graph, arraySchema),
            MapSchema mapSchema => ConvertToMap(graph, mapSchema),
            UnionSchema unionSchema => ConvertToUnion(graph, unionSchema),
            FixedSchema fixedSchema => graph,
            EnumSchema enumSchema => graph,
            PrimitiveSchema primitiveSchema => graph,
            _ => throw new NotSupportedException($"Unsupported schema type: {schema.GetType().Name}")
        };
    }

    GenericRecord ConvertToGenericRecord(object graph, RecordSchema recordSchema)
    {
        var record = new GenericRecord(recordSchema);
        var properties = graph.GetType().GetProperties();
        foreach (var field in recordSchema.Fields)
        {
            var property = properties.FirstOrDefault(p => p.Name.Equals(field.Name, StringComparison.OrdinalIgnoreCase));
            if (property != null)
            {
                var value = property.GetValue(graph)!;
                record.Add(field.Name, ConvertToAvroCompatible(value, field.Schema));
            }
        }
        return record;
    }

    object ConvertToArray(object graph, ArraySchema arraySchema)
    {
        if (graph is not IEnumerable<object> enumerable) throw new InvalidOperationException("Provided object is not an array or enumerable");
        return enumerable.Select(item => ConvertToAvroCompatible(item, arraySchema.ItemSchema)).ToList();
    }

    object ConvertToMap(object graph, MapSchema mapSchema)
    {
        if (graph is not IDictionary<string, object> dictionary) throw new InvalidOperationException("Provided object is not a map or dictionary");
        return dictionary.ToDictionary(kvp => kvp.Key, kvp => ConvertToAvroCompatible(kvp.Value, mapSchema.ValueSchema));
    }

    object ConvertToUnion(object graph, UnionSchema unionSchema)
    {
        foreach (var schema in unionSchema.Schemas)
        {
            try
            {
                return ConvertToAvroCompatible(graph, schema);
            }
            catch
            {
                // Continue to next schema in the union
            }
        }
        throw new InvalidOperationException("Provided object does not match any schema in the union.");
    }

}