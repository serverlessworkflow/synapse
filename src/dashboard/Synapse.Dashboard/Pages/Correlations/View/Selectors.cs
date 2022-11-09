/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0(the "License");
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
 */

using Neuroglia.Data.Flux;
using Synapse.Integration.Models;
using System.Reactive.Linq;

namespace Synapse.Dashboard.Pages.Correlations.View;

/// <summary>
/// Defines selectors for the <see cref="CorrelationViewState"/>
/// </summary>
public static class Selectors
{

    /// <summary>
    /// Selects the current <see cref="V1Correlation"/>, if any
    /// </summary>
    /// <param name="store">The global <see cref="IStore"/></param>
    /// <returns>A new <see cref="IObservable{T}"/> used to observe the current <see cref="V1Correlation"/></returns>
    public static IObservable<V1Correlation?> SelectCurrentCorrelation(IStore store)
    {
        return store.GetFeature<CorrelationViewState>()
              .Select(featureState => featureState.Correlation)
              .DistinctUntilChanged();
    }

    /// <summary>
    /// Selects a boolean indicating whether or not the publish cloud event modal is visible or not
    /// </summary>
    /// <param name="store">The global <see cref="IStore"/></param>
    /// <returns>A new <see cref="IObservable{T}"/> used to observe the boolean</returns>
    public static IObservable<bool> SelectPublishCloudEventModalOpened(IStore store)
    {
        return store.GetFeature<CorrelationViewState>()
              .Select(featureState => featureState.PublishCloudEventModalOpened)
              .DistinctUntilChanged();
    }

    /// <summary>
    /// Selects the cloud event to publish, if any
    /// </summary>
    /// <param name="store">The global <see cref="IStore"/></param>
    /// <returns>A new <see cref="IObservable{T}"/> used to observe the current <see cref="V1Event"/></returns>
    public static IObservable<V1Event?> SelectCurrentEvent(IStore store)
    {
        return store.GetFeature<CorrelationViewState>()
              .Select(featureState => featureState.Event)
              .DistinctUntilChanged();
    }

}

