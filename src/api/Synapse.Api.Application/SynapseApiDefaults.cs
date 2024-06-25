// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using IdentityModel;
using IdentityServer4.Models;

namespace Synapse.Api;

/// <summary>
/// Exposes constants used by the Synapse API
/// </summary>
public class SynapseApiDefaults
{

    /// <summary>
    /// Exposes constants about OIDC related constants
    /// </summary>
    public static class OpenIDConnect
    {

        /// <summary>
        /// Exposes the <see cref="IdentityResource"/>s used by the Synapse API
        /// </summary>
        public static class IdentityResources
        {
            /// <summary>
            /// Gets the OpenID identity resource
            /// </summary>
            public static readonly IdentityResource OpenId = new IdentityServer4.Models.IdentityResources.OpenId();
            /// <summary>
            /// Gets the Profile identity resource
            /// </summary>
            public static readonly IdentityResource Profile = new IdentityServer4.Models.IdentityResources.Profile();

            /// <summary>
            /// Gets an <see cref="IEnumerable{T}"/> containing all default <see cref="IdentityResource"/>s
            /// </summary>
            /// <returns>A new <see cref="IEnumerable{T}"/> containing all default <see cref="IdentityResource"/>s</returns>
            public static IEnumerable<IdentityResource> AsEnumerable()
            {
                yield return OpenId;
                yield return Profile;
            }

        }

        /// <summary>
        /// Exposes the OIDC resources used by the Synapse API
        /// </summary>
        public static class ApiResources
        {

            /// <summary>
            /// Gets the name identity resource
            /// </summary>
            public static readonly ApiResource Api = new("api", [JwtClaimTypes.Subject, JwtClaimTypes.Name, JwtClaimTypes.Email, JwtClaimTypes.Role, JwtClaimTypes.Roles]);

            /// <summary>
            /// Gets an <see cref="IEnumerable{T}"/> containing all default <see cref="ApiResource"/>s
            /// </summary>
            /// <returns>A new <see cref="IEnumerable{T}"/> containing all default <see cref="ApiResource"/>s</returns>
            public static IEnumerable<ApiResource> AsEnumerable()
            {
                yield return Api;
            }

        }

        /// <summary>
        /// Exposes the OIDC scopes used by the Synapse API
        /// </summary>
        public static class ApiScopes
        {

            /// <summary>
            /// Gets the Synapse API scope
            /// </summary>
            public static readonly ApiScope Api = new("api", "Synapse API");

            /// <summary>
            /// Gets an <see cref="IEnumerable{T}"/> containing all default API scopes
            /// </summary>
            /// <returns>A new <see cref="IEnumerable{T}"/> containing all default API scopes</returns>
            public static IEnumerable<ApiScope> AsEnumerable()
            {
                yield return Api;
            }

        }

    }

}
