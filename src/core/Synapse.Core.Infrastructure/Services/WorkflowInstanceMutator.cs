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

using Neuroglia.Data.Infrastructure.ResourceOriented.Services;
using Neuroglia.Data.Infrastructure.ResourceOriented;
using Synapse.Resources;
using Neuroglia.Data;

namespace Synapse.Core.Infrastructure.Services;

/// <summary>
/// Represents the service used to mutate <see cref="WorkflowInstance"/>s
/// </summary>
public class WorkflowInstanceMutator
    : IResourceMutator
{

    /// <inheritdoc/>
    public virtual bool AppliesTo(Operation operation, string group, string version, string plural, string? @namespace = null) => operation == Operation.Create && group == WorkflowInstance.ResourceDefinition.Group && version == WorkflowInstance.ResourceDefinition.Version && plural == WorkflowInstance.ResourceDefinition.Plural;

    /// <inheritdoc/>
    public virtual Task<AdmissionReviewResponse> MutateAsync(AdmissionReviewRequest context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var updated = context.UpdatedState.ConvertTo<WorkflowInstance>()!;
        updated.Metadata.Labels ??= new Dictionary<string, string>();
        updated.Metadata.Labels[SynapseDefaults.Resources.Labels.Workflow] = $"{updated.Spec.Definition.Name}.{updated.Spec.Definition.Namespace}";
        updated.Metadata.Labels[SynapseDefaults.Resources.Labels.WorkflowVersion] = updated.Spec.Definition.Version;
        var patch = JsonPatchUtility.CreateJsonPatchFromDiff(context.OriginalState, updated);
        return Task.FromResult(new AdmissionReviewResponse(context.Uid, true, new(PatchType.JsonPatch, patch)));
    }

}