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
/// Defines the fundamentals of an effect
/// </summary>
public interface IEffect
{

    /// <summary>
    /// Determines whether or not to apply the effect
    /// </summary>
    /// <param name="action">THe action to apply the effect to</param>
    /// <returns>A boolean indicating whether or not to apply the effect to the specified action</returns>
    bool AppliesTo(object action);

    /// <summary>
    /// Applies the effect to the specified action
    /// </summary>
    /// <param name="action">The action to apply the effect to</param>
    /// <param name="context">The <see cref="IEffect"/> context</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task ApplyAsync(object action, IEffectContext context);

}

/// <summary>
/// Defines the fundamentals of an effect
/// </summary>
/// <typeparam name="TAction">The type of the action to apply the effect to</typeparam>
public interface IEffect<TAction>
    : IEffect
{

    /// <summary>
    /// Applies the effect to the specified action
    /// </summary>
    /// <param name="action">The action to apply the effect to</param>
    /// <param name="context">The <see cref="IEffect"/> context</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task ApplyAsync(TAction action, IEffectContext context);

}
