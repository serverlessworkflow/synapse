﻿// Copyright © 2024-Present The Synapse Authors
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

namespace Synapse;

/// <summary>
/// Defines extensions for <see cref="DoTaskDefinition"/>s
/// </summary>
public static class DoTaskDefinitionExtensions
{

    /// <summary>
    /// Gets the next <see cref="TaskDefinition"/> to perform next, if any
    /// </summary>
    /// <param name="doTask">The extended <see cref="DoTaskDefinition"/></param>
    /// <param name="after">The <see cref="TaskInstance"/> to perform the next <see cref="TaskInstance"/> after</param>
    /// <returns>The next <see cref="TaskDefinition"/> to perform next, if any</returns>
    public static MapEntry<string, TaskDefinition>? GetTaskAfter(this DoTaskDefinition doTask, TaskInstance after)
    {
        ArgumentNullException.ThrowIfNull(after);
        switch (after.Next)
        {
            case FlowDirective.Continue:
                var afterIndex = doTask.Do.Select(t => t.Key).ToList().IndexOf(after.Name!);
                return doTask.Do.Skip(afterIndex + 1).FirstOrDefault();
            case FlowDirective.End: case FlowDirective.Exit: return default;
            default: return new(after.Next!, doTask.Do[after.Next!]);
        }
    }

    /// <summary>
    /// Attempts to get the next <see cref="TaskDefinition"/> to perform next, if any
    /// </summary>
    /// <param name="doTask">The extended <see cref="DoTaskDefinition"/></param>
    /// <param name="after">The <see cref="TaskInstance"/> to perform the next <see cref="TaskInstance"/> after</param>
    /// <param name="task">The next <see cref="TaskDefinition"/> to perform next, if any</param>
    /// <returns>A boolean indicating whether or not a next <see cref="TaskInstance"/> must be executed next</returns>
    public static bool TryGetTaskAfter(this DoTaskDefinition doTask, TaskInstance after, out MapEntry<string, TaskDefinition> task)
    {
        ArgumentNullException.ThrowIfNull(after);
        task = doTask.GetTaskAfter(after)!;
        return task != null;
    }

}
