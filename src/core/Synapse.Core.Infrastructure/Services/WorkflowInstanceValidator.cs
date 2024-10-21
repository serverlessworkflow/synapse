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
using Neuroglia.Data.Infrastructure.ResourceOriented.Properties;
using ServerlessWorkflow.Sdk;
using Neuroglia;

namespace Synapse.Core.Infrastructure.Services;

/// <summary>
/// Represents the service used to validate <see cref="WorkflowInstance"/>s
/// </summary>
/// <param name="resources">The service used to manage resources</param>
/// <param name="schemaHandlerProvider">The service used to provide <see cref="ISchemaHandler"/> implementations</param>
public class WorkflowInstanceValidator(IResourceRepository resources, ISchemaHandlerProvider schemaHandlerProvider)
    : IResourceValidator
{

    /// <summary>
    /// Gets the service used to manage resources
    /// </summary>
    protected IResourceRepository Resources { get; } = resources;

    /// <summary>
    /// Gets the service used to provide <see cref="ISchemaHandler"/> implementations
    /// </summary>
    protected ISchemaHandlerProvider SchemaHandlerProvider { get; } = schemaHandlerProvider;

    /// <inheritdoc/>
    public virtual bool AppliesTo(Operation operation, string group, string version, string plural, string? @namespace = null) => operation == Operation.Create && group == WorkflowInstance.ResourceDefinition.Group && version == WorkflowInstance.ResourceDefinition.Version && plural == WorkflowInstance.ResourceDefinition.Plural;

    /// <inheritdoc/>
    public virtual async Task<AdmissionReviewResponse> ValidateAsync(AdmissionReviewRequest context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var workflowInstance = context.UpdatedState.ConvertTo<WorkflowInstance>()!;
        var workflow = await this.Resources.GetAsync<Workflow>(workflowInstance.Spec.Definition.Name, workflowInstance.Spec.Definition.Namespace, cancellationToken).ConfigureAwait(false);
        if (workflow == null) return new(context.Uid, false, null, new(ProblemTypes.AdmissionFailed, ProblemTitles.ValidationFailed, ErrorStatus.Validation, $"Failed to find the specified workflow '{workflowInstance.Spec.Definition.Name}.{workflowInstance.Spec.Definition.Namespace}'", new("/spec/definition", UriKind.Relative)));
        var workflowDefinition = workflow.Spec.Versions.Get(workflowInstance.Spec.Definition.Version);
        if (workflowDefinition == null) return new(context.Uid, false, null, new(ProblemTypes.AdmissionFailed, ProblemTitles.ValidationFailed, ErrorStatus.Validation, $"Failed to find version '{workflowInstance.Spec.Definition.Version}' of workflow '{workflowInstance.Spec.Definition.Name}.{workflowInstance.Spec.Definition.Namespace}'", new("/spec/definition/version", UriKind.Relative)));
        if (workflowDefinition.Input?.Schema != null)
        {
            var schemaHandler = this.SchemaHandlerProvider.GetHandler(workflowDefinition.Input.Schema.Format) ?? throw new ArgumentNullException($"Failed to find an handler that supports the specified schema format '{workflowDefinition.Input.Schema.Format}'");
            var validationResult = await schemaHandler.ValidateAsync(workflowInstance.Spec.Input ?? [], workflowDefinition.Input.Schema, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsSuccess()) return new(context.Uid, false, null, new(ErrorType.Validation, ErrorTitle.Validation, ErrorStatus.Validation, $"Failed to validate the workflow instance's input:\n{string.Join('\n', validationResult.Errors?.FirstOrDefault()?.Errors?.Select(e => $"- {e.Key}: {e.Value.First()}") ?? [])}", new("/spec/input", UriKind.Relative)));
        }
        return new(context.Uid, true);
    }

}
