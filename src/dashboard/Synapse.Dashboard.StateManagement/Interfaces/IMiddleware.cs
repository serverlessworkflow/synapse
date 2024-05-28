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

namespace Synapse.Dashboard.StateManagement;

/// <summary>
/// Defines the fundamentals of a middleware
/// </summary>
public interface IMiddleware
{

    /// <summary>
    /// Invokes the <see cref="IMiddleware"/>
    /// </summary>
    /// <param name="context">The <see cref="IActionContext"/> in which to invoke the <see cref="IMiddleware"/></param>
    /// <returns>The result of the dispatched Flux action</returns>
    Task<object> InvokeAsync(IActionContext context);

}