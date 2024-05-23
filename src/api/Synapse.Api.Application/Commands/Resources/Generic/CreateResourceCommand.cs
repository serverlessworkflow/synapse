namespace Synapse.Api.Application.Commands.Resources.Generic;

/// <summary>
/// Represents the command used to create a new <see cref="IResource"/>
/// </summary>
/// <typeparam name="TResource">The type of <see cref="IResource"/> to create</typeparam>
/// <remarks>
/// Initializes a new <see cref="CreateResourceCommand{TResource}"/>
/// </remarks>
/// <param name="resource">The resource to create</param>
public class CreateResourceCommand<TResource>(TResource resource)
    : Command<TResource>
    where TResource : class, IResource, new()
{

    /// <summary>
    /// Gets the resource to create
    /// </summary>
    public TResource Resource { get; } = resource ?? throw new ArgumentNullException(nameof(resource));

}

/// <summary>
/// Represents the service used to handle <see cref="CreateResourceCommand"/>s
/// </summary>
/// <typeparam name="TResource">The type of <see cref="IResource"/> to create</typeparam>
/// <param name="repository">The service used to manage <see cref="IResource"/>s</param>
public class CreateResourceCommandHandler<TResource>(IResourceRepository repository)
    : ICommandHandler<CreateResourceCommand<TResource>, TResource>
    where TResource : class, IResource, new()
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult<TResource>> HandleAsync(CreateResourceCommand<TResource> command, CancellationToken cancellationToken)
    {
        var resource = await repository.AddAsync(command.Resource, false, cancellationToken);
        return new OperationResult<TResource>((int)HttpStatusCode.Created, resource);
    }

}
