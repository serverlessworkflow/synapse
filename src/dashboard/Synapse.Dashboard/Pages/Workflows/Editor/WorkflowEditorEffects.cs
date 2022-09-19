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

using Neuroglia.Data.Flux;
using Neuroglia.Serialization;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Dashboard.Pages.Workflows.Editor.Actions;
using Synapse.Dashboard.Services;
using static Synapse.EnvironmentVariables.Runtime.Correlation;
using Synapse.Dashboard.Pages.Workflows.Editor.State;
using static System.Net.Mime.MediaTypeNames;
using Octokit;

namespace Synapse.Dashboard.Pages.Workflows.Editor.Effects
{
    [Effect]
    public static class WorkflowEditorEffects
    {
        public static async Task OnInitiliazeState(InitializeState action, IEffectContext context)
        {
            var monacoEditorHelper = context.Services.GetRequiredService<IMonacoEditorHelper>();
            if (monacoEditorHelper == null)
                throw new NullReferenceException("Unable to resolved service 'IMonacoEditorHelper'.");
            var yamlConverter = context.Services.GetRequiredService<IYamlConverter>();
            if (yamlConverter == null)
                throw new NullReferenceException("Unable to resolved service 'IYamlConverter'.");
            var definition = new WorkflowDefinition() { Id = "new-workflow", Name = "New workflow", Version = "0.1.0" };
            var text = JsonConvert.SerializeObject(definition, Formatting.Indented, new JsonSerializerSettings() { ContractResolver = new NonPublicSetterContractResolver() });
            if (monacoEditorHelper.PreferedLanguage == PreferedLanguage.YAML)
                text = await yamlConverter.JsonToYaml(text);
            WorkflowEditorState initialState = new() { 
                WorkflowDefinition = definition,
                WorkflowDefinitionText = text,
                Updating = false
            };
            context.Dispatcher.Dispatch(new InitializeStateSuccessful(initialState));
        }

        /// <summary>
        /// Handles the form editor changes
        /// </summary>
        /// <param name="action"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public static async Task OnFormBasedEditorChange(HandleFormBasedEditorChange action, IEffectContext context)
        {
            var monacoEditorHelper = context.Services.GetRequiredService<IMonacoEditorHelper>();
            if (monacoEditorHelper == null)
                throw new NullReferenceException("Unable to resolved service 'IJsonSerializer'.");
            context.Dispatcher.Dispatch(new StartUpdating());
            context.Dispatcher.Dispatch(new UpdateDefinition(action.WorkflowDefinition));
            try
            {
                var text = JsonConvert.SerializeObject(action.WorkflowDefinition, Formatting.Indented, new JsonSerializerSettings() { ContractResolver = new NonPublicSetterContractResolver() });
                if (monacoEditorHelper.PreferedLanguage == PreferedLanguage.YAML)
                {
                    var yamlConverter = context.Services.GetRequiredService<IYamlConverter>();
                    if (yamlConverter == null)
                        throw new NullReferenceException("Unable to resolved service 'IYamlConverter'.");
                    text = await yamlConverter.JsonToYaml(text);
                }
                context.Dispatcher.Dispatch(new UpdateDefinitionText(text));
            }
            catch (Exception ex)
            {
                // Ignore parsing failure
            }
            context.Dispatcher.Dispatch(new StopUpdating());
        }

        /// <summary>
        /// Handles the text editor changes
        /// </summary>
        /// <param name="action"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public static async Task OnTextBasedEditorChange(HandleTextBasedEditorChange action, IEffectContext context)
        {
            var monacoEditorHelper = context.Services.GetRequiredService<IMonacoEditorHelper>();
            if (monacoEditorHelper == null)
                throw new NullReferenceException("Unable to resolved service 'IMonacoEditorHelper'.");
            var yamlConverter = context.Services.GetRequiredService<IYamlConverter>();
            if (yamlConverter == null)
                throw new NullReferenceException("Unable to resolved service 'IYamlConverter'.");
            var jsonSerializer = context.Services.GetRequiredService<IJsonSerializer>();
            if (jsonSerializer == null)
                throw new NullReferenceException("Unable to resolved service 'IJsonSerializer'.");
            var text = action.WorkflowDefinitionText;
            context.Dispatcher.Dispatch(new StartUpdating());
            context.Dispatcher.Dispatch(new UpdateDefinitionText(text));
            try {
                if (monacoEditorHelper.PreferedLanguage == PreferedLanguage.YAML)
                    text = await yamlConverter.YamlToJson(text);
                var definition = await jsonSerializer.DeserializeAsync<WorkflowDefinition>(text);
                context.Dispatcher.Dispatch(new UpdateDefinition(definition));
            }
            catch(Exception ex)
            {
                // Ignore parsing failure
            }
            context.Dispatcher.Dispatch(new StopUpdating());
        }

        public static async Task OnChangeTextLanguage(ChangeTextLanguage action, IEffectContext context)
        {
            context.Dispatcher.Dispatch(new StartUpdating());
            var monacoEditorHelper = context.Services.GetRequiredService<IMonacoEditorHelper>();
            if (monacoEditorHelper == null)
                throw new NullReferenceException("Unable to resolved service 'IMonacoEditorHelper'.");
            var yamlConverter = context.Services.GetRequiredService<IYamlConverter>();
            if (yamlConverter == null)
                throw new NullReferenceException("Unable to resolved service 'IYamlConverter'.");
            try
            {
                var text = action.Language == PreferedLanguage.YAML ?
                    await yamlConverter.JsonToYaml(action.WorkflowDefinitionText) :
                    await yamlConverter.JsonToYaml(action.WorkflowDefinitionText);
                context.Dispatcher.Dispatch(new UpdateDefinitionText(text));
            }
            catch(Exception ex)
            {
                await monacoEditorHelper.ChangePreferedLanguage(action.Language == PreferedLanguage.JSON ? PreferedLanguage.YAML : PreferedLanguage.JSON);
            }
            context.Dispatcher.Dispatch(new StopUpdating());
        }
    }
}
