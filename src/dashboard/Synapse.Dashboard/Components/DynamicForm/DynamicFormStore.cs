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
using ServerlessWorkflow.Sdk.Models;
using Synapse.Core.Infrastructure.Services;
using System.Xml.Schema;

namespace Synapse.Dashboard.Components.DynamicFormStateManagement;

/// <summary>
/// Represents the <see cref="DynamicForm"/>'s <see cref="ComponentStore{TState}"/>
/// </summary>
/// <param name="logger">The service used to perform logging</param>
/// <param name="externalResourceProvider">The service used to provide external resources</param>
/// <param name="jsonSerializer">The service used to serialize/deserialize data to/from JSON</param>
/// <param name="xmlSerializer">The service used to serialize/deserialize data to/from XML</param>
public class DynamicFormStore(ILogger<DynamicFormStore> logger, IExternalResourceProvider externalResourceProvider, IJsonSerializer jsonSerializer, IXmlSerializer xmlSerializer)
    : ComponentStore<DynamicFormState>(new())
{

    /// <inheritdoc/>
    public override Task InitializeAsync()
    {
        this.SchemaDefinition.SubscribeAsync(async _ => await this.LoadSchemaAsync(), cancellationToken: this.CancellationTokenSource.Token);
        this.Schema.Subscribe(BuildValue);
        return base.InitializeAsync();
    }

    #region Selectors

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="DynamicFormState.SchemaDefinition"/> changes
    /// </summary>
    public IObservable<SchemaDefinition?> SchemaDefinition => this.Select(state => state.SchemaDefinition).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="DynamicFormState.Schema"/> changes
    /// </summary>
    public IObservable<JsonSchema?> Schema => this.Select(state => state.Schema).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="DynamicFormState.Value"/> changes
    /// </summary>
    public IObservable<object?> Value => this.Select(state => state.Value).DistinctUntilChanged();

    #endregion

    #region Setters

    /// <summary>
    /// Sets the state <see cref="DynamicFormState.SchemaDefinition"/>'s value
    /// </summary>
    /// <param name="schemaDefinition">The <see cref="ServerlessWorkflow.Sdk.Models.SchemaDefinition"/> that defines the structure of the form's data to be rendered</param>
    public void SetSchemaDefinition(SchemaDefinition? schemaDefinition)
    {
        this.Reduce(state => state with
        {
            SchemaDefinition = schemaDefinition
        });
    }

    /// <summary>
    /// Sets the state <see cref="DynamicFormState.Schema"/>'s value
    /// </summary>
    /// <param name="schema">The <see cref="JsonSchema"/> that defines the structure of the form's data to be rendered</param>
    public void SetSchema(JsonSchema? schema)
    {
        this.Reduce(state => state with
        {
            Schema = schema
        });
    }

    /// <summary>
    /// Sets the value of the specified property
    /// </summary>
    /// <param name="name">The name of the property to set</param>
    /// <param name="value">The value of the property to set</param>
    public void SetPropertyValue(string name, object? value)
    {
        var map = this.Get(state => (EquatableDictionary<string, object?>)state.Value!);
        if(value == null) map.Remove(name);
        else map[name] = value;
        this.Reduce(state => state with
        {
            Value = map
        });
    }

    #endregion

    #region Actions

    async Task LoadSchemaAsync()
    {
        var schemaDefinition = this.Get(state => state.SchemaDefinition);
        if (schemaDefinition == null) return;
        JsonSchema schema;
        try
        {
            if (schemaDefinition.Resource == null)
            {
                switch (schemaDefinition.Format)
                {
                    case SchemaFormat.Avro:
                        schema = Avro.Schema.Parse(jsonSerializer.SerializeToText(schemaDefinition.Document)).ToJsonSchema();
                        break;
                    case SchemaFormat.Json:
                        schema = JsonSchema.FromText(jsonSerializer.SerializeToText(schemaDefinition.Document));
                        break;
                    case SchemaFormat.Xml:
                        var xml = xmlSerializer.SerializeToText(schemaDefinition.Document);
                        var stringReader = new StringReader(xml);
                        schema = XmlSchema.Read(stringReader, null)!.ToJsonSchema();
                        break;
                    default:
                        throw new NotSupportedException($"The specified schema format '{schemaDefinition.Format}' is not supported");
                }
            }
            else
            {
                using var stream = await externalResourceProvider.ReadAsync(schemaDefinition.Resource, cancellationToken: this.CancellationTokenSource.Token).ConfigureAwait(false);
                using var streamReader = new StreamReader(stream);
                schema = schemaDefinition.Format switch
                {
                    SchemaFormat.Avro => Avro.Schema.Parse(await streamReader.ReadToEndAsync(this.CancellationTokenSource.Token)).ToJsonSchema(),
                    SchemaFormat.Json => await JsonSchema.FromStream(stream).ConfigureAwait(false),
                    SchemaFormat.Xml => XmlSchema.Read(stream, null)!.ToJsonSchema(),
                    _ => throw new NotSupportedException($"The specified schema format '{schemaDefinition.Format}' is not supported"),
                };  
            }
            this.SetSchema(schema);
        }
        catch(Exception ex)
        {
            logger.LogError("An error occurred while loading the specified schema: {ex}", ex);
        }
    }

    void BuildValue(JsonSchema? schema)
    {
        object? value = null;
        if (schema != null)
        {
            switch (schema.GetJsonType())
            {
                case SchemaValueType.Null:
                    value = null;
                    break;
                case SchemaValueType.Array:
                    value = new List<object>();
                    break;
                case SchemaValueType.Object:
                    value = new EquatableDictionary<string, object?>();
                    break;
                default:

                    break;
            }
        }
        this.Reduce(state => state with
        {
            Value = value
        });
    }

    #endregion

}
