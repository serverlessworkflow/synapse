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
    /// Enumerates all supported values for a workflow activity status
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.StringEnumConverterFactory))]
    public enum V1WorkflowActivityStatus
    {
        /// <summary>
        /// Indicates that the activity is pending execution
        /// </summary>
        [EnumMember(Value = "pending")]
        Pending,
        /// <summary>
        /// Indicates that the activity is running
        /// </summary>
        [EnumMember(Value = "running")]
        Running,
        /// <summary>
        /// Indicates that the activity has been suspended
        /// </summary>
        [EnumMember(Value = "suspended")]
        Suspended,
        /// <summary>
        /// Indicates that the activity has faulted due to an unhandled exception
        /// </summary>
        [EnumMember(Value = "faulted")]
        Faulted,
        /// <summary>
        /// Indicates that the activity is being compensated
        /// </summary>
        [EnumMember(Value = "compensating")]
        Compensating,
        /// <summary>
        /// Indicates that the activity has been compensated
        /// </summary>
        [EnumMember(Value = "compensated")]
        Compensated,
        /// <summary>
        /// Indicates that the activity has been cancelled
        /// </summary>
        [EnumMember(Value = "cancelled")]
        Cancelled,
        /// <summary>
        /// Indicates that the activity has been skipped
        /// </summary>
        [EnumMember(Value = "skipped")]
        Skipped,
        /// <summary>
        /// Indicates that the activity ran to completion
        /// </summary>
        [EnumMember(Value = "completed")]
        Completed
    }

}
