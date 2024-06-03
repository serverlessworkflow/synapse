// Copyright © 2024-Present Neuroglia SRL. All rights reserved.
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

using Neuroglia.Data.Expressions;
using Neuroglia.Data.Expressions.Services;
using Neuroglia.Eventing.CloudEvents;
using Neuroglia.Eventing.CloudEvents.Infrastructure.Services;
using ServerlessWorkflow.Sdk.Models;
using System.Text.RegularExpressions;

namespace Synapse.Operator.Services;

/// <summary>
/// Represents the service used to handle a specific <see cref="Resources.Correlation"/>
/// </summary>
/// <param name="logger">The service used to perform logging</param>
/// <param name="resources">The service used to manage resources</param>
/// <param name="cloudEventBus">The service used to stream input and output <see cref="CloudEvent"/>s</param>
/// <param name="expressionEvaluator">The service used to evaluate runtime expressions</param>
/// <param name="correlation">The resource of the correlation to handle</param>
public class CorrelationHandler(ILogger<CorrelationHandler> logger, IResourceRepository resources, ICloudEventBus cloudEventBus, IExpressionEvaluator expressionEvaluator, Correlation correlation)
    : IDisposable, IAsyncDisposable
{

    bool _disposed;

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; } = logger;

    /// <summary>
    /// Gets the service used to manage resources
    /// </summary>
    protected IResourceRepository Resources { get; } = resources;

    /// <summary>
    /// Gets the service used to stream input and output <see cref="CloudEvent"/>s
    /// </summary>
    protected ICloudEventBus CloudEventBus { get; } = cloudEventBus;

    /// <summary>
    /// Gets the service used to evaluate runtime expressions
    /// </summary>
    protected IExpressionEvaluator ExpressionEvaluator { get; } = expressionEvaluator;

    /// <summary>
    /// Gets the resource of the correlation to manage the scheduling of
    /// </summary>
    protected Correlation Correlation { get; } = correlation;

    /// <summary>
    /// Gets the handler's subscription
    /// </summary>
    protected IDisposable? Subscription { get; set; }

    /// <summary>
    /// Attempt to perform the defined correlation
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public virtual Task CorrelateAsync(CancellationToken cancellationToken = default)
    {
        this.Subscription = this.CloudEventBus.InputStream
            .ToAsyncEnumerable()
            .SelectAwait(async e => await this.TryCorrelateEventAsync(e, cancellationToken).ConfigureAwait(false))
            .ToObservable()
            .Subscribe();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles the correlation of the specified event
    /// </summary>
    /// <param name="e">The event to correlate</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task<bool> TryCorrelateEventAsync(CloudEvent e, ICorrelationContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(e);
        ArgumentNullException.ThrowIfNull(context);
        if (this.Correlation.Spec.Events.All != null) return await this.Correlation.Spec.Events.All.ToAsyncEnumerable().AllAwaitAsync(async f => await this.TryCorrelateEventAsync(f, e, context, cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);
        else if (this.Correlation.Spec.Events.Any != null) return await this.Correlation.Spec.Events.Any.ToAsyncEnumerable().AnyAwaitAsync(async f => await this.TryCorrelateEventAsync(f, e, context, cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);
        else if (this.Correlation.Spec.Events.One != null) return await this.TryCorrelateEventAsync(this.Correlation.Spec.Events.One, e, context, cancellationToken).ConfigureAwait(false);
        else throw new Exception("The correlation's event consumption strategy must be set");
    }

    /// <summary>
    /// Handles the correlation of the specified event
    /// </summary>
    /// <param name="e">The event filter used to determine whether or not to correlate the specified event</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task<bool> TryCorrelateEventAsync(EventFilterDefinition filter, CloudEvent e, ICorrelationContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(e);
        if (filter.With?.Count > 0)
        {
            foreach(var attribute in filter.With)
            {
                if (!e.TryGetAttribute(attribute.Key, out var value) || value == null) return false;
                var valueStr = attribute.Value.ToString();
                if (valueStr?.IsRuntimeExpression() == true && !await this.ExpressionEvaluator.EvaluateConditionAsync(valueStr, e, cancellationToken: cancellationToken)) return false;
                else if (!string.IsNullOrWhiteSpace(valueStr) && !Regex.IsMatch(value.ToString() ?? string.Empty, valueStr, RegexOptions.IgnoreCase)) return false;
            }
        }
        var correlationKeys = new Dictionary<string, string>();
        if (filter.Correlate?.Count > 0)
        {
            foreach (var keyDefinition in filter.Correlate)
            {
                var correlationTerm = (keyDefinition.Value.From.IsRuntimeExpression()
                     ? (await this.ExpressionEvaluator.EvaluateAsync<object>(keyDefinition.Value.From, e, cancellationToken: cancellationToken).ConfigureAwait(false))?.ToString()
                     : e.GetAttribute(keyDefinition.Value.From)?.ToString())
                     ?? string.Empty;
                if (!e.TryGetAttribute(keyDefinition.Value.From, out var value) || value == null) return false;
                if (string.IsNullOrWhiteSpace(keyDefinition.Value.Expect))
                {
                    if (context.Keys.TryGetValue(keyDefinition.Key, out var existingCorrelationTerm) && existingCorrelationTerm != correlationTerm) return false;
                }
                else
                {
                    if (keyDefinition.Value.Expect.IsRuntimeExpression() && !await this.ExpressionEvaluator.EvaluateAsync<bool>(keyDefinition.Value.Expect, correlationTerm, cancellationToken: cancellationToken).ConfigureAwait(false)) return false;
                    else if(!keyDefinition.Value.Expect.Equals(correlationTerm, StringComparison.OrdinalIgnoreCase)) return false;
                }
                correlationKeys[keyDefinition.Key] = correlationTerm;
            }
        }
        await context.AddCorrelatedEventAsync(e, correlationKeys, cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Disposes of the <see cref="CorrelationHandler"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="CorrelationHandler"/> is being disposed of</param>
    /// <returns>A new awaitable <see cref="ValueTask"/></returns>
    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (!this._disposed) return;
        this._disposed = true;
        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await this.DisposeAsync(true).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of the <see cref="CorrelationHandler"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="CorrelationHandler"/> is being disposed of</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this._disposed) return;

        this._disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

}
