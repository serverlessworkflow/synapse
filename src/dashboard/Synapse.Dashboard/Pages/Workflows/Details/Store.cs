﻿// Copyright © 2024-Present The Synapse Authors
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

using Neuroglia.Blazor.Dagre.Models;
using Neuroglia.Collections;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Api.Client.Services;
using Synapse.Resources;

namespace Synapse.Dashboard.Pages.Workflows.Details;

/// <summary>
/// Represents the <see cref="View"/>'s store
/// </summary>
/// <param name="logger">The service used to perform logging</param>
/// <param name="apiClient">The service used to interact with the Synapse API</param>
/// <param name="resourceEventHub">The hub used to watch resource events</param>
/// <param name="jsRuntime">The service used for JS interop</param>
/// <param name="monacoEditorHelper">The service used ease Monaco Editor interactions</param>
/// <param name="jsonSerializer">The service used to serialize and deserialize JSON</param>
/// <param name="yamlSerializer">The service used to serialize and deserialize YAML</param>
/// <param name="monacoInterop">The service used to build a bridge with the monaco interop extension</param>
/// <param name="toastService">The service used display toast messages</param>
public class WorkflowDetailsStore(
    ILogger<WorkflowDetailsStore> logger,
    ISynapseApiClient apiClient,
    ResourceWatchEventHubClient resourceEventHub,
    IJSRuntime jsRuntime,
    IMonacoEditorHelper monacoEditorHelper,
    IJsonSerializer jsonSerializer,
    IYamlSerializer yamlSerializer,
    MonacoInterop monacoInterop,
    ToastService toastService
)
    : NamespacedResourceManagementComponentStore<WorkflowDetailsState, WorkflowInstance>(logger, apiClient, resourceEventHub)
{

    private TextModel? _textModel = null;
    private bool _disposed;
    private bool _hasTextEditorInitialized = false;

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
    /// Gets the service used to build a bridge with the monaco interop extension
    /// </summary>
    protected MonacoInterop MonacoInterop { get; } = monacoInterop;

    /// <summary>
    /// Gets the service used display toast messages
    /// </summary>
    protected ToastService ToastService { get; } = toastService;

    /// <summary>
    ///  Gets/sets the <see cref="BlazorMonaco.Editor.StandaloneEditorConstructionOptions"/> provider function
    /// </summary>
    public Func<StandaloneCodeEditor, StandaloneEditorConstructionOptions> StandaloneEditorConstructionOptions = monacoEditorHelper.GetStandaloneEditorConstructionOptions(string.Empty, true, monacoEditorHelper.PreferredLanguage);

    /// <summary>
    /// Gets/sets the <see cref="StandaloneCodeEditor"/>'s reference used to display the workflow definition
    /// </summary>
    public StandaloneCodeEditor? TextEditor { get; set; }

    /// <summary>
    /// Gets/sets the <see cref="Modal"/>'s reference used to display the workflow instance creation form
    /// </summary>
    public Modal? Modal { get; set; }

    #region Selectors
    
    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="WorkflowDetailsState.Workflow"/> changes
    /// </summary>
    public IObservable<Workflow?> Workflow => this.Select(state => state.Workflow).DistinctUntilChanged();

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

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe the <see cref="WorkflowDetailsState.ProblemDetails"/> changes
    /// </summary>
    public IObservable<ProblemDetails?> ProblemDetails => this.Select(state => state.ProblemDetails).DistinctUntilChanged();

    #endregion

    #region Setters

    /// <inheritdoc/>
    public override void SetActiveResourceName(string activeResourceName)
    {
        //base.SetActiveResourceName(activeResourceName);
        this.Reduce(state => state with
        {
            ActiveResourceName = activeResourceName,
            Workflow = null
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

    /// <summary>
    /// Sets the state's <see cref="WorkflowDetailsState.ProblemDetails"/>
    /// </summary>
    /// <param name="problemDetails">The <see cref="ProblemDetails"/> to set</param>
    public void SetProblemDetails(ProblemDetails? problemDetails)
    {
        this.Reduce(state => state with
        {
            ProblemDetails = problemDetails
        });
    }

    #endregion

    #region Actions

    /// <summary>
    /// Changes the value of the text editor
    /// </summary>
    /// <returns>A awaitable task</returns>
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
                this.Logger.LogError("Unable to set text editor value: {exception}", ex.ToString());
                await this.MonacoEditorHelper.ChangePreferredLanguageAsync(language == PreferredLanguage.YAML ? PreferredLanguage.JSON : PreferredLanguage.YAML);
            }
        }
    }

    /// <summary>
    /// Gets the workflow for the provided namespace and name
    /// </summary>
    /// <param name="ns"></param>
    /// <param name="name"></param>
    /// <returns>A awaitable task</returns>
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
            this.Logger.LogError("Unable to get workflow '{name}.{ns}': {exception}", name, ns, ex.ToString());
        }
    }

    /// <summary>
    /// Handles changed of the text editor's language
    /// </summary>
    /// <param name="_"></param>
    /// <returns>A awaitable task</returns>
    public async Task ToggleTextBasedEditorLanguageAsync(string _)
    {
        await this.InitializeTextBasedEditorAsync();
    }

    /// <summary>
    /// Handles initialization of the text editor
    /// </summary>
    /// <returns></returns>
    public async Task OnTextBasedEditorInitAsync()
    {
        this._hasTextEditorInitialized = true;
        await this.InitializeTextBasedEditorAsync();
    }

    /// <summary>
    /// Initializes the text editor
    /// </summary>
    /// <returns></returns>
    public async Task InitializeTextBasedEditorAsync()
    {
        if (this.TextEditor == null || !this._hasTextEditorInitialized) return;
        await this.SetTextBasedEditorLanguageAsync();
        await this.SetTextEditorValueAsync();
    }

    /// <summary>
    /// Sets the language of the text editor
    /// </summary>
    /// <returns>A awaitable task</returns>
    public async Task SetTextBasedEditorLanguageAsync()
    {
        try
        {
            var language = this.MonacoEditorHelper.PreferredLanguage;
            if (this.TextEditor != null && this._hasTextEditorInitialized)
            {
                if (this._textModel != null)
                {
                    await Global.SetModelLanguage(this.JSRuntime, this._textModel, language);
                }
                else
                {
                    var version = this.Get(state => state.WorkflowDefinitionVersion);
                    var reference = this.Get(state => state.Namespaces) + "." + this.Get(state => state.ActiveResourceName) + (!string.IsNullOrWhiteSpace(version) ? $":{version}" : "");
                    var resourceUri = $"inmemory://{reference.ToLower()}";
                    this._textModel = await Global.CreateModel(this.JSRuntime, "", language, resourceUri);
                }
                await this.TextEditor!.SetModel(this._textModel);
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError("Unable to set text editor language: {exception}", ex.ToString());
        }
    }

    /// <summary>
    /// Copies to content of the Monaco editor to the clipboard
    /// </summary>
    /// <returns>A awaitable task</returns>
    public async Task OnCopyToClipboard()
    {
        if (this.TextEditor == null || !this._hasTextEditorInitialized) return;
        var text = await this.TextEditor.GetValue();
        if (string.IsNullOrWhiteSpace(text)) return;
        try
        {
            await this.JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
            this.ToastService.Notify(new(ToastType.Success, "Copied to the clipboard!"));
        }
        catch (Exception ex)
        {
            this.ToastService.Notify(new(ToastType.Danger, "Failed to copy the definition to the clipboard."));
            this.Logger.LogError("Unable to copy to clipboard: {exception}", ex.ToString());
        }
    }

    /// <summary>
    /// Displays the modal used to provide the new workflow input
    /// </summary>
    /// <param name="workflowDefinition">The definition to start a new instance of</param>
    /// <param name="input">A default input payload, if any</param>
    /// <returns>A awaitable task</returns>
    public async Task OnShowCreateInstanceAsync(WorkflowDefinition workflowDefinition, EquatableDictionary<string, object>? input = null)
    {
        if (this.Modal != null)
        {
            var parameters = new Dictionary<string, object>
            {
                { nameof(CreateWorkflowInstanceDialog.WorkflowDefinition), workflowDefinition },
                { nameof(CreateWorkflowInstanceDialog.Input), input! },
                { nameof(CreateWorkflowInstanceDialog.OnCreate), EventCallback.Factory.Create<string>(this, CreateInstanceAsync) },
                { nameof(CreateWorkflowInstanceDialog.OnProblem), EventCallback.Factory.Create<ProblemDetails>(this, SetProblemDetails) }
            };
            await this.Modal.ShowAsync<CreateWorkflowInstanceDialog>(title: "Start a new workflow", parameters: parameters);
        }
    }

    /// <summary>
    /// Suspends the provided instance
    /// </summary>
    /// <param name="workflowInstance">The instance to suspend</param>
    /// <returns>A awaitable task</returns>
    public async Task SuspendInstanceAsync(WorkflowInstance workflowInstance)
    {
        await this.ApiClient.WorkflowInstances.SuspendAsync(workflowInstance.GetName(), workflowInstance.GetNamespace()!).ConfigureAwait(false);
    }

    /// <summary>
    /// Resumes the provided instance
    /// </summary>
    /// <param name="workflowInstance">The instance to resume</param>
    /// <returns>A awaitable task</returns>
    public async Task ResumeInstanceAsync(WorkflowInstance workflowInstance)
    {
        await this.ApiClient.WorkflowInstances.ResumeAsync(workflowInstance.GetName(), workflowInstance.GetNamespace()!).ConfigureAwait(false);
    }

    /// <summary>
    /// Cancels the provided instance
    /// </summary>
    /// <param name="workflowInstance">The instance to resume</param>
    /// <returns>A awaitable task</returns>
    public async Task CancelInstanceAsync(WorkflowInstance workflowInstance)
    {
        await this.ApiClient.WorkflowInstances.CancelAsync(workflowInstance.GetName(), workflowInstance.GetNamespace()!).ConfigureAwait(false);
    }

    /// <summary>
    /// Suspends selected instances
    /// </summary>
    /// <returns>A awaitable task</returns>
    public async Task OnSuspendSelectedInstancesAsync()
    {
        var selectedResourcesNames = this.Get(state => state.SelectedResourceNames);
        var resources = (this.Get(state => state.Resources) ?? []).Where(resource => selectedResourcesNames.Contains(resource.GetName()));
        foreach (var resource in resources)
        {
            await this.SuspendInstanceAsync(resource);
        }
        this.Reduce(state => state with
        {
            SelectedResourceNames = []
        });
    }

    /// <summary>
    /// Resumes selected instances
    /// </summary>
    /// <returns>A awaitable task</returns>
    public async Task OnResumeSelectedInstancesAsync()
    {
        var selectedResourcesNames = this.Get(state => state.SelectedResourceNames);
        var resources = (this.Get(state => state.Resources) ?? []).Where(resource => selectedResourcesNames.Contains(resource.GetName()));
        foreach (var resource in resources)
        {
            await this.ResumeInstanceAsync(resource);
        }
        this.Reduce(state => state with
        {
            SelectedResourceNames = []
        });
    }

    /// <summary>
    /// Cancels selected instances
    /// </summary>
    /// <returns>A awaitable task</returns>
    public async Task OnCancelSelectedInstancesAsync()
    {
        var selectedResourcesNames = this.Get(state => state.SelectedResourceNames);
        var resources = (this.Get(state => state.Resources) ?? []).Where(resource => selectedResourcesNames.Contains(resource.GetName()));
        foreach (var resource in resources)
        {
            await this.CancelInstanceAsync(resource);
        }
        this.Reduce(state => state with
        {
            SelectedResourceNames = []
        });
    }

    /// <summary>
    /// Creates a new instance of the workflow
    /// </summary>
    /// <param name="input">The input data, if any</param>
    /// <returns>A awaitable task</returns>
    public async Task CreateInstanceAsync(string input)
    {
        var workflowName = this.Get(state => state.ActiveResourceName);
        var workflowVersion = this.Get(state => state.WorkflowDefinitionVersion);
        var ns = this.Get(state => state.Namespace);
        if (string.IsNullOrWhiteSpace(workflowName) || string.IsNullOrWhiteSpace(workflowVersion) || string.IsNullOrWhiteSpace(ns))
        {
            await this.Modal!.HideAsync();
            return;
        }
        var inputData = new EquatableDictionary<string, object> { };
        if (!string.IsNullOrWhiteSpace(input)) inputData = this.MonacoEditorHelper.PreferredLanguage == PreferredLanguage.JSON ?
                this.JsonSerializer.Deserialize<EquatableDictionary<string, object>>(input) :
                this.YamlSerializer.Deserialize<EquatableDictionary<string, object>>(input);
        try
        {
            var instance = await this.ApiClient.WorkflowInstances.CreateAsync(new()
            {
                Metadata = new()
                {
                    Namespace =ns,
                    Name = $"{workflowName}-"
                },
                Spec = new()
                {
                    Definition = new()
                    {
                        Namespace = ns,
                        Name = workflowName,
                        Version = workflowVersion
                    },
                    Input = inputData
                }
            });
        }
        catch (Exception ex)
        {
            this.Logger.LogError("Unable to set create workflow instance: {exception}", ex.ToString());
        }
        await this.Modal!.HideAsync();
    }

    /// <summary>
    /// Delete the provided <see cref="WorkflowInstance"/>
    /// </summary>
    /// <param name="workflowInstance">The workflow instance to delete</param>
    /// <returns>A awaitable task</returns>
    public async Task DeleteWorkflowInstanceAsync(WorkflowInstance workflowInstance)
    {
        await this.ApiClient.ManageNamespaced<WorkflowInstance>().DeleteAsync(workflowInstance.GetName(), workflowInstance.GetNamespace()!).ConfigureAwait(false);
    }

    /// <summary>
    /// Selects the target node in the code editor
    /// </summary>
    /// <param name="e">The source of the event</param>
    /// <returns>A awaitable task</returns>
    public async Task SelectNodeInEditor(GraphEventArgs<MouseEventArgs> e)
    {
        if (e.GraphElement == null) return;
        if (this.TextEditor == null || !this._hasTextEditorInitialized) return;
        var source = await this.TextEditor.GetValue();
        var pointer = e.GraphElement.Id;
        var language = this.MonacoEditorHelper.PreferredLanguage;
        var range = await this.MonacoInterop.GetJsonPointerRangeAsync(source, pointer, language);
        await this.TextEditor.SetSelection(range, string.Empty);
        await this.TextEditor.RevealRangeInCenter(range);
    }

    #endregion

    /// <inheritdoc/>
    public override async Task InitializeAsync()
    {
        Observable.CombineLatest(
            this.Namespace.Where(ns => !string.IsNullOrWhiteSpace(ns)),
            this.ActiveResourceName.Where(name => !string.IsNullOrWhiteSpace(name)),
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
        this.MonacoEditorHelper.PreferredThemeChanged += OnPreferredThemeChangedAsync;
        await base.InitializeAsync();
    }

    /// <summary>
    /// Updates the editor theme
    /// </summary>
    /// <param name="newTheme"></param>
    /// <returns></returns>
    protected async Task OnPreferredThemeChangedAsync(string newTheme)
    {
        if (this.TextEditor != null && this._hasTextEditorInitialized)
        {
            await this.TextEditor.UpdateOptions(new EditorUpdateOptions() { Theme = newTheme });
        }
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
                this.MonacoEditorHelper.PreferredThemeChanged -= OnPreferredThemeChangedAsync;
            }
            this._disposed = true;
        }
    }
}
