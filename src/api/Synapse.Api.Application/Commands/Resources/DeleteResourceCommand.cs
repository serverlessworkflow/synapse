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

namespace Synapse.Api.Application.Commands.Resources;

/// <summary>
/// Represents the <see cref="ICommand"/> used to delete an existing <see cref="IResource"/>
/// </summary>
public class DeleteResourceCommand
    : Command<IResource>
{

    /// <summary>
    /// Initializes a new <see cref="DeleteResourceCommand"/>
    /// </summary>
    /// <param name="group">The API group the resource to delete belongs to</param>
    /// <param name="version">The version of the resource to delete</param>
    /// <param name="plural">The plural name of the type of resource to delete</param>
    /// <param name="name">The name of the <see cref="IResource"/> to delete</param>
    /// <param name="namespace">The namespace the <see cref="IResource"/> to delete belongs to</param>
    /// <param name="dryRun">A boolean indicating whether or not to persist changes</param>
    public DeleteResourceCommand(string group, string version, string plural, string name, string? @namespace, bool dryRun)
    {
        if (string.IsNullOrWhiteSpace(group)) throw new ArgumentNullException(nameof(group));
        if (string.IsNullOrWhiteSpace(version)) throw new ArgumentNullException(nameof(version));
        if (string.IsNullOrWhiteSpace(plural)) throw new ArgumentNullException(nameof(plural));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        this.Group = group;
        this.Version = version;
        this.Plural = plural;
        this.Name = name;
        this.Namespace = @namespace;
        this.DryRun = dryRun;
    }

    /// <summary>
    /// Gets the API group the resource to delete belongs to
    /// </summary>
    public string Group { get; }

    /// <summary>
    /// Gets the version of the resource to delete
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the plural name of the type of resource to delete
    /// </summary>
    public string Plural { get; }

    /// <summary>
    /// Gets the name of the <see cref="IResource"/> to delete
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the namespace the <see cref="IResource"/> to delete belongs to
    /// </summary>
    public string? Namespace { get; }

    /// <summary>
    /// Gets a boolean indicating whether or not to persist changes
    /// </summary>
    public bool DryRun { get; }

}

/// <summary>
/// Represents the service used to handle <see cref="DeleteResourceCommand"/>s
/// </summary>
/// <param name="repository">The service used to manage <see cref="IResource"/>s</param>
public class DeleteResourceCommandHandler(IResourceRepository repository)
    : ICommandHandler<DeleteResourceCommand, IResource>
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult<IResource>> HandleAsync(DeleteResourceCommand command, CancellationToken cancellationToken)
    {
        var resource = await repository.RemoveAsync(command.Group, command.Version, command.Plural, command.Name, command.Namespace, command.DryRun, cancellationToken).ConfigureAwait(false);
        return this.Ok(resource);
    }

}