/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0(the "License");
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
 */

namespace Synapse.Integration.Events.WorkflowActivities
{

    public partial class V1WorkflowActivityCompletedIntegrationEvent
		: IV1WorkflowActivityIntegrationEvent
	{

		/// <summary>
		/// Initializes a new <see cref="V1WorkflowActivityCompletedIntegrationEvent"/>
		/// </summary>
		public V1WorkflowActivityCompletedIntegrationEvent()
		{
			
		}

		/// <summary>
		/// Initializes a new <see cref="V1WorkflowActivityCompletedIntegrationEvent"/>
		/// </summary>
		/// <param name="id">The id of the completed workflow activity</param>
		/// <param name="output">The output of the completed workflow activity</param>
		public V1WorkflowActivityCompletedIntegrationEvent(string id, object output)
        {
			this.AggregateId = id;
			var outputValue = output as Any;
			if (outputValue == null
				&& output != null)
				outputValue = Any.FromObject(output);
			this.Output = outputValue;
        }

    }

}
