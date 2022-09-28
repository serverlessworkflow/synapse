using Neuroglia.Data.Flux;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Pages.Resources.Collections.Functions
{

    /// <summary>
    /// Represents the Flux state used to manage <see cref="V1FunctionDefinitionCollection"/>s
    /// </summary>
    [Feature]
    public record V1FunctionDefinitionCollectionState
    {

        /// <summary>
        /// Initializes a new <see cref="V1FunctionDefinitionCollectionState"/>
        /// </summary>
        public V1FunctionDefinitionCollectionState()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1FunctionDefinitionCollectionState"/>
        /// </summary>
        /// <param name="collections">A <see cref="List{T}"/> containing the currently available <see cref="V1FunctionDefinitionCollection"/>s</param>
        public V1FunctionDefinitionCollectionState(List<V1FunctionDefinitionCollection> collections)
        {
            Collections = collections;
        }

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing the currently available <see cref="V1FunctionDefinitionCollection"/>s
        /// </summary>
        public List<V1FunctionDefinitionCollection> Collections { get; set; } = null!;

    }

}
