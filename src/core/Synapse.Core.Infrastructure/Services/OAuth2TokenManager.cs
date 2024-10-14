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

using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Neuroglia.Serialization;
using ServerlessWorkflow.Sdk;
using ServerlessWorkflow.Sdk.Models.Authentication;
using System.Collections.Concurrent;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;

namespace Synapse.Core.Infrastructure.Services;

/// <summary>
/// Represents the default implementation of the <see cref="IOAuth2TokenManager"/> interface
/// </summary>
/// <param name="logger">The service used to perform logging</param>
/// <param name="jsonSerializer">The service used to serialize/deserialize to/from JSON</param>
/// <param name="httpClient">The service used to perform HTTP requests</param>
public class OAuth2TokenManager(ILogger<OAuth2TokenManager> logger, IJsonSerializer jsonSerializer, HttpClient httpClient)
    : IOAuth2TokenManager
{

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; } = logger;

    /// <summary>
    /// Gets the service used to serialize/deserialize to/from JSON
    /// </summary>
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;

    /// <summary>
    /// Gets the service used to perform HTTP requests
    /// </summary>
    protected HttpClient HttpClient { get; } = httpClient;

    /// <summary>
    /// Gets a <see cref="Dictionary{TKey, TValue}"/> containing all active <see cref="OAuth2Token"/>s
    /// </summary>
    protected ConcurrentDictionary<string, OAuth2Token> Tokens { get; } = [];

    /// <inheritdoc/>
    public virtual async Task<OAuth2Token> GetTokenAsync(OAuth2AuthenticationSchemeDefinitionBase configuration, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var tokenKey = $"{configuration.Client?.Id}@{configuration.Authority}";
        if (this.Tokens.TryGetValue(tokenKey, out var token) && token != null && !token.HasExpired) return token;
        Uri tokenEndpoint;
        if (configuration is OpenIDConnectSchemeDefinition)
        {
            var discoveryRequest = new DiscoveryDocumentRequest()
            {
                Address = configuration.Authority!.OriginalString,
                Policy = new()
                {
                    RequireHttps = false
                }
            };
            var discoveryDocument = await this.HttpClient.GetDiscoveryDocumentAsync(discoveryRequest, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(discoveryDocument.TokenEndpoint)) throw new NullReferenceException($"The token endpoint is not documented by the OIDC discovery document.{(discoveryDocument.IsError ? $" Discovery error [{discoveryDocument.ErrorType}]: {discoveryDocument.Error}" : string.Empty)}");
            tokenEndpoint = new(discoveryDocument.TokenEndpoint!);
        }
        else if (configuration is OAuth2AuthenticationSchemeDefinition oauth2) tokenEndpoint = oauth2.Endpoints.Token;
        else throw new NotSupportedException($"The specified scheme type '{configuration.GetType().FullName}' is not supported in this context");
        var properties = new Dictionary<string, string>()
        {
            { "grant_type", configuration.Grant! }
        };
        switch (configuration.Client?.Authentication)
        {
            case null:
                if(!string.IsNullOrWhiteSpace(configuration.Client?.Id) && !string.IsNullOrWhiteSpace(configuration.Client?.Secret))
                {
                    properties["client_id"] = configuration.Client.Id!;
                    properties["client_secret"] = configuration.Client.Secret!;
                }
                break;
            case OAuth2ClientAuthenticationMethod.Post:
                this.ThrowIfInvalidClientCredentials(configuration.Client);
                properties["client_id"] = configuration.Client.Id!;
                properties["client_secret"] = configuration.Client.Secret!;
                break;
            case OAuth2ClientAuthenticationMethod.JwT:
                this.ThrowIfInvalidClientCredentials(configuration.Client);
                properties["client_assertion_type"] = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
                properties["client_assertion"] = this.CreateClientAssertionJwt(configuration.Client.Id!, tokenEndpoint.OriginalString, new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.Client.Secret!)), SecurityAlgorithms.HmacSha256));
                break;
            case OAuth2ClientAuthenticationMethod.PrivateKey:
                this.ThrowIfInvalidClientCredentials(configuration.Client);
                throw new NotImplementedException(); //todo
            case OAuth2ClientAuthenticationMethod.Basic: 
                break;
            default: throw new NotSupportedException($"The specified OAUTH2 client authentication method '{configuration.Client?.Authentication}' is not supported");
        }
        if (configuration.Scopes?.Count > 0) properties["scope"] = string.Join(" ", configuration.Scopes);
        if (configuration.Audiences?.Count > 0) properties["audience"] = string.Join(" ", configuration.Audiences);
        if (!string.IsNullOrWhiteSpace(configuration.Username)) properties["username"] = configuration.Username;
        if (!string.IsNullOrWhiteSpace(configuration.Password)) properties["password"] = configuration.Password;
        if (configuration.Subject != null)
        {
            properties["subject_token"] = configuration.Subject.Token;
            properties["subject_token_type"] = configuration.Subject.Type;
        }
        if (configuration.Actor != null)
        {
            properties["actor_token"] = configuration.Actor.Token;
            properties["actor_token_type"] = configuration.Actor.Type;
        }
        if (token != null && token.HasExpired && !string.IsNullOrWhiteSpace(token.RefreshToken))
        {
            properties["grant_type"] = "refresh_token";
            properties["refresh_token"] = token.RefreshToken;
        }
        using var content = configuration.Request?.Encoding switch
        {
            null or OAuth2RequestEncoding.FormUrl => (HttpContent)new FormUrlEncodedContent(properties),
            OAuth2RequestEncoding.Json => new StringContent(this.JsonSerializer.SerializeToText(properties), Encoding.UTF8, MediaTypeNames.Application.Json),
            _ => throw new NotSupportedException($"The specified OAUTH2 request encoding '{configuration.Request?.Encoding ?? OAuth2RequestEncoding.FormUrl}' is not supported")
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint) { Content = content };
        if (configuration.Client?.Authentication == OAuth2ClientAuthenticationMethod.Basic)
        {
            this.ThrowIfInvalidClientCredentials(configuration.Client);
            request.Headers.Authorization = new("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{configuration.Client.Id}:{configuration.Client.Secret}")));
        }
        using var response = await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
        {
            this.Logger.LogError("An error occurred while generating a new JWT token: {details}", json);
            response.EnsureSuccessStatusCode();
        }
        token = this.JsonSerializer.Deserialize<OAuth2Token>(json)!;
        this.Tokens[tokenKey] = token;
        return token;
    }

    /// <summary>
    /// Throws a new <see cref="Exception"/> if the specified client credentials have not been properly configured, as required by the configured authentication method
    /// </summary>
    /// <param name="client">The client credentials to validate</param>
    protected virtual void ThrowIfInvalidClientCredentials(OAuth2AuthenticationClientDefinition? client)
    {
        if(string.IsNullOrWhiteSpace(client?.Id) || string.IsNullOrWhiteSpace(client?.Secret)) throw new NullReferenceException($"The client id and client secret must be configured when using the '{client?.Authentication}' OAUTH2 authentication method");
    }

    /// <summary>
    /// Creates a JSON Web Token (JWT) for client authentication using the provided client ID, audience and signing credentials.
    /// </summary>
    /// <param name="clientId">The client ID used as the subject and issuer of the JWT</param>
    /// <param name="audience">The audience for which the JWT is intended, typically the token endpoint URL</param>
    /// <param name="signingCredentials">The credentials used to signed the JWT</param>
    /// <returns>A signed JWT in string format, to be used as a client assertion in OAuth 2.0 requests</returns>
    protected virtual string CreateClientAssertionJwt(string clientId, string audience, SigningCredentials signingCredentials)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(audience);
        ArgumentNullException.ThrowIfNull(signingCredentials);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, clientId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = clientId,
            Audience = audience,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = signingCredentials
        };
        var tokenHandler = new JsonWebTokenHandler();
        return tokenHandler.CreateToken(tokenDescriptor);
    }

}
