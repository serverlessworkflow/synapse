using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.OData.UriParser;
using Neuroglia;
using System.Linq.Expressions;

namespace Synapse.Apis.Management.Http.Services
{

    /// <summary>
    /// Represents the default <see cref="ISearchBinder"/> implementation
    /// </summary>
    public class ODataSearchBinder
        : ISearchBinder
    {

        /// <inheritdoc/>
        public Expression BindSearch(SearchClause searchClause, QueryBinderContext context)
        {
            if (context.ElementClrType == typeof(Integration.Models.V1Workflow))
                return this.BindWorkflowSearch(searchClause, context);
            else if (context.ElementClrType == typeof(Integration.Models.V1WorkflowInstance))
                return this.BindWorkflowInstanceSearch(searchClause, context);
            else if (context.ElementClrType == typeof(Integration.Models.V1WorkflowActivity))
                return this.BindWorkflowActivitySearch(searchClause, context);
            else if (context.ElementClrType == typeof(Integration.Models.V1Correlation))
                return this.BindCorrelationSearch(searchClause, context);
            else
                throw new NotSupportedException($"Search is not allowed on element type '{context.ElementClrType.Name}'");
        }

        /// <summary>
        /// Binds the specified <see cref="Domain.Models.V1Workflow"/> <see cref="SearchClause"/>
        /// </summary>
        /// <param name="searchClause">The <see cref="Domain.Models.V1Workflow"/> <see cref="SearchClause"/> to bind</param>
        /// <param name="context">The current <see cref="QueryBinderContext"/></param>
        /// <returns>A new <see cref="Expression"/></returns>
        protected virtual Expression BindWorkflowSearch(SearchClause searchClause, QueryBinderContext context)
        {
            var searchTermNode = (SearchTermNode)searchClause.Expression;
            var searchTerm = searchTermNode.Text.ToLowerInvariant();
            Expression<Func<Integration.Models.V1Workflow, bool>> searchQuery = wf => 
                wf.Id.ToLower().Contains(searchTerm)
                || (!string.IsNullOrWhiteSpace(wf.Definition.Id) && wf.Definition.Id.ToLowerInvariant().Contains(searchTerm))
                || wf.Definition.Name.ToLowerInvariant().Contains(searchTerm)
                || (!string.IsNullOrWhiteSpace(wf.Definition.Description) && wf.Definition.Description.ToLowerInvariant().Contains(searchTerm))
                || wf.Definition.Version.ToLowerInvariant().Contains(searchTerm)
                || wf.Definition.SpecVersion.ToLowerInvariant().Contains(searchTerm)
                || (wf.Definition.Annotations != null && wf.Definition.Annotations.Any(a => a.ToLowerInvariant() == searchTerm));
            return searchQuery;
        }

        /// <summary>
        /// Binds the specified <see cref="Domain.Models.V1WorkflowInstance"/> <see cref="SearchClause"/>
        /// </summary>
        /// <param name="searchClause">The <see cref="Domain.Models.V1WorkflowInstance"/> <see cref="SearchClause"/> to bind</param>
        /// <param name="context">The current <see cref="QueryBinderContext"/></param>
        /// <returns>A new <see cref="Expression"/></returns>
        protected virtual Expression BindWorkflowInstanceSearch(SearchClause searchClause, QueryBinderContext context)
        {
            var searchTermNode = (SearchTermNode)searchClause.Expression;
            var searchTerm = searchTermNode.Text.ToLowerInvariant();
            Expression<Func<Integration.Models.V1WorkflowInstance, bool>> searchQuery = wfi =>
                wfi.Id.ToLower().Contains(searchTerm)
                || wfi.WorkflowId.ToLowerInvariant().Contains(searchTerm)
                || wfi.Key.ToLowerInvariant().Contains(searchTerm)
                || (!string.IsNullOrWhiteSpace(wfi.ParentId) && wfi.ParentId.ToLowerInvariant().Contains(searchTerm))
                || EnumHelper.Stringify(wfi.ActivationType).ToLowerInvariant().Contains(searchTerm)
                || EnumHelper.Stringify(wfi.Status).ToLowerInvariant().Contains(searchTerm);
            return searchQuery;
        }

        /// <summary>
        /// Binds the specified <see cref="Domain.Models.V1WorkflowActivity"/> <see cref="SearchClause"/>
        /// </summary>
        /// <param name="searchClause">The <see cref="Domain.Models.V1WorkflowActivity"/> <see cref="SearchClause"/> to bind</param>
        /// <param name="context">The current <see cref="QueryBinderContext"/></param>
        /// <returns>A new <see cref="Expression"/></returns>
        protected virtual Expression BindWorkflowActivitySearch(SearchClause searchClause, QueryBinderContext context)
        {
            var searchTermNode = (SearchTermNode)searchClause.Expression;
            var searchTerm = searchTermNode.Text.ToLowerInvariant();
            Expression<Func<Integration.Models.V1WorkflowActivity, bool>> searchQuery = wa =>
                wa.Id.ToLower().Contains(searchTerm)
                || (wa.Error != null && (wa.Error.Code.ToLowerInvariant().Contains(searchTerm) || wa.Error.Message.ToLowerInvariant().Contains(searchTerm)))
                || (!string.IsNullOrWhiteSpace(wa.ParentId) && wa.ParentId.ToLowerInvariant().Contains(searchTerm))
                || EnumHelper.Stringify(wa.Type).ToLowerInvariant().Contains(searchTerm)
                || EnumHelper.Stringify(wa.Status).ToLowerInvariant().Contains(searchTerm);
            return searchQuery;
        }

        /// <summary>
        /// Binds the specified <see cref="Domain.Models.V1Correlation"/> <see cref="SearchClause"/>
        /// </summary>
        /// <param name="searchClause">The <see cref="Domain.Models.V1Correlation"/> <see cref="SearchClause"/> to bind</param>
        /// <param name="context">The current <see cref="QueryBinderContext"/></param>
        /// <returns>A new <see cref="Expression"/></returns>
        protected virtual Expression BindCorrelationSearch(SearchClause searchClause, QueryBinderContext context)
        {
            var searchTermNode = (SearchTermNode)searchClause.Expression;
            var searchTerm = searchTermNode.Text.ToLowerInvariant();
            Expression<Func<Integration.Models.V1Correlation, bool>> searchQuery = crl =>
                crl.Id.ToLower().Contains(searchTerm)
                || (crl.Conditions != null && crl.Conditions.Any(c => c.Filters.Any(f => f.Attributes.Any(kvp => kvp.Key.ToLower().Contains(searchTerm) || kvp.Value.ToLower().Contains(searchTerm)) || f.CorrelationMappings.Any(kvp => kvp.Key.ToLower().Contains(searchTerm) || kvp.Value.ToLower().Contains(searchTerm)))))
                || (crl.Contexts != null && crl.Contexts.Any(c => c.Id.Contains(searchTerm)))
                || (crl.Contexts != null && crl.Contexts.Any(c => c.Mappings.Any(kvp => kvp.Key.ToLower().Contains(searchTerm) || kvp.Value.ToLower().Contains(searchTerm))))
                || EnumHelper.Stringify(crl.Lifetime).ToLowerInvariant().Contains(searchTerm)
                || EnumHelper.Stringify(crl.ConditionType).ToLowerInvariant().Contains(searchTerm);
            return searchQuery;
        }

    }

}
