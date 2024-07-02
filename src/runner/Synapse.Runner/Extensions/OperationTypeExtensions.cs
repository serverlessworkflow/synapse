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

namespace Synapse.Runner;

/// <summary>
/// Defines extensions for <see cref="HttpMethod"/>s
/// </summary>
public static class OperationTypeExtensions
{

    /// <summary>
    /// Converts the <see cref="Microsoft.OpenApi.Models.OperationType"/> into a new <see cref="HttpMethod"/>
    /// </summary>
    /// <param name="operationType">The <see cref="Microsoft.OpenApi.Models.OperationType"/> to convert</param>
    /// <returns>The resulting <see cref="HttpMethod"/></returns>
    public static HttpMethod ToHttpMethod(this Microsoft.OpenApi.Models.OperationType operationType)
    {
        return operationType switch
        {
            Microsoft.OpenApi.Models.OperationType.Delete => HttpMethod.Delete,
            Microsoft.OpenApi.Models.OperationType.Get => HttpMethod.Get,
            Microsoft.OpenApi.Models.OperationType.Head => HttpMethod.Head,
            Microsoft.OpenApi.Models.OperationType.Options => HttpMethod.Options,
            Microsoft.OpenApi.Models.OperationType.Patch => HttpMethod.Patch,
            Microsoft.OpenApi.Models.OperationType.Post => HttpMethod.Post,
            Microsoft.OpenApi.Models.OperationType.Put => HttpMethod.Put,
            Microsoft.OpenApi.Models.OperationType.Trace => HttpMethod.Trace,
            _ => throw new NotSupportedException($"The specified {nameof(Microsoft.OpenApi.Models.OperationType)} '{operationType}' is not supported"),
        };
    }

}
