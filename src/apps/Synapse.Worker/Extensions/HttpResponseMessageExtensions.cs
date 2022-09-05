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

namespace Synapse.Worker
{
    /// <summary>
    /// Defines extensions for <see cref="HttpResponseMessage"/>s
    /// </summary>
    public static class HttpResponseMessageExtensions
    {

        /// <summary>
        /// Ensures that the <see cref="HttpResponseMessage"/> returned a success status code, and throws if it's not the case
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/></param>
        /// <param name="responseContent">The reponse contents, if any</param>
        /// <exception cref="HttpRequestException">The exception is thrown if the <see cref="HttpResponseMessage"/> has a non-success status code</exception>
        public static void EnsureSuccessStatusCode(this HttpResponseMessage response, string? responseContent)
        {
            if (response.IsSuccessStatusCode)
                return;
            throw new HttpRequestException($"The server responsed with a non-success status code '{(int)response.StatusCode} {response.StatusCode}' to HTTP request {response.RequestMessage!.Method} {response.RequestMessage!.RequestUri}{Environment.NewLine}{(string.IsNullOrWhiteSpace(responseContent) ? "" : responseContent)}", null, response.StatusCode);
        }

    }

}