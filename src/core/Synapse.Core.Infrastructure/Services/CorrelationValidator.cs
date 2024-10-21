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
/// Represents the service used to validate <see cref="Correlation"/>s
/// </summary>
/// <param name="resources">The service used to manage resources</param>
/// <param name="schemaHandlerProvider">The service used to provide <see cref="ISchemaHandler"/> implementations</param>
public class CorrelationValidator(IResourceRepository resources, ISchemaHandlerProvider schemaHandlerProvider)
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
    public virtual bool AppliesTo(Operation operation, string group, string version, string plural, string? @namespace = null) => operation == Operation.Create && group == Correlation.ResourceDefinition.Group && version == Correlation.ResourceDefinition.Version && plural == Correlation.ResourceDefinition.Plural;

    /// <inheritdoc/>
    public virtual async Task<AdmissionReviewResponse> ValidateAsync(AdmissionReviewRequest context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var correlation = context.UpdatedState.ConvertTo<Correlation>()!;
        switch (correlation.Spec.Outcome.Type)
        {
            case CorrelationOutcomeType.Start:
                if (correlation.Spec.Outcome.Start == null) return new(context.Uid, false, null, new(ProblemTypes.AdmissionFailed, ProblemTitles.ValidationFailed, ErrorStatus.Validation, $"The '/spec/outcome/start' property must be set when the correlation outcome type has been set to '{CorrelationOutcomeType.Start}'", new("/spec/outcome/start", UriKind.Relative)));
                var workflow = await this.Resources.GetAsync<Workflow>(correlation.Spec.Outcome.Start.Workflow.Name, correlation.Spec.Outcome.Start.Workflow.Namespace, cancellationToken).ConfigureAwait(false);
                if (workflow == null) return new(context.Uid, false, null, new(ProblemTypes.AdmissionFailed, ProblemTitles.ValidationFailed, ErrorStatus.Validation, $"Failed to find the specified workflow '{correlation.Spec.Outcome.Start.Workflow.Name}.{correlation.Spec.Outcome.Start.Workflow.Namespace}'", new("/spec/outcome/start/workflow", UriKind.Relative)));
                var workflowDefinition = workflow.Spec.Versions.Get(correlation.Spec.Outcome.Start.Workflow.Version);
                if (workflowDefinition == null) return new(context.Uid, false, null, new(ProblemTypes.AdmissionFailed, ProblemTitles.ValidationFailed, ErrorStatus.Validation, $"Failed to find version '{correlation.Spec.Outcome.Start.Workflow.Version}' of workflow '{correlation.Spec.Outcome.Start.Workflow.Name}.{correlation.Spec.Outcome.Start.Workflow.Namespace}'", new("/spec/outcome/start/workflow/version", UriKind.Relative)));
                if (workflowDefinition.Input?.Schema != null)
                {
                    var schemaHandler = this.SchemaHandlerProvider.GetHandler(workflowDefinition.Input.Schema.Format) ?? throw new ArgumentNullException($"Failed to find an handler that supports the specified schema format '{workflowDefinition.Input.Schema.Format}'");
                    var validationResult = await schemaHandler.ValidateAsync(correlation.Spec.Outcome.Start.Input ?? new Dictionary<string, object>(), workflowDefinition.Input.Schema, cancellationToken).ConfigureAwait(false);
                    if (!validationResult.IsSuccess()) return new(context.Uid, false, null, new(ErrorType.Validation, ErrorTitle.Validation, ErrorStatus.Validation, $"Failed to validate the correlation outcome workflow input:\n{string.Join('\n', validationResult.Errors?.FirstOrDefault()?.Errors?.Select(e => $"- {e.Key}: {e.Value.First()}") ?? [])}", new("/spec/outcome/start/input", UriKind.Relative)));
                }
                break;
            case CorrelationOutcomeType.Correlate:
                if (correlation.Spec.Outcome.Correlate == null) return new(context.Uid, false, null, new(ProblemTypes.AdmissionFailed, ProblemTitles.ValidationFailed, ErrorStatus.Validation, $"The '/spec/outcome/correlate' property must be set when the correlation outcome type has been set to '{CorrelationOutcomeType.Correlate}'", new("/spec/outcome/correlate", UriKind.Relative)));
                var components = correlation.Spec.Outcome.Correlate.Instance.Split('.', StringSplitOptions.RemoveEmptyEntries);
                if (components.Length != 2) return new(context.Uid, false, null, new(ProblemTypes.AdmissionFailed, ProblemTitles.ValidationFailed, ErrorStatus.Validation, $"The specified value '{correlation.Spec.Outcome.Correlate.Instance}' is not a valid workflow instance qualified name ({{name}}.{{namespace}})", new("/spec/outcome/correlate/instance", UriKind.Relative)));
                var name = components[0];
                var @namespace = components[1];
                var workflowInstance = await this.Resources.GetAsync<WorkflowInstance>(name, @namespace, cancellationToken).ConfigureAwait(false);
                if (workflowInstance == null) return new(context.Uid, false, null, new(ProblemTypes.AdmissionFailed, ProblemTitles.ValidationFailed, ErrorStatus.Validation, $"Failed to find the specified workflow instance '{correlation.Spec.Outcome.Correlate.Instance}'", new("/spec/outcome/correlate/instance", UriKind.Relative)));
                var task = workflowInstance.Status?.Tasks?.FirstOrDefault(t => t.Reference.OriginalString == correlation.Spec.Outcome.Correlate.Task);
                if (task == null) return new(context.Uid, false, null, new(ProblemTypes.AdmissionFailed, ProblemTitles.ValidationFailed, ErrorStatus.Validation, $"Failed to find the task '{correlation.Spec.Outcome.Correlate.Task}' in workflow instance '{correlation.Spec.Outcome.Correlate.Instance}'", new("/spec/outcome/correlate/task", UriKind.Relative)));
                break;
            default:
                return new(context.Uid, false, null, new(ProblemTypes.AdmissionFailed, ProblemTitles.ValidationFailed, ErrorStatus.Validation, $"The specified correlation outcome type '{correlation.Spec.Outcome.Type}' is not supported", new("/spec/outcome/type", UriKind.Relative)));
        }
        return new(context.Uid, true);
    }

}