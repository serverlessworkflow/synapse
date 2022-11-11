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

using Microsoft.AspNetCore.Components;
using Neuroglia.Data.Flux;
using Synapse.Apis.Management;

namespace Synapse.Dashboard.Pages.Correlations.Create;

/// <summary>
/// Defines Flux effects for <see cref="CreateCorrelationState"/>-related actions
/// </summary>
[Effect]
public static class Effects
{

    /// <summary>
    /// Handles the specified <see cref="CreateCorrelation"/> action
    /// </summary>
    /// <param name="action">The <see cref="CreateCorrelation"/> action to handle</param>
    /// <param name="context">The current <see cref="IEffectContext"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public static async Task On(CreateCorrelation action, IEffectContext context)
    {
        try
        {
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            var correlation = await api.CreateCorrelationAsync(action.Command);
            context.Services.GetRequiredService<NavigationManager>().NavigateTo($"/correlations/{correlation.Id}");
        }
        catch(Exception ex)
        {
            context.Services.GetRequiredService<ILogger<CreateCorrelation>>().LogError("An error occured while creating a new correlation: {ex}", ex);
        }
    }

}
