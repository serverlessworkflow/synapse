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

namespace Synapse.Api.Server.Services;

/// <summary>
/// Represents the service used to handle the application's static token authentication
/// </summary>
/// <param name="options">The service used to access the current <see cref="StaticBearerAuthenticationOptions"/></param>
/// <param name="logger">The service used to create <see cref="ILogger"/>s</param>
/// <param name="encoder">The service used to encode URLs</param>
public class StaticBearerAuthenticationHandler(IOptionsMonitor<StaticBearerAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<StaticBearerAuthenticationOptions>(options, logger, encoder)
{

    /// <inheritdoc/>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorization = this.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorization)) return Task.FromResult(AuthenticateResult.Fail("Authorization header not found"));
        var token = this.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        if (this.Options.Tokens.TryGetValue(token, out var user))
        {
            var principal = new ClaimsPrincipal(user);
            var ticket = new AuthenticationTicket(principal, this.Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
    }
}