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

using Newtonsoft.Json.Schema;

namespace Synapse.Dashboard.Services
{

    /// <summary>
    /// Defines the fundamentals of a service used to manage schemas
    /// </summary>
    public interface ISchemaRegistry
    {

        /// <summary>
        /// Gets the <see cref="JSchema"/> at the specified <see cref="Uri"/>
        /// </summary>
        /// <param name="schemaUri">The <see cref="Uri"/> referencing the <see cref="JSchema"/> to get</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The <see cref="JSchema"/> at the specified <see cref="Uri"/></returns>
        Task<JSchema> GetJsonSchemaAsync(string schemaUri, CancellationToken cancellationToken = default);

    }

}
