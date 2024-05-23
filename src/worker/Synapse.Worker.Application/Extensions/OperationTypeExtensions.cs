using Microsoft.OpenApi.Models;

namespace Synapse.Worker;

/// <summary>
/// Defines extensions for <see cref="HttpMethod"/>s
/// </summary>
public static class OperationTypeExtensions
{

    /// <summary>
    /// Converts the <see cref="OperationType"/> into a new <see cref="HttpMethod"/>
    /// </summary>
    /// <param name="operationType">The <see cref="OperationType"/> to convert</param>
    /// <returns>The resulting <see cref="HttpMethod"/></returns>
    public static HttpMethod ToHttpMethod(this OperationType operationType)
    {
        return operationType switch
        {
            OperationType.Delete => HttpMethod.Delete,
            OperationType.Get => HttpMethod.Get,
            OperationType.Head => HttpMethod.Head,
            OperationType.Options => HttpMethod.Options,
            OperationType.Patch => HttpMethod.Patch,
            OperationType.Post => HttpMethod.Post,
            OperationType.Put => HttpMethod.Put,
            OperationType.Trace => HttpMethod.Trace,
            _ => throw new NotSupportedException($"The specified {nameof(OperationType)} '{operationType}' is not supported"),
        };
    }

}
