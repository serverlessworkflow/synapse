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
    /// Enumerates all possibles workflow schedule statuses
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.StringEnumConverterFactory))]
    public enum V1ScheduleStatus
    {
        /// <summary>
        /// Indicates that the schedule is active
        /// </summary>
        [EnumMember(Value = "active")]
        Active = 1,
        /// <summary>
        /// Indicates that the schedule has been suspended
        /// </summary>
        [EnumMember(Value = "suspended")]
        Suspended = 2,
        /// <summary>
        /// Indicates that the schedule has been retired, either manually or automatically because it reached its deadline
        /// </summary>
        [EnumMember(Value = "retired")]
        Retired = 4,
        /// <summary>
        /// Indicates that the schedule has been made obsolete by a newer version
        /// </summary>
        [EnumMember(Value = "obsolete")]
        Obsolete = 8
    }

}
