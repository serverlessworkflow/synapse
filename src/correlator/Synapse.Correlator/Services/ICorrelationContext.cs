// Copyright © 2024-Present Neuroglia SRL. All rights reserved.
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

using Neuroglia.Eventing.CloudEvents;

namespace Synapse.Operator.Services;

/// <summary>
/// Defines the fundamentals of a context in which a correlation is performed
/// </summary>
public interface ICorrelationContext
{

    /// <summary>
    /// Gets a key/value mapping of the context's correlation keys
    /// </summary>
    IReadOnlyDictionary<string, string> Keys { get; }

    /// <summary>
    /// Gets a list containing all correlated events
    /// </summary>
    IEnumerable<CloudEvent> Events { get; }

    /// <summary>
    /// Adds the specified correlated <see cref="CloudEvent"/>
    /// </summary>
    /// <param name="e">The correlated <see cref="CloudEvent"/></param>
    /// <param name="keys">A key/value mapping of the resolved correlation keys</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task AddCorrelatedEventAsync(CloudEvent e, IDictionary<string, string> keys, CancellationToken cancellationToken = default);

}
