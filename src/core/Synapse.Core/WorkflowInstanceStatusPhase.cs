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

namespace Synapse;

/// <summary>
/// Exposes the default status phases that a workflow can enter in Synapse.
/// </summary>
public static class WorkflowInstanceStatusPhase
{

    /// <summary>
    /// Indicates that the workflow is pending execution
    /// </summary>
    public const string Pending = "pending";
    /// <summary>
    /// Indicates that the workflow is being executed
    /// </summary>
    public const string Running = "running";
    /// <summary>
    /// Indicates that the workflow ran to completion
    /// </summary>
    public const string Completed = "completed";
    /// <summary>
    /// Indicates that the workflow's execution is waiting for user or event input
    /// </summary>
    public const string Waiting = "waiting";
    /// <summary>
    /// Indicates that the workflow's execution has been cancelled
    /// </summary>
    public const string Cancelled = "cancelled";
    /// <summary>
    /// Indicates that the workflow encountered an unhandled error during its execution and consequently faulted
    /// </summary>
    public const string Faulted = "faulted";

}