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

using ServerlessWorkflow.Sdk.Models.Processes;

namespace Synapse.Core.Infrastructure.Services;

/// <summary>
/// Defines the fundamentals of a service used to manage containers
/// </summary>
public interface IContainerPlatform
{

    /// <summary>
    /// Creates a new container based on the specified <see cref="ContainerProcessDefinition"/>
    /// </summary>
    /// <param name="definition">The <see cref="ContainerProcessDefinition"/> that defines the container to create</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IContainer"/></returns>
    Task<IContainer> CreateAsync(ContainerProcessDefinition definition, CancellationToken cancellationToken = default);

}
