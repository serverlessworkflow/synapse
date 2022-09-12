/*
 * Copyright © 2022-Present The Synapse Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using BlazorMonaco;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Synapse.Dashboard
{
    public class MonacoEditorHelper
        : IMonacoEditorHelper
    {
        public string PreferedLanguage { get; protected set; } = "json";

        public event PreferedLanguageChangedEventHandler? PreferedLanguageChanged;

        public Func<MonacoEditor, StandaloneEditorConstructionOptions> GetStandaloneEditorConstructionOptions(string value = "", bool readOnly = false, string language = "json") {
            return (MonacoEditor editor) => new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Minimap = new EditorMinimapOptions { Enabled = false },
                Language = language,
                ReadOnly = readOnly,
                Value = value
            };
        }

        public Func<MonacoDiffEditor, DiffEditorConstructionOptions> GetDiffEditorConstructionOptions(bool readOnly = true)
        {
            return (MonacoDiffEditor editor) => new DiffEditorConstructionOptions
            {
                AutomaticLayout = true,
                Minimap = new EditorMinimapOptions { Enabled = false },
                ReadOnly = readOnly
            };
        }

        public async Task ChangePreferedLanguage(string language)
        {
            if (!string.IsNullOrEmpty(language) && language != this.PreferedLanguage)
            {
                this.PreferedLanguage = language;
                await this.OnPreferedLanguageChange(language);
            }
        }

        protected async Task OnPreferedLanguageChange(string language)
        {
            if (this.PreferedLanguageChanged != null)
            {
                await this.PreferedLanguageChanged.Invoke(language);
            }
            await Task.CompletedTask;
        }
    }
}
