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

namespace Synapse.Runner.Services;

/// <summary>
/// Defines the fundamentals of a service used to create <see cref="ITaskExecutor"/>s
/// </summary>
public interface ITaskExecutorFactory
{

    /// <summary>
    /// Creates a new <see cref="ITaskExecutor"/> for the specified <see cref="TaskInstance"/>
    /// </summary>
    /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
    /// <param name="context">The <see cref="ITaskExecutionContext"/> to create a new <see cref="ITaskExecutor"/> for</param>
    /// <returns>A new <see cref="ITaskExecutor"/> for the specified <see cref="TaskInstance"/></returns>
    ITaskExecutor Create(IServiceProvider serviceProvider, ITaskExecutionContext context);

    /// <summary>
    /// Creates a new <see cref="ITaskExecutor"/> for the specified <see cref="TaskInstance"/>
    /// </summary>
    /// <typeparam name="TDefinition">The <see cref="TaskDefinition"/> of the <see cref="TaskInstance"/> to execute</typeparam>
    /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
    /// <param name="context">The <see cref="ITaskExecutionContext"/> to create a new <see cref="ITaskExecutor"/> for</param>
    /// <returns>A new <see cref="ITaskExecutor"/> for the specified <see cref="TaskInstance"/></returns>
    ITaskExecutor<TDefinition> Create<TDefinition>(IServiceProvider serviceProvider, ITaskExecutionContext<TDefinition> context)
        where TDefinition : TaskDefinition;

}
