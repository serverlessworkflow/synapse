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

namespace Synapse.Dashboard;

/// <summary>
/// Represents the base class for all stateful <see cref="ComponentBase"/> implementations
/// </summary>
/// <typeparam name="TComponent">The type of component inheriting the <see cref="StatefulComponent{TComponent, TStore, TState}"/></typeparam>
/// <typeparam name="TStore">The type of the store used to manage the component's state</typeparam>
/// <typeparam name="TState">The type of the component's state</typeparam>
public abstract class StatefulComponent<TComponent, TStore, TState>
    : ComponentBase, IDisposable
    where TComponent: StatefulComponent<TComponent, TStore, TState>
    where TStore : ComponentStore<TState>
{

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    [Inject]
    protected IServiceProvider ServiceProvider { get; set; } = null!;

    /// <summary>
    /// Gets the <see cref="StatefulComponent{TComponent, TStore, TState}"/>'s <see cref="System.Threading.CancellationTokenSource"/>
    /// </summary>
    protected CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

    private TStore _store = null!;
    /// <summary>
    /// Gets the component's store
    /// </summary>
    protected TStore Store
    {
        get
        {
            if (this._store != null) return this._store;
            this._store = ActivatorUtilities.CreateInstance<TStore>(this.ServiceProvider);
            return this._store;
        }
    }

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await this.Store.InitializeAsync();
    }

    /// <summary>
    /// Patches the component fields after a change
    /// </summary>
    /// <param name="patch">The patch to apply</param>
    protected void OnStateChanged(Action<TComponent> patch)
    {
        patch((TComponent)this);
        this.StateHasChanged();
    }

    private bool _Disposed;
    /// <summary>
    /// Disposes of the component
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the dispose of the component</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this._Disposed)
        {
            if (disposing)
            {
                this._store.Dispose();
                this.CancellationTokenSource.Dispose();
            }
            this._Disposed = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}