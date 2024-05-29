using Synapse.Resources;

namespace Synapse.Api.Http.Controllers;

/// <summary>
/// Represents the <see cref="NamespacedResourceController{TResource}"/> used to manage <see cref="Operator"/>s
/// </summary>
/// <param name="mediator">The service used to mediate calls</param>
[Route("api/v1/operators")]
public class OperatorsController(IMediator mediator)
    : NamespacedResourceController<Operator>(mediator)
{



}
