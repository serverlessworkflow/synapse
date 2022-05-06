/*
 * Copyright © 2022-Present The Synapse Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */
namespace Synapse.Dashboard
{
    /// <summary>
    /// Represents a sleep state node
    /// </summary>
    public class SleepNodeViewModel
        : WorkflowNodeViewModel
    {

        /// <summary>
        /// Initializes a new <see cref="SleepNodeViewModel"/>
        /// </summary>
        /// <param name="data"></param>
        public SleepNodeViewModel(TimeSpan delay)
            : base(System.Xml.XmlConvert.ToString(delay), "sleep-node")
        {
            this.Delay = delay;
            this.ComponentType = typeof(SleepNodeTemplate);
        }

        public TimeSpan Delay { get; set; }

    }
    

}
