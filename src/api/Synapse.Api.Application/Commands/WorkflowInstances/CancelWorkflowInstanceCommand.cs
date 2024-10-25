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

using Neuroglia.Data.Infrastructure.ResourceOriented.Properties;
using Synapse.Resources;

namespace Synapse.Api.Application.Commands.WorkflowInstances;

/// <summary>
/// Represents the <see cref="Command"/> used to cancel the execution of a <see cref="WorkflowInstance"/>
/// </summary>
/// <param name="name">The name of the <see cref="WorkflowInstance"/> to cancel the execution of</param>
/// <param name="namespace">The namespace the <see cref="WorkflowInstance"/> to cancel the execution of belongs to</param>
public class CancelWorkflowInstanceCommand(string name, string @namespace)
    : Command
{

    /// <summary>
    /// Gets the name of the <see cref="WorkflowInstance"/> to cancel the execution of
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the namespace the <see cref="WorkflowInstance"/> to cancel the execution of belongs to
    /// </summary>
    public string Namespace { get; } = @namespace;

}

/// <summary>
/// Represents the service used to handle <see cref="CancelWorkflowInstanceCommand"/>s
/// </summary>
/// <param name="resources">The service used to manage <see cref="IResource"/>s</param>
public class CancelWorkflowInstanceCommandHandler(IResourceRepository resources)
    : ICommandHandler<CancelWorkflowInstanceCommand>
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult> HandleAsync(CancelWorkflowInstanceCommand command, CancellationToken cancellationToken = default)
    {
        var workflowInstanceReference = new ResourceReference<WorkflowInstance>(command.Name, command.Namespace);
        var original = await resources.GetAsync<WorkflowInstance>(command.Name, command.Namespace, cancellationToken).ConfigureAwait(false)
            ?? throw new ProblemDetailsException(new(ProblemTypes.NotFound, ProblemTitles.NotFound, (int)HttpStatusCode.NotFound, ProblemDescriptions.ResourceNotFound.Format(workflowInstanceReference.ToString())));
        if (!string.IsNullOrWhiteSpace(original.Status?.Phase) && original.Status?.Phase != WorkflowInstanceStatusPhase.Pending && original.Status?.Phase != WorkflowInstanceStatusPhase.Running && original.Status?.Phase != WorkflowInstanceStatusPhase.Suspended && original.Status?.Phase != WorkflowInstanceStatusPhase.Waiting) 
            throw new ProblemDetailsException(new(ProblemTypes.AdmissionFailed, ProblemTitles.AdmissionFailed, (int)HttpStatusCode.BadRequest, $"The workflow instance '{workflowInstanceReference}' is in an expected phase '{original.Status?.Phase}'"));
        var updated = original.Clone()!;
        updated.Status ??= new();
        updated.Status.Phase = WorkflowInstanceStatusPhase.Cancelled;
        var jsonPatch = JsonPatchUtility.CreateJsonPatchFromDiff(original, updated);
        await resources.PatchStatusAsync<WorkflowInstance>(new(PatchType.JsonPatch, jsonPatch), command.Name, command.Namespace, cancellationToken: cancellationToken).ConfigureAwait(false);
        return this.Ok();
    }

}