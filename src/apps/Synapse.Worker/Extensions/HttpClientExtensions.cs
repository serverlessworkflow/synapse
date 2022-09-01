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

using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Text;

namespace Synapse.Worker
{

    /// <summary>
    /// Defines extensions for <see cref="HttpClient"/>s
    /// </summary>
    public static class HttpClientExtensions
    {

        /// <summary>
        /// Configures the <see cref="HttpClient"/> to use the specified <see cref="AuthenticationDefinition"/>
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> to configure</param>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        /// <param name="authentication">The <see cref="AuthenticationDefinition"/> that describes how to configure authorization</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public static async Task ConfigureAuthorizationAsync(this HttpClient httpClient, IServiceProvider serviceProvider, AuthenticationDefinition? authentication, CancellationToken cancellationToken = default)
        {
            if (authentication == null)
                return;
            string? scheme;
            string? value;
            switch (authentication.Properties)
            {
                case BasicAuthenticationProperties basic:
                    scheme = "Basic";
                    value = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{basic.Username}:{basic.Password}"));
                    break;
                case BearerAuthenticationProperties bearer:
                    scheme = "Bearer";
                    value = bearer.Token;
                    break;
                case OAuth2AuthenticationProperties oauth:
                    scheme = "Bearer";
                    var token = await serviceProvider.GetRequiredService<IOAuth2TokenManager>().GetTokenAsync(oauth, cancellationToken);
                    if (token == null)
                        throw new NullReferenceException($"Failed to generate an OAUTH2 token");
                    value = token.AccessToken;
                    break;
                default:
                    throw new NotSupportedException($"The specified authentication schema '{EnumHelper.Stringify(authentication.Scheme)}' is not supported");
            }
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, value);
        }

    }

}