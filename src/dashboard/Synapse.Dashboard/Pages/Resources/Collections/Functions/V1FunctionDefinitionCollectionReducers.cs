using Neuroglia.Data.Flux;

namespace Synapse.Dashboard.Pages.Resources.Collections.Functions
{

    /// <summary>
    /// Defines Flux reducers for <see cref="V1FunctionDefinitionCollectionState"/>-related actions
    /// </summary>
    [Reducer]
    public static class V1FunctionDefinitionCollectionReducers
    {

        /// <summary>
        /// Sets the current <see cref="V1FunctionDefinitionCollectionState"/>'s <see cref="V1FunctionDefinitionCollectionState.Collections"/>
        /// </summary>
        /// <param name="state">The <see cref="V1FunctionDefinitionCollectionState"/> to reduce</param>
        /// <param name="action">The Flux action used to reduce the specified <see cref="V1FunctionDefinitionCollectionState"/></param>
        /// <returns>The reduced <see cref="V1FunctionDefinitionCollectionState"/></returns>
        public static V1FunctionDefinitionCollectionState On(V1FunctionDefinitionCollectionState state, SetV1FunctionDefinitionCollections action)
        {
            return state with
            {
                Collections = action.Collections
            };
        }

        /// <summary>
        /// Adds the specified <see cref="V1FunctionDefinitionCollection"/> to the current state
        /// </summary>
        /// <param name="state">The <see cref="V1FunctionDefinitionCollectionState"/> to reduce</param>
        /// <param name="action">The Flux action used to reduce the specified <see cref="V1FunctionDefinitionCollectionState"/></param>
        /// <returns>The reduced <see cref="V1FunctionDefinitionCollectionState"/></returns>
        public static V1FunctionDefinitionCollectionState On(V1FunctionDefinitionCollectionState state, AddV1FunctionDefinitionCollection action)
        {
            var collections = state.Collections;
            collections.Add(action.Collection);
            return state with
            {
                Collections = collections
            };
        }

        /// <summary>
        /// Removes the specified <see cref="V1FunctionDefinitionCollection"/> from the current state
        /// </summary>
        /// <param name="state">The <see cref="V1FunctionDefinitionCollectionState"/> to reduce</param>
        /// <param name="action">The Flux action used to reduce the specified <see cref="V1FunctionDefinitionCollectionState"/></param>
        /// <returns>The reduced <see cref="V1FunctionDefinitionCollectionState"/></returns>
        public static V1FunctionDefinitionCollectionState On(V1FunctionDefinitionCollectionState state, RemoveV1FunctionDefinitionCollection action)
        {
            var collections = state.Collections;
            var collection = collections.FirstOrDefault(c => c.Id.Equals(action.CollectionId, StringComparison.InvariantCultureIgnoreCase));
            if (collection == null)
                return state;
            collections.Remove(collection);
            return state with
            {
                Collections = collections
            };
        }

    }

}
