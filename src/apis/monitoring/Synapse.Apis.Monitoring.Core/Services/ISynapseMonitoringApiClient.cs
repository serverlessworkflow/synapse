﻿/*
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

using Synapse.Integration.Models;
using System.ServiceModel;

namespace Synapse.Apis.Monitoring
{

    /// <summary>
    /// Defines the fundamentals of a Synapse Monitoring API client
    /// </summary>
    [ServiceContract]
    public interface ISynapseMonitoringApiClient
    {

        /// <summary>
        /// Handles the specified <see cref="V1Event"/>
        /// </summary>
        /// <param name="e">The <see cref="V1Event"/> to handle</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        [OperationContract]
        Task PublishIntegrationEvent(V1Event e);

    }

}
