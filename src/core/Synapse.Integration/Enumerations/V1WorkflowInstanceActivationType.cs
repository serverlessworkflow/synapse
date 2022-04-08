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
    /// Enumerates all types of workflow instanciation
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.StringEnumConverterFactory))]
    public enum V1WorkflowInstanceActivationType
    {
        /// <summary>
        /// Indicates that the workflow instance has been created manually
        /// </summary>
        [EnumMember(Value = "manual")]
        Manual = 0,
        /// <summary>
        /// Indicates that the workflow instance has been dynamically created by an event trigger
        /// </summary>
        [EnumMember(Value = "trigger")]
        Trigger = 1,
        /// <summary>
        /// Indicates that the workflow instance has been dynamically created by a CRON schedule
        /// </summary>
        [EnumMember(Value = "cron")]
        Cron = 2,
        /// <summary>
        /// Indicates that the workflow instance has been created by another workflow instance
        /// </summary>
        [EnumMember(Value = "subflow")]
        Subflow = 3
    }

}
