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

using OData.QueryBuilder.Builders;
using OData.QueryBuilder.Conventions.AddressingEntities.Query;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Pages.Resources.Collections
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
            Query = builder.ToUri(UriKind.Absolute).Query;
            if (!string.IsNullOrWhiteSpace(searchTerm))
                Query = $"$search={searchTerm}&{Query}";
        }

        /// <summary>
        /// Initializes a new <see cref="QueryV1FunctionDefinitionCollections"/>
        /// </summary>
        /// <param name="searchTerm">The term to search for</param>
        public QueryV1FunctionDefinitionCollections(string searchTerm)
            : this(searchTerm, _ => { })
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
            Collections = collections;
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
            Collection = collection;
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
            CollectionId = collectionId;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1FunctionDefinitionCollection"/> to remove
        /// </summary>
        public string CollectionId { get; }

    }

    /// <summary>
    /// Represents the action used to query <see cref="V1EventDefinitionCollection"/>s
    /// </summary>
    public class QueryV1EventDefinitionCollections
    {

        /// <summary>
        /// Initializes a new <see cref="QueryV1EventDefinitionCollections"/>
        /// </summary>
        public QueryV1EventDefinitionCollections()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="QueryV1EventDefinitionCollections"/>
        /// </summary>
        /// <param name="searchTerm">The term to search for</param>
        /// <param name="querySetup">An <see cref="Action{T}"/> used to setup the query to perform</param>
        public QueryV1EventDefinitionCollections(string? searchTerm, Action<IODataQueryCollection<V1EventDefinitionCollection>> querySetup)
        {
            var builder = new ODataQueryBuilder(new Uri("https://test.com"))
                .For<V1EventDefinitionCollection>("V1EventDefinitionCollections")
                .ByList();
            querySetup(builder);
            this.Query = builder.ToUri(UriKind.Absolute).Query;
            if (!string.IsNullOrWhiteSpace(searchTerm))
                this.Query = $"$search={searchTerm}&{Query}";
        }

        /// <summary>
        /// Initializes a new <see cref="QueryV1EventDefinitionCollections"/>
        /// </summary>
        /// <param name="searchTerm">The term to search for</param>
        public QueryV1EventDefinitionCollections(string searchTerm)
            : this(searchTerm, _ => { })
        {

        }

        /// <summary>
        /// Initializes a new <see cref="QueryV1EventDefinitionCollections"/>
        /// </summary>
        /// <param name="querySetup">An <see cref="Action{T}"/> used to setup the query to perform</param>
        public QueryV1EventDefinitionCollections(Action<IODataQueryCollection<V1EventDefinitionCollection>> querySetup)
            : this(null, querySetup)
        {

        }

        /// <summary>
        /// Gets the query to perform
        /// </summary>
        public string? Query { get; }

    }

    /// <summary>
    /// Represents the action used to set the currently available <see cref="V1EventDefinitionCollection"/>s
    /// </summary>
    public class SetV1EventDefinitionCollections
    {

        /// <summary>
        /// Initializes a new <see cref="SetV1EventDefinitionCollections"/>
        /// </summary>
        /// <param name="collections">A <see cref="List{T}"/> containing the currently available <see cref="V1EventDefinitionCollection"/>s</param>
        public SetV1EventDefinitionCollections(List<V1EventDefinitionCollection> collections)
        {
            Collections = collections;
        }

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing the currently available <see cref="V1EventDefinitionCollection"/>s
        /// </summary>
        public List<V1EventDefinitionCollection> Collections { get; }

    }

    /// <summary>
    /// Represents the action used to add a event definition to the current <see cref="V1CorrelationCollectionState"/>
    /// </summary>
    public class AddV1EventDefinitionCollection
    {

        /// <summary>
        /// Initializes a new <see cref="AddV1EventDefinitionCollection"/>
        /// </summary>
        /// <param name="collection">The <see cref="V1EventDefinitionCollection"/> to add</param>
        public AddV1EventDefinitionCollection(V1EventDefinitionCollection collection)
        {
            Collection = collection;
        }

        /// <summary>
        /// Gets the <see cref="V1EventDefinitionCollection"/> to add
        /// </summary>
        public V1EventDefinitionCollection Collection { get; }

    }

    /// <summary>
    /// Represents the action used to removed a event definition from the current <see cref="V1CorrelationCollectionState"/>
    /// </summary>
    public class RemoveV1EventDefinitionCollection
    {

        /// <summary>
        /// Initializes a new <see cref="RemoveV1EventDefinitionCollection"/>
        /// </summary>
        /// <param name="collectionId">The id of the <see cref="V1EventDefinitionCollection"/> to remove</param>
        public RemoveV1EventDefinitionCollection(string collectionId)
        {
            CollectionId = collectionId;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1EventDefinitionCollection"/> to remove
        /// </summary>
        public string CollectionId { get; }

    }

    /// <summary>
    /// Represents the action used to query <see cref="V1AuthenticationDefinitionCollection"/>s
    /// </summary>
    public class QueryV1AuthenticationDefinitionCollections
    {

        /// <summary>
        /// Initializes a new <see cref="QueryV1AuthenticationDefinitionCollections"/>
        /// </summary>
        public QueryV1AuthenticationDefinitionCollections()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="QueryV1AuthenticationDefinitionCollections"/>
        /// </summary>
        /// <param name="searchTerm">The term to search for</param>
        /// <param name="querySetup">An <see cref="Action{T}"/> used to setup the query to perform</param>
        public QueryV1AuthenticationDefinitionCollections(string? searchTerm, Action<IODataQueryCollection<V1AuthenticationDefinitionCollection>> querySetup)
        {
            var builder = new ODataQueryBuilder(new Uri("https://test.com"))
                .For<V1AuthenticationDefinitionCollection>("V1AuthenticationDefinitionCollections")
                .ByList();
            querySetup(builder);
            Query = builder.ToUri(UriKind.Absolute).Query;
            if (!string.IsNullOrWhiteSpace(searchTerm))
                Query = $"$search={searchTerm}&{Query}";
        }

        /// <summary>
        /// Initializes a new <see cref="QueryV1AuthenticationDefinitionCollections"/>
        /// </summary>
        /// <param name="searchTerm">The term to search for</param>
        public QueryV1AuthenticationDefinitionCollections(string searchTerm)
            : this(searchTerm, _ => { })
        {

        }

        /// <summary>
        /// Initializes a new <see cref="QueryV1AuthenticationDefinitionCollections"/>
        /// </summary>
        /// <param name="querySetup">An <see cref="Action{T}"/> used to setup the query to perform</param>
        public QueryV1AuthenticationDefinitionCollections(Action<IODataQueryCollection<V1AuthenticationDefinitionCollection>> querySetup)
            : this(null, querySetup)
        {

        }

        /// <summary>
        /// Gets the query to perform
        /// </summary>
        public string? Query { get; }

    }

    /// <summary>
    /// Represents the action used to set the currently available <see cref="V1AuthenticationDefinitionCollection"/>s
    /// </summary>
    public class SetV1AuthenticationDefinitionCollections
    {

        /// <summary>
        /// Initializes a new <see cref="SetV1AuthenticationDefinitionCollections"/>
        /// </summary>
        /// <param name="collections">A <see cref="List{T}"/> containing the currently available <see cref="V1AuthenticationDefinitionCollection"/>s</param>
        public SetV1AuthenticationDefinitionCollections(List<V1AuthenticationDefinitionCollection> collections)
        {
            this.Collections = collections;
        }

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing the currently available <see cref="V1AuthenticationDefinitionCollection"/>s
        /// </summary>
        public List<V1AuthenticationDefinitionCollection> Collections { get; }

    }

    /// <summary>
    /// Represents the action used to add a authentication definition to the current <see cref="V1CorrelationCollectionState"/>
    /// </summary>
    public class AddV1AuthenticationDefinitionCollection
    {

        /// <summary>
        /// Initializes a new <see cref="AddV1AuthenticationDefinitionCollection"/>
        /// </summary>
        /// <param name="collection">The <see cref="V1AuthenticationDefinitionCollection"/> to add</param>
        public AddV1AuthenticationDefinitionCollection(V1AuthenticationDefinitionCollection collection)
        {
            this.Collection = collection;
        }

        /// <summary>
        /// Gets the <see cref="V1AuthenticationDefinitionCollection"/> to add
        /// </summary>
        public V1AuthenticationDefinitionCollection Collection { get; }

    }

    /// <summary>
    /// Represents the action used to removed a authentication definition from the current <see cref="V1CorrelationCollectionState"/>
    /// </summary>
    public class RemoveV1AuthenticationDefinitionCollection
    {

        /// <summary>
        /// Initializes a new <see cref="RemoveV1AuthenticationDefinitionCollection"/>
        /// </summary>
        /// <param name="collectionId">The id of the <see cref="V1AuthenticationDefinitionCollection"/> to remove</param>
        public RemoveV1AuthenticationDefinitionCollection(string collectionId)
        {
            this.CollectionId = collectionId;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1AuthenticationDefinitionCollection"/> to remove
        /// </summary>
        public string CollectionId { get; }

    }

}
