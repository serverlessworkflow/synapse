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

namespace Synapse.Dashboard.Components;

/// <summary>
/// Represents the base class for all components used to manage <see cref="IResource"/>s
/// </summary>
/// <typeparam name="TComponent">The type of component inheriting the <see cref="NamespacedResourceManagementComponent{TComponent, TStore, TState, TResource}"/></typeparam>
/// <typeparam name="TStore">The type of the component's <see cref="Store"/></typeparam>
/// <typeparam name="TState">The type of the component's state</typeparam>
/// <typeparam name="TResource">The type of <see cref="IResource"/> to manage</typeparam>
public abstract class NamespacedResourceManagementComponent<TComponent, TStore, TState, TResource>
    : ResourceManagementComponent<TComponent, TStore, TState, TResource>
    where TComponent : NamespacedResourceManagementComponent<TComponent, TStore, TState, TResource>
    where TStore : NamespacedResourceManagementComponentStore<TState, TResource>
    where TState : NamespacedResourceManagementComponentState<TResource>, new()
    where TResource : Resource, new()
{

    /// <summary>
    /// Gets the list of available <see cref="Namespace"/>s
    /// </summary>
    protected EquatableList<Namespace>? Namespaces { get; set; }

    /// <summary>
    /// Gets current namespace
    /// </summary>
    protected string? Namespace { get; set; }

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        this.Store.Namespace.Subscribe(@namespace => this.OnStateChanged(cmp => cmp.Namespace = @namespace), token: this.CancellationTokenSource.Token);
        this.Store.Namespaces.Subscribe(namespaces => this.OnStateChanged(cmp => cmp.Namespaces = namespaces), token: this.CancellationTokenSource.Token);
        await this.Store.ListNamespaceAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Handles changes of namespace
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    protected void OnNamespaceChanged(ChangeEventArgs e)
    {
        this.Store.SetNamespace(e.Value?.ToString());
    }

}

/// <summary>
/// Represents the base class for all components used to manage <see cref="IResource"/>s
/// </summary>
/// <typeparam name="TStore">The type of the component's <see cref="Store"/></typeparam>
/// <typeparam name="TState">The type of the component's state</typeparam>
/// <typeparam name="TResource">The type of <see cref="IResource"/> to manage</typeparam>
public abstract class NamespacedResourceManagementComponent<TStore, TState, TResource>
    : NamespacedResourceManagementComponent<NamespacedResourceManagementComponent<TStore, TState, TResource>, TStore, TState, TResource>
    where TStore : NamespacedResourceManagementComponentStore<TState, TResource>
    where TState : NamespacedResourceManagementComponentState<TResource>, new()
    where TResource : Resource, new()
{



}

/// <summary>
/// Represents the base class for all components used to manage <see cref="IResource"/>s
/// </summary>
/// <typeparam name="TState">The type of the component's state</typeparam>
/// <typeparam name="TResource">The type of <see cref="IResource"/> to manage</typeparam>
public abstract class NamespacedResourceManagementComponent<TState, TResource>
    : NamespacedResourceManagementComponent<NamespacedResourceManagementComponentStore<TState, TResource>, TState, TResource>
    where TState : NamespacedResourceManagementComponentState<TResource>, new()
    where TResource : Resource, new()
{



}

/// <summary>
/// Represents the base class for all components used to manage <see cref="IResource"/>s
/// </summary>
/// <typeparam name="TResource">The type of <see cref="IResource"/> to manage</typeparam>
public abstract class NamespacedResourceManagementComponent<TResource>
    : NamespacedResourceManagementComponent<NamespacedResourceManagementComponentStore<NamespacedResourceManagementComponentState<TResource>, TResource>, NamespacedResourceManagementComponentState<TResource>, TResource>
    where TResource : Resource, new()
{



}
