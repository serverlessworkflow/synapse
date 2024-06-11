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

using System.Net;

namespace Synapse;

/// <summary>
/// Defines extensions for <see cref="Exception"/>s
/// </summary>
public static class ExceptionExtensions
{

    /// <summary>
    /// Converts the <see cref="Exception"/> into a new <see cref="Error"/>
    /// </summary>
    /// <param name="ex">The <see cref="Exception"/> to convert</param>
    /// <param name="instance">The <see cref="Uri"/>, if any, that references the instance to which the <see cref="Error"/> to create applies</param>
    /// <returns>A new <see cref="Error"/> based on the specified <see cref="Exception"/></returns>
    public static Error ToError(this Exception ex, Uri? instance = null) 
    {
        return ex switch
        {
            HttpRequestException httpEx => new()
            {
                Status = (ushort)(httpEx.StatusCode ?? HttpStatusCode.InternalServerError),
                Type = ErrorType.Communication,
                Title = ErrorTitle.Communication,
                Instance = instance,
                Detail = httpEx.Message
            },
            _ => new()
            {
                Status = ErrorStatus.Runtime,
                Type = ErrorType.Runtime,
                Title = ErrorTitle.Runtime,
                Instance = instance,
                Detail = ex.Message
            }
        };
    }

}
