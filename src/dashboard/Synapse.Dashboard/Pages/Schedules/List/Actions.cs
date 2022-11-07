using OData.QueryBuilder.Builders;
using OData.QueryBuilder.Conventions.AddressingEntities.Query;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Pages.Schedules.List
{

    /// <summary>
    /// Represents the action used to query <see cref="V1Schedule"/>s
    /// </summary>
    public class QuerySchedules
    {

        /// <summary>
        /// Initializes a new <see cref="QuerySchedules"/>
        /// </summary>
        public QuerySchedules()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="QuerySchedules"/>
        /// </summary>
        /// <param name="searchTerm">The term to search for</param>
        /// <param name="querySetup">An <see cref="Action{T}"/> used to setup the query to perform</param>
        public QuerySchedules(string? searchTerm, Action<IODataQueryCollection<V1Schedule>> querySetup)
        {
            var builder = new ODataQueryBuilder(new Uri("https://test.com"))
                .For<V1Schedule>("V1V1Schedules")
                .ByList();
            querySetup(builder);
            Query = builder.ToUri(UriKind.Absolute).Query;
            if (!string.IsNullOrWhiteSpace(searchTerm))
                Query = $"$search={searchTerm}&{Query}";
        }

        /// <summary>
        /// Initializes a new <see cref="QuerySchedules"/>
        /// </summary>
        /// <param name="searchTerm">The term to search for</param>
        public QuerySchedules(string searchTerm)
            : this(searchTerm, _ => { })
        {

        }

        /// <summary>
        /// Initializes a new <see cref="QuerySchedules"/>
        /// </summary>
        /// <param name="querySetup">An <see cref="Action{T}"/> used to setup the query to perform</param>
        public QuerySchedules(Action<IODataQueryCollection<V1Schedule>> querySetup)
            : this(null, querySetup)
        {

        }

        /// <summary>
        /// Gets the query to perform
        /// </summary>
        public string? Query { get; }

    }

    /// <summary>
    /// Represents the action used to handle the differed results of a <see cref="QuerySchedules"/> action
    /// </summary>
    public class HandleScheduleQueryResults
    {

        /// <summary>
        /// Initializes a new <see cref="HandleScheduleQueryResults"/>
        /// </summary>
        /// <param name="results">A <see cref="List{T}"/> that contains the differed results of the <see cref="QuerySchedules"/> action</param>
        public HandleScheduleQueryResults(List<V1Schedule> results)
        {
            this.Results = results;
        }

        /// <summary>
        /// Gets a <see cref="List{T}"/> that contains the differed results of the <see cref="QuerySchedules"/> action
        /// </summary>
        public List<V1Schedule> Results { get; }

    }

}
