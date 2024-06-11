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

using Microsoft.OpenApi.Models;

namespace Synapse.Runner;

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
