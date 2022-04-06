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
    /// Enumerates all supported runtime correlation modes
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.StringEnumConverterFactory))]
    public enum RuntimeCorrelationMode
    {
        /// <summary>
        /// Indicates that the runtime actibvely for inbound events for a specified amount of time before shutting down and waiting for a Synapse Correlator to restart it upon consumption of correlated events
        /// </summary>
        [EnumMember(Value = "dual")]
        Dual,
        /// <summary>
        /// Indicates that the runtime actively listens for inbound events
        /// </summary>
        [EnumMember(Value = "active")]
        Active,
        /// <summary>
        /// Indicates that when requested to consume events, the runtime shuts down and waits for a Synapse Correlator to restart it upon consumption of correlated events
        /// </summary>
        [EnumMember(Value = "passive")]
        Passive
    }

}
