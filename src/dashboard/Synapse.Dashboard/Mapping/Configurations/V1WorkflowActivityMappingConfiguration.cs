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
using AutoMapper;
using Neuroglia.Mapping;
using Synapse.Integration.Events.WorkflowActivities;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Mapping.Configurations
{
    internal class V1WorkflowActivityMappingConfiguration
        : IMappingConfiguration<V1WorkflowActivity, V1WorkflowActivityCreatedIntegrationEvent>,
        IMappingConfiguration<V1WorkflowActivityCreatedIntegrationEvent, V1WorkflowActivity>
    {

        void IMappingConfiguration<V1WorkflowActivity, V1WorkflowActivityCreatedIntegrationEvent>.Configure(IMappingExpression<V1WorkflowActivity, V1WorkflowActivityCreatedIntegrationEvent> mapping)
        {
            mapping.ForMember(e => e.AggregateId, options => options.MapFrom(activity => activity.Id));
        }

        void IMappingConfiguration<V1WorkflowActivityCreatedIntegrationEvent, V1WorkflowActivity>.Configure(IMappingExpression<V1WorkflowActivityCreatedIntegrationEvent, V1WorkflowActivity> mapping)
        {
            mapping.ForMember(activity => activity.Id, options => options.MapFrom(e => e.AggregateId));
        }

    }
}
