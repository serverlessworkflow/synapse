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
/// Defines the fundamentals of a Flux feature
/// </summary>
public interface IFeature
{

    /// <summary>
    /// Gets the <see cref="IFeature"/>'s state
    /// </summary>
    object State { get; }

    /// <summary>
    /// Determines whether or not the <see cref="IFeature"/> defines <see cref="IReducer"/>s for the specified action
    /// </summary>
    /// <param name="action">The action to get check for <see cref="IReducer"/>s</param>
    /// <returns>A boolean indicating whether or not the <see cref="IFeature"/>'s state is reduced by the specified action</returns>
    bool ShouldReduceStateFor(object action);

    /// <summary>
    /// Adds the specified <see cref="IReducer"/> to the <see cref="IFeature"/>
    /// </summary>
    /// <param name="reducer">The <see cref="IReducer"/> to add</param>
    void AddReducer(IReducer reducer);

    /// <summary>
    /// Reduces the <see cref="IFeature"/>'s state
    /// </summary>
    /// <param name="context">The <see cref="IActionContext"/> in which to reduce the feature</param>
    /// <param name="reducerPipelineBuilder">A <see cref="Func{T, TResult}"/> used to build the reducer pîpeline</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task ReduceStateAsync(IActionContext context, Func<DispatchDelegate, DispatchDelegate> reducerPipelineBuilder);

}

/// <summary>
/// Defines the fundamentals of a Flux feature
/// </summary>
/// <typeparam name="TState">The type of the <see cref="IFeature"/>'s state</typeparam>
public interface IFeature<TState>
    : IFeature, IObservable<TState>
{

    /// <summary>
    /// Gets the <see cref="IFeature"/>'s state
    /// </summary>
    new TState State { get; }

    /// <summary>
    /// Adds the specified <see cref="IReducer"/> to the <see cref="IFeature"/>
    /// </summary>
    /// <param name="reducer">The <see cref="IReducer"/> to add</param>
    void AddReducer(IReducer<TState> reducer);

}