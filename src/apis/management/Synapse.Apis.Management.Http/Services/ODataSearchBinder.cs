using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.OData.UriParser;
using Neuroglia;
using System.Linq.Expressions;
using System.Reflection;

namespace Synapse.Apis.Management.Http.Services
{

    /// <summary>
    /// Represents the default <see cref="ISearchBinder"/> implementation
    /// </summary>
    public partial class ODataSearchBinder
        : QueryBinder, ISearchBinder
    {

        private static readonly Dictionary<BinaryOperatorKind, ExpressionType> BinaryOperatorMapping = new Dictionary<BinaryOperatorKind, ExpressionType>
        {
            { BinaryOperatorKind.And, ExpressionType.AndAlso },
            { BinaryOperatorKind.Or, ExpressionType.OrElse },
        };

        private static readonly MethodInfo FilterCorrelationMethod = typeof(ODataSearchBinder).GetMethod(nameof(FilterCorrelation), BindingFlags.Static | BindingFlags.NonPublic)!;

        /// <inheritdoc/>
        public Expression BindSearch(SearchClause searchClause, QueryBinderContext context)
        {
            return Expression.Lambda(this.BindSingleValueNode(searchClause.Expression, context), context.CurrentParameter);
        }

        /// <summary>
        /// Binds the specified <see cref="SingleValueCastNode"/>
        /// </summary>
        /// <param name="node">The <see cref="SingleValueCastNode"/> to bind</param>
        /// <param name="context">The current <see cref="QueryBinderContext"/></param>
        /// <returns>A new <see cref="Expression"/></returns>
        public override Expression BindSingleValueNode(SingleValueNode node, QueryBinderContext context)
        {
            return node switch
            {
                BinaryOperatorNode binaryOperatorNode => this.BindBinaryOperatorNode(binaryOperatorNode, context),
                SearchTermNode searchTermNode => this.BindSearchTerm(searchTermNode, context),
                UnaryOperatorNode unaryOperatorNode => this.BindUnaryOperatorNode(unaryOperatorNode, context),
                _ => throw new NotSupportedException($"The specified {nameof(SingleValueNode)} type '{node.GetType().Name}' is not supported")
            };
        }

        /// <summary>
        /// Binds the specified <see cref="BinaryOperatorNode"/>
        /// </summary>
        /// <param name="binaryOperatorNode">The <see cref="BinaryOperatorNode"/> to bind</param>
        /// <param name="context">The current <see cref="QueryBinderContext"/></param>
        /// <returns>A new <see cref="Expression"/></returns>
        public override Expression BindBinaryOperatorNode(BinaryOperatorNode binaryOperatorNode, QueryBinderContext context)
        {
            var left = this.Bind(binaryOperatorNode.Left, context);
            var right = this.Bind(binaryOperatorNode.Right, context);
            if (!BinaryOperatorMapping.TryGetValue(binaryOperatorNode.OperatorKind, out ExpressionType binaryExpressionType))
                throw new NotImplementedException($"Binary operator '{binaryOperatorNode.OperatorKind}' is not supported!");
            return Expression.MakeBinary(binaryExpressionType, left, right);
        }

        /// <summary>
        /// Binds the specified <see cref="SearchTermNode"/>
        /// </summary>
        /// <param name="term">The <see cref="SearchTermNode"/> to bind</param>
        /// <param name="context">The current <see cref="QueryBinderContext"/></param>
        /// <returns>A new <see cref="Expression"/></returns>
        public Expression BindSearchTerm(SearchTermNode term, QueryBinderContext context)
        {
            if (term == null)
                throw new ArgumentNullException(nameof(term));
            if (context.ElementClrType == typeof(Integration.Models.V1Workflow))
                return this.BindWorkflowSearchTerm(term, context);
            else if (context.ElementClrType == typeof(Integration.Models.V1WorkflowInstance))
                return this.BindWorkflowInstanceSearchTerm(term, context);
            else if (context.ElementClrType == typeof(Integration.Models.V1WorkflowActivity))
                return this.BindWorkflowActivitySearchTerm(term, context);
            else if (context.ElementClrType == typeof(Integration.Models.V1Correlation))
                return this.BindCorrelationSearchTerm(term, context);
            else
                throw new NotSupportedException($"Search is not allowed on element type '{context.ElementClrType.Name}'");
        }

        /// <summary>
        /// Binds the specified <see cref="Domain.Models.V1Workflow"/> <see cref="SearchTermNode"/>
        /// </summary>
        /// <param name="searchTermNode">The <see cref="Domain.Models.V1Workflow"/> <see cref="SearchTermNode"/> to bind</param>
        /// <param name="context">The current <see cref="QueryBinderContext"/></param>
        /// <returns>A new <see cref="Expression"/></returns>
        protected virtual Expression BindWorkflowSearchTerm(SearchTermNode searchTermNode, QueryBinderContext context)
        {
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
        /// Binds the specified <see cref="Domain.Models.V1WorkflowInstance"/> <see cref="SearchTermNode"/>
        /// </summary>
        /// <param name="searchTermNode">The <see cref="Domain.Models.V1WorkflowInstance"/> <see cref="SearchTermNode"/> to bind</param>
        /// <param name="context">The current <see cref="QueryBinderContext"/></param>
        /// <returns>A new <see cref="Expression"/></returns>
        protected virtual Expression BindWorkflowInstanceSearchTerm(SearchTermNode searchTermNode, QueryBinderContext context)
        {
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
        /// Binds the specified <see cref="Domain.Models.V1WorkflowActivity"/> <see cref="SearchTermNode"/>
        /// </summary>
        /// <param name="searchTermNode">The <see cref="Domain.Models.V1WorkflowActivity"/> <see cref="SearchTermNode"/> to bind</param>
        /// <param name="context">The current <see cref="QueryBinderContext"/></param>
        /// <returns>A new <see cref="Expression"/></returns>
        protected virtual Expression BindWorkflowActivitySearchTerm(SearchTermNode searchTermNode, QueryBinderContext context)
        {
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
        /// Binds the specified <see cref="Domain.Models.V1Correlation"/> <see cref="SearchTermNode"/>
        /// </summary>
        /// <param name="searchTermNode">The <see cref="Domain.Models.V1Correlation"/> <see cref="SearchTermNode"/> to bind</param>
        /// <param name="context">The current <see cref="QueryBinderContext"/></param>
        /// <returns>A new <see cref="Expression"/></returns>
        protected virtual Expression BindCorrelationSearchTerm(SearchTermNode searchTermNode, QueryBinderContext context)
        {
            var searchTerm = searchTermNode.Text.ToLowerInvariant();
            var searchQuery = Expression.IsTrue(Expression.Call(null, FilterCorrelationMethod, context.CurrentParameter, Expression.Constant(searchTerm)));
            return searchQuery;
        }

        static bool FilterCorrelation(Integration.Models.V1Correlation crl, string searchTerm)
        {
            return crl.Id.ToLower().Contains(searchTerm)
                || (crl.Conditions != null && crl.Conditions.Any(c =>
                    c.Filters != null
                    && (c.Filters.Any(f =>
                        (f.Attributes != null && f.Attributes.Any(kvp => kvp.Key.ToLower().Contains(searchTerm) || kvp.Value.ToLower().Contains(searchTerm)))
                        || (f.CorrelationMappings != null && f.CorrelationMappings.Any(kvp => kvp.Key.ToLower().Contains(searchTerm) || (!string.IsNullOrWhiteSpace(kvp.Value) && kvp.Value.ToLower().Contains(searchTerm))))))))
                || (crl.Contexts != null && crl.Contexts.Any(c => c.Id.Contains(searchTerm)))
                || (crl.Contexts != null && crl.Contexts.Any(c => c.Mappings != null && (c.Mappings.Any(kvp => kvp.Key.ToLower().Contains(searchTerm) || kvp.Value.ToLower().Contains(searchTerm)))))
                || EnumHelper.Stringify(crl.Lifetime).ToLowerInvariant().Contains(searchTerm)
                || EnumHelper.Stringify(crl.ConditionType).ToLowerInvariant().Contains(searchTerm);
        }

    }

}
