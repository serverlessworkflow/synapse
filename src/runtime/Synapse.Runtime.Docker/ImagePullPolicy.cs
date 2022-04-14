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

using System.Runtime.Serialization;

namespace Synapse.Runtime.Docker
{

    /// <summary>
    /// Enumerates all supported Docker image pull policies
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.StringEnumConverterFactory))]
    public enum ImagePullPolicy
    {
        /// <summary>
        /// Indicates that the image should only be pulled when not present
        /// </summary>
        [EnumMember(Value = "IfNotPresent")]
        IfNotPresent,
        /// <summary>
        /// Indicates that the image should always be pulled
        /// </summary>
        [EnumMember(Value = "Always")]
        Always
    }

}
