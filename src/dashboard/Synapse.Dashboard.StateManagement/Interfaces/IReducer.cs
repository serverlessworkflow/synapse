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
/// Defines the fundamentals of a Flux reducer
/// </summary>
public interface IReducer
{

    /// <summary>
    /// Reduces the specified state, for the specified action
    /// </summary>
    /// <param name="state">The state to reduce</param>
    /// <param name="action">The Flux action the reducer applies to</param>
    /// <returns>A new object that represents the reduced state</returns>
    object Reduce(object state, object action);

}

/// <summary>
/// Defines the fundamentals of a Flux reducer
/// </summary>
/// <typeparam name="TState">The type of state to reduce</typeparam>
public interface IReducer<TState>
    : IReducer
{

    /// <summary>
    /// Reduces the specified state, for the specified action
    /// </summary>
    /// <param name="state">The state to reduce</param>
    /// <param name="action">The Flux action the reducer applies to</param>
    /// <returns>A new object that represents the reduced state</returns>
    TState Reduce(TState state, object action);

}

/// <summary>
/// Defines the fundamentals of a Flux reducer
/// </summary>
/// <typeparam name="TState">The type of state to reduce</typeparam>
/// <typeparam name="TAction">The type of flux action the reducer applies to</typeparam>
public interface IReducer<TState, TAction>
    : IReducer<TState>
{

    /// <summary>
    /// Reduces the specified state, for the specified action
    /// </summary>
    /// <param name="state">The state to reduce</param>
    /// <param name="action">The Flux action the reducer applies to</param>
    /// <returns>A new object that represents the reduced state</returns>
    TState Reduce(TState state, TAction action);

}
