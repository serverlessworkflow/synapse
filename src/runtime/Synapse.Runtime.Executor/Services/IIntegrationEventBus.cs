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

using System.Reactive.Subjects;

namespace Synapse.Runtime.Executor.Services
{

    /// <summary>
    /// Defines the fundamentals of a service used to publish and subscribe to events
    /// </summary>
    public interface IIntegrationEventBus
        : IDisposable
    {

        /// <summary>
        /// Gets the outbound <see cref="V1Event"/> stream
        /// </summary>
        ISubject<V1Event> OutboundStream { get; }

        /// <summary>
        /// Gets the inbound <see cref="V1Event"/> stream
        /// </summary>
        ISubject<V1Event> InboundStream { get; }

    }

}
