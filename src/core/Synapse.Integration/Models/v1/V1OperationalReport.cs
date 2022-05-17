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

    [ReadModel]
    [DataContract]
    public class V1OperationalReport
        : IIdentifiable<string>
    {

        protected V1OperationalReport()
        {

        }

        public V1OperationalReport(DateTime date)
        {
            this.Id = GetIdFor(date);
            this.Date = date;
        }

        [DataMember(Order = 1)]
        public string Id { get; set; }

        object IIdentifiable.Id => this.Id;


        [DataMember(Order = 2)]
        public virtual DateTime Date { get; set; }


        [DataMember(Order = 3)]
        public virtual long TotalDefinitions { get; set; }


        [DataMember(Order = 4)]
        public virtual long TotalDefinitionVersions { get; set; }


        [DataMember(Order = 5)]
        public virtual long TotalInstances { get; set; }


        [DataMember(Order = 6)]
        public virtual long RunningInstances { get; set; }


        [DataMember(Order = 7)]
        public virtual long ExecutedInstances { get; set; }


        [DataMember(Order = 8)]
        public virtual long CompletedInstances { get; set; }


        [DataMember(Order = 9)]
        public virtual long FaultedInstances { get; set; }


        [DataMember(Order = 10)]
        public virtual long CancelledInstances { get; set; }


        [DataMember(Order = 11)]
        public virtual long TotalActivities { get; set; }


        [DataMember(Order = 12)]
        public virtual long RunningActivities { get; set; }


        [DataMember(Order = 13)]
        public virtual long ExecutedActivities { get; set; }


        [DataMember(Order = 14)]
        public virtual long CompletedActivities { get; set; }


        [DataMember(Order = 15)]
        public virtual long FaultedActivities { get; set; }


        [DataMember(Order = 16)]
        public virtual long CancelledActivities { get; set; }


        [DataMember(Order = 17)]
        public virtual long SkippedActivities { get; set; }

        public static string GetIdFor(DateTime date)
        {
            return date.ToString("yyyyMMdd");
        }

    }

}
