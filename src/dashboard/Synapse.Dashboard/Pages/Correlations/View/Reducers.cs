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

namespace Synapse.Dashboard.Pages.Correlations.View;

/// <summary>
/// Defines Flux reducers for <see cref="CorrelationViewState"/>-related actions
/// </summary>
[Reducer]
public static class Reducers
{

    /// <summary>
    /// Handles the specified <see cref="HandleGetCorrelationByIdResult"/>
    /// </summary>
    /// <param name="state">The state to reduce</param>
    /// <param name="action">The <see cref="HandleGetCorrelationByIdResult"/> action to reduce</param>
    /// <returns>The reduced state</returns>
    public static CorrelationViewState On(CorrelationViewState state, HandleGetCorrelationByIdResult action)
    {
        return state with
        {
            Correlation = action.Result
        };
    }

    /// <summary>
    /// Handles the specified <see cref="ShowPublishCloudEventModal"/>
    /// </summary>
    /// <param name="state">The state to reduce</param>
    /// <param name="action">The <see cref="ShowPublishCloudEventModal"/> action to reduce</param>
    /// <returns>The reduced state</returns>
    public static CorrelationViewState On(CorrelationViewState state, ShowPublishCloudEventModal action)
    {
        return state with
        {
            PublishCloudEventModalOpened = true,
            Event = action.Event ?? new()
        };
    }

    /// <summary>
    /// Handles the specified <see cref="HidePublishCloudEventModal"/>
    /// </summary>
    /// <param name="state">The state to reduce</param>
    /// <param name="action">The <see cref="HidePublishCloudEventModal"/> action to reduce</param>
    /// <returns>The reduced state</returns>
    public static CorrelationViewState On(CorrelationViewState state, HidePublishCloudEventModal action)
    {
        return state with
        {
            PublishCloudEventModalOpened = false
        };
    }

    /// <summary>
    /// Handles the specified <see cref="AddContextToV1Correlation"/>
    /// </summary>
    /// <param name="state">The state to reduce</param>
    /// <param name="action">The <see cref="AddContextToV1Correlation"/> action to reduce</param>
    /// <returns>The reduced state</returns>
    public static CorrelationViewState On(CorrelationViewState state, AddContextToV1Correlation action)
    {
        if (state.Correlation?.Id != action.CorrelationId) return state;
        var correlation = state.Correlation.Clone()!;
        if (correlation.Contexts == null) correlation.Contexts = new List<V1CorrelationContext>();
        if(!correlation.Contexts.Any(c => c.Id == action.Context.Id)) correlation.Contexts.Add(action.Context);
        return state with { Correlation = correlation };
    }

    /// <summary>
    /// Handles the specified <see cref="CorrelateV1Event"/>
    /// </summary>
    /// <param name="state">The state to reduce</param>
    /// <param name="action">The <see cref="CorrelateV1Event"/> action to reduce</param>
    /// <returns>The reduced state</returns>
    public static CorrelationViewState On(CorrelationViewState state, CorrelateV1Event action)
    {
        if (state.Correlation?.Id != action.CorrelationId) return state;
        var correlation = state.Correlation.Clone()!;
        var context = correlation.Contexts?.FirstOrDefault(c => c.Id == action.ContextId);
        if (context == null) return state;
        if (context.PendingEvents == null) context.PendingEvents = new List<V1Event>();
        if(!context.PendingEvents.Any(c => c.Id == action.Event.Id)) context.PendingEvents.Add(action.Event);
        return state with { Correlation = correlation };
    }

    /// <summary>
    /// Handles the specified <see cref="RemoveContextFromV1Correlation"/>
    /// </summary>
    /// <param name="state">The state to reduce</param>
    /// <param name="action">The <see cref="RemoveContextFromV1Correlation"/> action to reduce</param>
    /// <returns>The reduced state</returns>
    public static CorrelationViewState On(CorrelationViewState state, RemoveContextFromV1Correlation action)
    {
        if (state.Correlation?.Id != action.CorrelationId) return state;
        var correlation = state.Correlation.Clone()!;
        var context = correlation.Contexts?.FirstOrDefault(c => c.Id == action.ContextId);
        if (correlation.Contexts == null || context == null) return state;
        correlation.Contexts.Remove(context);
        return state with { Correlation = correlation };
    }

}