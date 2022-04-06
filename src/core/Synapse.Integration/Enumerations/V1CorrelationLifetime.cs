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
    /// Enumerates all supported correlation modes
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.StringEnumConverterFactory))]
    public enum V1CorrelationLifetime
    {
        /// <summary>
        /// Indicates that the correlation is a singleton, and will be disposed of upon release of the single context it is bound to
        /// </summary>
        [EnumMember(Value = "singleton")]
        Singleton,
        /// <summary>
        /// Indicates that the correlation is transient, and a new context instance is expected to be created for each event matching any of its conditions
        /// </summary>
        [EnumMember(Value = "transient")]
        Transient
    }

}
