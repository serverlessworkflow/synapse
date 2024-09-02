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
/// Defines the fundamentals of a task life cycle event
/// </summary>
public interface ITaskLifeCycleEvent
{

    /// <summary>
    /// Gets the type of task life cycle event
    /// </summary>
    string Type { get; }

    /// <summary>
    /// Gets the task life cycle event's data, if any
    /// </summary>
    object? Data { get; }

}
