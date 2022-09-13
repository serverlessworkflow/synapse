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

using CloudNative.CloudEvents;

namespace Synapse
{

    /// <summary>
    /// Exposes constants about <see cref="CloudEvent"/>s produced by Synapse
    /// </summary>
    public static class CloudEvents
    {

        /// <summary>
        /// Gets the source uri of Synapse <see cref="CloudEvent"/>s
        /// </summary>
        public static Uri Source = new("https://synapse.io/runtime/events");

        /// <summary>
        /// Gets the <see cref="CloudEvent"/> path for the specified event CLR type
        /// </summary>
        /// <param name="eventType">The event type to get the <see cref="CloudEvent"/> path of</param>
        /// <param name="aggregateType">The type of aggregate that has produced the event</param>
        /// <returns>The <see cref="CloudEvent"/> path</returns>
        public static string LogicalPathOf(Type eventType, Type aggregateType)
        {
            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType));
            if (aggregateType == null)
                throw new ArgumentNullException(nameof(aggregateType));
            var aggregateName = AggregateHelper.NameOf(aggregateType);
            var version = AggregateHelper.VersionOf(aggregateType);
            var actionName = eventType.Name
                .Replace(aggregateName, string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("v1", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("DomainEvent", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("IntegrationEvent", string.Empty, StringComparison.OrdinalIgnoreCase)
                .ToLower();
            return $"/{aggregateName}/{version}/{actionName}";
        }

        /// <summary>
        /// Gets the <see cref="CloudEvent"/> type for the specified event CLR type
        /// </summary>
        /// <param name="eventType">The event type to get the <see cref="CloudEvent"/> type of</param>
        /// <param name="aggregateType">The type of aggregate that has produced the event</param>
        /// <returns>The <see cref="CloudEvent"/> type</returns>
        public static string TypeOf(Type eventType, Type aggregateType)
        {
            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType));
            if (aggregateType == null)
                throw new ArgumentNullException(nameof(aggregateType));
            return $"io.synapse{LogicalPathOf(eventType, aggregateType)}";
        }

        /// <summary>
        /// Gets the <see cref="CloudEvent"/> schema <see cref="Uri"/> for the specified event CLR type
        /// </summary>
        /// <param name="eventType">The event type to get the <see cref="CloudEvent"/> schema <see cref="Uri"/> for</param>
        /// <param name="aggregateType">The type of aggregate that has produced the event</param>
        /// <returns>The <see cref="CloudEvent"/> schema <see cref="Uri"/></returns>
        public static Uri SchemaOf(Type eventType, Type aggregateType)
        {
            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType));
            if (aggregateType == null)
                throw new ArgumentNullException(nameof(aggregateType));
            var aggregateName = AggregateHelper.NameOf(aggregateType);
            var version = AggregateHelper.VersionOf(aggregateType);
            var actionName = eventType.Name
                .Replace(aggregateName, string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("v1", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("DomainEvent", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("IntegrationEvent", string.Empty, StringComparison.OrdinalIgnoreCase)
                .ToLower();
            return new($"https://synapse.io/events{LogicalPathOf(eventType, aggregateType)}");
        }

    }

}
