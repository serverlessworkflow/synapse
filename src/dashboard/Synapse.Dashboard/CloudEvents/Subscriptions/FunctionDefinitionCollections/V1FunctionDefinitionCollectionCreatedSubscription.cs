﻿/*
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

using Neuroglia.Data.Flux;
using Neuroglia.Mapping;
using Synapse.Dashboard.Pages.Resources.Collections;
using Synapse.Integration.Events.FunctionDefinitionCollections;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.CloudEvents.Subscriptions.FunctionDefinitionCollections
{
    class V1FunctionDefinitionCollectionCreatedSubscription
        : CloudEventSubscription<V1FunctionDefinitionCollection, V1FunctionDefinitionCollectionCreatedIntegrationEvent>
    {

        public V1FunctionDefinitionCollectionCreatedSubscription(ILoggerFactory loggerFactory, IDispatcher dispatcher, IMapper mapper)
            : base(loggerFactory, dispatcher, mapper)
        {

        }

        public override void Handle(V1FunctionDefinitionCollectionCreatedIntegrationEvent e)
        {
            try
            {
                this.Dispatcher.Dispatch(new AddV1FunctionDefinitionCollection(new() 
                {
                    Id = e.AggregateId,
                    CreatedAt = e.CreatedAt,
                    LastModified = e.CreatedAt,
                    Name = e.Name,
                    Version = e.Version,
                    Description = e.Description,
                    Functions = e.Functions
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }

    class V1FunctionDefinitionCollectionDeletedSubscription
        : CloudEventSubscription<V1FunctionDefinitionCollection, V1FunctionDefinitionCollectionDeletedIntegrationEvent>
    {

        public V1FunctionDefinitionCollectionDeletedSubscription(ILoggerFactory loggerFactory, IDispatcher dispatcher, IMapper mapper)
            : base(loggerFactory, dispatcher, mapper)
        {

        }

        public override void Handle(V1FunctionDefinitionCollectionDeletedIntegrationEvent e)
        {
            try
            {
                this.Dispatcher.Dispatch(new RemoveV1FunctionDefinitionCollection(e.AggregateId));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }

}
