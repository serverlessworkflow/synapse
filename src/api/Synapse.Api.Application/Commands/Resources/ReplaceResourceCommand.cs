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
/// Represents the <see cref="ICommand"/> used to replace an existing <see cref="IResource"/>
/// </summary>
public class ReplaceResourceCommand
    : Command<IResource>
{

    /// <summary>
    /// Initializes a new <see cref="ReplaceResourceCommand"/>
    /// </summary>
    /// <param name="group">The API group the resource to replace belongs to</param>
    /// <param name="version">The version of the resource to replace</param>
    /// <param name="plural">The plural name of the type of resource to replace</param>
    /// <param name="resource">The updated <see cref="IResource"/> to replace</param>
    public ReplaceResourceCommand(string group, string version, string plural, IResource resource)
    {
        if (string.IsNullOrWhiteSpace(group)) throw new ArgumentNullException(nameof(group));
        if (string.IsNullOrWhiteSpace(version)) throw new ArgumentNullException(nameof(version));
        if (string.IsNullOrWhiteSpace(plural)) throw new ArgumentNullException(nameof(plural));
        this.Group = group;
        this.Version = version;
        this.Plural = plural;
        this.Resource = resource;
    }

    /// <summary>
    /// Gets the API group the resource to replace belongs to
    /// </summary>
    public string Group { get; }

    /// <summary>
    /// Gets the version of the resource to replace
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the plural name of the type of resource to replace
    /// </summary>
    public string Plural { get; }

    /// <summary>
    /// Gets the updated <see cref="IResource"/> to replace
    /// </summary>
    public IResource Resource { get; }

}

/// <summary>
/// Represents the service used to handle <see cref="ReplaceResourceCommand"/>s
/// </summary>
/// <param name="repository">The service used to manage <see cref="IResource"/>s</param>
public class ReplaceResourceCommandHandler(IResourceRepository repository)
    : ICommandHandler<ReplaceResourceCommand, IResource>
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult<IResource>> HandleAsync(ReplaceResourceCommand command, CancellationToken cancellationToken)
    {
        var resource = await repository.ReplaceAsync(command.Resource, command.Group, command.Version, command.Plural, false, cancellationToken).ConfigureAwait(false);
        return new OperationResult<IResource>((int)HttpStatusCode.OK, resource);
    }

}
