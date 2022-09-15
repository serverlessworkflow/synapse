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

using Neuroglia.Data.Flux;
using Neuroglia.Mapping;
using Synapse.Integration.Events.Workflows;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.CloudEvents.Subscriptions.Workflows
{

    class V1WorkflowCreatedSubscription
        : CloudEventSubscription<V1Workflow, V1WorkflowCreatedIntegrationEvent>
    {

        public V1WorkflowCreatedSubscription(ILoggerFactory loggerFactory, IDispatcher dispatcher, IMapper mapper) 
            : base(loggerFactory, dispatcher, mapper)
        {

        }

        public override void Handle(V1WorkflowCreatedIntegrationEvent e)
        {
            try
            {
                this.Dispatcher.Dispatch(new AddV1Workflow(this.Mapper.Map<V1Workflow>(e)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }

}
