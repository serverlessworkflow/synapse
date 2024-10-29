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
    private string _preferredTheme = "vs-dark";

    /// <inheritdoc />
    public string PreferredLanguage { get; protected set; } = "yaml";


    /// <inheritdoc />
    public event PreferredLanguageChangedEventHandler? PreferredLanguageChanged;

    /// <inheritdoc />
    public event PreferredThemeChangedEventHandler? PreferredThemeChanged;

    /// <inheritdoc />
    public Func<StandaloneCodeEditor, StandaloneEditorConstructionOptions> GetStandaloneEditorConstructionOptions(string value = "", bool readOnly = false, string language = "yaml") {
        return (StandaloneCodeEditor editor) => new StandaloneEditorConstructionOptions
        {
            Theme = _preferredTheme,
            AutomaticLayout = true,
            Minimap = new EditorMinimapOptions { Enabled = false },
            Language = language,
            ReadOnly = readOnly,
            Value = value,
            TabSize = 2,
            FormatOnPaste = true,
            FormatOnType = true,
            QuickSuggestions = new QuickSuggestionsOptions
            {
                Other = "true",
                Strings = "true",
            }
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
            if (this.PreferredLanguageChanged != null)
            {
                await this.PreferredLanguageChanged.Invoke(language);
            }
        }
    }

    /// <inheritdoc />
    public async Task ChangePreferredThemeAsync(string theme)
    {
        if (!string.IsNullOrEmpty(theme) && theme != this._preferredTheme)
        {
            if (theme == "dark")
            {
                theme = "vs-dark";
            }
            else if (theme == "light") {
                theme = "vs";
            }
            this._preferredTheme = theme;
            if (this.PreferredThemeChanged != null)
            {
                await this.PreferredThemeChanged.Invoke(theme);
            }
        }
    }

    /// <inheritdoc />
    public int GetNextModelIndex() {
        this._modelCount++;
        return this._modelCount;
    }
}
