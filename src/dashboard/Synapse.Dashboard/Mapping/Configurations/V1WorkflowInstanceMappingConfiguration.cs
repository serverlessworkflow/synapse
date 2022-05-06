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
using Synapse.Integration.Events.WorkflowInstances;
using Synapse.Integration.Models;
using System.Collections.ObjectModel;

namespace Synapse.Dashboard.Mapping.Configurations
{
    internal class V1WorkflowInstanceMappingConfiguration
        : IMappingConfiguration<V1WorkflowInstance, V1WorkflowInstanceCreatedIntegrationEvent>,
        IMappingConfiguration<V1WorkflowInstanceCreatedIntegrationEvent, V1WorkflowInstance>
    {

        void IMappingConfiguration<V1WorkflowInstance, V1WorkflowInstanceCreatedIntegrationEvent>.Configure(IMappingExpression<V1WorkflowInstance, V1WorkflowInstanceCreatedIntegrationEvent> mapping)
        {
            mapping.ForMember(e => e.AggregateId, options => options.MapFrom(instance => instance.Id));
        }

        void IMappingConfiguration<V1WorkflowInstanceCreatedIntegrationEvent, V1WorkflowInstance>.Configure(IMappingExpression<V1WorkflowInstanceCreatedIntegrationEvent, V1WorkflowInstance> mapping)
        {
            mapping.ForMember(instance => instance.Id, options => options.MapFrom(e => e.AggregateId));
            mapping.ForMember(instance => instance.Activities, options => options.MapFrom(e => new Collection<V1WorkflowActivity>()));
        }

    }
}
