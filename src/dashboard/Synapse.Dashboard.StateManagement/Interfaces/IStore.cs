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
/// Defines the fundamentals of a Flux store
/// </summary>
public interface IStore
    : IObservable<object>
{

    /// <summary>
    /// Gets the <see cref="IStore"/>'s state
    /// </summary>
    object State { get; }

    /// <summary>
    /// Adds a new <see cref="IFeature"/> to the store
    /// </summary>
    /// <typeparam name="TState">The type of state managed by the <see cref="IFeature"/> to add</typeparam>
    /// <param name="feature">The <see cref="IFeature"/> to add</param>
    void AddFeature<TState>(IFeature<TState> feature);

    /// <summary>
    /// Adds a new <see cref="IMiddleware"/> to the store
    /// </summary>
    void AddMiddleware(Type middlewareType);

    /// <summary>
    /// Adds a new <see cref="IEffect"/> to the store
    /// </summary>
    /// <param name="effect">The <see cref="IEffect"/> to add</param>
    void AddEffect(IEffect effect);

    /// <summary>
    /// Gets the <see cref="IFeature"/> of the specified type
    /// </summary>
    /// <typeparam name="TState">The type of state managed by the <see cref="IFeature"/> to get</typeparam>
    /// <returns>The <see cref="IFeature"/> with the specified state type</returns>
    IFeature<TState> GetFeature<TState>();

}
