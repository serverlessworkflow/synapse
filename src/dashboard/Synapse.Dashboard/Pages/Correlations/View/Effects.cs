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

using Microsoft.AspNetCore.Components;
using Neuroglia.Data.Flux;
using Synapse.Apis.Management;

namespace Synapse.Dashboard.Pages.Correlations.View;

/// <summary>
/// Defines Flux effects for <see cref="CorrelationViewState"/>-related actions
/// </summary>
[Effect]
public static class Effects
{

    /// <summary>
    /// Handles the specified <see cref="GetCorrelationById"/>
    /// </summary>
    /// <param name="action">The <see cref="GetCorrelationById"/> to handle</param>
    /// <param name="context">The current <see cref="IEffectContext"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public static async Task On(GetCorrelationById action, IEffectContext context)
    {
        var api = context.Services.GetRequiredService<ISynapseManagementApi>();
        try
        {
            var correlation = await api.GetCorrelationByIdAsync(action.Id);
            context.Dispatcher.Dispatch(new HandleGetCorrelationByIdResult(correlation));
        }
        catch (Exception ex)
        {
            context.Services.GetRequiredService<ILogger<GetCorrelationById>>().LogError("An error occured while retrieving the correlation with the specified id '{correlationId}': {ex}", action.Id, ex);
        }
    }

    /// <summary>
    /// Handles the specified <see cref="DeleteCorrelation"/>
    /// </summary>
    /// <param name="action">The <see cref="DeleteCorrelation"/> to handle</param>
    /// <param name="context">The current <see cref="IEffectContext"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public static async Task On(DeleteCorrelation action, IEffectContext context)
    {
        var api = context.Services.GetRequiredService<ISynapseManagementApi>();
        try
        {
            await api.DeleteCorrelationAsync(action.CorrelationId);
            context.Dispatcher.Dispatch(new HandleDeleteCorrelationResult());
            context.Services.GetRequiredService<NavigationManager>().NavigateTo("/correlations");
        }
        catch (Exception ex)
        {
            context.Services.GetRequiredService<ILogger<GetCorrelationById>>().LogError("An error occured while deleting the correlation with id '{correlationId}': {ex}", action.CorrelationId, ex);
        }
    }

    /// <summary>
    /// Handles the specified <see cref="DeleteCorrelationContext"/>
    /// </summary>
    /// <param name="action">The <see cref="DeleteCorrelationContext"/> to handle</param>
    /// <param name="context">The current <see cref="IEffectContext"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public static async Task On(DeleteCorrelationContext action, IEffectContext context)
    {
        var api = context.Services.GetRequiredService<ISynapseManagementApi>();
        try
        {
            await api.DeleteCorrelationContextAsync(action.CorrelationId, action.ContextId);
            context.Dispatcher.Dispatch(new HandleDeleteCorrelationContextResult());
        }
        catch (Exception ex)
        {
            context.Services.GetRequiredService<ILogger<GetCorrelationById>>().LogError("An error occured while deleting the context with id '{correlationContextId}', owned by the correlation with id '{correlationId}': {ex}", action.ContextId, action.CorrelationId, ex);
        }
    }

    /// <summary>
    /// Handles the specified <see cref="DeleteCorrelatedEvent"/>
    /// </summary>
    /// <param name="action">The <see cref="DeleteCorrelatedEvent"/> to handle</param>
    /// <param name="context">The current <see cref="IEffectContext"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public static async Task On(DeleteCorrelatedEvent action, IEffectContext context)
    {
        var api = context.Services.GetRequiredService<ISynapseManagementApi>();
        try
        {
            await api.DeleteCorrelationContextEventAsync(action.CorrelationId, action.ContextId, action.EventId);
            context.Dispatcher.Dispatch(new HandleDeleteCorrelatedEventResult(action.CorrelationId, action.ContextId, action.EventId));
        }
        catch (Exception ex)
        {
            context.Services.GetRequiredService<ILogger<GetCorrelationById>>().LogError("An error occured while deleting the event with id '{eventId}' in the context with id '{contextId}', owned by the correlation with id '{correlationId}': {ex}", action.EventId, action.ContextId, action.CorrelationId, ex);
        }
    }

}

