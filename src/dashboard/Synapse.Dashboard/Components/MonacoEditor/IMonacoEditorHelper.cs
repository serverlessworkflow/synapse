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

/// <summary>
/// Represents a delegate that is used to handle events related to changes in a user's preferred language.
/// </summary>
/// <param name="newLanguage">The new preferred language.</param>
/// <returns>A task representing the asynchronous operation of handling the event.</returns>
public delegate Task PreferredLanguageChangedEventHandler(string newLanguage);

/// <summary>
/// Represents a service used to facilitate the Monaco editor configuration
/// </summary>
public interface IMonacoEditorHelper
{
    /// <summary>
    /// The preferred editor language
    /// </summary>
    string PreferredLanguage { get; }

    /// <summary>
    /// Emits when the editor language changes
    /// </summary>
    event PreferredLanguageChangedEventHandler? PreferredLanguageChanged;

    /// <summary>
    /// A function used to facilitate the construction of <see cref="StandaloneEditorConstructionOptions"/> 
    /// </summary>
    /// <param name="value">The text of the editor</param>
    /// <param name="readOnly">Defines if the editor should be in read only</param>
    /// <param name="language">The default preferred language</param>
    /// <returns>A function used to build <see cref="StandaloneEditorConstructionOptions"/></returns>
    Func<StandaloneCodeEditor, StandaloneEditorConstructionOptions> GetStandaloneEditorConstructionOptions(string value = "", bool readOnly = false, string language = "json");

    /// <summary>
    /// A function used to facilitate the construction of <see cref="DiffEditorConstructionOptions"/> 
    /// </summary>
    /// <param name="readOnly">Defines if the editor should be in read only</param>
    /// <returns>A function used to build <see cref="DiffEditorConstructionOptions"/></returns>
    Func<StandaloneDiffEditor, DiffEditorConstructionOptions> GetDiffEditorConstructionOptions(bool readOnly = true);

    /// <summary>
    /// Changes the preferred editor language
    /// </summary>
    /// <param name="language">The new language to use</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task ChangePreferredLanguageAsync(string language);

}
