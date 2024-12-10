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
using Neuroglia;
using Neuroglia.Serialization;
using ServerlessWorkflow.Sdk;
using System.Net;

namespace Synapse.Core.Infrastructure.Services;

/// <summary>
/// Represents the <see cref="ISchemaHandler"/> implementation used to handle JSON schemas
/// </summary>
/// <param name="externalResourceProvider">The service used to provide external resources</param>
/// <param name="serializer">The service used to serialize/deserialize data to/from JSON</param>
public class JsonSchemaHandler(IExternalResourceProvider externalResourceProvider, IJsonSerializer serializer)
    : ISchemaHandler
{

    /// <summary>
    /// Gets the service used to provide external resources
    /// </summary>
    protected IExternalResourceProvider ExternalResourceProvider { get; } = externalResourceProvider;

    /// <summary>
    /// Gets the service used to serialize/deserialize data to/from JSON
    /// </summary>
    protected IJsonSerializer Serializer { get; } = serializer;

    /// <inheritdoc/>
    public virtual bool Supports(string format) => format.Equals(SchemaFormat.Json, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public virtual async Task<IOperationResult> ValidateAsync(object graph, SchemaDefinition schema, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(schema);
        if (!this.Supports(schema.Format)) throw new NotSupportedException($"The specified schema format '{schema.Format}' is not supported in this context");
        var json = string.Empty;
        if (schema.Resource == null)
        {
            json = this.Serializer.SerializeToText(schema.Document);
        }
        else
        {
            using var stream = await this.ExternalResourceProvider.ReadAsync(schema.Resource, cancellationToken: cancellationToken).ConfigureAwait(false);
            using var streamReader = new StreamReader(stream);
            json = await streamReader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        }
        var jsonSchema = JsonSchema.FromText(json);
        var jsonDocument = this.Serializer.SerializeToDocument(graph)!;
        var options = new EvaluationOptions()
        {
             OutputFormat = OutputFormat.List
        };
        var results = jsonSchema.Evaluate(jsonDocument, options);
        if (results.IsValid) return new OperationResult((int)HttpStatusCode.OK);
        else return new OperationResult((int)HttpStatusCode.BadRequest, null, new Neuroglia.Error()
        {
            Type = ErrorType.Validation,
            Title = ErrorTitle.Validation,
            Status = ErrorStatus.Validation,
            Errors = new(results.Details.Where(d => d.Errors != null).SelectMany(d => d.Errors!).GroupBy(e => e.Key).Select(e => new KeyValuePair<string, string[]>(e.Key, e.Select(e => e.Value).ToArray())))
        }); 
    }

}
