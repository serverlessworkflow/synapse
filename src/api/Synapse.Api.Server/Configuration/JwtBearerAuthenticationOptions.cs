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
using System.Security.Cryptography;

namespace Synapse.Api.Server.Configuration;

/// <summary>
/// Represents the options used to configure the application's JWT authentication
/// </summary>
public class JwtBearerAuthenticationOptions
{

    /// <summary>
    /// Gets/sets the uri of the JWT authority to use
    /// </summary>
    public virtual string Authority { get; set; } = null!;

    /// <summary>
    /// Gets/sets the application's required JWT audience, if any
    /// </summary>
    public virtual string? Audience { get; set; }

    /// <summary>
    /// Gets/sets the key used by the JWT authority to sign tokens
    /// </summary>
    public virtual string SigningKey { get; set; } = null!;

    /// <summary>
    /// Gets/sets the expected JWT issuer, if any
    /// </summary>
    public virtual string? Issuer { get; set; }

    /// <summary>
    /// Gets the configured issuer signing key
    /// </summary>
    /// <returns>A new <see cref="SecurityKey"/></returns>
    public virtual SecurityKey GetSigningKey()
    {
        var keyBytes = Convert.FromBase64String(Environment.GetEnvironmentVariable("JWT_SIGNING_KEY")!);
        var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);
        return new RsaSecurityKey(rsa);
    }

}
