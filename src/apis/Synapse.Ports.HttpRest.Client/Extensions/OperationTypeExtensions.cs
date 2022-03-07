/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Microsoft.OpenApi.Models;

namespace Synapse
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
