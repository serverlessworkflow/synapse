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

    public partial class V1Workflow
    {

        /// <summary>
        /// Gets the total count of the described workflow's instances
        /// </summary>
        [DataMember(Order = 20)]
        public virtual long TotalInstanceCount { get; set; }

        /// <summary>
        /// Gets the total count of the described workflow's running instances
        /// </summary>
        [DataMember(Order = 21)]
        public virtual long RunningInstanceCount { get; set; }

        /// <summary>
        /// Gets the total count of executed instances
        /// </summary>
        [DataMember(Order = 22)]
        public virtual long ExecutedInstanceCount { get; set; }

        /// <summary>
        /// Gets the total count of completed instances
        /// </summary>
        [DataMember(Order = 23)]
        public virtual long CompletedInstanceCount { get; set; }

        /// <summary>
        /// Gets the total count of faulted instances
        /// </summary>
        [DataMember(Order = 24)]
        public virtual long FaultedInstanceCount { get; set; }

        /// <summary>
        /// Gets the total count of cancelled instances
        /// </summary>
        [DataMember(Order = 25)]
        public virtual long CancelledInstanceCount { get; set; }

        /// <summary>
        /// Gets the total execution time of the described workflow's instances
        /// </summary>
        [DataMember(Order = 26)]
        public virtual TimeSpan TotalInstanceExecutionTime { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Gets the shortest execution time of the described workflow's instances
        /// </summary>
        [DataMember(Order = 27)]
        public virtual TimeSpan? ShortestInstanceDuration { get; set; }

        /// <summary>
        /// Gets the longest execution time of the described workflow's instances
        /// </summary>
        [DataMember(Order = 28)]
        public virtual TimeSpan? LongestInstanceDuration { get; set; }

        /// <summary>
        /// Gets the average duration the describe workflow's instances
        /// </summary>
        [IgnoreDataMember]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [YamlDotNet.Serialization.YamlIgnore]
        public virtual TimeSpan AverageInstanceDuration => this.ExecutedInstanceCount < 1 ? TimeSpan.Zero : TimeSpan.FromTicks(this.TotalInstanceExecutionTime.Ticks / this.ExecutedInstanceCount);

    }

}
