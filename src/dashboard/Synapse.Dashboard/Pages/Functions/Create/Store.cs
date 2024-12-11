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

namespace Synapse.Dashboard.Pages.Functions.Create;

/// <summary>
/// Represents the <see cref="CreateFunctionViewState"/>
/// </summary>
/// <param name="logger">The service used to perform logging</param>
/// <param name="apiClient">The service used to interact with the Synapse API</param>
/// <param name="monacoEditorHelper">The service used to help handling Monaco editors</param>
/// <param name="jsonSerializer">The service used to serialize/deserialize data to/from JSON</param>
/// <param name="yamlSerializer">The service used to serialize/deserialize data to/from YAML</param>
/// <param name="jsRuntime">The service used for JS interop</param>
/// <param name="navigationManager">The service used to provides an abstraction for querying and managing URI navigation</param>
/// <param name="specificationSchemaManager">The service used to download the specification schemas</param>
/// <param name="monacoInterop">The service to build a bridge with the monaco interop extension</param>
public class CreateFunctionViewStore(
    ILogger<CreateFunctionViewStore> logger,
    ISynapseApiClient apiClient,
    IMonacoEditorHelper monacoEditorHelper,
    IJsonSerializer jsonSerializer,
    IYamlSerializer yamlSerializer,
    IJSRuntime jsRuntime,
    NavigationManager navigationManager,
    SpecificationSchemaManager specificationSchemaManager,
    MonacoInterop monacoInterop
)
    : ComponentStore<CreateFunctionViewState>(new())
{

    private TextModel? _textModel = null;
    private string _textModelUri = string.Empty;
    private bool _disposed = false;
    private bool _processingVersion = false;

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger<CreateFunctionViewStore> Logger { get; } = logger;

    /// <summary>
    /// Gets the service used to interact with the Synapse API
    /// </summary>
    protected ISynapseApiClient ApiClient { get; } = apiClient;

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
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="CreateFunctionViewState.Name"/> changes
    /// </summary>
    public IObservable<string?> Name => this.Select(state => state.Name).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="CreateFunctionViewState.ChosenName"/> changes
    /// </summary>
    public IObservable<string?> ChosenName => this.Select(state => state.ChosenName).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe changes to the state's <see cref="CreateFunctionViewState.Function"/> property
    /// </summary>
    public IObservable<TaskDefinition?> Function => this.Select(state => state.Function).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe changes to the state's <see cref="CreateFunctionViewState.Version"/> property
    /// </summary>
    public IObservable<SemVersion?> Version => this.Select(state => state.Version).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe changes to the state's <see cref="CreateFunctionViewState.FunctionText"/> property
    /// </summary>
    public IObservable<string?> FunctionText => this.Select(state => state.FunctionText).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe changes to the state's <see cref="CreateFunctionViewState.Loading"/> property
    /// </summary>
    public IObservable<bool> Loading => this.Select(state => state.Loading).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe changes to the state's <see cref="CreateFunctionViewState.Saving"/> property
    /// </summary>
    public IObservable<bool> Saving => this.Select(state => state.Saving).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="CreateFunctionViewState.ProblemType"/> changes
    /// </summary>
    public IObservable<Uri?> ProblemType => this.Select(state => state.ProblemType).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="CreateFunctionViewState.ProblemTitle"/> changes
    /// </summary>
    public IObservable<string> ProblemTitle => this.Select(state => state.ProblemTitle).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="CreateFunctionViewState.ProblemDetail"/> changes
    /// </summary>
    public IObservable<string> ProblemDetail => this.Select(state => state.ProblemDetail).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="CreateFunctionViewState.ProblemStatus"/> changes
    /// </summary>
    public IObservable<int> ProblemStatus => this.Select(state => state.ProblemStatus).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="CreateFunctionViewState.ProblemErrors"/> changes
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
    /// Sets the state's <see cref="CreateFunctionViewState.Name"/>
    /// </summary>
    /// <param name="name">The new <see cref="CreateFunctionViewState.Name"/> value</param>
    public void SetName(string? name)
    {
        this.Reduce(state => state with
        {
            Name = name,
            Loading = true
        });
    }

    /// <summary>
    /// Sets the state's <see cref="CreateFunctionViewState.ChosenName"/>
    /// </summary>
    /// <param name="name">The new <see cref="CreateFunctionViewState.ChosenName"/> value</param>
    public void SetChosenName(string? name)
    {
        this.Reduce(state => state with
        {
            ChosenName = name
        });
    }

    /// <summary>
    /// Sets the state's <see cref="CreateFunctionViewState" /> <see cref="ProblemDetails"/>'s related data
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
    /// Gets the <see cref="CustomFunction"/> for the specified namespace and name
    /// </summary>
    /// <param name="name">The name of the <see cref="CustomFunction"/> to create a new version of</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task GetCustomFunctionAsync(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var resources = await this.ApiClient.CustomFunctions.GetAsync(name) ?? throw new NullReferenceException($"Failed to find the specified function '{name}'");
        var version = resources.Spec.Versions.GetLatestVersion();
        var function = resources.Spec.Versions.GetLatest();
        var nextVersion = SemVersion.Parse(version, SemVersionStyles.Strict);
        nextVersion = nextVersion.WithPatch(nextVersion.Patch + 1);
        this.Reduce(s => s with
        {
            Function = function,
            Version = nextVersion,
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
                FunctionText = document
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
        var document = this.Get(state => state.FunctionText);
        if (this.TextEditor == null || string.IsNullOrWhiteSpace(document))
        {
            return;
        }
        try
        {
            await this.TextEditor.SetValue(document);
            await Task.Delay(10);
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
        this.Reduce(state => state with
        {
            FunctionText = document
        });
    }

    /// <summary>
    /// Saves the <see cref="CustomFunction"/> by posting it to the Synapse API
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task SaveCustomFunctionAsync()
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
        var functionText = await this.TextEditor.GetValue();
        if (string.IsNullOrWhiteSpace(functionText))
        {
            this.Reduce(state => state with
            {
                ProblemTitle = "Invalid function",
                ProblemDetail = "The function cannot be empty."
            });
            return;
        }
        try
        {
            var function = this.MonacoEditorHelper.PreferredLanguage == PreferredLanguage.JSON ?
                this.JsonSerializer.Deserialize<TaskDefinition>(functionText)! :
                this.YamlSerializer.Deserialize<TaskDefinition>(functionText)!;
            var name = this.Get(state => state.Name) ?? this.Get(state => state.ChosenName);
            var version = this.Get(state => state.Version).ToString();
            if (string.IsNullOrEmpty(name))
            {
                this.Reduce(state => state with
                {
                    ProblemTitle = "Invalid function",
                    ProblemDetail = "The name cannot be empty."
                });
                return;
            }
            this.Reduce(s => s with
            {
                Saving = true
            });
            CustomFunction? resource = null;
            try
            {
                resource = await this.ApiClient.CustomFunctions.GetAsync(name);
            }
            catch
            {
                // Assume 404, might need actual handling
            }
            if (resource == null)
            {
                resource = await this.ApiClient.CustomFunctions.CreateAsync(new()
                {
                    Metadata = new()
                    {
                        Name = name
                    },
                    Spec = new()
                    {
                        Versions = [new(version, function)]
                    }
                });
            }
            else
            {
                var updatedResource = resource.Clone()!;
                updatedResource.Spec.Versions.Add(new(version, function));
                var jsonPatch = JsonPatch.FromDiff(this.JsonSerializer.SerializeToElement(resource)!.Value, this.JsonSerializer.SerializeToElement(updatedResource)!.Value);
                var patch = this.JsonSerializer.Deserialize<Json.Patch.JsonPatch>(jsonPatch.RootElement);
                if (patch != null)
                {
                    var resourcePatch = new Patch(PatchType.JsonPatch, jsonPatch);
                    await this.ApiClient.ManageCluster<CustomFunction>().PatchAsync(name, resourcePatch, null, this.CancellationTokenSource.Token);
                }
            }
            this.NavigationManager.NavigateTo($"/functions/{name}");
        }
        catch (ProblemDetailsException ex)
        {
            this.SetProblemDetails(ex.Problem);
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            this.Reduce(state => state with
            {
                ProblemTitle = "Serialization error",
                ProblemDetail = "The function definition cannot be serialized.",
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
            this.Reduce(state => state with
            {
                ProblemTitle = "Serialization error",
                ProblemDetail = "The function definition cannot be serialized.",
                ProblemErrors = new Dictionary<string, string[]>()
                {
                    {"Message", [ex.Message] },
                    {"LineNumber", [ex.LineNumber?.ToString()??""] }
                }
            });
        }
        catch (Exception ex)
        {
            this.Logger.LogError("Unable to save function definition: {exception}", ex.ToString());
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
        this.Function.SubscribeAsync(async definition => {
            string document = string.Empty;
            if (definition != null)
            {
                document = this.MonacoEditorHelper.PreferredLanguage == PreferredLanguage.JSON ?
                    this.JsonSerializer.SerializeToText(definition) :
                    this.YamlSerializer.SerializeToText(definition);
            }
            this.Reduce(state => state with
            {
                FunctionText = document
            });
            await this.OnTextBasedEditorInitAsync();
        }, cancellationToken: this.CancellationTokenSource.Token);
        this.Name.Where(name => !string.IsNullOrWhiteSpace(name))
            .SubscribeAsync(async name => 
                await this.GetCustomFunctionAsync(name!), 
                cancellationToken: this.CancellationTokenSource.Token);
        this.MonacoEditorHelper.PreferredThemeChanged += OnPreferredThemeChangedAsync;
        await this.SetValidationSchema();
        await base.InitializeAsync();
    }

    /// <summary>
    /// Updates the editor theme
    /// </summary>
    /// <param name="newTheme"></param>
    /// <returns></returns>
    protected async Task OnPreferredThemeChangedAsync(string newTheme)
    {
        if (this.TextEditor != null)
        {
            await this.TextEditor.UpdateOptions(new EditorUpdateOptions() { Theme = newTheme });
        }
    }

    /// <summary>
    /// Adds validation for the specification of the specified version
    /// </summary>
    /// <param name="version">The version of the spec to add the validation for</param>
    /// <returns>An awaitable task</returns>
    protected async Task SetValidationSchema(string? version = null)
    {
        version ??= await this.SpecificationSchemaManager.GetLatestVersion();
        if (this._processingVersion)
        {
            return;
        }
        this.SetProblemDetails(null);
        this._processingVersion = true;
        try
        {
            var schema = $"https://raw.githubusercontent.com/serverlessworkflow/serverlessworkflow.github.io/main/static/schemas/{version}/workflow.yaml#/$defs/task";
            var type = $"create_{typeof(CustomFunction).Name.ToLower()}_{version}_schema";
            await this.MonacoInterop.AddValidationSchemaAsync(schema, $"https://synapse.io/schemas/{type}.json", $"{type}*").ConfigureAwait(false);
            this._textModelUri = this.MonacoEditorHelper.GetResourceUri(type);
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
                this.MonacoEditorHelper.PreferredThemeChanged -= OnPreferredThemeChangedAsync;
            }
            this._disposed = true;
        }
    }

}
