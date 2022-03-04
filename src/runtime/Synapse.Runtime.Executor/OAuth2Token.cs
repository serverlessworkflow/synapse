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

namespace Synapse.Runtime
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

        }

        /// <summary>
        /// Gets the UTC date and time at which the <see cref="OAuth2Token"/> has been created
        /// </summary>
        public virtual DateTime CreatedAt { get; protected set; }

        /// <summary>
        /// Gets the OAUTH2 access token
        /// </summary>
        public virtual string? AccessToken { get; protected set; }

        /// <summary>
        /// Gets the OAUTH2 refresh token
        /// </summary>
        public virtual string? RefreshToken { get; protected set; }

        /// <summary>
        /// Gets the <see cref="OAuth2Token"/> Time To Live, in seconds
        /// </summary>
        public virtual int Ttl { get; protected set; }

        /// <summary>
        /// Gets the UTC date and time at which the <see cref="OAuth2Token"/> expires
        /// </summary>
        public virtual DateTime ExpiresAt { get; protected set; }

        /// <summary>
        /// Gets a boolean indicating whether or not the <see cref="OAuth2Token"/> has expired
        /// </summary>
        public virtual bool HasExpired => DateTime.UtcNow > this.ExpiresAt;

    }

}
