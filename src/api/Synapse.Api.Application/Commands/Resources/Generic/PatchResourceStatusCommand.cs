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

using Neuroglia.Data.Infrastructure.ResourceOriented;

namespace Synapse.Api.Application.Commands.Resources.Generic;

/// <summary>
/// Represents the <see cref="ICommand"/> used to patch the status of an <see cref="IResource"/>
/// </summary>
/// <typeparam name="TResource">The type of <see cref="IResource"/> to patch</typeparam>
public class PatchResourceStatusCommand<TResource>
    : Command<TResource>
    where TResource : class, IResource, new()
{

    /// <summary>
    /// Initializes a new <see cref="PatchResourceStatusCommand{TResource}"/>
    /// </summary>
    /// <param name="name">The name of the <see cref="IResource"/> to patch</param>
    /// <param name="namespace">The namespace the <see cref="IResource"/> to patch belongs to</param>
    /// <param name="patch">The patch to apply</param>
    /// <param name="resourceVersion">The expected resource version, if any, used for optimistic concurrency</param>
    public PatchResourceStatusCommand(string name, string? @namespace, Patch patch, string? resourceVersion)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        this.Name = name;
        this.Namespace = @namespace;
        this.Patch = patch ?? throw new ArgumentNullException(nameof(patch));
        this.ResourceVersion = resourceVersion;
    }

    /// <summary>
    /// Gets the name of the <see cref="IResource"/> to patch
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the name of the <see cref="IResource"/> to patch
    /// </summary>
    public string? Namespace { get; }

    /// <summary>
    /// Gets the patch to apply
    /// </summary>
    public Patch Patch { get; }

    /// <summary>
    /// Gets the expected resource version, if any, used for optimistic concurrency
    /// </summary>
    public string? ResourceVersion { get; }

}

/// <summary>
/// Represents the service used to handle <see cref="PatchResourceStatusCommand{TResource}"/>s
/// </summary>
/// <typeparam name="TResource">The type of <see cref="IResource"/> to patch</typeparam>
/// <param name="repository">The service used to manage <see cref="IResource"/>s</param>
public class PatchResourceStatusCommandHandler<TResource>(IResourceRepository repository)
    : ICommandHandler<PatchResourceStatusCommand<TResource>, TResource>
    where TResource : class, IResource, new()
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult<TResource>> HandleAsync(PatchResourceStatusCommand<TResource> command, CancellationToken cancellationToken)
    {
        var resource = await repository.PatchStatusAsync<TResource>(command.Patch, command.Name, command.Namespace, command.ResourceVersion, false, cancellationToken).ConfigureAwait(false);
        return this.Ok(resource);
    }

}