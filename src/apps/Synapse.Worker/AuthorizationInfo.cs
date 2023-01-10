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
using System.Text;

namespace Synapse.Worker;

/// <summary>
/// Describes the credentials used to authenticate a user agent with an application
/// </summary>
public class AuthorizationInfo
{

    /// <summary>
    /// Initializes a new <see cref="AuthorizationInfo"/>
    /// </summary>
    public AuthorizationInfo() { }

    /// <summary>
    /// Initializes a new <see cref="AuthorizationInfo"/>
    /// </summary>
    /// <param name="scheme">The authorization scheme</param>
    /// <param name="parameters">The authorization parameters</param>
    public AuthorizationInfo(string scheme, string parameters)
    {
        this.Scheme = scheme;
        this.Parameters = parameters;
    }

    /// <summary>
    /// Gets the authorization scheme
    /// </summary>
    public virtual string Scheme { get; set; } = null!;

    /// <summary>
    /// Gets the authorization parameters
    /// </summary>
    public virtual string Parameters { get; set; } = null!;

    /// <inheritdoc/>
    public override string ToString() => $"{this.Scheme} {this.Parameters}";

    /// <summary>
    /// Creates a new <see cref="AuthorizationInfo"/> from the specified <see cref="AuthenticationDefinition"/>
    /// </summary>
    /// <param name="serviceProvider">The current <see cref="ISerializerProvider"/></param>
    /// <param name="authentication">The <see cref="AuthenticationDefinition"/> that describes the authentication mechanism to use</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="AuthorizationInfo"/></returns>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    public static async Task<AuthorizationInfo?> CreateAsync(IServiceProvider serviceProvider, AuthenticationDefinition? authentication, CancellationToken cancellationToken = default)
    {
        if (authentication == null) return null;
        string scheme;
        string value;
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
                if (token == null) throw new NullReferenceException($"Failed to generate an OAUTH2 token");
                value = token.AccessToken!;
                break;
            default:
                throw new NotSupportedException($"The specified authentication schema '{EnumHelper.Stringify(authentication.Scheme)}' is not supported");
        }
        return new(scheme, value);
    }

}