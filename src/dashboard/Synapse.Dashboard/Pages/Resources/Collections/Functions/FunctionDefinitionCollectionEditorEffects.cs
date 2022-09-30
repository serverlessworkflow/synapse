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

using Microsoft.AspNetCore.Components;
using Neuroglia.Data.Flux;
using Neuroglia.Serialization;
using Newtonsoft.Json;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Apis.Management;
using Synapse.Dashboard.Services;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Pages.Resources.Collections.Functions
{

    /// <summary>
    /// Defines Flux effects that apply to <see cref="FunctionDefinitionCollectionEditorState"/>-related Flux actions
    /// </summary>
    [Effect]
    public static class FunctionDefinitionCollectionEditorEffects
    {

        /// <summary>
        /// Handles the state initialization
        /// </summary>
        /// <param name="action">The <see cref="InitializeState"/> action</param>
        /// <param name="context">The <see cref="IEffectContext"/> context</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public static async Task On(InitializeState action, IEffectContext context)
        {
            try
            {
                var monacoEditorHelper = context.Services.GetRequiredService<IMonacoEditorHelper>();
                var yamlConverter = context.Services.GetRequiredService<IYamlConverter>();
                V1FunctionDefinitionCollection collection;
                if (string.IsNullOrWhiteSpace(action.CollectionId))
                    collection = new() { Name = "Undefined", Version = "0.1.0", Functions = new List<FunctionDefinition>() };
                else
                    collection = (await context.Services.GetRequiredService<ISynapseManagementApi>().GetFunctionDefinitionCollectionByIdAsync(action.CollectionId));
                var serializedCollection = JsonConvert.SerializeObject(collection, Formatting.Indented, JsonConvert.DefaultSettings!()!);
                if (monacoEditorHelper.PreferedLanguage == PreferedLanguage.YAML)
                    serializedCollection = await yamlConverter.JsonToYaml(serializedCollection);
                var initialState = new FunctionDefinitionCollectionEditorState()
                {
                    Collection = collection,
                    SerializedCollection = serializedCollection,
                    Updating = false,
                    Saving = false,
                    ExpanderStates = new Dictionary<string, bool>()
                    {
                        { "general", true },
                        { "functions", true }
                    }
                };
                context.Dispatcher.Dispatch(new InitializeStateSuccessful(initialState, action.IfNotExists));
            }
            catch(Exception ex)
            {
                context.Services.GetRequiredService<ILogger<HandleTextBasedEditorChange>>().LogError(ex.ToString());
            }
        }

        /// <summary>
        /// Handles the form editor changes
        /// </summary>
        /// <param name="action">The <see cref="HandleFormBasedEditorChange"/> action</param>
        /// <param name="context">The <see cref="IEffectContext"/> context</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public static async Task On(HandleFormBasedEditorChange action, IEffectContext context)
        {
            try
            {
                var monacoEditorHelper = context.Services.GetRequiredService<IMonacoEditorHelper>();
                context.Dispatcher.Dispatch(new StartUpdating());
                context.Dispatcher.Dispatch(new UpdateCollection(action.Collection));
                var serializedCollection = JsonConvert.SerializeObject(action.Collection, Formatting.Indented, JsonConvert.DefaultSettings!()!);
                if (monacoEditorHelper.PreferedLanguage == PreferedLanguage.YAML)
                {
                    var yamlConverter = context.Services.GetRequiredService<IYamlConverter>();
                    serializedCollection = await yamlConverter.JsonToYaml(serializedCollection);
                }
                context.Dispatcher.Dispatch(new UpdateSerializedCollection(serializedCollection));
                context.Dispatcher.Dispatch(new StopUpdating());
            }
            catch (Exception ex)
            {
                context.Services.GetRequiredService<ILogger<HandleTextBasedEditorChange>>().LogError(ex.ToString());
            }
        }

        /// <summary>
        /// Handles the text editor changes
        /// </summary>
        /// <param name="action">The <see cref="HandleTextBasedEditorChange"/> action</param>
        /// <param name="context">The <see cref="IEffectContext"/> context</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public static async Task On(HandleTextBasedEditorChange action, IEffectContext context)
        {
            try
            {
                var monacoEditorHelper = context.Services.GetRequiredService<IMonacoEditorHelper>();
                var yamlConverter = context.Services.GetRequiredService<IYamlConverter>();
                var jsonSerializer = context.Services.GetRequiredService<IJsonSerializer>();
                var serializedCollection = action.SerializedCollection;
                context.Dispatcher.Dispatch(new StartUpdating());
                context.Dispatcher.Dispatch(new UpdateSerializedCollection(serializedCollection));
                if (monacoEditorHelper.PreferedLanguage == PreferedLanguage.YAML)
                    serializedCollection = await yamlConverter.YamlToJson(serializedCollection);
                var collection = await jsonSerializer.DeserializeAsync<V1FunctionDefinitionCollection>(serializedCollection);
                context.Dispatcher.Dispatch(new UpdateCollection(collection));
                context.Dispatcher.Dispatch(new StopUpdating());
            }
            catch (Exception ex)
            {
                context.Services.GetRequiredService<ILogger<HandleTextBasedEditorChange>>().LogError(ex.ToString());
            }
        }

        /// <summary>
        /// Handles the text editor language changes
        /// </summary>
        /// <param name="action">The <see cref="ChangeEditorLanguage"/> action</param>
        /// <param name="context">The <see cref="IEffectContext"/> context</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public static async Task On(ChangeEditorLanguage action, IEffectContext context)
        {
            var monacoEditorHelper = context.Services.GetRequiredService<IMonacoEditorHelper>();
            var yamlConverter = context.Services.GetRequiredService<IYamlConverter>();
            context.Dispatcher.Dispatch(new StartUpdating());
            try
            {
                var serializedCollection = action.Language == PreferedLanguage.YAML ?
                    await yamlConverter.JsonToYaml(action.SerializedCollection) :
                    await yamlConverter.YamlToJson(action.SerializedCollection);
                context.Dispatcher.Dispatch(new UpdateSerializedCollection(serializedCollection));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await monacoEditorHelper.ChangePreferedLanguage(action.Language == PreferedLanguage.JSON ? PreferedLanguage.YAML : PreferedLanguage.JSON);
            }
            context.Dispatcher.Dispatch(new StopUpdating());
        }

        /// <summary>
        /// Saves the specified collection,
        /// </summary>
        /// <param name="action">The <see cref="SaveCollection"/> Flux action</param>
        /// <param name="context">The <see cref="IEffectContext"/> context</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public static async Task On(SaveCollection action, IEffectContext context)
        {
            try
            {
                var api = context.Services.GetRequiredService<ISynapseManagementApi>();
                var collection = await api.CreateFunctionDefinitionCollectionAsync(new() 
                { 
                    Name = action.Collection.Name,
                    Version = action.Collection.Version,
                    Description = action.Collection.Description,
                    Functions = action.Collection.Functions
                });
                context.Dispatcher.Dispatch(new CollectionSaved(collection));
                context.Dispatcher.Dispatch(new InitializeState(false));
                var navigationManager = context.Services.GetRequiredService<NavigationManager>();
                navigationManager.NavigateTo($"/resources");
            }
            catch (Exception ex)
            {
                context.Dispatcher.Dispatch(new CollectionSaveFailed(ex.Message));
            }
        }

    }

}
