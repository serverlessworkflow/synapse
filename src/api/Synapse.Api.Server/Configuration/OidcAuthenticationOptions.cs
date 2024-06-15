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
/// Represents the options used to configure the OIDC authentication of a Synapse API server
/// </summary>
public class OidcAuthenticationOptions
{

    /// <summary>
    /// Gets/sets the uri of the OIDC authority to use
    /// </summary>
    public virtual string Authority { get; set; } = null!;

    /// <summary>
    /// Gets/sets the uri of the OIDC client id to use
    /// </summary>
    public virtual string ClientId { get; set; } = null!;

    /// <summary>
    /// Gets/sets the uri of the OIDC client secret to use
    /// </summary>
    public virtual string? ClientSecret { get; set; } = null!;

    /// <summary>
    /// Gets/sets the uri of the OIDC scope(s) to use
    /// </summary>
    public virtual List<string> Scope { get; set; } = [];

    /// <summary>
    /// Gets or sets the 'resource'.
    /// </summary>
    public string? Resource { get; set; }

    /// <summary>
    /// Gets or sets the 'response_mode'.
    /// </summary>
    public string ResponseMode { get; set; } = "form_post";

    /// <summary>
    /// Gets or sets the 'response_type'.
    /// </summary>
    public string ResponseType { get; set; } = "id_token";

    /// <summary>
    /// Gets/sets a boolean indicating whether or not to use of the Proof Key for Code Exchange (PKCE) standard
    /// </summary>
    public virtual bool UsePkce { get; set; } = true;

    /// <summary>
    /// Gets/sets the key used by the OIDC authority to sign tokens
    /// </summary>
    public virtual string SigningKey { get; set; } = null!;

    /// <summary>
    /// Gets/sets the expected OIDC audience, if any
    /// </summary>
    public virtual string? Audience { get; set; }

    /// <summary>
    /// Gets/sets the expected OIDC issuer, if any
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
