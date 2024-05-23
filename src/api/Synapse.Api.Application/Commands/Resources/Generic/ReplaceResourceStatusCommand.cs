namespace Synapse.Api.Application.Commands.Resources.Generic;

/// <summary>
/// Represents the <see cref="ICommand"/> used to replace the status of an <see cref="IResource"/>
/// </summary>
/// <typeparam name="TResource">The type of <see cref="IResource"/> to replace</typeparam>
/// <remarks>
/// Initializes a new <see cref="ReplaceResourceCommand{TResource}"/>
/// </remarks>
/// <param name="resource">The updated <see cref="IResource"/> to replace</param>
public class ReplaceResourceStatusCommand<TResource>(TResource resource)
    : Command<TResource>
    where TResource : class, IResource, new()
{

    /// <summary>
    /// Gets the updated <see cref="IResource"/> to replace
    /// </summary>
    public TResource Resource { get; } = resource;

}

/// <summary>
/// Represents the service used to handle <see cref="ReplaceResourceStatusCommand{TResource}"/>s
/// </summary>
/// <typeparam name="TResource">The type of <see cref="IResource"/> to replace</typeparam>
/// <param name="repository">The service used to manage <see cref="IResource"/>s</param>
public class ReplaceResourceStatusCommandHandler<TResource>(IResourceRepository repository)
    : ICommandHandler<ReplaceResourceStatusCommand<TResource>, TResource>
    where TResource : class, IResource, new()
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult<TResource>> HandleAsync(ReplaceResourceStatusCommand<TResource> command, CancellationToken cancellationToken)
    {
        var resource = await repository.ReplaceStatusAsync(command.Resource, false, cancellationToken).ConfigureAwait(false);
        return new OperationResult<TResource>((int)HttpStatusCode.OK, resource);
    }

}