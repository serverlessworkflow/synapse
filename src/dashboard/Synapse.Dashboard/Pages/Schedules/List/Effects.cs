/*
 * Copyright © 2022-Present The Synapse Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Neuroglia.Data.Flux;
using Synapse.Apis.Management;

namespace Synapse.Dashboard.Pages.Schedules.List;

/// <summary>
/// Defines Flux effects that apply to <see cref="ScheduleCollectionState"/>-related actions
/// </summary>
[Effect]
public static class ScheduleCollectionStateEffects
{

    /// <summary>
    /// Handles the specified <see cref="QueryScheduledWorkflows"/>
    /// </summary>
    /// <param name="action">The <see cref="QueryScheduledWorkflows"/> to handle</param>
    /// <param name="context">The current <see cref="IEffectContext"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public static async Task On(QueryScheduledWorkflows action, IEffectContext context)
    {
        var api = context.Services.GetRequiredService<ISynapseManagementApi>();
        var workflows = await api.GetWorkflowsAsync(action.Query);
        workflows = workflows
            .Where(w => w.Definition.Start != null && w.Definition.Start.Schedule != null)
            .GroupBy(w => w.Definition.Id)
            .Select(w => w.OrderByDescending(w => w.Definition.Version).First())
            .ToList();
        context.Dispatcher.Dispatch(new HandleScheduledWorkflowQueryResults(workflows));
    }

}
