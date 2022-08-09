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

    /// <summary>
    /// Describes an application's operational report
    /// </summary>
    [ReadModel]
    [DataContract]
    public class V1OperationalReport
        : IIdentifiable<string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1OperationalReport"/>
        /// </summary>
        protected V1OperationalReport()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1OperationalReport"/>
        /// </summary>
        /// <param name="date">The date the <see cref="V1OperationalReport"/> applies to</param>
        public V1OperationalReport(DateTime date)
        {
            this.Id = GetIdFor(date);
            this.Date = date;
        }

        /// <summary>
        /// Gets the <see cref="V1OperationalReport"/>'s id
        /// </summary>
        [DataMember(Order = 1)]
        public string Id { get; set; }

        object IIdentifiable.Id => this.Id;

        /// <summary>
        /// Gets the date the <see cref="V1OperationalReport"/> applies to
        /// </summary>
        [DataMember(Order = 2)]
        public virtual DateTime Date { get; set; }

        /// <summary>
        /// Gets the total amount of workflow definitions that were created during the day the <see cref="V1OperationalReport"/> applies to
        /// </summary>
        [DataMember(Order = 3)]
        public virtual long TotalDefinitions { get; set; }

        /// <summary>
        /// Gets the total amount of workflow definition versions that were created during the day the <see cref="V1OperationalReport"/> applies to
        /// </summary>
        [DataMember(Order = 4)]
        public virtual long TotalDefinitionVersions { get; set; }

        /// <summary>
        /// Gets the total amount of workflow instances that were created during the day the <see cref="V1OperationalReport"/> applies to
        /// </summary>
        [DataMember(Order = 5)]
        public virtual long TotalInstances { get; set; }

        /// <summary>
        /// Gets the total amount of running workflow instances at the moment the <see cref="V1OperationalReport"/> was created
        /// </summary>
        [DataMember(Order = 6)]
        public virtual long RunningInstances { get; set; }

        /// <summary>
        /// Gets the total amount of executed workflow instances at the moment the <see cref="V1OperationalReport"/> was created
        /// </summary>
        [DataMember(Order = 7)]
        public virtual long ExecutedInstances { get; set; }

        /// <summary>
        /// Gets the total amount of completed workflow instances at the moment the <see cref="V1OperationalReport"/> was created
        /// </summary>
        [DataMember(Order = 8)]
        public virtual long CompletedInstances { get; set; }

        /// <summary>
        /// Gets the total amount of faulted workflow instances at the moment the <see cref="V1OperationalReport"/> was created
        /// </summary>
        [DataMember(Order = 9)]
        public virtual long FaultedInstances { get; set; }

        /// <summary>
        /// Gets the total amount of cancelled workflow instances at the moment the <see cref="V1OperationalReport"/> was created
        /// </summary>
        [DataMember(Order = 10)]
        public virtual long CancelledInstances { get; set; }

        /// <summary>
        /// Gets the total amount of workflow activities that were created during the day the <see cref="V1OperationalReport"/> applies to
        /// </summary>
        [DataMember(Order = 11)]
        public virtual long TotalActivities { get; set; }

        /// <summary>
        /// Gets the total amount of running workflow activities at the moment the <see cref="V1OperationalReport"/> was created
        /// </summary>
        [DataMember(Order = 12)]
        public virtual long RunningActivities { get; set; }

        /// <summary>
        /// Gets the total amount of executed workflow activities at the moment the <see cref="V1OperationalReport"/> was created
        /// </summary>
        [DataMember(Order = 13)]
        public virtual long ExecutedActivities { get; set; }

        /// <summary>
        /// Gets the total amount of completed workflow activities at the moment the <see cref="V1OperationalReport"/> was created
        /// </summary>
        [DataMember(Order = 14)]
        public virtual long CompletedActivities { get; set; }

        /// <summary>
        /// Gets the total amount of faulted workflow activities at the moment the <see cref="V1OperationalReport"/> was created
        /// </summary>
        [DataMember(Order = 15)]
        public virtual long FaultedActivities { get; set; }

        /// <summary>
        /// Gets the total amount of cancelled workflow activities at the moment the <see cref="V1OperationalReport"/> was created
        /// </summary>
        [DataMember(Order = 16)]
        public virtual long CancelledActivities { get; set; }

        /// <summary>
        /// Gets the total amount of skipped workflow activities at the moment the <see cref="V1OperationalReport"/> was created
        /// </summary>
        [DataMember(Order = 17)]
        public virtual long SkippedActivities { get; set; }

        /// <summary>
        /// Builds a new <see cref="V1OperationalReport"/> id for the specified date
        /// </summary>
        /// <param name="date">The date to create a new <see cref="V1OperationalReport"/> id for</param>
        /// <returns>A new <see cref="V1OperationalReport"/> id for the specified date</returns>
        public static string GetIdFor(DateTime date)
        {
            return date.ToString("yyyyMMdd");
        }

    }

}
