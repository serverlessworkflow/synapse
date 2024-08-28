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

using System.Reactive.Linq;

namespace Synapse.Dashboard.StateManagement;

/// <summary>
/// Represents the base class for all component stores
/// </summary>
/// <typeparam name="TState">The type of the component store's state</typeparam>
/// <remarks>
/// Initializes a new <see cref="ComponentStore{TState}"/>
/// </remarks>
/// <param name="state">The store's initial state</param>
public abstract class ComponentStore<TState>(TState state)
    : IComponentStore<TState>
{

    readonly BehaviorSubject<TState> _subject = new(state);
    TState _state = state;
    bool _disposed;

    /// <summary>
    /// Gets the <see cref="ComponentStore{TState}"/>'s <see cref="System.Threading.CancellationTokenSource"/>
    /// </summary>
    protected CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

    /// <inheritdoc/>
    public virtual Task InitializeAsync() => Task.CompletedTask;

    /// <summary>
    /// Sets the <see cref="ComponentStore{TState}"/>'s state
    /// </summary>
    /// <param name="state">The updated state to set</param>
    protected virtual void Set(TState state)
    {
        this._state = state;
        this._subject.OnNext(this._state);
    }

    /// <summary>
    /// Patches the <see cref="ComponentStore{TState}"/>'s state
    /// </summary>
    /// <param name="reducer">A <see cref="Func{T, TResult}"/> used to reduce the <see cref="ComponentStore{TState}"/>'s state</param>
    protected virtual void Reduce(Func<TState, TState> reducer)
    {
        this.Set(reducer(this._state));
    }

    /// <summary>
    /// Get the <see cref="ComponentStore{TState}"/>'s state
    /// </summary>
    /// <returns></returns>
    protected virtual TState Get()
    {
        return this._state;
    }

    /// <summary>
    /// Get a <see cref="ComponentStore{TState}"/>'s state slice for the provided projection
    /// </summary>
    /// <returns></returns>
    protected virtual T Get<T>(Func<TState, T> project)
    {
        return project(this._state);
    }

    /// <inheritdoc/>
    public virtual IDisposable Subscribe(IObserver<TState> observer) => this._subject.Throttle(TimeSpan.FromMicroseconds(1)).Subscribe(observer);

    /// <summary>
    /// Disposes of the <see cref="ComponentStore{TState}"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="ComponentStore{TState}"/> is being disposed of</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this._disposed)
        {
            if (disposing)
            {
                this.CancellationTokenSource.Dispose();
                this._subject.Dispose();
            }
            this._disposed = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of the <see cref="ComponentStore{TState}"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="ComponentStore{TState}"/> is being disposed of</param>
    /// <returns>A new awaitable <see cref="ValueTask"/></returns>
    protected virtual ValueTask DisposeAsync(bool disposing)
    {
        if (!this._disposed)
        {
            if (disposing)
            {
                this.CancellationTokenSource.Dispose();
                this._subject.Dispose();
            }
            this._disposed = true;
        }
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await this.DisposeAsync(disposing: true);
        GC.SuppressFinalize(this);
    }

}