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

using Synapse.Runner;

namespace Synapse;

/// <summary>
/// Defines extensions for <see cref="ITaskExecutionContext"/>s
/// </summary>
public static class ITaskExecutionContextExtensions
{

    /// <summary>
    /// Gets a new <see cref="TaskDescriptor"/> used to describe the <see cref="ITaskExecutionContext"/>
    /// </summary>
    /// <param name="task">The <see cref="ITaskExecutionContext"/> to describe</param>
    /// <returns>A new <see cref="TaskDescriptor"/></returns>
    public static TaskDescriptor GetDescriptor(this ITaskExecutionContext task)
    {
        return new()
        {
            Name = task.Instance.Name,
            Reference = task.Instance.Reference.OriginalString,
            Definition = task.Definition,
            Input = task.Input,
            Output = task.Output,
            StartedAt = task.Instance.StartedAt?.GetDescriptor()
        };
    }

}
