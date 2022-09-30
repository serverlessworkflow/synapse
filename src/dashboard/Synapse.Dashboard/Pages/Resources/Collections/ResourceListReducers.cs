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

namespace Synapse.Dashboard.Pages.Resources.Collections
{

    /// <summary>
    /// Defines Flux reducers for <see cref="ResourceListState"/>-related actions
    /// </summary>
    [Reducer]
    public static class ResourceListReducers
    {

        /// <summary>
        /// Sets the current <see cref="ResourceListState"/>'s <see cref="ResourceListState.FunctionDefinitionCollections"/>
        /// </summary>
        /// <param name="state">The <see cref="ResourceListState"/> to reduce</param>
        /// <param name="action">The Flux action used to reduce the specified <see cref="ResourceListState"/></param>
        /// <returns>The reduced <see cref="ResourceListState"/></returns>
        public static ResourceListState On(ResourceListState state, SetV1FunctionDefinitionCollections action)
        {
            return state with
            {
                FunctionDefinitionCollections = action.Collections
            };
        }

        /// <summary>
        /// Adds the specified <see cref="V1FunctionDefinitionCollection"/> to the current state
        /// </summary>
        /// <param name="state">The <see cref="ResourceListState"/> to reduce</param>
        /// <param name="action">The Flux action used to reduce the specified <see cref="ResourceListState"/></param>
        /// <returns>The reduced <see cref="ResourceListState"/></returns>
        public static ResourceListState On(ResourceListState state, AddV1FunctionDefinitionCollection action)
        {
            var collections = state.FunctionDefinitionCollections;
            collections.Add(action.Collection);
            return state with
            {
                FunctionDefinitionCollections = collections
            };
        }

        /// <summary>
        /// Removes the specified <see cref="V1FunctionDefinitionCollection"/> from the current state
        /// </summary>
        /// <param name="state">The <see cref="ResourceListState"/> to reduce</param>
        /// <param name="action">The Flux action used to reduce the specified <see cref="ResourceListState"/></param>
        /// <returns>The reduced <see cref="ResourceListState"/></returns>
        public static ResourceListState On(ResourceListState state, RemoveV1FunctionDefinitionCollection action)
        {
            var collections = state.FunctionDefinitionCollections;
            var collection = collections.FirstOrDefault(c => c.Id.Equals(action.CollectionId, StringComparison.InvariantCultureIgnoreCase));
            if (collection == null)
                return state;
            collections.Remove(collection);
            return state with
            {
                FunctionDefinitionCollections = collections
            };
        }

        /// <summary>
        /// Sets the current <see cref="ResourceListState"/>'s <see cref="ResourceListState.EventDefinitionCollections"/>
        /// </summary>
        /// <param name="state">The <see cref="ResourceListState"/> to reduce</param>
        /// <param name="action">The Flux action used to reduce the specified <see cref="ResourceListState"/></param>
        /// <returns>The reduced <see cref="ResourceListState"/></returns>
        public static ResourceListState On(ResourceListState state, SetV1EventDefinitionCollections action)
        {
            return state with
            {
                EventDefinitionCollections = action.Collections
            };
        }

        /// <summary>
        /// Adds the specified <see cref="V1EventDefinitionCollection"/> to the current state
        /// </summary>
        /// <param name="state">The <see cref="ResourceListState"/> to reduce</param>
        /// <param name="action">The Flux action used to reduce the specified <see cref="ResourceListState"/></param>
        /// <returns>The reduced <see cref="ResourceListState"/></returns>
        public static ResourceListState On(ResourceListState state, AddV1EventDefinitionCollection action)
        {
            var collections = state.EventDefinitionCollections;
            collections.Add(action.Collection);
            return state with
            {
                EventDefinitionCollections = collections
            };
        }

        /// <summary>
        /// Removes the specified <see cref="V1EventDefinitionCollection"/> from the current state
        /// </summary>
        /// <param name="state">The <see cref="ResourceListState"/> to reduce</param>
        /// <param name="action">The Flux action used to reduce the specified <see cref="ResourceListState"/></param>
        /// <returns>The reduced <see cref="ResourceListState"/></returns>
        public static ResourceListState On(ResourceListState state, RemoveV1EventDefinitionCollection action)
        {
            var collections = state.EventDefinitionCollections;
            var collection = collections.FirstOrDefault(c => c.Id.Equals(action.CollectionId, StringComparison.InvariantCultureIgnoreCase));
            if (collection == null)
                return state;
            collections.Remove(collection);
            return state with
            {
                EventDefinitionCollections = collections
            };
        }

        /// <summary>
        /// Sets the current <see cref="ResourceListState"/>'s <see cref="ResourceListState.AuthenticationDefinitionCollections"/>
        /// </summary>
        /// <param name="state">The <see cref="ResourceListState"/> to reduce</param>
        /// <param name="action">The Flux action used to reduce the specified <see cref="ResourceListState"/></param>
        /// <returns>The reduced <see cref="ResourceListState"/></returns>
        public static ResourceListState On(ResourceListState state, SetV1AuthenticationDefinitionCollections action)
        {
            return state with
            {
                AuthenticationDefinitionCollections = action.Collections
            };
        }

        /// <summary>
        /// Adds the specified <see cref="V1AuthenticationDefinitionCollection"/> to the current state
        /// </summary>
        /// <param name="state">The <see cref="ResourceListState"/> to reduce</param>
        /// <param name="action">The Flux action used to reduce the specified <see cref="ResourceListState"/></param>
        /// <returns>The reduced <see cref="ResourceListState"/></returns>
        public static ResourceListState On(ResourceListState state, AddV1AuthenticationDefinitionCollection action)
        {
            var collections = state.AuthenticationDefinitionCollections;
            collections.Add(action.Collection);
            return state with
            {
                AuthenticationDefinitionCollections = collections
            };
        }

        /// <summary>
        /// Removes the specified <see cref="V1AuthenticationDefinitionCollection"/> from the current state
        /// </summary>
        /// <param name="state">The <see cref="ResourceListState"/> to reduce</param>
        /// <param name="action">The Flux action used to reduce the specified <see cref="ResourceListState"/></param>
        /// <returns>The reduced <see cref="ResourceListState"/></returns>
        public static ResourceListState On(ResourceListState state, RemoveV1AuthenticationDefinitionCollection action)
        {
            var collections = state.AuthenticationDefinitionCollections;
            var collection = collections.FirstOrDefault(c => c.Id.Equals(action.CollectionId, StringComparison.InvariantCultureIgnoreCase));
            if (collection == null)
                return state;
            collections.Remove(collection);
            return state with
            {
                AuthenticationDefinitionCollections = collections
            };
        }

    }

}
