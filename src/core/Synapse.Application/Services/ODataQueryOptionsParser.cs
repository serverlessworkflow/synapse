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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Synapse.Application.Services
{
    /// <summary>
    /// Represents the default implementation of the <see cref="IODataQueryOptionsParser"/>
    /// </summary>
    public class ODataQueryOptionsParser
        : IODataQueryOptionsParser
    {

        /// <summary>
        /// Initializes a new <see cref="ODataQueryOptionsParser"/>
        /// </summary>
        /// <param name="edmModel">The semantic description of the application's EDM model</param>
        public ODataQueryOptionsParser(IEdmModel edmModel)
        {
            this.EdmModel = edmModel;
        }

        /// <summary>
        /// Gets the semantic description of the application's EDM model
        /// </summary>
        protected IEdmModel EdmModel { get; }

        /// <inheritdoc/>
        public virtual ODataQueryOptions<TEntity> Parse<TEntity>(string? query)
            where TEntity : class, IIdentifiable
        {
            ODataQueryOptions<TEntity> queryOptions = null!;
            if (!string.IsNullOrWhiteSpace(query))
            {
                var context = new DefaultHttpContext();
                context.Request.QueryString = new($"?{query}");
                var parser = new ODataUriParser(this.EdmModel, new(string.Empty, UriKind.Relative));
                var path = parser.ParsePath();
                queryOptions = new ODataQueryOptions<TEntity>(new(this.EdmModel, typeof(TEntity), path), context.Request);
            }
            return queryOptions;
        }

    }

}
