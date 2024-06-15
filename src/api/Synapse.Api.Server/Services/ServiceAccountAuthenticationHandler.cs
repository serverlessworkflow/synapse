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
using Neuroglia.Data.Infrastructure.ResourceOriented;
using Neuroglia.Data.Infrastructure.ResourceOriented.Services;
using Synapse.Resources;
using System.IdentityModel.Tokens.Jwt;

namespace Synapse.Api.Server.Services;

/// <summary>
/// Represents the service used to handle the application's static token authentication
/// </summary>
/// <param name="options">The service used to access the current <see cref="ServiceAccountAuthenticationOptions"/></param>
/// <param name="logger">The service used to create <see cref="ILogger"/>s</param>
/// <param name="encoder">The service used to encode URLs</param>
public class ServiceAccountAuthenticationHandler(IOptionsMonitor<ServiceAccountAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<ServiceAccountAuthenticationOptions>(options, logger, encoder)
{

    /// <inheritdoc/>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorization = this.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorization)) return AuthenticateResult.Fail("Authorization header not found");
        var token = this.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var serviceAccountClaim = jwt.Claims.FirstOrDefault(c => c.Type == SynapseDefaults.Claims.ServiceAccount);
        if (serviceAccountClaim == null) return AuthenticateResult.Fail("The token has not been issued by an authenticated service");
        var qualifiedNameComponents = serviceAccountClaim.Value.Split('.', StringSplitOptions.RemoveEmptyEntries);
        var serviceName = qualifiedNameComponents[0];
        var serviceNamespace = qualifiedNameComponents[1];
        var serviceAccount = await this.Context.RequestServices.GetRequiredService<IResourceRepository>().GetAsync<ServiceAccount>(serviceName, serviceNamespace);
        if (serviceAccount == null) return AuthenticateResult.Fail($"Failed to find the specified ServiceAccount '{serviceName}.{serviceNamespace}'");
        var validationResult = await handler.ValidateTokenAsync(token, new()
        {
            ValidIssuer = serviceName,
            ValidateIssuer = true,
            ValidAudience = SynapseDefaults.Audiences.Api,
            ValidateAudience = true,
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(serviceAccount.Spec.Key)),
            NameClaimType = JwtClaimTypes.Name,
            RoleClaimType = JwtClaimTypes.Role
        }).ConfigureAwait(false);
        if (!validationResult.IsValid) return AuthenticateResult.Fail("Invalid token");
        var identity = new ClaimsIdentity(jwt.Claims, this.Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, this.Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }

}
