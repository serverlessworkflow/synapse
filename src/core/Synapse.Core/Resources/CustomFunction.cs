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
/// Represents the resource used to describe and configure a custom, callable task
/// </summary>
[DataContract]
public record CustomFunction
    : Resource<CustomFunctionSpec>
{

    /// <summary>
    /// Gets the <see cref="CustomFunction"/>'s resource type
    /// </summary>
    public static readonly ResourceDefinitionInfo ResourceDefinition = new CustomFunctionResourceDefinition()!;

    /// <inheritdoc/>
    public CustomFunction() : base(ResourceDefinition) { }

    /// <inheritdoc/>
    public CustomFunction(ResourceMetadata metadata, CustomFunctionSpec spec) : base(ResourceDefinition, metadata, spec) { }

}
