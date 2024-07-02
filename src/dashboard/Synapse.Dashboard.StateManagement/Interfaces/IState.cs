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
/// Defines the fundamentals of a Flux state
/// </summary>
public interface IState
{

    /// <summary>
    /// Gets the <see cref="IState"/>'s value
    /// </summary>
    object Value { get; }

    /// <summary>
    /// Attempts to dispatch the specified Flux action
    /// </summary>
    /// <param name="action">The Flux action to dispatch</param>
    /// <returns>A boolean indicating whether or not the Flux action could be dispatched</returns>
    bool TryDispatch(object action);

    /// <summary>
    /// Adds an <see cref="IReducer"/> to the state
    /// </summary>
    /// <param name="reducer">The <see cref="IReducer"/> to add</param>
    void AddReducer(IReducer reducer);

}

/// <summary>
/// Defines the fundamentals of a Flux state
/// </summary>
/// <typeparam name="TState">The type of the <see cref="IState"/>'s value</typeparam>
public interface IState<TState>
    : IState, IObservable<TState>
{

    /// <summary>
    /// Gets the <see cref="IState"/>'s value
    /// </summary>
    new TState Value { get; }

    /// <summary>
    /// Adds an <see cref="IReducer"/> to the state
    /// </summary>
    /// <param name="reducer">The <see cref="IReducer"/> to add</param>
    void AddReducer(IReducer<TState> reducer);

}
