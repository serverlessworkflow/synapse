﻿// Copyright © 2024-Present The Synapse Authors
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

using Microsoft.Extensions.Logging;
using Neuroglia.Data.Infrastructure.ResourceOriented;
using Synapse.Resources;
using System.Security.Cryptography;

namespace Synapse.Core.Infrastructure.Services;

/// <summary>
/// Represents the service used to initialize the Synapse resource database
/// </summary>
/// <inheritdoc/>
public class DatabaseInitializer(ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
    : Neuroglia.Data.Infrastructure.ResourceOriented.Services.DatabaseInitializer(loggerFactory, serviceProvider)
{

    /// <inheritdoc/>
    protected override async Task SeedAsync(CancellationToken cancellationToken)
    {
        foreach (var definition in SynapseDefaults.Resources.Definitions.AsEnumerable()) await this.Database.CreateResourceAsync(definition, cancellationToken: cancellationToken).ConfigureAwait(false);
        var keyBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(keyBytes);
        var key = Convert.ToBase64String(keyBytes);
        await this.Database.CreateResourceAsync(new ServiceAccount()
        {
            Metadata = new()
            {
                Namespace = Namespace.DefaultNamespaceName,
                Name = ServiceAccount.DefaultServiceAccountName
            },
            Spec = new()
            {
                Key = key,
                Claims = new Dictionary<string, string>()
                {

                }
            }
        }, false, cancellationToken).ConfigureAwait(false);
    }

}