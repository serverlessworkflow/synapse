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
using Synapse.Api.Client.Services;
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
public class CreateWorkflowViewStore(
    ILogger<CreateWorkflowViewStore> logger,
    ISynapseApiClient api,
    IMonacoEditorHelper monacoEditorHelper,
    IJsonSerializer jsonSerializer,
    IYamlSerializer yamlSerializer,
    IJSRuntime jsRuntime,
    NavigationManager navigationManager,
    SpecificationSchemaManager specificationSchemaManager,
    MonacoInterop monacoInterop
)
    : ComponentStore<CreateWorkflowViewState>(new())
{

    private TextModel? _textModel = null;
    private string _textModelUri = string.Empty;
    private bool _disposed = false;
    private bool _processingVersion = false;

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
    /// The <see cref="BlazorMonaco.Editor.StandaloneEditorConstructionOptions"/> provider function
    /// </summary>
    public Func<StandaloneCodeEditor, StandaloneEditorConstructionOptions> StandaloneEditorConstructionOptions = monacoEditorHelper.GetStandaloneEditorConstructionOptions(string.Empty, false, monacoEditorHelper.PreferredLanguage);

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
    /// Gets an <see cref="IObservable{T}"/> used to observe computed <see cref="Neuroglia.ProblemDetails"/>
    /// </summary>
    public IObservable<ProblemDetails?> ProblemDetails => Observable.CombineLatest(
        this.ProblemType,
        this.ProblemTitle,
        this.ProblemStatus,
        this.ProblemDetail,
        this.ProblemErrors,
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
        this.Reduce(state => state with
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
        this.Reduce(state => state with
        {
            Name = name,
            Loading = true
        });
    }

    /// <summary>
    /// Sets the state's <see cref="ResourceEditorState{TResource}" /> <see cref="ProblemDetails"/>'s related data
    /// </summary>
    /// <param name="problem">The <see cref="ProblemDetails"/> to populate the data with</param>
    public void SetProblemDetails(ProblemDetails? problem)
    {
        this.Reduce(state => state with
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
        var workflow = await this.Api.Workflows.GetAsync(name, @namespace) ?? throw new NullReferenceException($"Failed to find the specified workflow '{name}.{@namespace}'");
        var definition = workflow.Spec.Versions.GetLatest();
        var nextVersion = SemVersion.Parse(definition.Document.Version, SemVersionStyles.Strict);
        nextVersion = nextVersion.WithPatch(nextVersion.Patch + 1);
        definition.Document.Version = nextVersion.ToString();
        this.Reduce(s => s with
        {
            WorkflowDefinition = definition,
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
        if (this.TextEditor == null)
        {
            return;
        }
        var language = this.MonacoEditorHelper.PreferredLanguage;
        try
        {
            var document = await this.TextEditor.GetValue();
            if (document == null)
            {
                return;
            }
            document = language == PreferredLanguage.YAML ?
                this.YamlSerializer.ConvertFromJson(document) :
                this.YamlSerializer.ConvertToJson(document);
            this.Reduce(state => state with
            {
                WorkflowDefinitionText = document
            });
            await this.OnTextBasedEditorInitAsync();
        }
        catch (Exception ex)
        {
            this.Logger.LogError("Unable to change text editor language: {exception}", ex.ToString());
            await this.MonacoEditorHelper.ChangePreferredLanguageAsync(language == PreferredLanguage.YAML ? PreferredLanguage.JSON : PreferredLanguage.YAML);
        }
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
        if (this.TextEditor == null)
        {
            return;
        }
        try
        {
            var language = this.MonacoEditorHelper.PreferredLanguage;
            this._textModel = await Global.GetModel(this.JSRuntime, this._textModelUri);
            this._textModel ??= await Global.CreateModel(this.JSRuntime, "", language, this._textModelUri);
            await Global.SetModelLanguage(this.JSRuntime, this._textModel, language);
            await this.TextEditor!.SetModel(this._textModel);
        }
        catch (Exception ex)
        {
            this.Logger.LogError("Unable to set text editor language: {exception}", ex.ToString());
        }
    }

    /// <summary>
    /// Changes the value of the text editor
    /// </summary>
    /// <returns></returns>
    async Task SetTextEditorValueAsync()
    {
        var document = this.Get(state => state.WorkflowDefinitionText);
        if (this.TextEditor == null || string.IsNullOrWhiteSpace(document))
        {
            return;
        }
        await this.TextEditor.SetValue(document);
        try
        {
            await this.TextEditor.Trigger("", "editor.action.formatDocument");
        }
        catch (Exception ex)
        {
            this.Logger.LogError("Unable to set text editor value: {exception}", ex.ToString());
        }
    }

    /// <summary>
    /// Handles text editor content changes
    /// </summary>
    /// <param name="e">The <see cref="ModelContentChangedEvent"/></param>
    /// <returns>An awaitable task</returns>
    public async Task OnDidChangeModelContent(ModelContentChangedEvent e)
    {
        if (this.TextEditor == null) return;
        var document = await this.TextEditor.GetValue();
        this.Reduce(state => state with {
            WorkflowDefinitionText = document
        });
    }

    /// <summary>
    /// Saves the <see cref="WorkflowDefinition"/> by posting it to the Synapse API
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task SaveWorkflowDefinitionAsync()
    {
        if (this.TextEditor == null)
        {
            this.Reduce(state => state with
            {
                ProblemTitle = "Text editor",
                ProblemDetail = "The text editor must be initialized."
            });
            return;
        }
        var workflowDefinitionText = await this.TextEditor.GetValue();
        if (string.IsNullOrWhiteSpace(workflowDefinitionText))
        {
            this.Reduce(state => state with
            {
                ProblemTitle = "Invalid definition",
                ProblemDetail = "The workflow definition cannot be empty."
            });
            return;
        }
        try
        {
            var workflowDefinition = this.MonacoEditorHelper.PreferredLanguage == PreferredLanguage.JSON ?
                this.JsonSerializer.Deserialize<WorkflowDefinition>(workflowDefinitionText) :
                this.YamlSerializer.Deserialize<WorkflowDefinition>(workflowDefinitionText);
            var @namespace = workflowDefinition!.Document.Namespace;
            var name = workflowDefinition.Document.Name;
            var version = workflowDefinition.Document.Version;
            this.Reduce(s => s with
            {
                Saving = true
            });
            Workflow? workflow = null;
            try
            {
                workflow = await this.Api.Workflows.GetAsync(name, @namespace);
            }
            catch
            {
                // Assume 404, might need actual handling
            }
            if (workflow == null)
            {
                workflow = await this.Api.Workflows.CreateAsync(new()
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
                });
            }
            else
            {
                var updatedResource = workflow.Clone()!;
                var documentVersion = SemVersion.Parse(version, SemVersionStyles.Strict)!;
                var latestVersion = SemVersion.Parse(updatedResource.Spec.Versions.GetLatest().Document.Version, SemVersionStyles.Strict)!;
                if (updatedResource.Spec.Versions.Any(v => SemVersion.Parse(v.Document.Version, SemVersionStyles.Strict).CompareSortOrderTo(documentVersion) >= 0))
                {
                    this.Reduce(state => state with
                    {
                        ProblemTitle = "Invalid version",
                        ProblemDetail = $"The specified version '{documentVersion}' must be strictly superior to the latest version '{latestVersion}'."
                    });
                    return;
                }
                updatedResource.Spec.Versions.Add(workflowDefinition!);
                var jsonPatch = JsonPatch.FromDiff(this.JsonSerializer.SerializeToElement(workflow)!.Value, this.JsonSerializer.SerializeToElement(updatedResource)!.Value);
                var patch = this.JsonSerializer.Deserialize<Json.Patch.JsonPatch>(jsonPatch.RootElement);
                if (patch != null)
                {
                    var resourcePatch = new Patch(PatchType.JsonPatch, jsonPatch);
                    await this.Api.ManageNamespaced<Workflow>().PatchAsync(name, @namespace, resourcePatch, null, this.CancellationTokenSource.Token);
                }
            }
            this.NavigationManager.NavigateTo($"/workflows/details/{@namespace}/{name}/{version}");
        }
        catch (ProblemDetailsException ex)
        {
            this.SetProblemDetails(ex.Problem);
        }
        catch (Exception ex)
        {
            this.Logger.LogError("Unable to save workflow definition: {exception}", ex.ToString());
        }
        finally
        {
            this.Reduce(s => s with
            {
                Saving = false
            });
        }
    }
    #endregion

    /// <inheritdoc/>
    public override async Task InitializeAsync()
    {
        this.WorkflowDefinition.SubscribeAsync(async definition => {
            string document = "";
            if (definition != null)
            {
                document = this.MonacoEditorHelper.PreferredLanguage == PreferredLanguage.JSON ?
                    this.JsonSerializer.SerializeToText(definition) :
                    this.YamlSerializer.SerializeToText(definition);
            }
            this.Reduce(state => state with
            {
                WorkflowDefinitionText = document
            });
            await this.OnTextBasedEditorInitAsync();
        }, cancellationToken: this.CancellationTokenSource.Token);
        Observable.CombineLatest(
            this.Namespace.Where(ns => !string.IsNullOrWhiteSpace(ns)),
            this.Name.Where(name => !string.IsNullOrWhiteSpace(name)),
            (ns, name) => (ns!, name!)
        ).SubscribeAsync(async ((string ns, string name) workflow) =>
        {
            await this.GetWorkflowDefinitionAsync(workflow.ns, workflow.name);
        }, cancellationToken: this.CancellationTokenSource.Token);
        this.WorkflowDefinitionText.Where(document => !string.IsNullOrEmpty(document)).Throttle(new(100)).SubscribeAsync(async (document) => {
            if (string.IsNullOrWhiteSpace(document))
            {
                return;
            }
            var currentDslVersion = this.Get(state => state.DslVersion);
            var versionExtractor = new Regex("'?\"?(dsl|DSL)'?\"?\\s*:\\s*'?\"?([\\w\\.\\-\\+]*)'?\"?");
            var match = versionExtractor.Match(document);
            if (match == null)
            {
                return;
            }
            var documentDslVersion = match.Groups[2].Value;
            if (documentDslVersion == currentDslVersion)
            {
                return;
            }
            await this.SetValidationSchema("v" + documentDslVersion);
        }, cancellationToken: this.CancellationTokenSource.Token);
        await base.InitializeAsync();
    }

    /// <summary>
    /// Adds validation for the specification of the specified version
    /// </summary>
    /// <param name="version">The version of the spec to add the validation for</param>
    /// <returns>An awaitable task</returns>
    protected async Task SetValidationSchema(string? version = null)
    {
        version ??= await this.SpecificationSchemaManager.GetLatestVersion();
        var currentVersion = this.Get(state => state.DslVersion);
        if (currentVersion == version)
        {
            return;
        }
        if (this._processingVersion)
        {
            return;
        }
        this.SetProblemDetails(null);
        this._processingVersion = true;
        try
        {
            var schema = await this.SpecificationSchemaManager.GetSchema(version);
            var type = $"create_{typeof(WorkflowDefinition).Name.ToLower()}_{version}_schema";
            await this.MonacoInterop.AddValidationSchemaAsync(schema, $"https://synapse.io/schemas/{type}.json", $"{type}*").ConfigureAwait(false);
            this._textModelUri = this.MonacoEditorHelper.GetResourceUri(type);
            this.Reduce(state => state with
            {
                DslVersion = version
            });
        }
        catch (Exception ex)
        {
            this.Logger.LogError("Unable to set the validation schema: {exception}", ex.ToString());
            this.SetProblemDetails(new ProblemDetails(new Uri("about:blank"), "Unable to set the validation schema", 404, $"Unable to set the validation schema for the specification version '{version}'. Make sure the version exists."));
        }
        this._processingVersion = false;
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
