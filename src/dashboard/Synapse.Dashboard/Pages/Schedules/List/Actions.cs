using OData.QueryBuilder.Builders;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Pages.Schedules.List
{

    /// <summary>
    /// Represents the action used to query scheduled <see cref="V1Schedule"/>s
    /// </summary>
    public class QuerySchedules
    {

        /// <summary>
        /// Initializes a new <see cref="QuerySchedules"/>
        /// </summary>
        public QuerySchedules()
        {
            var builder = new ODataQueryBuilder(new Uri("https://test.com"))
                .For<V1Workflow>("V1Schedules")
                .ByList();
            this.Query = builder.ToUri(UriKind.Absolute).Query;
            if (!string.IsNullOrWhiteSpace(this.Query)) this.Query = this.Query[1..];
        }

        /// <summary>
        /// Gets the query to perform
        /// </summary>
        public string Query { get; }

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
