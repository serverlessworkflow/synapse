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

namespace Synapse.Core.Infrastructure.Services;

/// <summary>
/// Defines the fundamentals of a service used to provide external resources
/// </summary>
public interface IExternalResourceProvider
{

    /// <summary>
    /// Reads the specified external resource
    /// </summary>
    /// <param name="workflow">The <see cref="WorkflowDefinition"/> in the context of which to read the specified resource</param>
    /// <param name="resource">The reference to the external resource to get</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A <see cref="Stream"/> used to read the external resource's contents</returns>
    Task<Stream> ReadAsync(WorkflowDefinition workflow, ExternalResourceDefinition resource, CancellationToken cancellationToken = default);

}