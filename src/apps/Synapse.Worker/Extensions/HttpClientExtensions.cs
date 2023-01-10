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

using System.Net.Http.Headers;

namespace Synapse.Worker
{

    /// <summary>
    /// Defines extensions for <see cref="HttpClient"/>s
    /// </summary>
    public static class HttpClientExtensions
    {

        /// <summary>
        /// Configures the <see cref="HttpClient"/> to use the specified <see cref="AuthorizationInfo"/>
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> to configure</param>
        /// <param name="authorization">An object that describes the authorization mechanism to use</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public static void UseAuthorization(this HttpClient httpClient, AuthorizationInfo? authorization)
        {
            if (authorization == null) return;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authorization.Scheme, authorization.Parameters);
        }

    }

}