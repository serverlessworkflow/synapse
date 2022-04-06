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
    /// Defines extensions for <see cref="OAuth2AuthenticationProperties"/>
    /// </summary>
    public static class OAuth2AuthenticationPropertiesExtensions
    {

        /// <summary>
        /// Converts the <see cref="OAuth2AuthenticationProperties"/> into a new <see cref="IDictionary{TKey, TValue}"/>
        /// </summary>
        /// <param name="oauth2">The <see cref="OAuth2AuthenticationProperties"/> to convert</param>
        /// <returns>A new <see cref="IDictionary{TKey, TValue}"/></returns>
        public static IDictionary<string, string> ToDictionary(this OAuth2AuthenticationProperties oauth2)
        {
            var properties = new Dictionary<string, string>();
            if (oauth2.Audiences != null)
                properties.Add("audience", string.Join(" ", oauth2.Audiences));
            if (!string.IsNullOrWhiteSpace(oauth2.ClientId))
                properties.Add("client_id", oauth2.ClientId);
            if (!string.IsNullOrWhiteSpace(oauth2.ClientSecret))
                properties.Add("client_secret", oauth2.ClientSecret);
            properties.Add("grant_type", EnumHelper.Stringify(oauth2.GrantType));
            if (!string.IsNullOrWhiteSpace(oauth2.Username))
                properties.Add("username", oauth2.Username);
            if (!string.IsNullOrWhiteSpace(oauth2.Password))
                properties.Add("password", oauth2.Password);
            if (oauth2.Scopes != null)
                properties.Add("scope", string.Join(" ", oauth2.Scopes));
            return properties;
        }

    }

}
