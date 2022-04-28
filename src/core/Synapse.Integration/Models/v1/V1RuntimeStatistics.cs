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
    public class V1RuntimeStatistics
        : EntityDto<string>
    {

        public virtual DateOnly Date { get; set; }

        public virtual long TotalDefinitions { get; set; }

        public virtual long TotalDefinitionVersions { get; set; }

        public virtual long TotalInstances { get; set; }

        public virtual long RunningInstances { get; set; }

        public virtual long ExecutedInstances { get; set; }

        public virtual long CompletedInstances { get; set; }

        public virtual long FaultedInstances { get; set; }

        public virtual long CancelledInstances { get; set; }

        public virtual long TotalActivities { get; set; }

        public virtual long RunningActivities { get; set; }

        public virtual long ExecutedActivities { get; set; }

        public virtual long CompletedActivities { get; set; }

        public virtual long FaultedActivities { get; set; }

        public virtual long CancelledActivities { get; set; }

        public virtual long SkippedActivities { get; set; }

    }

}
