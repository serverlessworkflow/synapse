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

using Neuroglia.Data.Infrastructure.ResourceOriented;
using ServerlessWorkflow.Sdk.Models.Authentication;
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
    /// <param name="workflow">The <see cref="WorkflowDefinition"/> that defines the <see cref="AuthenticationPolicyDefinition"/> to create a new <see cref="AuthorizationInfo"/> for</param>
    /// <param name="authentication">The <see cref="AuthenticationPolicyDefinition"/> to create a new <see cref="AuthorizationInfo"/> for</param>
    /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="AuthorizationInfo"/> based on the specified <see cref="AuthenticationPolicyDefinition"/></returns>
    public static async Task<AuthorizationInfo> CreateAsync(WorkflowDefinition workflow, AuthenticationPolicyDefinition authentication, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(authentication);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        string scheme, parameter;
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AuthenticationPolicyHandler");
        if (!string.IsNullOrWhiteSpace(authentication.Use))
        {
            if (workflow.Use?.Authentications?.TryGetValue(authentication.Use, out AuthenticationPolicyDefinition? referencedAuthentication) != true || referencedAuthentication == null) throw new NullReferenceException($"Failed to find the specified authentication policy '{authentication.Use}'");
            else authentication = referencedAuthentication;
        }
        var isSecretBased = authentication.TryGetBaseSecret(out var secretName);
        object? authenticationProperties = null;
        if (isSecretBased && !string.IsNullOrWhiteSpace(secretName))
        {
            logger.LogDebug("Authentication is secret based");
            var secretsManager = serviceProvider.GetRequiredService<ISecretsManager>();
            var secrets = await secretsManager.GetSecretsAsync(cancellationToken).ConfigureAwait(false);
            if (!secrets.TryGetValue(secretName, out authenticationProperties) || authenticationProperties == null)
            {
                logger.LogError("Failed to resolve the specified secret '{secret}'", secretName);
                throw new NullReferenceException($"Failed to resolve the specified secret '{secretName}'");
            }
            logger.LogDebug("Authentication secret loaded");
        }
        switch (authentication.Scheme)
        {
            case AuthenticationScheme.Basic:
                if (authentication.Basic == null) throw new Exception("Missing or invalid configuration of the specified authentication scheme");
                var basic = authenticationProperties == null ? authentication.Basic : authenticationProperties.ConvertTo<BasicAuthenticationSchemeDefinition>()!;
                scheme = AuthenticationScheme.Basic;
                parameter = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{basic.Username}:{basic.Password}"));
                break;
            case AuthenticationScheme.Bearer:
                if (authentication.Bearer == null) throw new Exception("Missing or invalid configuration of the specified authentication scheme");
                var bearer = authenticationProperties == null ? authentication.Bearer : authenticationProperties.ConvertTo<BearerAuthenticationSchemeDefinition>()!;
                scheme = AuthenticationScheme.Bearer;
                parameter = bearer.Token ?? throw new Exception("The Bearer token must be set");
                break;
            case AuthenticationScheme.OAuth2:
                if (authentication.OAuth2 == null) throw new Exception("Missing or invalid configuration of the specified authentication scheme");
                var oauth2 = authenticationProperties == null ? authentication.OAuth2 : authenticationProperties.ConvertTo<OAuth2AuthenticationSchemeDefinition>()!;
                scheme = AuthenticationScheme.Bearer;
                var token = await serviceProvider.GetRequiredService<IOAuth2TokenManager>().GetTokenAsync(oauth2, cancellationToken).ConfigureAwait(false) ?? throw new NullReferenceException("Failed to generate an OAUTH2 token");
                parameter = token.AccessToken!;
                break;
            case AuthenticationScheme.OpenIDConnect:
                if (authentication.Oidc == null) throw new Exception("Missing or invalid configuration of the specified authentication scheme");
                var oidc = authenticationProperties == null ? authentication.Oidc : authenticationProperties.ConvertTo<OpenIDConnectSchemeDefinition>()!;
                scheme = AuthenticationScheme.Bearer;
                token = await serviceProvider.GetRequiredService<IOAuth2TokenManager>().GetTokenAsync(oidc, cancellationToken).ConfigureAwait(false) ?? throw new NullReferenceException("Failed to generate an OIDC token");
                parameter = token.AccessToken!;
                break;
            default:
                throw new NotSupportedException($"The specified authentication schema '{authentication.Scheme}' is not supported");
        }
        return new(scheme, parameter);
    }

}
