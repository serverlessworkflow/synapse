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
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Synapse.Resources;

namespace Synapse.Api.Application.Services;

/// <summary>
/// Represents a <see cref="ServiceAccount"/>-based <see cref="IClientStore"/> implementation
/// </summary>
/// <param name="resources">The service used to manage <see cref="IResource"/>s</param>
public class ServiceAccountClientStore(IResourceRepository resources)
    : IClientStore
{

    /// <summary>
    /// Gets the service used to manage <see cref="IResource"/>s
    /// </summary>
    protected IResourceRepository Resources { get; } = resources;

    /// <inheritdoc/>
    public virtual async Task<IdentityServer4.Models.Client> FindClientByIdAsync(string clientId)
    {
        var qualifiedNameComponents = clientId.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (qualifiedNameComponents.Length != 2) return null!;
        var name = qualifiedNameComponents[0];
        var @namespace = qualifiedNameComponents[1];
        var serviceAccount = await Resources.GetAsync<ServiceAccount>(name, @namespace);
        if (serviceAccount == null) return null!;
        return new()
        {
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientId = clientId,
            ClientSecrets = [new(serviceAccount.Spec.Key.Sha256())],
            Claims = 
            [
                new ClientClaim(JwtClaimTypes.Name, clientId),
                .. serviceAccount.Spec.Claims.Select(c => new ClientClaim(c.Key, c.Value))
            ],
            AlwaysSendClientClaims = true,
            ClientClaimsPrefix = null,
            AllowedScopes = ["api"]
        };
    }

}