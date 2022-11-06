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

    public partial class V1Schedule
    {

        /// <summary>
        /// Gets the total number of occurences
        /// </summary>
        [DataMember(Name = "totalOccurences", Order = 11)]
        [Description("The total number of occurences")]
        public virtual long TotalOccurences { get; set; }

        /// <summary>
        /// Gets a list containing the ids of the schedule's active occurences
        /// </summary>
        [DataMember(Name = "activeOccurences", Order = 12)]
        [Description("A list containing the ids of the schedule's active occurences")]
        public virtual List<string> ActiveOccurences { get; set; }

    }

}
