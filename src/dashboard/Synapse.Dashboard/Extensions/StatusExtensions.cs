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

using Synapse.Resources;

namespace Synapse.Dashboard.Extensions;

/// <summary>
/// Provides extension methods for statuses/phases
/// </summary>
public static class StatusExtensions
{
    /// <summary>
    /// Returns the color css class for the provided status or phase
    /// </summary>
    /// <param name="status">The status to return the color class for</param>
    /// <returns></returns>
    public static string GetColorClass(this string? status) => status switch
    {
        // commented = same as above, which lead to the error "The pattern is unreachable. It has already been handled by a previous arm of the switch expression or it is impossible to match."
        WorkflowInstanceStatusPhase.Running => "accent",
        //CorrelatorStatusPhase.Running => "accent",
        //OperatorStatusPhase.Running => "accent",
        CorrelationContextStatus.Active => "accent",
        //TaskInstanceStatus.Running => "accent", 
        WorkflowInstanceStatusPhase.Faulted => "danger",
        //TaskInstanceStatus.Faulted => "danger",
        WorkflowInstanceStatusPhase.Cancelled => "warning",
        //TaskInstanceStatus.Cancelled => "warning",
        //CorrelationContextStatus.Cancelled => "warning",
        WorkflowInstanceStatusPhase.Completed => "success",
        //TaskInstanceStatus.Completed => "success",
        //CorrelationContextStatus.Completed => "success",
        WorkflowInstanceStatusPhase.Waiting => "cinereous",
        TaskInstanceStatus.Suspended => "icterine",
        TaskInstanceStatus.Skipped => "cinereous",
        WorkflowInstanceStatusPhase.Pending => "mute",
        //TaskInstanceStatus.Pending => "mute",
        CorrelationContextStatus.Inactive => "mute",
        CorrelatorStatusPhase.Stopped => "secondary",
        //OperatorStatusPhase.Stopped => "secondary",
        _ => ""
    };
}
