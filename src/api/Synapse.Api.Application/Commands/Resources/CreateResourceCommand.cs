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
/// Represents the command used to create a new <see cref="IResource"/>
/// </summary>
public class CreateResourceCommand
    : Command<IResource>
{

    /// <summary>
    /// Initializes a new <see cref="CreateResourceCommand"/>
    /// </summary>
    /// <param name="resource">The resource to create</param>
    /// <param name="group">The API group the resource to create belongs to</param>
    /// <param name="version">The version of the resource to create</param>
    /// <param name="plural">The plural name of the type of resource to create</param>
    /// <param name="dryRun">A boolean indicating whether or not to persist the changes induces by the command</param>
    public CreateResourceCommand(IResource resource, string group, string version, string plural, bool dryRun)
    {
        if (string.IsNullOrWhiteSpace(group)) throw new ArgumentNullException(nameof(group));
        if (string.IsNullOrWhiteSpace(version)) throw new ArgumentNullException(nameof(version));
        if (string.IsNullOrWhiteSpace(plural)) throw new ArgumentNullException(nameof(plural));
        this.Resource = resource ?? throw new ArgumentNullException(nameof(resource));
 
        this.Group = group;
        this.Version = version;
        this.Plural = plural;
        this.DryRun = dryRun;
    }

    /// <summary>
    /// Gets the resource to create
    /// </summary>
    public IResource Resource { get; }

    /// <summary>
    /// Gets the API group the resource to create belongs to
    /// </summary>
    public string Group { get; }

    /// <summary>
    /// Gets the version of the resource to create
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the plural name of the type of resource to create
    /// </summary>
    public string Plural { get; }

    /// <summary>
    /// Gets a boolean indicating whether or not to persist the changes induces by the command
    /// </summary>
    public bool DryRun { get; }

}

/// <summary>
/// Represents the service used to handle <see cref="CreateResourceCommand"/>s
/// </summary>
/// <param name="repository">The service used to manage <see cref="IResource"/>s</param>
public class CreateResourceCommandHandler(IResourceRepository repository)
    : ICommandHandler<CreateResourceCommand, IResource>
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult<IResource>> HandleAsync(CreateResourceCommand command, CancellationToken cancellationToken)
    {
        if (command.Resource.GetName().Trim().EndsWith('-')) command.Resource.Metadata.Name = $"{command.Resource.GetName().Trim()}{Guid.NewGuid().ToString("N")[..15]}";
        var resource = await repository.AddAsync(command.Resource, command.Group, command.Version, command.Plural, command.DryRun, cancellationToken);
        return new OperationResult<IResource>((int)HttpStatusCode.Created, resource);
    }

}
