/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

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

        private static readonly MethodInfo FilterWorkflowMethod = typeof(ODataSearchBinder).GetMethod(nameof(FilterWorkflow), BindingFlags.Static | BindingFlags.NonPublic)!;
        private static readonly MethodInfo FilterWorkflowInstanceMethod = typeof(ODataSearchBinder).GetMethod(nameof(FilterWorkflowInstance), BindingFlags.Static | BindingFlags.NonPublic)!;
        private static readonly MethodInfo FilterWorkflowActivityMethod = typeof(ODataSearchBinder).GetMethod(nameof(FilterWorkflowActivity), BindingFlags.Static | BindingFlags.NonPublic)!;
        private static readonly MethodInfo FilterCorrelationMethod = typeof(ODataSearchBinder).GetMethod(nameof(FilterCorrelation), BindingFlags.Static | BindingFlags.NonPublic)!;
        private static readonly MethodInfo FilterFunctionCollectionMethod = typeof(ODataSearchBinder).GetMethod(nameof(FilterFunctionCollection), BindingFlags.Static | BindingFlags.NonPublic)!;

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
            else if (context.ElementClrType == typeof(Integration.Models.V1FunctionDefinitionCollection))
                return this.BindFunctionCollectionSearchTerm(term, context);
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
            var searchQuery = Expression.IsTrue(Expression.Call(null, FilterWorkflowMethod, context.CurrentParameter, Expression.Constant(searchTerm)));
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
            var searchQuery = Expression.IsTrue(Expression.Call(null, FilterWorkflowInstanceMethod, context.CurrentParameter, Expression.Constant(searchTerm)));
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
            var searchQuery = Expression.IsTrue(Expression.Call(null, FilterWorkflowActivityMethod, context.CurrentParameter, Expression.Constant(searchTerm)));
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

        /// <summary>
        /// Binds the specified <see cref="Domain.Models.V1FunctionDefinitionCollection"/> <see cref="SearchTermNode"/>
        /// </summary>
        /// <param name="searchTermNode">The <see cref="Domain.Models.V1FunctionDefinitionCollection"/> <see cref="SearchTermNode"/> to bind</param>
        /// <param name="context">The current <see cref="QueryBinderContext"/></param>
        /// <returns>A new <see cref="Expression"/></returns>
        protected virtual Expression BindFunctionCollectionSearchTerm(SearchTermNode searchTermNode, QueryBinderContext context)
        {
            var searchTerm = searchTermNode.Text.ToLowerInvariant();
            var searchQuery = Expression.IsTrue(Expression.Call(null, FilterFunctionCollectionMethod, context.CurrentParameter, Expression.Constant(searchTerm)));
            return searchQuery;
        }

        static bool FilterWorkflow(Integration.Models.V1Workflow workflow, string searchTerm)
        {
            return workflow.Id.ToLower().Contains(searchTerm)
                || (!string.IsNullOrWhiteSpace(workflow.Definition.Id) && workflow.Definition.Id.ToLowerInvariant().Contains(searchTerm))
                || workflow.Definition.Name.ToLowerInvariant().Contains(searchTerm)
                || (!string.IsNullOrWhiteSpace(workflow.Definition.Description) && workflow.Definition.Description.ToLowerInvariant().Contains(searchTerm))
                || workflow.Definition.Version.ToLowerInvariant().Contains(searchTerm)
                || workflow.Definition.SpecVersion.ToLowerInvariant().Contains(searchTerm)
                || (workflow.Definition.Annotations != null && workflow.Definition.Annotations.Any(a => a.ToLowerInvariant() == searchTerm));
        }

        static bool FilterWorkflowInstance(Integration.Models.V1WorkflowInstance workflowInstance, string searchTerm)
        {
            return workflowInstance.Id.ToLower().Contains(searchTerm)
                || workflowInstance.WorkflowId.ToLowerInvariant().Contains(searchTerm)
                || workflowInstance.Key.ToLowerInvariant().Contains(searchTerm)
                || (!string.IsNullOrWhiteSpace(workflowInstance.ParentId) && workflowInstance.ParentId.ToLowerInvariant().Contains(searchTerm))
                || EnumHelper.Stringify(workflowInstance.ActivationType).ToLowerInvariant().Contains(searchTerm)
                || EnumHelper.Stringify(workflowInstance.Status).ToLowerInvariant().Contains(searchTerm);
        }

        static bool FilterWorkflowActivity(Integration.Models.V1WorkflowActivity workflowActivity, string searchTerm)
        {
            return workflowActivity.Id.ToLower().Contains(searchTerm)
                || (workflowActivity.Error != null && (workflowActivity.Error.Code.ToLowerInvariant().Contains(searchTerm) || workflowActivity.Error.Message.ToLowerInvariant().Contains(searchTerm)))
                || (!string.IsNullOrWhiteSpace(workflowActivity.ParentId) && workflowActivity.ParentId.ToLowerInvariant().Contains(searchTerm))
                || EnumHelper.Stringify(workflowActivity.Type).ToLowerInvariant().Contains(searchTerm)
                || EnumHelper.Stringify(workflowActivity.Status).ToLowerInvariant().Contains(searchTerm);
        }

        static bool FilterCorrelation(Integration.Models.V1Correlation correlation, string searchTerm)
        {
            return correlation.Id.ToLower().Contains(searchTerm)
                || (correlation.Conditions != null && correlation.Conditions.Any(c =>
                    c.Filters != null
                    && (c.Filters.Any(f =>
                        (f.Attributes != null && f.Attributes.Any(kvp => kvp.Key.ToLower().Contains(searchTerm) || kvp.Value.ToLower().Contains(searchTerm)))
                        || (f.CorrelationMappings != null && f.CorrelationMappings.Any(kvp => kvp.Key.ToLower().Contains(searchTerm) || (!string.IsNullOrWhiteSpace(kvp.Value) && kvp.Value.ToLower().Contains(searchTerm))))))))
                || (correlation.Contexts != null && correlation.Contexts.Any(c => c.Id.Contains(searchTerm)))
                || (correlation.Contexts != null && correlation.Contexts.Any(c => c.Mappings != null && (c.Mappings.Any(kvp => kvp.Key.ToLower().Contains(searchTerm) || kvp.Value.ToLower().Contains(searchTerm)))))
                || EnumHelper.Stringify(correlation.Lifetime).ToLowerInvariant().Contains(searchTerm)
                || EnumHelper.Stringify(correlation.ConditionType).ToLowerInvariant().Contains(searchTerm);
        }

        static bool FilterFunctionCollection(Integration.Models.V1FunctionDefinitionCollection collection, string searchTerm)
        {
            return collection.Id.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase)
                || collection.Name.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase)
                || collection.Version.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase)
                || collection.Description.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase)
                || (collection.Functions != null && collection.Functions.Any(f => f.Name.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase)))
                || (collection.Functions != null && collection.Functions.Any(f => f.Operation.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase)))
                || (collection.Functions != null && collection.Functions.Any(f => EnumHelper.Stringify(f.Type).Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase)));
        }

    }

}
