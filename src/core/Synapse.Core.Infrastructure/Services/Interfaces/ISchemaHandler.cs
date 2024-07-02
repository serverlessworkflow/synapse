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

namespace Synapse.Core.Infrastructure.Services;

/// <summary>
/// Defines the fundamentals of a service used to handle <see cref="SchemaDefinition"/>s
/// </summary>
public interface ISchemaHandler
{

    /// <summary>
    /// Determines whether or not the <see cref="ISchemaHandler"/> supports the specified schema format
    /// </summary>
    /// <param name="format">The format to check</param>
    /// <returns>A boolean indicating whether or not the <see cref="ISchemaHandler"/> supports the specified schema format</returns>
    bool Supports(string format);

    /// <summary>
    /// Validates an object against the specified schema
    /// </summary>
    /// <param name="graph">The object to validate</param>
    /// <param name="schema">The schema to validate the graph against</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>An object that describes the validation result</returns>
    Task<IOperationResult> ValidateAsync(object graph, SchemaDefinition schema, CancellationToken cancellationToken = default);

}