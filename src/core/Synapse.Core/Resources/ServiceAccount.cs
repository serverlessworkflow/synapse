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

using Neuroglia.Data.Infrastructure.ResourceOriented;

namespace Synapse.Resources;

/// <summary>
/// Represents a resource used to configure a service security account
/// </summary>
[DataContract]
public record ServiceAccount
    : Resource<ServiceAccountSpec>
{

    /// <summary>
    /// Gets the name of the default service account
    /// </summary>
    public const string DefaultServiceAccountName = "default";

    /// <summary>
    /// Gets the <see cref="ServiceAccount"/>'s resource type
    /// </summary>
    public static readonly ResourceDefinitionInfo ResourceDefinition = new ServiceAccountResourceDefinition()!;

    /// <inheritdoc/>
    public ServiceAccount() : base(ResourceDefinition) { }

    /// <inheritdoc/>
    public ServiceAccount(ResourceMetadata metadata, ServiceAccountSpec spec) : base(ResourceDefinition, metadata, spec) { }


}
