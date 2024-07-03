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

using ServerlessWorkflow.Sdk.Models;
using Synapse.Api.Client.Services;
using Synapse.Resources;

namespace Synapse.Dashboard.Pages.Workflows.Details;

/// <summary>
/// Represents the <see cref="View"/>'s store
/// </summary>
/// <param name="apiClient">The service used to interact with the Synapse API</param>
/// <param name="resourceEventHub">The hub used to watch resource events</param>
/// <param name="jsRuntime">The service used for JS interop</param>
/// <param name="monacoEditorHelper">The service used ease Monaco Editor interactions</param>
/// <param name="jsonSerializer">The service used to serialize and deserialize JSON</param>
/// <param name="yamlSerializer">The service used to serialize and deserialize YAML</param>
public class WorkflowDetailsStore(
    ISynapseApiClient apiClient,
    ResourceWatchEventHubClient resourceEventHub,
    IJSRuntime jsRuntime,
    IMonacoEditorHelper monacoEditorHelper,
    IJsonSerializer jsonSerializer,
    IYamlSerializer yamlSerializer
)
    : NamespacedResourceManagementComponentStore<WorkflowDetailsState, WorkflowInstance>(apiClient, resourceEventHub)
{

    private TextModel? _textModel = null;
    private bool _disposed;

    /// <summary>
    /// Gets the service used for JS interop
    /// </summary>
    protected IJSRuntime JSRuntime { get; } = jsRuntime;

    /// <summary>
    /// Gets the service used ease Monaco Editor interactions
    /// </summary>
    protected IMonacoEditorHelper MonacoEditorHelper { get; } = monacoEditorHelper;

    /// <summary>
    /// Gets the service used to serialize and deserialize JSON
    /// </summary>
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;

    /// <summary>
    /// Gets the service used to serialize and deserialize YAML
    /// </summary>
    protected IYamlSerializer YamlSerializer { get; } = yamlSerializer;

    /// <summary>
    /// The <see cref="BlazorMonaco.Editor.StandaloneEditorConstructionOptions"/> provider function
    /// </summary>
    public Func<StandaloneCodeEditor, StandaloneEditorConstructionOptions> StandaloneEditorConstructionOptions = monacoEditorHelper.GetStandaloneEditorConstructionOptions(string.Empty, true, monacoEditorHelper.PreferredLanguage);

    /// <summary>
    /// The <see cref="StandaloneCodeEditor"/> reference
    /// </summary>
    public StandaloneCodeEditor? TextEditor { get; set; }

    #region Selectors
    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="WorkflowDetailsState.Workflow"/> changes
    /// </summary>
    public IObservable<Workflow?> Workflow => this.Select(state => state.Workflow).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="WorkflowDetailsState.WorkflowDefinitionName"/> changes
    /// </summary>
    public IObservable<string?> WorkflowDefinitionName => this.Select(state => state.WorkflowDefinitionName).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="WorkflowDetailsState.WorkflowDefinitionVersion"/> changes
    /// </summary>
    public IObservable<string?> WorkflowDefinitionVersion => this.Select(state => state.WorkflowDefinitionVersion).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="WorkflowDetailsState.WorkflowInstanceName"/> changes
    /// </summary>
    public IObservable<string?> WorkflowInstanceName => this.Select(state => state.WorkflowInstanceName).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> exposing the <see cref="WorkflowDefinition"/>
    /// </summary>
    public IObservable<WorkflowDefinition?> WorkflowDefinition => Observable.CombineLatest(
        this.Workflow,
        this.WorkflowDefinitionVersion,
        (workflow, version) =>
        {
            if (workflow == null)
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(version) || version.Equals("latest", StringComparison.CurrentCultureIgnoreCase))
            {
                var latest = workflow.Spec.Versions.GetLatest()?.Document.Version;
                if (!string.IsNullOrWhiteSpace(latest))
                {
                    this.SetWorkflowDefinitionVersion(latest);
                }
                return null;
            }
            return workflow.Spec.Versions.Get(version)?.Clone();
        }
    ).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="WorkflowDetailsState.WorkflowDefinitionVersion"/> changes
    /// </summary>
    public IObservable<EquatableList<Workflow>?> Workflows => this.Select(state => state.Workflows).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="WorkflowDetailsState.WorkflowDefinitionJson"/> changes
    /// </summary>
    public IObservable<string> Document => this.Select(state => state.WorkflowDefinitionJson).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe the displayed <see cref="WorkflowInstance"/> changes
    /// </summary>
    public IObservable<WorkflowInstance?> WorkflowInstance => Observable.CombineLatest(
        this.Resources,
        this.WorkflowInstanceName,
        (instances, name) => {
            if (instances == null || instances.Count == 0 || name == null)
            {
                return null;
            }
            return instances.FirstOrDefault(instance => instance.Metadata.Name == name)?.Clone();
        }
    ).DistinctUntilChanged();
    #endregion

    #region Setters
    /// <summary>
    /// Sets the state's <see cref="WorkflowDetailsState.WorkflowDefinitionName"/>
    /// </summary>
    /// <param name="workflowDefinitionName">The new <see cref="WorkflowDetailsState.WorkflowDefinitionName"/> value</param>
    public void SetWorkflowDefinitionName(string? workflowDefinitionName)
    {
        this.Reduce(state => state with
        {
            Workflow = null,
            WorkflowDefinitionName = workflowDefinitionName
        });
    }

    /// <summary>
    /// Sets the state's <see cref="WorkflowDetailsState.WorkflowDefinitionVersion"/>
    /// </summary>
    /// <param name="workflowDefinitionVersion">The new <see cref="WorkflowDetailsState.WorkflowDefinitionVersion"/> value</param>
    public void SetWorkflowDefinitionVersion(string? workflowDefinitionVersion)
    {
        this.Reduce(state => state with
        {
            WorkflowDefinitionVersion = workflowDefinitionVersion
        });
    }

    /// <summary>
    /// Sets the state's <see cref="WorkflowDetailsState.WorkflowInstanceName"/>
    /// </summary>
    /// <param name="instanceName">The new <see cref="WorkflowDetailsState.WorkflowInstanceName"/> value</param>
    public void SetWorkflowInstanceName(string? instanceName)
    {
        this.Reduce(state => state with
        {
            WorkflowInstanceName = instanceName
        });
    }
    #endregion

    #region Actions
    /// <summary>
    /// Gets the workflow for the provided namespace and name
    /// </summary>
    /// <param name="ns"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task GetWorkflowAsync(string ns, string name)
    {
        try
        {
            var workflow = await this.ApiClient.Workflows.GetAsync(name, ns);
            this.Reduce(state => state with
            {
                Workflow = workflow
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            // todo: implement proper error handling
        }
    }

    /// <summary>
    /// Handles changed of the text editor's language
    /// </summary>
    /// <param name="_"></param>
    /// <returns></returns>
    public async Task ToggleTextBasedEditorLanguageAsync(string _)
    {
        await this.OnTextBasedEditorInitAsync();
    }

    /// <summary>
    /// Handles initialization of the text editor
    /// </summary>
    /// <returns></returns>
    public async Task OnTextBasedEditorInitAsync()
    {
        await this.SetTextBasedEditorLanguageAsync();
        await this.SetTextEditorValueAsync();
    }

    /// <summary>
    /// Sets the language of the text editor
    /// </summary>
    /// <returns></returns>
    public async Task SetTextBasedEditorLanguageAsync()
    {
        try
        {
            var language = this.MonacoEditorHelper.PreferredLanguage;
            if (this.TextEditor != null)
            {
                if (this._textModel != null)
                {
                    await Global.SetModelLanguage(this.JSRuntime, this._textModel, language);
                }
                else
                {
                    var version = this.Get(state => state.WorkflowDefinitionVersion);
                    var reference = this.Get(state => state.Namespaces) + "." + this.Get(state => state.WorkflowDefinitionName) + (!string.IsNullOrWhiteSpace(version) ? $":{version}" : "");
                    var resourceUri = $"inmemory://{reference.ToLower()}";
                    this._textModel = await Global.CreateModel(this.JSRuntime, "", language, resourceUri);
                }
                await this.TextEditor!.SetModel(this._textModel);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            // todo: handle exception
        }
    }

    /// <summary>
    /// Changes the value of the text editor
    /// </summary>
    /// <returns></returns>
    async Task SetTextEditorValueAsync()
    {
        var document = this.Get(state => state.WorkflowDefinitionJson);
        var language = this.MonacoEditorHelper.PreferredLanguage;
        if (this.TextEditor != null && !string.IsNullOrWhiteSpace(document))
        {
            try
            {
                if (language == PreferredLanguage.YAML)
                {
                    document = this.YamlSerializer.ConvertFromJson(document);
                }
                await this.TextEditor.SetValue(document);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await this.MonacoEditorHelper.ChangePreferredLanguageAsync(language == PreferredLanguage.YAML ? PreferredLanguage.JSON : PreferredLanguage.YAML);
            }
        }
    }
    #endregion

    /// <inheritdoc/>
    public override async Task InitializeAsync()
    {
        this.WorkflowDefinition.Where(definition => definition != null).SubscribeAsync(async (definition) =>
        {
            await Task.Delay(1);
            var document = this.JsonSerializer.SerializeToText(definition.Clone());
            this.Reduce(state => state with
            {
                WorkflowDefinitionJson = document
            });
            await this.SetTextEditorValueAsync();
            if (this.MonacoEditorHelper.PreferredLanguage != PreferredLanguage.YAML)
            {
                await this.MonacoEditorHelper.ChangePreferredLanguageAsync(PreferredLanguage.YAML);
            }
        }, cancellationToken: this.CancellationTokenSource.Token);
        Observable.CombineLatest(
            this.Namespace.Where(ns => !string.IsNullOrWhiteSpace(ns)),
            this.WorkflowDefinitionName.Where(name => !string.IsNullOrWhiteSpace(name)),
            (ns, name) => (ns!, name!)
        ).SubscribeAsync(async ((string ns, string name) workflow) =>
        {
            this.RemoveLabelSelector(SynapseDefaults.Resources.Labels.Workflow);
            this.AddLabelSelector(new(SynapseDefaults.Resources.Labels.Workflow, LabelSelectionOperator.Equals, $"{workflow.name}.{workflow.ns}"));
            await this.GetWorkflowAsync(workflow.ns, workflow.name);
        }, cancellationToken: this.CancellationTokenSource.Token);
        this.WorkflowDefinitionVersion.Where(version => version != null).Subscribe(version =>
        {
            this.RemoveLabelSelector(SynapseDefaults.Resources.Labels.WorkflowVersion);
            this.AddLabelSelector(new(SynapseDefaults.Resources.Labels.WorkflowVersion, LabelSelectionOperator.Equals, version!));
        });
        await base.InitializeAsync();
    }

    /// <summary>
    /// Disposes of the store
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the dispose of the store</param>
    protected override void Dispose(bool disposing)
    {
        if (!this._disposed)
        {
            if (disposing)
            {
                if (this._textModel != null)
                {
                    this._textModel.DisposeModel();
                    this._textModel = null;
                }
                if (this.TextEditor != null)
                {
                    this.TextEditor.Dispose();
                    this.TextEditor = null;
                }
            }
            this._disposed = true;
        }
    }
}
