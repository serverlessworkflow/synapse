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

namespace Synapse.Dashboard.StateManagement;

/// <summary>
/// Represents the default implementation of the <see cref="IActionContext"/> interface
/// </summary>
/// <remarks>
/// Initializes a new <see cref="ActionContext"/>
/// </remarks>
/// <param name="services">The current <see cref="IServiceProvider"/></param>
/// <param name="store">The current <see cref="IStore"/></param>
/// <param name="action">The action to dispatch</param>
public class ActionContext(IServiceProvider services, IStore store, object action)
        : IActionContext
{

    /// <inheritdoc/>
    public IServiceProvider Services { get; } = services;

    /// <inheritdoc/>
    public IStore Store { get; } = store;

    /// <inheritdoc/>
    public object Action { get; set; } = action;

}
