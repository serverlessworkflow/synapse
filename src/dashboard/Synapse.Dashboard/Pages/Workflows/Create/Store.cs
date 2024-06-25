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

using Microsoft.JSInterop;
using Semver;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Api.Client.Services;
using Synapse.Dashboard.Components.ReferenceDetailsStateManagement;
using Synapse.Resources;

namespace Synapse.Dashboard.Pages.Workflows.Create;

/// <summary>
/// Represents the <see cref="CreateWorkflowViewState"/>
/// </summary>
/// <param name="api">The service used to interact with the Synapse API</param>
/// <param name="monacoEditorHelper">The service used to help handling Monaco editors</param>
/// <param name="jsonSerializer">The service used to serialize/deserialize data to/from JSON</param>
/// <param name="yamlSerializer">The service used to serialize/deserialize data to/from YAML</param>
public class CreateWorkflowViewStore(ISynapseApiClient api, IMonacoEditorHelper monacoEditorHelper, IJsonSerializer jsonSerializer, IYamlSerializer yamlSerializer)
    : ComponentStore<CreateWorkflowViewState>(new())
{

    Workflow? _workflow;
    WorkflowDefinition? _workflowDefinition;

    /// <summary>
    /// Gets the service used to interact with the Synapse API
    /// </summary>
    protected ISynapseApiClient Api { get; } = api;

    /// <summary>
    /// Gets the service used to help handling Monaco editors
    /// </summary>
    protected IMonacoEditorHelper MonacoEditorHelper { get; } = monacoEditorHelper;

    /// <summary>
    /// Gets the service used to serialize/deserialize data to/from JSON
    /// </summary>
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;

    /// <summary>
    /// Gets the service used to serialize/deserialize data to/from YAML
    /// </summary>
    protected IYamlSerializer YamlSerializer { get; } = yamlSerializer;

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe changes to the state's <see cref="CreateWorkflowViewState.Workflow"/> property
    /// </summary>
    public IObservable<Workflow?> Workflow => this.Select(state => state.Workflow).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe changes to the state's <see cref="CreateWorkflowViewState.WorkflowDefinition"/> property
    /// </summary>
    public IObservable<WorkflowDefinition?> WorkflowDefinition => this.Select(state => state.WorkflowDefinition).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe changes to the state's <see cref="CreateWorkflowViewState.WorkflowDefinitionText"/> property
    /// </summary>
    public IObservable<string?> WorkflowDefinitionText => this.Select(state => state.WorkflowDefinitionText).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe changes to the state's <see cref="CreateWorkflowViewState.Loading"/> property
    /// </summary>
    public IObservable<bool> Loading => this.Select(state => state.Loading).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe changes to the state's <see cref="CreateWorkflowViewState.Saving"/> property
    /// </summary>
    public IObservable<bool> Saving => this.Select(state => state.Saving).DistinctUntilChanged();

    /// <summary>
    /// Creates a new <see cref="WorkflowDefinition"/> from scratch
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public Task CreateWorkflowDefinitionAsync()
    {
        var workflowDefinition = new WorkflowDefinition()
        {
            Document = new()
            {
                Dsl = "1.0.0",
                Namespace = Namespace.DefaultNamespaceName,
                Name = "new-workflow",
                Version = "0.1.0"
            },
            Do = []
        };
        return this.SetWorkflowDefinitionAsync(workflowDefinition);
    }

    /// <summary>
    /// Creates a new <see cref="ServerlessWorkflow.Sdk.Models.WorkflowDefinition"/> from the specified one
    /// </summary>
    /// <param name="namespace">The namespace the <see cref="ServerlessWorkflow.Sdk.Models.WorkflowDefinition"/> to create a new version of belongs to</param>
    /// <param name="name">The name of the <see cref="ServerlessWorkflow.Sdk.Models.WorkflowDefinition"/> to create a new version of</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task CreateWorkflowDefinitionAsync(string @namespace, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        this._workflow = await this.Api.Workflows.GetAsync(name, @namespace) ?? throw new NullReferenceException($"Failed to find the specified workflow '{name}.{@namespace}'");
        this.Reduce(s => s with
        {
            Workflow = this._workflow
        });
        var workflowDefinition = this._workflow.Spec.Versions.GetLatest();
        await this.SetWorkflowDefinitionAsync(workflowDefinition);
    }

    /// <summary>
    /// Sets the <see cref="WorkflowDefinition"/> to create
    /// </summary>
    /// <param name="workflowDefinition">The base <see cref="WorkflowDefinition"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected Task SetWorkflowDefinitionAsync(WorkflowDefinition workflowDefinition)
    {
        ArgumentNullException.ThrowIfNull(workflowDefinition);
        this.Reduce(s => s with
        {
            Loading = true
        });
        var serializer = this.MonacoEditorHelper.PreferredLanguage switch
        {
            PreferredLanguage.JSON => (ITextSerializer)this.JsonSerializer,
            PreferredLanguage.YAML => this.YamlSerializer,
            _ => throw new NotSupportedException($"The specified language '{this.MonacoEditorHelper.PreferredLanguage}' is not supported")
        };
        var text = serializer.SerializeToText(workflowDefinition);
        this._workflowDefinition = workflowDefinition;
        this.Reduce(s => s with
        {
            WorkflowDefinition = workflowDefinition,
            WorkflowDefinitionText = text,
            Loading = false
        });
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets the state's <see cref="ReferenceDetailsState.TextEditor"/>
    /// </summary>
    /// <param name="textEditor">The new <see cref="ReferenceDetailsState.TextEditor"/> value</param>
    public void SetTextEditor(StandaloneCodeEditor? textEditor)
    {
        ArgumentNullException.ThrowIfNull(textEditor);
        this.Reduce(state => state with
        {
            TextEditor = textEditor
        });
    }

    /// <summary>
    /// Saves the <see cref="WorkflowDefinition"/> by posting it to the Synapse API
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task SaveWorkflowDefinitionAsync()
    {
        if (this._workflowDefinition == null) throw new NullReferenceException("The workflow definition cannot be null");
        this.Reduce(s => s with
        {
            Saving = true
        });
        if(this._workflow == null)
        {
            this._workflow = await this.Api.Workflows.CreateAsync(new()
            {
                Metadata = new()
                {
                    Namespace = this._workflowDefinition.Document.Namespace,
                    Name = this._workflowDefinition.Document.Name
                },
                Spec = new()
                {
                    Versions = [this._workflowDefinition]
                }
            });
        }
        else
        {
            var originalResource = await this.Api.Workflows.GetAsync(this._workflowDefinition.Document.Name, this._workflowDefinition.Document.Namespace);
            var updatedResource = originalResource.Clone()!;
            var latestVersion = SemVersion.Parse(updatedResource.Spec.Versions.GetLatest().Document.Version, SemVersionStyles.Strict)!;
            if (updatedResource.Spec.Versions.Any(v => SemVersion.Parse(v.Document.Version, SemVersionStyles.Strict).CompareSortOrderTo(latestVersion) >= 0)) throw new Exception($"The specified version '{this._workflowDefinition.Document.Version}' must be strictly superior to the latest version '{latestVersion}'");
            updatedResource.Spec.Versions.Add(this._workflowDefinition);
        }
        this.Reduce(s => s with
        {
            Workflow = this._workflow,
            Saving = false
        });
    }

}
