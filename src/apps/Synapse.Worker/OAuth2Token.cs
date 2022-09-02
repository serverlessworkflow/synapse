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
    /// Describes an OAUTH2 token
    /// </summary>
    public class OAuth2Token
    {

        /// <summary>
        /// Initializes a new <see cref="OAuth2Token"/>
        /// </summary>
        protected OAuth2Token()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the UTC date and time at which the <see cref="OAuth2Token"/> has been created
        /// </summary>
        public virtual DateTime CreatedAt { get; protected set; }

        /// <summary>
        /// Gets the OAUTH2 token type
        /// </summary>
        [Newtonsoft.Json.JsonProperty("token_type")]
        [System.Text.Json.Serialization.JsonPropertyName("token_type")]
        public virtual string? TokenType { get; protected set; }

        /// <summary>
        /// Gets the OAUTH2 token id
        /// </summary>
        [Newtonsoft.Json.JsonProperty("token_id")]
        [System.Text.Json.Serialization.JsonPropertyName("token_id")]
        public virtual string? TokenId { get; protected set; }

        /// <summary>
        /// Gets the OAUTH2 access token
        /// </summary>
        [Newtonsoft.Json.JsonProperty("access_token")]
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public virtual string? AccessToken { get; protected set; }

        /// <summary>
        /// Gets the OAUTH2 refresh token
        /// </summary>
        [Newtonsoft.Json.JsonProperty("refresh_token")]
        [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
        public virtual string? RefreshToken { get; protected set; }

        /// <summary>
        /// Gets the <see cref="OAuth2Token"/> Time To Live, in seconds
        /// </summary>
        [Newtonsoft.Json.JsonProperty("expires_in")]
        [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        public virtual int Ttl { get; protected set; }

        /// <summary>
        /// Gets the UTC date and time at which the <see cref="OAuth2Token"/> expires
        /// </summary>
        [Newtonsoft.Json.JsonProperty("expires_on")]
        [System.Text.Json.Serialization.JsonPropertyName("expires_on")]
        public virtual DateTime ExpiresAt { get; protected set; }

        /// <summary>
        /// Gets a boolean indicating whether or not the <see cref="OAuth2Token"/> has expired
        /// </summary>
        public virtual bool HasExpired => DateTime.UtcNow > this.ExpiresAt;

    }

}
