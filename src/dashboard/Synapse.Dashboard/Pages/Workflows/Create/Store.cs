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

using JsonCons.Utilities;
using Neuroglia.Data;
using Semver;
using ServerlessWorkflow.Sdk.Models;
using ServerlessWorkflow.Sdk.Validation;
using Synapse.Api.Client.Services;
using Synapse.Dashboard.Pages.Workflows.List;
using Synapse.Resources;
using System.Text.RegularExpressions;

namespace Synapse.Dashboard.Pages.Workflows.Create;

/// <summary>
/// Represents the <see cref="CreateWorkflowViewState"/>
/// </summary>
/// <param name="logger">The service used to perform logging</param>
/// <param name="api">The service used to interact with the Synapse API</param>
/// <param name="monacoEditorHelper">The service used to help handling Monaco editors</param>
/// <param name="jsonSerializer">The service used to serialize/deserialize data to/from JSON</param>
/// <param name="yamlSerializer">The service used to serialize/deserialize data to/from YAML</param>
/// <param name="jsRuntime">The service used for JS interop</param>
/// <param name="navigationManager">The service used to provides an abstraction for querying and managing URI navigation</param>
/// <param name="specificationSchemaManager">The service used to download the specification schemas</param>
/// <param name="monacoInterop">The service to build a bridge with the monaco interop extension</param>
/// <param name="workflowDefinitionValidator">The service to validate workflow definitions</param>
public class CreateWorkflowViewStore(
    ILogger<CreateWorkflowViewStore> logger,
    ISynapseApiClient api,
    IMonacoEditorHelper monacoEditorHelper,
    IJsonSerializer jsonSerializer,
    IYamlSerializer yamlSerializer,
    IJSRuntime jsRuntime,
    NavigationManager navigationManager,
    SpecificationSchemaManager specificationSchemaManager,
    MonacoInterop monacoInterop,
    IWorkflowDefinitionValidator workflowDefinitionValidator
)
    : ComponentStore<CreateWorkflowViewState>(new())
{

    private TextModel? _textModel = null;
    private string _textModelUri = string.Empty;
    private bool _disposed = false;
    private bool _processingVersion = false;
    private bool _hasTextEditorInitialized = false;

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger<CreateWorkflowViewStore> Logger { get; } = logger;

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
    /// Gets the service used for JS interop
    /// </summary>
    protected IJSRuntime JSRuntime { get; } = jsRuntime;

    /// <summary>
    /// Gets the service used to provides an abstraction for querying and managing URI navigation
    /// </summary>
    protected NavigationManager NavigationManager { get; } = navigationManager;

    /// <summary>
    /// Gets the service used to download the specification schemas
    /// </summary>
    protected SpecificationSchemaManager SpecificationSchemaManager { get; } = specificationSchemaManager;

    /// <summary>
    /// Gets the service to build a bridge with the monaco interop extension
    /// </summary>
    protected MonacoInterop MonacoInterop { get; } = monacoInterop;

    /// <summary>
    /// Gets the service to  to validate workflow definitions
    /// </summary>
    protected IWorkflowDefinitionValidator WorkflowDefinitionValidator { get; } = workflowDefinitionValidator;

    /// <summary>
    /// The <see cref="BlazorMonaco.Editor.StandaloneEditorConstructionOptions"/> provider function
    /// </summary>
    public Func<StandaloneCodeEditor, StandaloneEditorConstructionOptions> StandaloneEditorConstructionOptions = monacoEditorHelper.GetStandaloneEditorConstructionOptions(" ", false, monacoEditorHelper.PreferredLanguage);

    /// <summary>
    /// The <see cref="StandaloneCodeEditor"/> reference
    /// </summary>
    public StandaloneCodeEditor? TextEditor { get; set; }

    #region Selectors
    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="CreateWorkflowViewState.Namespace"/> changes
    /// </summary>
    public IObservable<string?> Namespace => this.Select(state => state.Namespace).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="CreateWorkflowViewState.Name"/> changes
    /// </summary>
    public IObservable<string?> Name => this.Select(state => state.Name).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe changes to the state's <see cref="CreateWorkflowViewState.WorkflowDefinition"/> property
    /// </summary>
    public IObservable<WorkflowDefinition?> WorkflowDefinition => this.Select(state => state.WorkflowDefinition).DistinctUntilChanged();
    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe changes to the state's <see cref="CreateWorkflowViewState.WorkflowDefinitionText"/> property
    /// </summary>
    public IObservable<string?> WorkflowDefinitionText => this.Select(state => state.WorkflowDefinitionText).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="CreateWorkflowViewState.Labels"/> changes
    /// </summary>
    public IObservable<EquatableDictionary<string, string>> Labels => this.Select(state => state.Labels).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="CreateWorkflowViewState.Annotations"/> changes
    /// </summary>
    public IObservable<EquatableDictionary<string, string>> Annotations => this.Select(state => state.Annotations).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe changes to the state's <see cref="CreateWorkflowViewState.Loading"/> property
    /// </summary>
    public IObservable<bool> Loading => this.Select(state => state.Loading).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe changes to the state's <see cref="CreateWorkflowViewState.Saving"/> property
    /// </summary>
    public IObservable<bool> Saving => this.Select(state => state.Saving).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="CreateWorkflowViewState.ProblemType"/> changes
    /// </summary>
    public IObservable<Uri?> ProblemType => this.Select(state => state.ProblemType).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="CreateWorkflowViewState.ProblemTitle"/> changes
    /// </summary>
    public IObservable<string> ProblemTitle => this.Select(state => state.ProblemTitle).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="CreateWorkflowViewState.ProblemDetail"/> changes
    /// </summary>
    public IObservable<string> ProblemDetail => this.Select(state => state.ProblemDetail).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="CreateWorkflowViewState.ProblemStatus"/> changes
    /// </summary>
    public IObservable<int> ProblemStatus => this.Select(state => state.ProblemStatus).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="CreateWorkflowViewState.ProblemErrors"/> changes
    /// </summary>
    public IObservable<IDictionary<string, string[]>> ProblemErrors => this.Select(state => state.ProblemErrors).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="WorkflowListState.Operators"/> changes
    /// </summary>
    public IObservable<EquatableList<Operator>?> Operators => this.Select(s => s.Operators).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="WorkflowListState.Operator"/> changes
    /// </summary>
    public IObservable<string?> Operator => this.Select(state => state.Labels.TryGetValue(SynapseDefaults.Resources.Labels.Operator, out var label) ? label : null).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe computed <see cref="Neuroglia.ProblemDetails"/>
    /// </summary>
    public IObservable<ProblemDetails?> ProblemDetails => Observable.CombineLatest(
        ProblemType,
        ProblemTitle,
        ProblemStatus,
        ProblemDetail,
        ProblemErrors,
        (type, title, status, details, errors) =>
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }
            return new ProblemDetails(type ?? new Uri("unknown://"), title, status, details, null, errors, null);
        }
    );
    #endregion

    #region Setters
    /// <summary>
    /// Sets the state's <see cref="CreateWorkflowViewState.Namespace"/>
    /// </summary>
    /// <param name="ns">The new <see cref="CreateWorkflowViewState.Namespace"/> value</param>
    public void SetNamespace(string? ns)
    {
        Reduce(state => state with
        {
            Namespace = ns,
            Loading = true
        });
    }

    /// <summary>
    /// Sets the state's <see cref="CreateWorkflowViewState.Name"/>
    /// </summary>
    /// <param name="name">The new <see cref="CreateWorkflowViewState.Name"/> value</param>
    public void SetName(string? name)
    {
        Reduce(state => state with
        {
            Name = name,
            Loading = true
        });
    }

    /// <summary>
    /// Sets the state's <see cref="CreateWorkflowViewState.IsNew"/>
    /// </summary>
    /// <param name="isNew">The new <see cref="CreateWorkflowViewState.IsNew"/> value</param>
    public void SetIsNew(bool isNew)
    {
        Reduce(state => state with
        {
            IsNew = isNew
        });
    }

    /// <summary>
    /// Sets the state's <see cref="CreateWorkflowViewState" /> <see cref="ProblemDetails"/>'s related data
    /// </summary>
    /// <param name="problem">The <see cref="ProblemDetails"/> to populate the data with</param>
    public void SetProblemDetails(ProblemDetails? problem)
    {
        Reduce(state => state with
        {
            ProblemType = problem?.Type,
            ProblemTitle = problem?.Title ?? string.Empty,
            ProblemStatus = problem?.Status ?? 0,
            ProblemDetail = problem?.Detail ?? string.Empty,
            ProblemErrors = problem?.Errors?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? []
        });
    }
    #endregion

    #region Actions
    /// <summary>
    /// Gets the <see cref="ServerlessWorkflow.Sdk.Models.WorkflowDefinition"/> for the specified namespace and name
    /// </summary>
    /// <param name="namespace">The namespace the <see cref="ServerlessWorkflow.Sdk.Models.WorkflowDefinition"/> to create a new version of belongs to</param>
    /// <param name="name">The name of the <see cref="ServerlessWorkflow.Sdk.Models.WorkflowDefinition"/> to create a new version of</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task GetWorkflowDefinitionAsync(string @namespace, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var workflow = await Api.Workflows.GetAsync(name, @namespace) ?? throw new NullReferenceException($"Failed to find the specified workflow '{name}.{@namespace}'");
        var definition = workflow.Spec.Versions.GetLatest();
        var nextVersion = SemVersion.Parse(definition.Document.Version, SemVersionStyles.Strict);
        nextVersion = nextVersion.WithPatch(nextVersion.Patch + 1);
        definition.Document.Version = nextVersion.ToString();
        Reduce(s => s with
        {
            WorkflowDefinition = definition,
            Labels = workflow.Metadata.Labels != null ? [..workflow.Metadata.Labels] : [],
            Annotations = workflow.Metadata.Annotations != null ? [.. workflow.Metadata.Annotations] : [],
            Loading = false
        });
    }

    /// <summary>
    /// Handles changed of the text editor's language
    /// </summary>
    /// <param name="_"></param>
    /// <returns></returns>
    public async Task ToggleTextBasedEditorLanguageAsync(string _)
    {
        if (TextEditor == null || !_hasTextEditorInitialized) return;
        var language = MonacoEditorHelper.PreferredLanguage;
        try
        {
            var document = await TextEditor.GetValue();
            if (document == null) return;
            document = language == PreferredLanguage.YAML ?
                YamlSerializer.ConvertFromJson(document) :
                YamlSerializer.ConvertToJson(document);
            Reduce(state => state with
            {
                WorkflowDefinitionText = document
            });
            await InitializeTextBasedEditorAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError("Unable to change text editor language: {exception}", ex.ToString());
            await MonacoEditorHelper.ChangePreferredLanguageAsync(language == PreferredLanguage.YAML ? PreferredLanguage.JSON : PreferredLanguage.YAML);
        }
    }

    /// <summary>
    /// Handles initialization of the text editor
    /// </summary>
    /// <returns></returns>
    public async Task OnTextBasedEditorInitAsync()
    {
        _hasTextEditorInitialized = true;
        await InitializeTextBasedEditorAsync();
    }

    /// <summary>
    /// Initializes the text editor
    /// </summary>
    /// <returns></returns>
    public async Task InitializeTextBasedEditorAsync()
    {
        if (TextEditor == null || !_hasTextEditorInitialized) return;
        await SetTextBasedEditorLanguageAsync();
        await SetTextEditorValueAsync();
    }

    /// <summary>
    /// Sets the language of the text editor
    /// </summary>
    /// <returns></returns>
    public async Task SetTextBasedEditorLanguageAsync()
    {
        if (TextEditor == null || !_hasTextEditorInitialized) return;
        try
        {
            var language = MonacoEditorHelper.PreferredLanguage;
            _textModel = await Global.GetModel(JSRuntime, _textModelUri);
            _textModel ??= await Global.CreateModel(JSRuntime, "", language, _textModelUri);
            await Global.SetModelLanguage(JSRuntime, _textModel, language);
            await TextEditor!.SetModel(_textModel);
        }
        catch (Exception ex)
        {
            Logger.LogError("Unable to set text editor language: {exception}", ex.ToString());
        }
    }

    /// <summary>
    /// Changes the value of the text editor
    /// </summary>
    /// <returns></returns>
    async Task SetTextEditorValueAsync()
    {
        var document = Get(state => state.WorkflowDefinitionText);
        if (TextEditor == null || string.IsNullOrWhiteSpace(document) || !_hasTextEditorInitialized) return;
        try
        {
            await TextEditor.SetValue(document);
            await Task.Delay(10);
            await TextEditor.Trigger("", "editor.action.formatDocument");
        }
        catch (Exception ex)
        {
            Logger.LogError("Unable to set text editor value: {exception}", ex.ToString());
        }
    }

    /// <summary>
    /// Handles text editor content changes
    /// </summary>
    /// <param name="e">The <see cref="ModelContentChangedEvent"/></param>
    /// <returns>An awaitable task</returns>
    public async Task OnDidChangeModelContent(ModelContentChangedEvent e)
    {
        if (TextEditor == null || !_hasTextEditorInitialized) return;
        var document = await TextEditor.GetValue();
        Reduce(state => state with
        {
            WorkflowDefinitionText = document
        });
    }

    /// <summary>
    /// Sets the workflow labels
    /// </summary>
    /// <param name="labels">The new labels</param>
    public virtual void SetLabels(EquatableDictionary<string, string>? labels)
    {
        this.Reduce(state => state with
        {
            Labels = labels != null ? [.. labels] : []
        });
    }

    /// <summary>
    /// Adds a single label
    /// </summary>
    /// <param name="key">The key of the label</param>
    /// <param name="value">The value of the label</param>
    public virtual void AddLabel(string key, string value)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
        {
            return;
        }
        var labels = new EquatableDictionary<string, string>(this.Get(state => state.Labels));
        if (labels.ContainsKey(key))
        {
            labels.Remove(key);
        }
        labels.Add(key, value);
        this.SetLabels(labels);
    }

    /// <summary>
    /// Removes a single label using it's key
    /// </summary>
    /// <param name="key">The label selector key to remove</param>
    public void RemoveLabel(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }
        var labels = new EquatableDictionary<string, string>(this.Get(state => state.Labels));
        if (labels.ContainsKey(key))
        {
            labels.Remove(key);
        }
        this.SetLabels(labels);
    }

    /// <summary>
    /// Sets the <see cref="WorkflowListState.Operator"/> 
    /// </summary>
    /// <param name="operatorName">The new value</param>
    public void SetOperator(string? operatorName)
    {
        if (string.IsNullOrEmpty(operatorName))
            RemoveLabel(SynapseDefaults.Resources.Labels.Operator);
        else
            AddLabel(SynapseDefaults.Resources.Labels.Operator, operatorName);
    }

    /// <summary>
    /// Sets the workflow annotations
    /// </summary>
    /// <param name="annotations">The new annotations</param>
    public virtual void SetAnnotations(EquatableDictionary<string, string>? annotations)
    {
        this.Reduce(state => state with
        {
            Annotations = annotations != null ? [.. annotations] : []
        });
    }

    /// <summary>
    /// Adds a single annotation
    /// </summary>
    /// <param name="key">The key of the annotation</param>
    /// <param name="value">The value of the annotation</param>
    public virtual void AddAnnotation(string key, string value)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
        {
            return;
        }
        var annotations = new EquatableDictionary<string, string>(this.Get(state => state.Annotations));
        if (annotations.ContainsKey(key))
        {
            annotations.Remove(key);
        }
        annotations.Add(key, value);
        this.SetAnnotations(annotations);
    }

    /// <summary>
    /// Removes a single annotation using it's key
    /// </summary>
    /// <param name="key">The annotation selector key to remove</param>
    public void RemoveAnnotation(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }
        var annotations = new EquatableDictionary<string, string>(this.Get(state => state.Annotations));
        if (annotations.ContainsKey(key))
        {
            annotations.Remove(key);
        }
        this.SetAnnotations(annotations);
    }

    /// <summary>
    /// Saves the <see cref="WorkflowDefinition"/> by posting it to the Synapse API
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task SaveWorkflowDefinitionAsync()
    {
        if (TextEditor == null || !_hasTextEditorInitialized)
        {
            Reduce(state => state with
            {
                ProblemTitle = "Text editor",
                ProblemDetail = "The text editor must be initialized."
            });
            return;
        }
        var workflowDefinitionText = await TextEditor.GetValue();
        if (string.IsNullOrWhiteSpace(workflowDefinitionText))
        {
            Reduce(state => state with
            {
                ProblemTitle = "Invalid definition",
                ProblemDetail = "The workflow definition cannot be empty."
            });
            return;
        }
        try
        {
            var workflowDefinition = MonacoEditorHelper.PreferredLanguage == PreferredLanguage.JSON ?
                JsonSerializer.Deserialize<WorkflowDefinition>(workflowDefinitionText)! :
                YamlSerializer.Deserialize<WorkflowDefinition>(workflowDefinitionText)!;
            var validationResult = await WorkflowDefinitionValidator.ValidateAsync(workflowDefinition);
            if (!validationResult.IsValid)
            {
                var errors = new Dictionary<string, string[]>();
                validationResult.Errors?.Select(e => e.Reference ?? "")?.Distinct()?.ToList()?.ForEach(reference =>
                {
                    errors.Add(reference, validationResult.Errors!.Where(e => e.Reference == reference).Select(e => e.Details ?? "").ToArray());
                });
                Reduce(state => state with
                {
                    ProblemTitle = "Invalid definition",
                    ProblemDetail = "The workflow definition is not valid.",
                    ProblemErrors = errors
                });
                return;
            }
            var @namespace = workflowDefinition!.Document.Namespace;
            var name = workflowDefinition.Document.Name;
            var version = workflowDefinition.Document.Version;
            var labels = Get(state => state.Labels);
            var annotations = Get(state => state.Annotations);
            var isNew = Get(state => state.IsNew);
            Reduce(s => s with
            {
                Saving = true
            });
            Workflow? workflow = null;
            if (isNew)
            {
                try {
                    workflow = new()
                    {
                        Metadata = new()
                        {
                            Namespace = workflowDefinition!.Document.Namespace,
                            Name = workflowDefinition.Document.Name
                        },
                        Spec = new()
                        {
                            Versions = [workflowDefinition]
                        }
                    };
                    if (labels.Count > 0) workflow.Metadata.Labels = labels;
                    if (annotations.Count > 0) workflow.Metadata.Annotations = annotations;
                    workflow = await Api.Workflows.CreateAsync(workflow);
                    NavigationManager.NavigateTo($"/workflows/details/{@namespace}/{name}/{version}");
                    return;
                }
                catch (ProblemDetailsException ex) when (ex.Problem.Title == "Conflict" && ex.Problem.Detail != null && ex.Problem.Detail.EndsWith("already exists"))
                {
                    // the workflow exists, try to update it instead
                }
            }
            workflow = await Api.Workflows.GetAsync(name, @namespace);
            var updatedResource = workflow.Clone()!;
            var documentVersion = SemVersion.Parse(version, SemVersionStyles.Strict)!;
            var latestVersion = SemVersion.Parse(updatedResource.Spec.Versions.GetLatest().Document.Version, SemVersionStyles.Strict)!;
            if (updatedResource.Spec.Versions.Any(v => SemVersion.Parse(v.Document.Version, SemVersionStyles.Strict).CompareSortOrderTo(documentVersion) >= 0))
            {
                Reduce(state => state with
                {
                    ProblemTitle = "Invalid version",
                    ProblemDetail = $"The specified version '{documentVersion}' must be strictly superior to the latest version '{latestVersion}'."
                });
                return;
            }
            updatedResource.Metadata.Labels = labels;
            updatedResource.Metadata.Annotations = annotations;
            updatedResource.Spec.Versions.Add(workflowDefinition!);
            var jsonPatch = JsonPatch.FromDiff(JsonSerializer.SerializeToElement(workflow)!.Value, JsonSerializer.SerializeToElement(updatedResource)!.Value);
            var patch = JsonSerializer.Deserialize<Json.Patch.JsonPatch>(jsonPatch.RootElement);
            if (patch != null)
            {
                var resourcePatch = new Patch(PatchType.JsonPatch, jsonPatch);
                await Api.ManageNamespaced<Workflow>().PatchAsync(name, @namespace, resourcePatch, null, CancellationTokenSource.Token);
            }
            NavigationManager.NavigateTo($"/workflows/details/{@namespace}/{name}/{version}");
        }
        catch (ProblemDetailsException ex)
        {
            SetProblemDetails(ex.Problem);
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            Reduce(state => state with
            {
                ProblemTitle = "Serialization error",
                ProblemDetail = "The workflow definition cannot be serialized.",
                ProblemErrors = new Dictionary<string, string[]>()
                {
                    {"Message", [ex.Message] },
                    {"Start", [ex.Start.ToString()] },
                    {"End", [ex.End.ToString()] }
                }
            });
        }
        catch (System.Text.Json.JsonException ex)
        {
            Reduce(state => state with
            {
                ProblemTitle = "Serialization error",
                ProblemDetail = "The workflow definition cannot be serialized.",
                ProblemErrors = new Dictionary<string, string[]>()
                {
                    {"Message", [ex.Message] },
                    {"LineNumber", [ex.LineNumber?.ToString()??""] }
                }
            });
        }
        catch (Exception ex)
        {
            Logger.LogError("Unable to save workflow definition: {exception}", ex.ToString());
            Reduce(state => state with
            {
                ProblemTitle = "Error",
                ProblemDetail = "An error occurred while saving the workflow.",
                ProblemErrors = new Dictionary<string, string[]>()
                {
                    {"Message", [ex.ToString()] }
                }
            });
        }
        finally
        {
            Reduce(s => s with
            {
                Saving = false
            });
        }
    }
    
    /// <summary>
    /// Lists all available <see cref="Operator"/>s
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task ListOperatorsAsync()
    {
        var operatorList = new EquatableList<Operator>(await (await api.Operators.ListAsync().ConfigureAwait(false)).OrderBy(ns => ns.GetQualifiedName()).ToListAsync().ConfigureAwait(false));
        Reduce(s => s with
        {
            Operators = operatorList
        });
    }
    #endregion

    /// <inheritdoc/>
    public override async Task InitializeAsync()
    {
        await ListOperatorsAsync().ConfigureAwait(false);
        Loading.Where(loading => loading == true).Subscribe(loading =>
        {
            _hasTextEditorInitialized = false; // reset text editor state when loading
        }, token: CancellationTokenSource.Token);
        WorkflowDefinition.SubscribeAsync(async definition => {
            string document = "";
            if (definition != null)
            {
                document = MonacoEditorHelper.PreferredLanguage == PreferredLanguage.JSON ?
                    JsonSerializer.SerializeToText(definition) :
                    YamlSerializer.SerializeToText(definition);
            }
            Reduce(state => state with
            {
                WorkflowDefinitionText = document
            });
            await InitializeTextBasedEditorAsync();
        }, cancellationToken: CancellationTokenSource.Token);
        Observable.CombineLatest(
            Namespace.Where(ns => !string.IsNullOrWhiteSpace(ns)),
            Name.Where(name => !string.IsNullOrWhiteSpace(name)),
            (ns, name) => (ns!, name!)
        ).SubscribeAsync(async ((string ns, string name) workflow) =>
        {
            await GetWorkflowDefinitionAsync(workflow.ns, workflow.name);
        }, cancellationToken: CancellationTokenSource.Token);
        WorkflowDefinitionText.Where(document => !string.IsNullOrEmpty(document)).Throttle(new(100)).SubscribeAsync(async (document) => 
        {
            if (string.IsNullOrWhiteSpace(document)) return;
            var currentDslVersion = Get(state => state.DslVersion);
            var versionExtractor = new Regex("'?\"?(dsl|DSL)'?\"?\\s*:\\s*'?\"?([\\w\\.\\-\\+]*)'?\"?");
            var match = versionExtractor.Match(document);
            if (match == null) return;
            var documentDslVersion = match.Groups[2].Value;
            if (documentDslVersion == currentDslVersion) return;
            await SetValidationSchema(documentDslVersion);
        }, cancellationToken: CancellationTokenSource.Token);
        MonacoEditorHelper.PreferredThemeChanged += OnPreferredThemeChangedAsync;
        await base.InitializeAsync();
    }

    /// <summary>
    /// Adds validation for the specification of the specified version
    /// </summary>
    /// <param name="version">The version of the spec to add the validation for</param>
    /// <returns>An awaitable task</returns>
    protected async Task SetValidationSchema(string? version = null)
    {
        version ??= await SpecificationSchemaManager.GetLatestVersion();
        var currentVersion = Get(state => state.DslVersion);
        if (currentVersion == version)
        {
            return;
        }
        if (_processingVersion)
        {
            return;
        }
        SetProblemDetails(null);
        _processingVersion = true;
        try
        {
            var schema = await SpecificationSchemaManager.GetSchema(version);
            var type = $"create_{typeof(WorkflowDefinition).Name.ToLower()}_{version}_schema";
            await MonacoInterop.AddValidationSchemaAsync(schema, $"https://synapse.io/schemas/{type}.json", $"{type}*").ConfigureAwait(false);
            _textModelUri = MonacoEditorHelper.GetResourceUri(type);
            Reduce(state => state with
            {
                DslVersion = version
            });
        }
        catch (Exception ex)
        {
            Logger.LogError("Unable to set the validation schema: {exception}", ex.ToString());
            SetProblemDetails(new ProblemDetails(new Uri("about:blank"), "Unable to set the validation schema", 404, $"Unable to set the validation schema for the specification version '{version}'. Make sure the version exists."));
        }
        _processingVersion = false;
    }

    /// <summary>
    /// Updates the editor theme
    /// </summary>
    /// <param name="newTheme"></param>
    /// <returns></returns>
    protected async Task OnPreferredThemeChangedAsync(string newTheme)
    {
        if (TextEditor != null && _hasTextEditorInitialized)
        {
            await TextEditor.UpdateOptions(new EditorUpdateOptions() { Theme = newTheme });
        }
    }

    /// <summary>
    /// Disposes of the store
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the dispose of the store</param>
    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_textModel != null)
                {
                    _textModel.DisposeModel();
                    _textModel = null;
                }
                if (TextEditor != null)
                {
                    TextEditor.Dispose();
                    TextEditor = null;
                }
                MonacoEditorHelper.PreferredThemeChanged -= OnPreferredThemeChangedAsync;
            }
            _disposed = true;
        }
    }

}
