using OData.QueryBuilder.Builders;
using OData.QueryBuilder.Conventions.AddressingEntities.Query;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Pages.Resources.Collections.Functions
{

    /// <summary>
    /// Represents the action used to query <see cref="V1FunctionDefinitionCollection"/>s
    /// </summary>
    public class QueryV1FunctionDefinitionCollections
    {

        /// <summary>
        /// Initializes a new <see cref="QueryV1FunctionDefinitionCollections"/>
        /// </summary>
        public QueryV1FunctionDefinitionCollections()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="QueryV1FunctionDefinitionCollections"/>
        /// </summary>
        /// <param name="searchTerm">The term to search for</param>
        /// <param name="querySetup">An <see cref="Action{T}"/> used to setup the query to perform</param>
        public QueryV1FunctionDefinitionCollections(string? searchTerm, Action<IODataQueryCollection<V1FunctionDefinitionCollection>> querySetup)
        {
            var builder = new ODataQueryBuilder(new Uri("https://test.com"))
                .For<V1FunctionDefinitionCollection>("V1FunctionDefinitionCollections")
                .ByList();
            querySetup(builder);
            this.Query = builder.ToUri(UriKind.Absolute).Query;
            if (!string.IsNullOrWhiteSpace(searchTerm))
                this.Query = $"$search={searchTerm}&{this.Query}";
        }

        /// <summary>
        /// Initializes a new <see cref="QueryV1FunctionDefinitionCollections"/>
        /// </summary>
        /// <param name="searchTerm">The term to search for</param>
        public QueryV1FunctionDefinitionCollections(string searchTerm)
            : this(searchTerm, _ => {  })
        {
            
        }

        /// <summary>
        /// Initializes a new <see cref="QueryV1FunctionDefinitionCollections"/>
        /// </summary>
        /// <param name="querySetup">An <see cref="Action{T}"/> used to setup the query to perform</param>
        public QueryV1FunctionDefinitionCollections(Action<IODataQueryCollection<V1FunctionDefinitionCollection>> querySetup)
            : this(null, querySetup)
        {
            
        }

        /// <summary>
        /// Gets the query to perform
        /// </summary>
        public string? Query { get; }

    }

    /// <summary>
    /// Represents the action used to set the currently available <see cref="V1FunctionDefinitionCollection"/>s
    /// </summary>
    public class SetV1FunctionDefinitionCollections
    {

        /// <summary>
        /// Initializes a new <see cref="SetV1FunctionDefinitionCollections"/>
        /// </summary>
        /// <param name="collections">A <see cref="List{T}"/> containing the currently available <see cref="V1FunctionDefinitionCollection"/>s</param>
        public SetV1FunctionDefinitionCollections(List<V1FunctionDefinitionCollection> collections)
        {
            this.Collections = collections;
        }

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing the currently available <see cref="V1FunctionDefinitionCollection"/>s
        /// </summary>
        public List<V1FunctionDefinitionCollection> Collections { get; }

    }

    /// <summary>
    /// Represents the action used to add a function definition to the current <see cref="V1CorrelationCollectionState"/>
    /// </summary>
    public class AddV1FunctionDefinitionCollection
    {

        /// <summary>
        /// Initializes a new <see cref="AddV1FunctionDefinitionCollection"/>
        /// </summary>
        /// <param name="collection">The <see cref="V1FunctionDefinitionCollection"/> to add</param>
        public AddV1FunctionDefinitionCollection(V1FunctionDefinitionCollection collection)
        {
            this.Collection = collection;
        }

        /// <summary>
        /// Gets the <see cref="V1FunctionDefinitionCollection"/> to add
        /// </summary>
        public V1FunctionDefinitionCollection Collection { get; }

    }

    /// <summary>
    /// Represents the action used to removed a function definition from the current <see cref="V1CorrelationCollectionState"/>
    /// </summary>
    public class RemoveV1FunctionDefinitionCollection
    {

        /// <summary>
        /// Initializes a new <see cref="RemoveV1FunctionDefinitionCollection"/>
        /// </summary>
        /// <param name="collectionId">The id of the <see cref="V1FunctionDefinitionCollection"/> to remove</param>
        public RemoveV1FunctionDefinitionCollection(string collectionId)
        {
            this.CollectionId = collectionId;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1FunctionDefinitionCollection"/> to remove
        /// </summary>
        public string CollectionId { get; }

    }

}
