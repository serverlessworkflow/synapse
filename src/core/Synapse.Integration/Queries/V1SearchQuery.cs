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

namespace Synapse.Integration.Queries
{

    /// <summary>
    /// Describes a search query
    /// </summary>
    public class V1SearchQuery
    {

        /// <summary>
        /// Initializes a new <see cref="V1SearchQuery"/>
        /// </summary>
        protected V1SearchQuery()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1SearchQuery"/>
        /// </summary>
        /// <param name="term">The term to search for</param>
        public V1SearchQuery(string term)
        {
            if(string.IsNullOrWhiteSpace(term))
                throw new ArgumentNullException(nameof(term));
            this.Term = term;
        }

        /// <summary>
        /// Initializes a new <see cref="V1SearchQuery"/>
        /// </summary>
        /// <param name="term">The term to search for</param>
        /// <param name="query">The optional query string</param>
        public V1SearchQuery(string term, string query)
            : this(term)
        {
            this.Query = query;
        }

        /// <summary>
        /// Gets/sets the term to search for
        /// </summary>
        public virtual string Term { get; set; }

        /// <summary>
        /// Gets/sets the optional query string
        /// </summary>
        public virtual string Query { get; set; }

    }

}
