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

namespace Synapse.Dashboard.Pages.Schedules.View
{

    /// <summary>
    /// Defines Flux effects for <see cref="ScheduleViewState"/>-related Flux actions
    /// </summary>
    [Effect]
    public static class Effects
    {

        /// <summary>
        /// Handles the specified <see cref="GetScheduleById"/>
        /// </summary>
        /// <param name="action">The <see cref="GetScheduleById"/> to handle</param>
        /// <param name="context">The current <see cref="IEffectContext"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public static async Task On(GetScheduleById action, IEffectContext context)
        {
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            var schedule = await api.GetScheduleByIdAsync(action.Id);
            context.Dispatcher.Dispatch(new HandleGetScheduleByIdResult(schedule));
        }

        /// <summary>
        /// Handles the specified <see cref="TriggerSchedule"/>
        /// </summary>
        /// <param name="action">The <see cref="TriggerSchedule"/> to handle</param>
        /// <param name="context">The current <see cref="IEffectContext"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public static async Task On(TriggerSchedule action, IEffectContext context)
        {
            var logger = context.Services.GetRequiredService<ILogger<TriggerSchedule>>();
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            try
            {
                await api.TriggerScheduleAsync(action.Id);
                context.Dispatcher.Dispatch(new GetScheduleById(action.Id));
            }
            catch (Exception ex)
            {
                logger.LogError("An error occured while triggering a new occurence of the schedule with id '{scheduleId}': {ex}", action.Id, ex);
            }
        }

        /// <summary>
        /// Handles the specified <see cref="SuspendSchedule"/>
        /// </summary>
        /// <param name="action">The <see cref="SuspendSchedule"/> to handle</param>
        /// <param name="context">The current <see cref="IEffectContext"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public static async Task On(SuspendSchedule action, IEffectContext context)
        {
            var logger = context.Services.GetRequiredService<ILogger<SuspendSchedule>>();
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            try
            {
                await api.SuspendScheduleAsync(action.Id);
                context.Dispatcher.Dispatch(new GetScheduleById(action.Id));
            }
            catch(Exception ex)
            {
                logger.LogError("An error occured while suspending the schedule with id '{scheduleId}': {ex}", action.Id, ex);
            }
        }

        /// <summary>
        /// Handles the specified <see cref="ResumeSchedule"/>
        /// </summary>
        /// <param name="action">The <see cref="SuspendSchedule"/> to handle</param>
        /// <param name="context">The current <see cref="IEffectContext"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public static async Task On(ResumeSchedule action, IEffectContext context)
        {
            var logger = context.Services.GetRequiredService<ILogger<ResumeSchedule>>();
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            try
            {
                await api.ResumeScheduleAsync(action.Id);
                context.Dispatcher.Dispatch(new GetScheduleById(action.Id));
            }
            catch (Exception ex)
            {
                logger.LogError("An error occured while resuming the schedule with id '{scheduleId}': {ex}", action.Id, ex);
            }
        }

        /// <summary>
        /// Handles the specified <see cref="SuspendSchedule"/>
        /// </summary>
        /// <param name="action">The <see cref="SuspendSchedule"/> to handle</param>
        /// <param name="context">The current <see cref="IEffectContext"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public static async Task On(RetireSchedule action, IEffectContext context)
        {
            var logger = context.Services.GetRequiredService<ILogger<RetireSchedule>>();
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            try
            {
                await api.RetireScheduleAsync(action.Id);
                context.Dispatcher.Dispatch(new GetScheduleById(action.Id));
            }
            catch (Exception ex)
            {
                logger.LogError("An error occured while retiring the schedule with id '{scheduleId}': {ex}", action.Id, ex);
            }
        }

        /// <summary>
        /// Handles the specified <see cref="DeleteSchedule"/>
        /// </summary>
        /// <param name="action">The <see cref="DeleteSchedule"/> to handle</param>
        /// <param name="context">The current <see cref="IEffectContext"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public static async Task On(DeleteSchedule action, IEffectContext context)
        {
            var logger = context.Services.GetRequiredService<ILogger<DeleteSchedule>>();
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            try
            {
                await api.DeleteScheduleAsync(action.Id);
                context.Dispatcher.Dispatch(new HandleScheduleDeleted());
            }
            catch (Exception ex)
            {
                logger.LogError("An error occured while deleting the schedule with id '{scheduleId}': {ex}", action.Id, ex);
            }
        }

    }

}
