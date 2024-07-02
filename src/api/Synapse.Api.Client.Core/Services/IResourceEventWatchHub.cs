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

namespace Synapse.Api.Client.Services;

/// <summary>
/// Defines the fundamentals of a service used by clients to watch resource-related events
/// </summary>
public interface IResourceEventWatchHub
{

    /// <summary>
    /// Subscribes to events produced by resources of the specified type
    /// </summary>
    /// <param name="resourceDefinition">The type of resources to watch</param>
    /// <param name="namespace">The namespace the resources to watch belong to, if any</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task Watch(ResourceDefinitionInfo resourceDefinition, string? @namespace = null);

    /// <summary>
    /// Unsubscribes from events produced by resources of the specified type
    /// </summary>
    /// <param name="resourceDefinition">The type of resources to stop watching</param>
    /// <param name="namespace">The namespace the resources to stop watching belong to, if any</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task StopWatching(ResourceDefinitionInfo resourceDefinition, string? @namespace = null);

}
