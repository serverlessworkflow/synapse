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
using Neuroglia.Serialization;
using ServerlessWorkflow.Sdk.Models.Authentication;
using System.Collections.Concurrent;

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
    public virtual async Task<OAuth2Token> GetTokenAsync(OAuth2AuthenticationSchemeDefinition configuration, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var tokenKey = $"{configuration.Client.Id}@{configuration.Authority}";
        var properties = new Dictionary<string, string>()
        {
            { "grant_type", configuration.Grant },
            { "client_id", configuration.Client.Id }
        };
        if (!string.IsNullOrWhiteSpace(configuration.Client.Secret)) properties["client_secret"] = configuration.Client.Secret;
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
        if (this.Tokens.TryGetValue(tokenKey, out var token) && token != null)
        {
            if (token.HasExpired
                && !string.IsNullOrWhiteSpace(token.RefreshToken))
            {
                properties["grant_type"] = "refresh_token";
                properties["refresh_token"] = token.RefreshToken;
            }
            else return token;
        }
        var discoveryDocument = await this.HttpClient.GetDiscoveryDocumentAsync(configuration.Authority.ToString(), cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(configuration.Authority, discoveryDocument.TokenEndpoint))
        {
            Content = new FormUrlEncodedContent(properties)
        };
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
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

}
