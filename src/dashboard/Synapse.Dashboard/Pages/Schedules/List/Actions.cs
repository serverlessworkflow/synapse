using OData.QueryBuilder.Builders;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Pages.Schedules.List
{

    /// <summary>
    /// Represents the action used to query scheduled <see cref="V1Workflow"/>s
    /// </summary>
    public class QueryScheduledWorkflows
    {

        /// <summary>
        /// Initializes a new <see cref="V1Workflow"/>
        /// </summary>
        public QueryScheduledWorkflows()
        {
            var builder = new ODataQueryBuilder(new Uri("https://test.com"))
                .For<V1Workflow>("V1Workflows")
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
    /// Represents the action used to handle the differed results of a <see cref="QueryScheduledWorkflows"/> action
    /// </summary>
    public class HandleScheduledWorkflowQueryResults
    {

        /// <summary>
        /// Initializes a new <see cref="HandleScheduledWorkflowQueryResults"/>
        /// </summary>
        /// <param name="results">A <see cref="List{T}"/> that contains the differed results of the <see cref="QueryScheduledWorkflows"/> action</param>
        public HandleScheduledWorkflowQueryResults(List<V1Workflow> results)
        {
            this.Results = results;
        }

        /// <summary>
        /// Gets a <see cref="List{T}"/> that contains the differed results of the <see cref="QueryScheduledWorkflows"/> action
        /// </summary>
        public List<V1Workflow> Results { get; }

    }

}
