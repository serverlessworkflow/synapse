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

using Cronos;

namespace Synapse
{

    /// <summary>
    /// Defines extensuons for <see cref="ScheduleDefinition"/>s
    /// </summary>
    public static class ScheduleDefinitionExtensions
    {

        /// <summary>
        /// Gets the next occured of the specified <see cref="ScheduleDefinition"/>
        /// </summary>
        /// <param name="scheduleDefinition">The <see cref="ScheduleDefinition"/> to get the next occurence for</param>
        /// <param name="lastOccurence">The <see cref="ScheduleDefinition"/>'s next occurence</param>
        /// <returns>The date and time at which the <see cref="ScheduleDefinition"/> will next occur, if any</returns>
        public static DateTimeOffset? GetNextOccurence(this ScheduleDefinition scheduleDefinition, DateTimeOffset? lastOccurence = null)
        {
            TimeZoneInfo timeZone = null;
            if (scheduleDefinition.Timezone != null && TimeZoneInfo.TryConvertIanaIdToWindowsId(scheduleDefinition.Timezone, out var timeZoneId)) timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            if (timeZone == null) timeZone = TimeZoneInfo.Local;
            var validUntilDateTime = scheduleDefinition.Cron.ValidUntil;
            var validUntilDateTimeOffset = (DateTimeOffset?)null;
            if (validUntilDateTime.HasValue) validUntilDateTimeOffset = new(validUntilDateTime.Value, timeZone.GetUtcOffset(validUntilDateTime.Value));
            if (lastOccurence == null) lastOccurence = DateTimeOffset.Now;
            if (scheduleDefinition.Cron != null)
            {
                if(validUntilDateTimeOffset.HasValue && DateTimeOffset.Now >= validUntilDateTimeOffset) return null;
                else return CronExpression.Parse(scheduleDefinition.Cron.Expression).GetNextOccurrence(lastOccurence.Value, timeZone);
            }
            else if (scheduleDefinition.Interval != null) return lastOccurence.Value.Add(scheduleDefinition.Interval.Value);
            else return null;
        }

    }

}
