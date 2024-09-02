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
using IdentityServer4.Services;
using Synapse.Resources;

namespace Synapse.Api.Application.Services;

/// <summary>
/// Represents a <see cref="ServiceAccount"/>-based <see cref="IProfileService"/> implementation
/// </summary>
/// <param name="resources">The service used to manage <see cref="IResource"/>s</param>
public class ServiceAccountProfileService(IResourceRepository resources)
    : IProfileService
{

    /// <summary>
    /// Gets the service used to manage <see cref="IResource"/>s
    /// </summary>
    protected IResourceRepository Resources { get; } = resources;

    /// <inheritdoc/>
    public virtual async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        if (context.Subject.Identity == null || !context.Subject.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(context.Subject.Identity.Name)) return;
        var qualifiedName = context.Subject.Identity.Name;
        var qualifiedNameComponents = qualifiedName.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (qualifiedNameComponents.Length != 2) return;
        var name = qualifiedNameComponents[0];
        var @namespace = qualifiedNameComponents[1];
        var serviceAccount = await Resources.GetAsync<ServiceAccount>(name, @namespace);
        if (serviceAccount == null) return;
        context.IssuedClaims =
        [
            new(JwtClaimTypes.Subject, serviceAccount.GetQualifiedName()),
            new(JwtClaimTypes.Name, serviceAccount.GetQualifiedName())
        ];
        foreach (var claim in serviceAccount.Spec.Claims) context.IssuedClaims.Add(new(claim.Key, claim.Value));
    }

    /// <inheritdoc/>
    public virtual async Task IsActiveAsync(IsActiveContext context)
    {
        if (context.Subject.Identity == null || !context.Subject.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(context.Subject.Identity.Name)) return;
        var qualifiedName = context.Subject.Identity.Name;
        var qualifiedNameComponents = qualifiedName.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (qualifiedNameComponents.Length != 2) return;
        var name = qualifiedNameComponents[0];
        var @namespace = qualifiedNameComponents[1];
        context.IsActive = await Resources.GetAsync<ServiceAccount>(name, @namespace) != null;
    }

}
