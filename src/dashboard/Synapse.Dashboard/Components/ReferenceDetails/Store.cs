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

using Synapse.Dashboard.Components.ResourceEditorStateManagement;

namespace Synapse.Dashboard.Components.ReferenceDetailsStateManagement;

/// <summary>
/// Represents the <see cref="ComponentStore{TState}" /> of a <see cref="ReferenceDetails"/>
/// </summary>
/// <param name="api">The service used interact with Synapse API</param>
/// <param name="jsRuntime">The service used from JS interop</param>
/// <param name="monacoEditorHelper">The service used ease Monaco Editor interactions</param>
/// <param name="jsonSerializer">The service used to serialize and deserialize JSON</param>
/// <param name="yamlSerializer">The service used to serialize and deserialize YAML</param>
public class ReferenceDetailsStore(
    Api.Client.Services.ISynapseApiClient api,
    IJSRuntime jsRuntime,
    IMonacoEditorHelper monacoEditorHelper,
    IJsonSerializer jsonSerializer,
    IYamlSerializer yamlSerializer
)
    : ComponentStore<ReferenceDetailsState>(new())
{
    /// <summary>
    /// The <see cref="BlazorMonaco.Editor.StandaloneEditorConstructionOptions"/> provider function
    /// </summary>
    public Func<StandaloneCodeEditor, StandaloneEditorConstructionOptions> StandaloneEditorConstructionOptions = monacoEditorHelper.GetStandaloneEditorConstructionOptions(string.Empty, true, monacoEditorHelper.PreferredLanguage);


    #region Selectors
    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ReferenceDetailsState.Label"/> changes
    /// </summary>
    public IObservable<string> Label => this.Select(state => state.Label).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ReferenceDetailsState.Reference"/> changes
    /// </summary>
    public IObservable<string> Reference => this.Select(state => state.Reference).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ReferenceDetailsState.Document"/> changes
    /// </summary>
    public IObservable<string> Document => this.Select(state => state.Document).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ReferenceDetailsState.Loaded"/> changes
    /// </summary>
    public IObservable<bool> Loaded => this.Select(state => state.Loaded).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ReferenceDetailsState.TextEditor"/> changes
    /// </summary>
    public IObservable<StandaloneCodeEditor?> TextEditor => this.Select(state => state.TextEditor).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ResourceEditorState{TResource}.ProblemType"/> changes
    /// </summary>
    public IObservable<Uri?> ProblemType => this.Select(state => state.ProblemType).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ResourceEditorState{TResource}.ProblemTitle"/> changes
    /// </summary>
    public IObservable<string> ProblemTitle => this.Select(state => state.ProblemTitle).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ResourceEditorState{TResource}.ProblemDetail"/> changes
    /// </summary>
    public IObservable<string> ProblemDetail => this.Select(state => state.ProblemDetail).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ResourceEditorState{TResource}.ProblemStatus"/> changes
    /// </summary>
    public IObservable<int> ProblemStatus => this.Select(state => state.ProblemStatus).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ResourceEditorState{TResource}.ProblemErrors"/> changes
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
    /// Sets the state's <see cref="ReferenceDetailsState.Label"/>
    /// </summary>
    /// <param name="label">The new <see cref="ReferenceDetailsState.Label"/> value</param>
    public void SetLabel(string label)
    {
        this.Reduce(state => state with
        {
            Label = label
        });
    }

    /// <summary>
    /// Sets the state's <see cref="ReferenceDetailsState.Reference"/>
    /// </summary>
    /// <param name="reference">The new <see cref="ReferenceDetailsState.Reference"/> value</param>
    public void SetReference(string reference)
    {
        this.Reduce(state => state with
        {
            Reference = reference,
            Document = string.Empty,
            Loaded = false,
        });
    }

    /// <summary>
    /// Sets the state's <see cref="ReferenceDetailsState.TextEditor"/>
    /// </summary>
    /// <param name="textEditor">The new <see cref="ReferenceDetailsState.TextEditor"/> value</param>
    public void SetTextEditor(StandaloneCodeEditor? textEditor)
    {
        this.Reduce(state => state with
        {
            TextEditor = textEditor
        });
    }
    #endregion

    #region Actions
    

    /// <summary>
    /// Loads the referenced documents
    /// </summary>
    /// <returns></returns>
    public async Task LoadReferencedDocumentAsync()
    {
        var reference = this.Get(state => state.Reference);
        var loaded = this.Get(state => state.Loaded);
        if (loaded || string.IsNullOrWhiteSpace(reference)) return;
        try 
        { 
            var document = await api.Documents.GetAsync(reference);
            string documentText = jsonSerializer.SerializeToText(document.Content);
            this.Reduce(state => state with
            {
                Document = documentText,
                Loaded = true
            });
            await this.SetTextEditorValueAsync();
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
                    ProblemErrors = ex.Problem?.Errors?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? []
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            // todo: handle exception
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
            var textEditor = this.Get(state => state.TextEditor);
            var textModel = this.Get(state => state.TextModel);
            var language = monacoEditorHelper.PreferredLanguage;
            if (textEditor != null)
            {
                if (textModel != null)
                {
                    await Global.SetModelLanguage(jsRuntime, textModel, language);
                }
                else
                {
                    var reference = this.Get(state => state.Reference);
                    var resourceUri = $"inmemory://{reference.ToLower()}";
                    textModel = await Global.CreateModel(jsRuntime, "", language, resourceUri);
                }
                await textEditor!.SetModel(textModel);
                this.Reduce(state => state with
                {
                    TextModel = textModel
                });
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
        var textEditor = this.Get(state => state.TextEditor);
        var document = this.Get(state => state.Document);
        var language = monacoEditorHelper.PreferredLanguage;
        if (textEditor != null && !string.IsNullOrWhiteSpace(document))
        {
            try
            {
                if (language == PreferredLanguage.YAML)
                {
                    document = yamlSerializer.ConvertFromJson(document);
                }
                await textEditor.SetValue(document);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await monacoEditorHelper.ChangePreferredLanguageAsync(language == PreferredLanguage.YAML ? PreferredLanguage.JSON : PreferredLanguage.YAML);
            }
        }
    }
    #endregion

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
                var textEditor = this.Get(state => state.TextEditor);
                var textModel = this.Get(state => state.TextModel);
                if (textModel != null)
                {
                    textModel.DisposeModel();
                    this.Reduce(state => state with
                    { 
                        TextModel = null 
                    });
                }
                if (textEditor != null)
                {
                    textEditor.Dispose();
                    this.SetTextEditor(null);
                }
            }
            this.disposed = true;
        }
    }

}