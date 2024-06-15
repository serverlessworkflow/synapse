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

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Synapse.Api.Server.Configuration;

/// <summary>
/// Represents the <see cref="AuthenticationSchemeOptions"/> used to configure the application's static token based authentication
/// </summary>
public class StaticBearerAuthenticationOptions
    : AuthenticationSchemeOptions
{

    /// <summary>
    /// Gets/sets a token/user mapping of the application's static bearer tokens
    /// </summary>
    public IDictionary<string, ClaimsIdentity> Tokens { get; set; } = new Dictionary<string, ClaimsIdentity>();

    /// <summary>
    /// Adds a new token for the specified <see cref="ClaimsIdentity"/>
    /// </summary>
    /// <param name="token">The token to add</param>
    /// <param name="identity">The <see cref="ClaimsIdentity"/> to build and add a new token for</param>
    /// <returns>The token generated for the specified <see cref="ClaimsIdentity"/></returns>
    public virtual string AddToken(string token, ClaimsIdentity identity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        ArgumentNullException.ThrowIfNull(identity);
        this.Tokens[token] = identity;
        return token;
    }

    /// <summary>
    /// Adds a new token for the specified <see cref="ClaimsIdentity"/>
    /// </summary>
    /// <param name="identity">The <see cref="ClaimsIdentity"/> to build and add a new token for</param>
    /// <returns>The token generated for the specified <see cref="ClaimsIdentity"/></returns>
    public virtual string AddToken(ClaimsIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = identity,
            Issuer = "synapse-api",
            Audience = "synapse-api"
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var encoded = tokenHandler.WriteToken(token);
        this.Tokens[encoded] = identity;
        return encoded;
    }

}
