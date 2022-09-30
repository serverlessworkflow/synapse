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
using Synapse.Integration.Models;
using System.Reactive.Linq;

namespace Synapse.Dashboard.Pages.Resources.Collections.Functions
{

    /// <summary>
    /// Defines selectors for the <see cref="FunctionDefinitionCollectionEditorState"/>
    /// </summary>
    public static class FunctionDefinitionCollectionEditorSelectors
    {


        /// <summary>
        /// Selects the workflow definition
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public static IObservable<V1FunctionDefinitionCollection?> SelectFunctionDefinitionCollection(IStore store)
        {
            return store.GetFeature<FunctionDefinitionCollectionEditorState>()
                .Select(featureState => featureState.Collection)
                .DistinctUntilChanged();
        }

        /// <summary>
        /// Selects the workflow definition text
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public static IObservable<string?> SelectSerializedFunctionDefinitionCollection(IStore store)
        {
            return store.GetFeature<FunctionDefinitionCollectionEditorState>()
                .Select(featureState => featureState.SerializedCollection)
                .DistinctUntilChanged();
        }

    }

}
