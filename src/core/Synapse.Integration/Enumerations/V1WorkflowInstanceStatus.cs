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
    /// Enumerates all possible workflow instance statuses
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.StringEnumConverterFactory))]
    public enum V1WorkflowInstanceStatus
    {
        /// <summary>
        /// Indicates that the workflow instance is pending execution
        /// </summary>
        [EnumMember(Value = "pending")]
        Pending,
        /// <summary>
        /// Indicates that the workflow instance is being scheduled
        /// </summary>
        [EnumMember(Value = "scheduling")]
        Scheduling,
        /// <summary>
        /// Indicates that the workflow instance has been scheduled
        /// </summary>
        [EnumMember(Value = "scheduled")]
        Scheduled,
        /// <summary>
        /// Indicates that the workflow instance is starting
        /// </summary>
        [EnumMember(Value = "starting")]
        Starting,
        /// <summary>
        /// Indicates that the workflow instance is running
        /// </summary>
        [EnumMember(Value = "running")]
        Running,
        /// <summary>
        /// Indicates that the execution of the workflow instance is being suspended
        /// </summary>
        [EnumMember(Value = "suspending")]
        Suspending,
        /// <summary>
        /// Indicates that the workflow instance has been suspended
        /// </summary>
        [EnumMember(Value = "suspended")]
        Suspended,
        /// <summary>
        /// Indicates that the execution of the workflow instance is resuming
        /// </summary>
        [EnumMember(Value = "resuming")]
        Resuming,
        /// <summary>
        /// Indicates that the workflow instance has faulted due to an unhandled exception
        /// </summary>
        [EnumMember(Value = "faulted")]
        Faulted,
        /// <summary>
        /// Indicates that the execution of the workflow instance is being cancelled
        /// </summary>
        [EnumMember(Value = "cancelling")]
        Cancelling,
        /// <summary>
        /// Indicates that the workflow instance has been cancelled
        /// </summary>
        [EnumMember(Value = "cancelled")]
        Cancelled,
        /// <summary>
        /// Indicates that the workflow instance ran to completion
        /// </summary>
        [EnumMember(Value = "completed")]
        Completed
    }

}
