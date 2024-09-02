// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Synapse.Resources;

namespace Synapse;

/// <summary>
/// Defines extensions for <see cref="CorrelationContext"/>s
/// </summary>
public static class CorrelationContextExtensions
{

    /// <summary>
    /// Determines whether or not the <see cref="CorrelationContext"/> satisfies the filter(s) defined by specified <see cref="EventConsumptionStrategyDefinition"/>
    /// </summary>
    /// <param name="context">The <see cref="CorrelationContext"/> to check</param>
    /// <param name="eventConsumptionStrategy">The <see cref="EventConsumptionStrategyDefinition"/> that configures the <see cref="EventFilterDefinition"/>s to satisfy</param>
    /// <returns>A boolean indicating whether or not the <see cref="CorrelationContext"/> satisfies the filter(s) defined by specified <see cref="EventConsumptionStrategyDefinition"/></returns>
    public static bool Satisfies(this CorrelationContext context, EventConsumptionStrategyDefinition eventConsumptionStrategy)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(eventConsumptionStrategy);
        if (eventConsumptionStrategy.All != null && context.Events.Count == eventConsumptionStrategy.All.Count) return true;
        else if(eventConsumptionStrategy.Any != null && context.Events.Count > 0) return true;
        else if (eventConsumptionStrategy.One != null && context.Events.Count > 0) return true;
        return false;
    }

}