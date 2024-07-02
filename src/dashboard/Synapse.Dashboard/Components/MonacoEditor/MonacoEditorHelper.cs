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

namespace Synapse.Dashboard.Components;

/// <inheritdoc />
public class MonacoEditorHelper
    : IMonacoEditorHelper
{
    private int _modelCount = 0;

    /// <inheritdoc />
    public string PreferredLanguage { get; protected set; } = "yaml";

    /// <inheritdoc />
    public event PreferredLanguageChangedEventHandler? PreferredLanguageChanged;

    /// <inheritdoc />
    public Func<StandaloneCodeEditor, StandaloneEditorConstructionOptions> GetStandaloneEditorConstructionOptions(string value = "", bool readOnly = false, string language = "yaml") {
        return (StandaloneCodeEditor editor) => new StandaloneEditorConstructionOptions
        {
            Theme = "vs-dark",
            AutomaticLayout = true,
            Minimap = new EditorMinimapOptions { Enabled = false },
            Language = language,
            ReadOnly = readOnly,
            Value = value,
            TabSize = 2
        };
    }

    /// <inheritdoc />
    public Func<StandaloneDiffEditor, DiffEditorConstructionOptions> GetDiffEditorConstructionOptions(bool readOnly = true)
    {
        return (StandaloneDiffEditor editor) => new DiffEditorConstructionOptions
        {
            AutomaticLayout = true,
            Minimap = new EditorMinimapOptions { Enabled = false },
            ReadOnly = readOnly
        };
    }

    /// <inheritdoc />
    public async Task ChangePreferredLanguageAsync(string language)
    {
        if (!string.IsNullOrEmpty(language) && language != this.PreferredLanguage)
        {
            this.PreferredLanguage = language;
            await this.OnPreferredLanguageChangeAsync(language);
        }
    }

    /// <inheritdoc />
    protected async Task OnPreferredLanguageChangeAsync(string language)
    {
        if (this.PreferredLanguageChanged != null)
        {
            await this.PreferredLanguageChanged.Invoke(language);
        }
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public int GetNextModelIndex() {
        this._modelCount++;
        return this._modelCount;
    }
}
