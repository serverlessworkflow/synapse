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

using Synapse.Core.Infrastructure.Services;
using ServerlessWorkflow.Sdk;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Synapse;

/// <summary>
/// Describes the credentials used to authenticate a user agent with an application
/// </summary>
/// <param name="scheme">The authorization scheme</param>
/// <param name="parameter">The authorization parameter</param>
public class AuthorizationInfo(string scheme, string parameter)
{

    /// <summary>
    /// Gets the authorization scheme
    /// </summary>
    public virtual string Scheme { get; set; } = scheme;

    /// <summary>
    /// Gets the authorization parameter
    /// </summary>
    public virtual string Parameter { get; set; } = parameter;

    /// <inheritdoc/>
    public override string ToString() => $"{Scheme} {Parameter}";

    /// <summary>
    /// Creates a new <see cref="AuthorizationInfo"/> based on the specified <see cref="AuthenticationPolicyDefinition"/>
    /// </summary>
    /// <param name="authentication">The <see cref="AuthenticationPolicyDefinition"/> to create a new <see cref="AuthorizationInfo"/> for</param>
    /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="AuthorizationInfo"/> based on the specified <see cref="AuthenticationPolicyDefinition"/></returns>
    public static async Task<AuthorizationInfo> CreateAsync(AuthenticationPolicyDefinition authentication, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(nameof(authentication));
        ArgumentNullException.ThrowIfNull(nameof(serviceProvider));
        string scheme, parameter;
        switch (authentication.Scheme)
        {
            case AuthenticationScheme.Basic:
                if (authentication.Basic == null) throw new Exception("Missing or invalid configuration of the specified authentication scheme");
                scheme = AuthenticationScheme.Basic;
                parameter = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{authentication.Basic.Username}:{authentication.Basic.Password}"));
                break;
            case AuthenticationScheme.Bearer:
                if (authentication.Bearer == null) throw new Exception("Missing or invalid configuration of the specified authentication scheme");
                scheme = AuthenticationScheme.Basic;
                parameter = authentication.Bearer.Token;
                break;
            case AuthenticationScheme.OAuth2:
                if (authentication.OAuth2 == null) throw new Exception("Missing or invalid configuration of the specified authentication scheme");
                scheme = AuthenticationScheme.Bearer;
                var token = await serviceProvider.GetRequiredService<IOAuth2TokenManager>().GetTokenAsync(authentication.OAuth2, cancellationToken).ConfigureAwait(false) ?? throw new NullReferenceException($"Failed to generate an OAUTH2 token");
                parameter = token.AccessToken!;
                break;
            default:
                throw new NotSupportedException($"The specified authentication schema '{authentication.Scheme}' is not supported");
        }
        return new(scheme, parameter);
    }

}
