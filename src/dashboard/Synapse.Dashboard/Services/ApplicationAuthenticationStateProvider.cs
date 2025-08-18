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
using Microsoft.AspNetCore.Components.Authorization;
using Synapse.Api.Client.Services;
using Synapse.Dashboard.Components.DocumentDetailsStateManagement;
using System.Security.Claims;

namespace Synapse.Dashboard.Services;

/// <summary>
/// Represents an <see cref="AuthenticationStateProvider"/> implementation used to determine the current authentication state using Synapse Static Bearer Tokens
/// </summary>
/// <param name="logger">The service used to perform logging</param>
/// <param name="api">The service used to interact with the Synapse API</param>
/// <param name="tokenManager">The service used to manage security tokens</param>
/// <param name="navigationManager">The service used to provides an abstraction for querying and managing URI navigation.</param>
public class ApplicationAuthenticationStateProvider(ILogger<ApplicationAuthenticationStateProvider> logger, ISynapseApiClient api, ISecurityTokenManager tokenManager, NavigationManager navigationManager)
    : AuthenticationStateProvider
{

    static readonly ClaimsPrincipal Anonymous = new();

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger<ApplicationAuthenticationStateProvider> Logger { get; } = logger;

    /// <summary>
    /// Gets the service used to interact with the Synapse API
    /// </summary>
    protected ISynapseApiClient Api { get; } = api;

    /// <summary>
    /// Gets the service used to manage security tokens
    /// </summary>
    protected ISecurityTokenManager TokenManager { get; } = tokenManager;

    /// <inheritdoc/>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await this.TokenManager.GetTokenAsync();
            if (string.IsNullOrWhiteSpace(token)) return new AuthenticationState(Anonymous);
            var profile = await this.Api.Users.GetUserProfileAsync();
            var claims = profile.Claims?.Select(c => new Claim(c.Type, c.Value)).ToList() ?? [];
            var identity = new ClaimsIdentity(claims, profile.AuthenticationType, JwtClaimTypes.Name, JwtClaimTypes.Role);
            var principal = new ClaimsPrincipal(identity);
            return new AuthenticationState(principal);
        }
        catch (Exception ex)
        {
            this.Logger.LogError("Unable to get authentication state: {exception}", ex.ToString());
            await this.TokenManager.SetTokenAsync("");
            navigationManager.NavigateTo("/");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    /// <summary>
    /// Notifies the application that its authentication state has changed
    /// </summary>
    public void NotifyNotifyAuthenticationStateChanged() => base.NotifyAuthenticationStateChanged(this.GetAuthenticationStateAsync());

}