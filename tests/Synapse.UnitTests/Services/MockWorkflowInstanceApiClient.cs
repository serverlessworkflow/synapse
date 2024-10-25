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

using Neuroglia.Data;
using Neuroglia;
using Neuroglia.Data.Infrastructure;
using Neuroglia.Data.Infrastructure.ResourceOriented.Properties;
using Neuroglia.Data.Infrastructure.ResourceOriented;
using Neuroglia.Data.Infrastructure.ResourceOriented.Services;
using Neuroglia.Data.Infrastructure.Services;
using Synapse.Api.Client.Services;
using System.Net;

namespace Synapse.UnitTests.Services;

internal class MockWorkflowInstanceApiClient(IResourceRepository resources, ITextDocumentRepository<string> logs)
    : MockNamespacedResourceApiClient<WorkflowInstance>(resources), IWorkflowInstanceApiClient
{

    public async Task SuspendAsync(string name, string @namespace, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
        var workflowInstanceReference = new ResourceReference<WorkflowInstance>(name, @namespace);
        var original = await this.Resources.GetAsync<WorkflowInstance>(name, @namespace, cancellationToken).ConfigureAwait(false)
            ?? throw new ProblemDetailsException(new(ProblemTypes.NotFound, ProblemTitles.NotFound, (int)HttpStatusCode.NotFound, ProblemDescriptions.ResourceNotFound.Format(workflowInstanceReference.ToString())));
        if (original.Status?.Phase != WorkflowInstanceStatusPhase.Running) throw new ProblemDetailsException(new(ProblemTypes.AdmissionFailed, ProblemTitles.AdmissionFailed, (int)HttpStatusCode.BadRequest, $"The workflow instance '{workflowInstanceReference}' is in an expected phase '{original.Status?.Phase}'"));
        var updated = original.Clone()!;
        updated.Status ??= new();
        updated.Status.Phase = WorkflowInstanceStatusPhase.Waiting;
        var jsonPatch = JsonPatchUtility.CreateJsonPatchFromDiff(original, updated);
        await this.Resources.PatchStatusAsync<WorkflowInstance>(new(PatchType.JsonPatch, jsonPatch), name, @namespace, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task ResumeAsync(string name, string @namespace, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
        var workflowInstanceReference = new ResourceReference<WorkflowInstance>(name, @namespace);
        var original = await this.Resources.GetAsync<WorkflowInstance>(name, @namespace, cancellationToken).ConfigureAwait(false)
            ?? throw new ProblemDetailsException(new(ProblemTypes.NotFound, ProblemTitles.NotFound, (int)HttpStatusCode.NotFound, ProblemDescriptions.ResourceNotFound.Format(workflowInstanceReference.ToString())));
        if (original.Status?.Phase != WorkflowInstanceStatusPhase.Waiting) throw new ProblemDetailsException(new(ProblemTypes.AdmissionFailed, ProblemTitles.AdmissionFailed, (int)HttpStatusCode.BadRequest, $"The workflow instance '{workflowInstanceReference}' is in an expected phase '{original.Status?.Phase}'"));
        var updated = original.Clone()!;
        updated.Status ??= new();
        updated.Status.Phase = WorkflowInstanceStatusPhase.Running;
        var jsonPatch = JsonPatchUtility.CreateJsonPatchFromDiff(original, updated);
        await this.Resources.PatchStatusAsync<WorkflowInstance>(new(PatchType.JsonPatch, jsonPatch), name, @namespace, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task CancelAsync(string name, string @namespace, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
        var workflowInstanceReference = new ResourceReference<WorkflowInstance>(name, @namespace);
        var original = await this.Resources.GetAsync<WorkflowInstance>(name, @namespace, cancellationToken).ConfigureAwait(false)
            ?? throw new ProblemDetailsException(new(ProblemTypes.NotFound, ProblemTitles.NotFound, (int)HttpStatusCode.NotFound, ProblemDescriptions.ResourceNotFound.Format(workflowInstanceReference.ToString())));
        if (original.Status?.Phase != WorkflowInstanceStatusPhase.Pending && original.Status?.Phase != WorkflowInstanceStatusPhase.Running && original.Status?.Phase != WorkflowInstanceStatusPhase.Waiting)
            throw new ProblemDetailsException(new(ProblemTypes.AdmissionFailed, ProblemTitles.AdmissionFailed, (int)HttpStatusCode.BadRequest, $"The workflow instance '{workflowInstanceReference}' is in an expected phase '{original.Status?.Phase}'"));
        var updated = original.Clone()!;
        updated.Status ??= new();
        updated.Status.Phase = WorkflowInstanceStatusPhase.Cancelled;
        var jsonPatch = JsonPatchUtility.CreateJsonPatchFromDiff(original, updated);
        await this.Resources.PatchStatusAsync<WorkflowInstance>(new(PatchType.JsonPatch, jsonPatch), name, @namespace, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public Task<string> ReadLogsAsync(string name, string @namespace, CancellationToken cancellationToken = default) => logs.ReadToEndAsync($"{name}.{@namespace}", cancellationToken);


    public async Task<IAsyncEnumerable<ITextDocumentWatchEvent>> WatchLogsAsync(string name, string @namespace, CancellationToken cancellationToken = default) => (await logs.WatchAsync($"{name}.{@namespace}", cancellationToken)).ToAsyncEnumerable();

}