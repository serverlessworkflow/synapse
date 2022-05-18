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

namespace Synapse
{
    /// <summary>
    /// Defines extensions for <see cref="DateTime"/>s
    /// </summary>
    public static class DateTimeExtensions
    {

        /// <summary>
        /// Gets the first day of the week the specified date belongs to
        /// </summary>
        /// <param name="date">The date that belongs to the week to get the first day of</param>
        /// <param name="startOfWeek">The first day of the week. Defaults to <see cref="DayOfWeek.Monday"/></param>
        /// <returns>The first day of the week the specified date belongs to</returns>
        public static DateTime GetFirstDayOfWeek(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// Gets the last day of the week the specified date belongs to
        /// </summary>
        /// <param name="date">The date that belongs to the week to get the last day of</param>
        /// <param name="startOfWeek">The last day of the week. Defaults to <see cref="DayOfWeek.Monday"/></param>
        /// <returns>The last day of the week the specified date belongs to</returns>
        public static DateTime GetLastDayOfWeek(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(6 - diff).Date;
        }

    }

}
