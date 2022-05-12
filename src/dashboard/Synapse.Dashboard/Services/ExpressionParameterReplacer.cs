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

using System.Linq.Expressions;

namespace Synapse.Dashboard.Services
{
    public class ExpressionParameterReplacer
        : ExpressionVisitor
    {

        public ExpressionParameterReplacer(ParameterExpression parameterExpression)
        {
            this.ParameterExpression = parameterExpression;
        }

        protected ParameterExpression ParameterExpression { get; }

        /// <inheritdoc/>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return base.VisitParameter(this.ParameterExpression);
        }

    }

}
