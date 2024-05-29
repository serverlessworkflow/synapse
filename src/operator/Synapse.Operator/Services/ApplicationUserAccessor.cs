using Neuroglia.Security.Services;
using System.Security.Claims;

namespace Synapse.Operator.Services;

/// <summary>
/// Represents an <see cref="IUserAccessor"/> implementation used to access the user that represents the executing application
/// </summary>
public class ApplicationUserAccessor
    : IUserAccessor
{

    /// <inheritdoc/>
    public ClaimsPrincipal? User => new(new ClaimsIdentity());

}
