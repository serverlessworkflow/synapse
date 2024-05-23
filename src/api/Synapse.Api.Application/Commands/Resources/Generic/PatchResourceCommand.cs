namespace Synapse.Api.Application.Commands.Resources.Generic;

/// <summary>
/// Represents the <see cref="ICommand"/> used to patch an existing <see cref="IResource"/>
/// </summary>
/// <typeparam name="TResource">The type of <see cref="IResource"/> to patch</typeparam>
public class PatchResourceCommand<TResource>
    : Command<TResource>
    where TResource : class, IResource, new()
{

    /// <summary>
    /// Initializes a new <see cref="PatchResourceCommand{TResource}"/>
    /// </summary>
    /// <param name="name">The name of the <see cref="IResource"/> to patch</param>
    /// <param name="namespace">The namespace the <see cref="IResource"/> to patch belongs to</param>
    /// <param name="patch">The patch to apply</param>
    public PatchResourceCommand(string name, string? @namespace, Patch patch)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        this.Name = name;
        this.Namespace = @namespace;
        this.Patch = patch ?? throw new ArgumentNullException(nameof(patch));
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

}

/// <summary>
/// Represents the service used to handle <see cref="PatchResourceCommand"/>s
/// </summary>
/// <typeparam name="TResource">The type of <see cref="IResource"/> to patch</typeparam>
/// <param name="repository">The service used to manage <see cref="IResource"/>s</param>
public class PatchResourceCommandHandler<TResource>(IResourceRepository repository)
    : ICommandHandler<PatchResourceCommand<TResource>, TResource>
    where TResource : class, IResource, new()
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult<TResource>> HandleAsync(PatchResourceCommand<TResource> command, CancellationToken cancellationToken)
    {
        var resource = await repository.PatchAsync<TResource>(command.Patch, command.Name, command.Namespace, false, cancellationToken).ConfigureAwait(false);
        return this.Ok(resource);
    }

}
