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

using Synapse.Api.Client.Services;
using Synapse.Dashboard.Components.WorkflowInstanceLogsStateManagement;

namespace Synapse.Dashboard.Components.DocumentDetailsStateManagement;

/// <summary>
/// Represents the <see cref="ComponentStore{TState}" /> of a <see cref="DocumentDetails"/>
/// </summary>
/// <param name="logger">The service used to perform logging</param>
/// <param name="apiClient">The service used interact with Synapse API</param>
/// <param name="jsRuntime">The service used from JS interop</param>
/// <param name="monacoEditorHelper">The service used ease Monaco Editor interactions</param>
/// <param name="jsonSerializer">The service used to serialize and deserialize JSON</param>
/// <param name="yamlSerializer">The service used to serialize and deserialize YAML</param>
public class DocumentDetailsStore(
    ILogger<DocumentDetailsStore> logger,
    ISynapseApiClient apiClient,
    IJSRuntime jsRuntime,
    IMonacoEditorHelper monacoEditorHelper,
    IJsonSerializer jsonSerializer,
    IYamlSerializer yamlSerializer
)
    : ComponentStore<DocumentDetailsState>(new())
{
    
    private TextModel? _textModel = null;
    private readonly string _textModelUri = monacoEditorHelper.GetResourceUri();

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger<DocumentDetailsStore> Logger { get; } = logger;

    /// <summary>
    /// Gets the service used to interact with the Synapse API
    /// </summary>
    protected ISynapseApiClient ApiClient { get; } = apiClient;

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

    /// <summary>
    /// Gets/sets the logs <see cref="Collapse"/> panel
    /// </summary>
    public Collapse? Collapse { get; set; }

    #region Selectors
    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="DocumentDetailsState.Label"/> changes
    /// </summary>
    public IObservable<string> Label => this.Select(state => state.Label).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="DocumentDetailsState.Reference"/> changes
    /// </summary>
    public IObservable<string?> Reference => this.Select(state => state.Reference).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="DocumentDetailsState.DocumentJson"/> changes
    /// </summary>
    public IObservable<string> DocumentJson => this.Select(state => state.DocumentJson).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="DocumentDetailsState.Loaded"/> changes
    /// </summary>
    public IObservable<bool> Loaded => this.Select(state => state.Loaded).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="WorkflowInstanceLogsState.IsExpanded"/> changes
    /// </summary>
    public IObservable<bool> IsExpanded => this.Select(state => state.IsExpanded).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="DocumentDetailsState.ProblemType"/> changes
    /// </summary>
    public IObservable<Uri?> ProblemType => this.Select(state => state.ProblemType).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="DocumentDetailsState.ProblemTitle"/> changes
    /// </summary>
    public IObservable<string> ProblemTitle => this.Select(state => state.ProblemTitle).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="DocumentDetailsState.ProblemDetail"/> changes
    /// </summary>
    public IObservable<string> ProblemDetail => this.Select(state => state.ProblemDetail).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="DocumentDetailsState.ProblemStatus"/> changes
    /// </summary>
    public IObservable<int> ProblemStatus => this.Select(state => state.ProblemStatus).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="DocumentDetailsState.ProblemErrors"/> changes
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
    /// Sets the state's <see cref="DocumentDetailsState.Label"/>
    /// </summary>
    /// <param name="label">The new <see cref="DocumentDetailsState.Label"/> value</param>
    public void SetLabel(string label)
    {
        this.Reduce(state => state with
        {
            Label = label
        });
    }

    /// <summary>
    /// Sets the state's <see cref="DocumentDetailsState.Reference"/>
    /// </summary>
    /// <param name="reference">The new <see cref="DocumentDetailsState.Reference"/> value</param>
    public void SetReference(string? reference)
    {
        var documentJson = this.Get(state => state.DocumentJson);
        this.Reduce(state => state with
        {
            Reference = reference,
            DocumentJson = reference != null ? string.Empty : documentJson,
            Loaded = false,
        });
    }

    /// <summary>
    /// Sets the state's <see cref="DocumentDetailsState.Reference"/>
    /// </summary>
    /// <param name="document">The new <see cref="DocumentDetailsState.Reference"/> value</param>
    public void SetDocument(object? document)
    {
        try
        {
            var documentJson = document != null ? this.JsonSerializer.SerializeToText(document) : string.Empty;
            this.Reduce(state => state with
            {
                DocumentJson = documentJson,
                Loaded = true,
            });
        }
        catch (Exception ex)
        {
            this.Logger.LogError("Unable to set document, exception: {exception}", ex.ToString());
        }
    }
    #endregion

    #region Actions
    /// <summary>
    /// Toggles the <see cref="Collapse"/> panel
    /// </summary>
    public async Task ToggleAsync()
    {
        if (this.Collapse != null)
        {
            var isExpanded = !this.Get(state => state.IsExpanded);
            await (isExpanded ? this.Collapse.ShowAsync() : this.Collapse.HideAsync());
            this.Reduce(state => state with
            {
                IsExpanded = isExpanded
            });
        }
    }

    /// <summary>
    /// Toggles the <see cref="Collapse"/> panel
    /// </summary>
    public async Task HideAsync()
    {
        if (this.Collapse != null)
        {
            await this.Collapse.HideAsync();
            this.Reduce(state => state with
            {
                IsExpanded = false
            });
        }
    }

    /// <summary>
    /// Loads the referenced documents
    /// </summary>
    /// <returns></returns>
    public async Task LoadReferencedDocumentAsync()
    {
        var reference = this.Get(state => state.Reference);
        var loaded = this.Get(state => state.Loaded);
        if (loaded) return;
        if (string.IsNullOrWhiteSpace(reference))
        {
            this.Reduce(state => state with
            {
                Loaded = true
            });
            return;
        }
        try 
        { 
            var document = await this.ApiClient.Documents.GetAsync(reference);
            string documentText = this.JsonSerializer.SerializeToText(document.Content);
            this.Reduce(state => state with
            {
                DocumentJson = documentText,
                Loaded = true
            });
        }
        catch (ProblemDetailsException ex)
        {
            if (ex.Problem != null)
            {
                this.Reduce(state => state with
                {
                    ProblemType = ex.Problem?.Type,
                    ProblemTitle = ex.Problem?.Title ?? string.Empty,
                    ProblemStatus = ex.Problem?.Status ?? 0,
                    ProblemDetail = ex.Problem?.Detail ?? string.Empty,
                    ProblemErrors = new EquatableDictionary<string, string[]>(ex.Problem?.Errors ?? new Dictionary<string, string[]>())
                });
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError("Unabled to load referenced document: {exception}", ex.ToString());
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
                this._textModel = await Global.GetModel(this.JSRuntime, this._textModelUri);
                this._textModel ??= await Global.CreateModel(this.JSRuntime, "", language, this._textModelUri);
                await Global.SetModelLanguage(this.JSRuntime, this._textModel, language);
                await this.TextEditor!.SetModel(this._textModel);
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError("Unabled to set text editor language: {exception}", ex.ToString());
        }
    }

    /// <summary>
    /// Changes the value of the text editor
    /// </summary>
    /// <returns></returns>
    async Task SetTextEditorValueAsync()
    {
        var document = this.Get(state => state.DocumentJson);
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
                this.Logger.LogError("Unabled to set text editor value: {exception}", ex.ToString());
                await this.MonacoEditorHelper.ChangePreferredLanguageAsync(language == PreferredLanguage.YAML ? PreferredLanguage.JSON : PreferredLanguage.YAML);
            }
        }
    }
    #endregion

    /// <inheritdoc/>
    public override Task InitializeAsync()
    {
        this.DocumentJson.SubscribeAsync(async (_) => {
            await this.SetTextEditorValueAsync();
        }, cancellationToken: this.CancellationTokenSource.Token);
        return base.InitializeAsync();
    }

    private bool disposed;
    /// <summary>
    /// Disposes of the store
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the dispose of the store</param>
    protected override void Dispose(bool disposing)
    {
        if (!this.disposed)
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
            this.disposed = true;
        }
    }

}