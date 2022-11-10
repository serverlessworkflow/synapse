/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Neuroglia.Data.Flux;
using Synapse.Integration.Commands.Correlations;
using Synapse.Integration.Models;
using System.Reactive.Linq;

namespace Synapse.Dashboard.Pages.Correlations.Create;

/// <summary>
/// Defines <see cref="CreateCorrelationState"/>-related selectors
/// </summary>
public static class Selectors
{

    /// <summary>
    /// Selects the <see cref="V1Correlation"/> to create
    /// </summary>
    /// <param name="store">The current <see cref="IStore"/></param>
    /// <returns>A new <see cref="IObservable{T}"/></returns>
    public static IObservable<V1CreateCorrelationCommand?> SelectCreateCorrelationCommand(IStore store)
    {
        return store.GetFeature<CreateCorrelationState>()
            .Select(state => state.Command)
            .DistinctUntilChanged();
    }

    /// <summary>
    /// Selects a boolean indicating whether or not the <see cref="V1Correlation"/> is being saved
    /// </summary>
    /// <param name="store">The current <see cref="IStore"/></param>
    /// <returns>A new <see cref="IObservable{T}"/></returns>
    public static IObservable<bool> SelectIsCorrelationBeingSaved(IStore store)
    {
        return store.GetFeature<CreateCorrelationState>()
            .Select(state => state.Saving)
            .DistinctUntilChanged();
    }

}
