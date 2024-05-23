namespace Synapse.Api.Http.Controllers;

/// <summary>
/// Represents the <see cref="NamespacedResourceController{TResource}"/> used to manage <see cref="Namespace"/>s
/// </summary>
/// <param name="mediator">The service used to mediate calls</param>
[Route("api/v1/namespaces")]
public class NamespacesController(IMediator mediator)
    : ClusterResourceController<Namespace>(mediator)
{



}
