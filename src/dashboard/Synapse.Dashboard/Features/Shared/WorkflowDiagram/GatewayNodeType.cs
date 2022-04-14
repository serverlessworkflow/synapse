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

namespace Synapse.Dashboard
{
    /// <summary>
    /// Enumerates all supported types of <see cref="GatewayNodeViewModel"/>s
    /// </summary>
    public enum GatewayNodeType
    {
        /// <summary>
        /// Indicates a cumulative gateway
        /// </summary>
        And,
        /// <summary>
        /// Indicates an exclusive gateway
        /// </summary>
        Xor,
        /// <summary>
        /// Indicates a cumulative gateway for N instances
        /// </summary>
        N
    }

}
