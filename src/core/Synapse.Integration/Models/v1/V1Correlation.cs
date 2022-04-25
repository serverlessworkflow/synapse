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

namespace Synapse.Integration.Models
{
    public partial class V1Correlation
    {

        /// <summary>
        /// Determines whether or not the specified <see cref="V1Event"/> matches one of the <see cref="V1Correlation"/>'s conditions
        /// </summary>
        /// <param name="e">The <see cref="V1Event"/> to check</param>
        /// <returns>A boolean indicating whether or not the specified <see cref="V1Event"/> matches one of the <see cref="V1Correlation"/>'s conditions</returns>
        public virtual bool AppliesTo(V1Event e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            return this.GetMatchingConditionFor(e) != null;
        }

        /// <summary>
        /// Gets the first matching <see cref="V1CorrelationCondition"/> for the specified <see cref="V1Event"/>
        /// </summary>
        /// <param name="e">The <see cref="V1Event"/> to get the <see cref="V1CorrelationCondition"/> for</param>
        /// <returns>The first matching <see cref="V1CorrelationCondition"/> for the specified <see cref="V1Event"/>, if any</returns>
        public virtual V1CorrelationCondition? GetMatchingConditionFor(V1Event e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            return this.Conditions.FirstOrDefault(c => c.Matches(e));
        }

    }

}
