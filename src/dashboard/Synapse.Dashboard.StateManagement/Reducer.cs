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
/// Represents the default implementation of the <see cref="IReducer{TState, TAction}"/> interface
/// </summary>
/// <typeparam name="TState">The type of state to reduce</typeparam>
/// <typeparam name="TAction">The type of flux action the reducer applies to</typeparam>
public class Reducer<TState, TAction>
    : IReducer<TState, TAction>
{

    /// <summary>
    /// Initializes a new <see cref="Reducer{TState, TAction}"/>
    /// </summary>
    /// <param name="reduceFunction">The function used to reduce the state</param>
    public Reducer(Func<TState, TAction, TState> reduceFunction)
    {
        this.ReduceFunction = reduceFunction;
    }

    /// <summary>
    /// Gets the function used to reduce the state
    /// </summary>
    protected Func<TState, TAction, TState> ReduceFunction { get; }

    /// <inheritdoc/>
    public TState Reduce(TState state, TAction action)
    {
        return this.ReduceFunction(state, action);
    }

    TState IReducer<TState>.Reduce(TState state, object action)
    {
        return this.Reduce(state, (TAction)action);
    }

    object IReducer.Reduce(object state, object action)
    {
        return this.Reduce((TState)state, (TAction)action)!;
    }

}
