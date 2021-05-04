using Microsoft.OpenApi.Models;
using System;
using System.Net.Http;

namespace Synapse.Runner.Application
{

    /// <summary>
    /// Defines extensions for <see cref="HttpMethod"/>s
    /// </summary>
    public static class OperationTypeExtensions
    {

        /// <summary>
        /// Converts the <see cref="OperationType"/> into a new <see cref="HttpMethod"/>
        /// </summary>
        /// <param name="operationType">The <see cref="OperationType"/> to convert</param>
        /// <returns>The reuslting <see cref="HttpMethod"/></returns>
        public static HttpMethod ToHttpMethod(this OperationType operationType)
        {
            switch (operationType)
            {
                case OperationType.Delete:
                    return HttpMethod.Delete;
                case OperationType.Get:
                    return HttpMethod.Get;
                case OperationType.Head:
                    return HttpMethod.Head;
                case OperationType.Options:
                    return HttpMethod.Options;
                case OperationType.Patch:
                    return HttpMethod.Patch;
                case OperationType.Post:
                    return HttpMethod.Post;
                case OperationType.Put:
                    return HttpMethod.Put;
                case OperationType.Trace:
                    return HttpMethod.Trace;
                default:
                    throw new NotSupportedException($"The specified {nameof(OperationType)} '{operationType}' is not supported");
            }
        }

    }

}
