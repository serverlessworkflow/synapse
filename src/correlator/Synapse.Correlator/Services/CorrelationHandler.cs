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

namespace Synapse.Correlator.Services;

/// <summary>
/// Represents the service used to handle a specific <see cref="Resources.Correlation"/>
/// </summary>
/// <param name="logger">The service used to perform logging</param>
/// <param name="resources">The service used to manage resources</param>
/// <param name="cloudEventBus">The service used to stream input and output <see cref="CloudEvent"/>s</param>
/// <param name="expressionEvaluator">The service used to evaluate runtime expressions</param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
/// <param name="correlation">The resource of the correlation to handle</param>
public class CorrelationHandler(ILogger<CorrelationHandler> logger, IResourceRepository resources, ICloudEventBus cloudEventBus, IExpressionEvaluator expressionEvaluator, IJsonSerializer serializer, IResourceMonitor<Correlation> correlation)
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
    /// Gets the service used to serialize/deserialize objects to/from JSON
    /// </summary>
    protected IJsonSerializer Serializer { get; } = serializer;

    /// <summary>
    /// Gets the resource of the correlation to manage the scheduling of
    /// </summary>
    protected IResourceMonitor<Correlation> Correlation { get; } = correlation;

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
        this.Subscription = this.CloudEventBus.InputStream.SubscribeAsync(async e => await this.CorrelateEventAsync(e, cancellationToken).ConfigureAwait(false));
        return Task.CompletedTask;
    }

    /// <summary>
    /// Attempts to correlate the specified input event
    /// </summary>
    /// <param name="e">The event to attempt to correlate</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task CorrelateEventAsync(CloudEvent e, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(e);
        try 
        {
            this.Correlation.Resource.Status ??= new();
            Dictionary<int, EventFilterDefinition>? matchingFilters = null;
            if (this.Correlation.Resource.Spec.Events.All != null)
            {
                matchingFilters = (await this.Correlation.Resource.Spec.Events.All!.ToAsyncEnumerable().Select((Filter, Index) => new KeyValuePair<int, EventFilterDefinition>(Index, Filter)).WhereAwait(async f => await this.TryFilterEventAsync(f.Value, e, cancellationToken).ConfigureAwait(false)).ToListAsync(cancellationToken).ConfigureAwait(false)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            else if (this.Correlation.Resource.Spec.Events.Any != null)
            {
                matchingFilters = (await this.Correlation.Resource.Spec.Events.Any!.ToAsyncEnumerable().Select((Filter, Index) => new KeyValuePair<int, EventFilterDefinition>(Index, Filter)).WhereAwait(async f => await this.TryFilterEventAsync(f.Value, e, cancellationToken).ConfigureAwait(false)).ToListAsync(cancellationToken).ConfigureAwait(false)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            else if (this.Correlation.Resource.Spec.Events.One != null)
            {
                if (await this.TryFilterEventAsync(this.Correlation.Resource.Spec.Events.One!, e, cancellationToken).ConfigureAwait(false)) matchingFilters = new() { { 0, this.Correlation.Resource.Spec.Events.One! } };
            }
            else throw new Exception("The correlation's event consumption strategy must be set");
            if (matchingFilters == null || matchingFilters.Count < 1) return;
            var contextCorrelationResults = await this.Correlation.Resource.Status.Contexts.ToAsyncEnumerable()
                .Where(c => c.Status == CorrelationContextStatus.Active)
                .SelectAwait(async c => await this.TryCorrelateToContextAsync(c, e, matchingFilters, cancellationToken).ConfigureAwait(false))
                .Where(c => c.Succeeded)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            var filter = matchingFilters.First();
            var (Succeeded, CorrelationKeys) = await this.TryExtractCorrelationKeysAsync(e, filter.Value.Correlate, cancellationToken).ConfigureAwait(false);
            if (!Succeeded) return;
            CorrelationContext? context;
            switch (this.Correlation.Resource.Spec.Lifetime)
            {
                case CorrelationLifetime.Ephemeral:
                    var contextCorrelationResult = contextCorrelationResults.SingleOrDefault();
                    if (contextCorrelationResult == default)
                    {
                        this.Logger.LogInformation("Failed to find a matching correlation context");
                        if (this.Correlation.Resource.Status.Contexts.Count != 0) throw new Exception("Failed to correlate event"); //should not happen
                        this.Logger.LogInformation("Creating a new correlation context...");
                        context = new CorrelationContext()
                        {
                            Id = Guid.NewGuid().ToString("N")[..15],
                            Events = [new(filter.Key, e)],
                            Keys = CorrelationKeys == null ? new() : new(CorrelationKeys)
                        };
                        this.Logger.LogInformation("Correlation context with id '{contextId}' successfully created", context.Id);
                        this.Logger.LogInformation("Event successfully correlated to context with id '{contextId}'", context.Id);
                    }
                    else
                    {
                        context = contextCorrelationResult.Context.Clone()!;
                        if (CorrelationKeys != null)
                        {
                            foreach (var kvp in CorrelationKeys)
                            {
                                if (context.Keys.TryGetValue(kvp.Key, out var value) && !string.IsNullOrWhiteSpace(value)) continue;
                                context.Keys[kvp.Key] = kvp.Value;
                            }
                        }
                        context.Events[contextCorrelationResult.FilterIndex!] = e;
                    }
                    await this.CreateOrUpdateContextAsync(context, cancellationToken).ConfigureAwait(false);
                    break;
                case CorrelationLifetime.Durable:
                    if (contextCorrelationResults.Count < 1)
                    {
                        this.Logger.LogInformation("Failed to find a matching correlation context");
                        this.Logger.LogInformation("Creating a new correlation context...");
                        context = new CorrelationContext()
                        {
                            Id = Guid.NewGuid().ToString("N")[..15],
                            Events = [new(filter.Key, e)],
                            Keys = CorrelationKeys == null ? new() : new(CorrelationKeys)
                        };
                        await this.CreateOrUpdateContextAsync(context, cancellationToken).ConfigureAwait(false);
                        this.Logger.LogInformation("Correlation context with id '{contextId}' successfully created", context.Id);
                        this.Logger.LogInformation("Event successfully correlated to context with id '{contextId}'", context.Id);
                    }
                    else
                    {
                        this.Logger.LogInformation("Found {matchingContextCount} matching correlation contexts", contextCorrelationResults.Count);
                        foreach (var result in contextCorrelationResults)
                        {
                            context = result.Context.Clone()!;
                            if (result.CorrelationKeys != null)
                            {
                                foreach (var kvp in result.CorrelationKeys)
                                {
                                    if (context.Keys.TryGetValue(kvp.Key, out var value) && !string.IsNullOrWhiteSpace(value)) continue;
                                    context.Keys[kvp.Key] = kvp.Value;
                                }
                            }
                            context.Events[result.FilterIndex!] = e;
                            await this.CreateOrUpdateContextAsync(context, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    break;
                default: throw new NotSupportedException($"The specified correlation lifetime '{this.Correlation.Resource.Spec.Lifetime}' is not supported");
            }
        }
        catch(Exception ex)
        {
            this.Logger.LogError("An error occurred while attempting to correlate the cloud event with id '{eventId}': {ex}", e.Id, ex.Message);
        }   
    }

    /// <summary>
    /// Attempts to filter the specified <see cref="CloudEvent"/>
    /// </summary>
    /// <param name="filter">The filter to use</param>
    /// <param name="e">The event to filter</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task<bool> TryFilterEventAsync(EventFilterDefinition filter, CloudEvent e, CancellationToken cancellationToken)
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
        if (filter.Correlate?.Count > 0)
        {
            foreach (var keyDefinition in filter.Correlate.Where(k => !k.Value.From.IsRuntimeExpression()))
            {
                if (!e.TryGetAttribute(keyDefinition.Value.From, out _)) return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Attempts to correlate a <see cref="CorrelationContext"/> to the specified <see cref="CloudEvent"/>
    /// </summary>
    /// <param name="context">The <see cref="CorrelationContext"/> to attempt to correlate the specified <see cref="CloudEvent"/> to</param>
    /// <param name="e">The <see cref="CloudEvent"/> to attempt correlating</param>
    /// <param name="filters">A list containing the filters to match</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A boolean indicating whether or not the specified context could be correlated</returns>
    protected virtual async Task<(CorrelationContext Context, bool Succeeded, IDictionary<string, string>? CorrelationKeys, int? FilterIndex)> TryCorrelateToContextAsync(CorrelationContext context, CloudEvent e, IDictionary<int, EventFilterDefinition> filters, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(e);
        ArgumentNullException.ThrowIfNull(filters);
        var correlationKeys = new Dictionary<string, string>();
        foreach (var filter in filters.Where(f => !context.Events.ContainsKey(f.Key)))
        {
            if (filter.Value.Correlate?.Count > 0)
            {
                foreach (var keyDefinition in filter.Value.Correlate)
                {
                    var correlationTerm = (keyDefinition.Value.From.IsRuntimeExpression()
                         ? (await this.ExpressionEvaluator.EvaluateAsync<object>(keyDefinition.Value.From, e, cancellationToken: cancellationToken).ConfigureAwait(false))?.ToString()
                         : e.GetAttribute(keyDefinition.Value.From)?.ToString())
                         ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(keyDefinition.Value.Expect))
                    {
                        if (context.Keys.TryGetValue(keyDefinition.Key, out var existingCorrelationTerm) && existingCorrelationTerm != correlationTerm) continue;
                    }
                    else
                    {
                        if (keyDefinition.Value.Expect.IsRuntimeExpression() && !await this.ExpressionEvaluator.EvaluateAsync<bool>(keyDefinition.Value.Expect, correlationTerm, cancellationToken: cancellationToken).ConfigureAwait(false)) continue;
                        else if (!keyDefinition.Value.Expect.Equals(correlationTerm, StringComparison.OrdinalIgnoreCase)) continue;
                    }
                    correlationKeys[keyDefinition.Key] = correlationTerm;
                    return (context, true, correlationKeys, filter.Key);
                }
                break;
            }
        }
        return (context, false, null, null);
    }

    /// <summary>
    /// Attempts extracting the specified correlation keys from the specified cloud event
    /// </summary>
    /// <param name="e">The cloud event to extract correlation keys from</param>
    /// <param name="keyDefinitions">A name/definition mapping of the correlation keys to extract</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>An object used to describe the result of the operation</returns>
    protected virtual async Task<(bool Succeeded, IDictionary<string, string>? CorrelationKeys)> TryExtractCorrelationKeysAsync(CloudEvent e, IDictionary<string, CorrelationDefinition>? keyDefinitions, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(e);
        var correlationKeys = new Dictionary<string, string>();
        if (keyDefinitions == null || keyDefinitions.Count < 1) return (true, correlationKeys);
        foreach (var keyDefinition in keyDefinitions)
        {
            var correlationTerm = (keyDefinition.Value.From.IsRuntimeExpression()
                ? (await this.ExpressionEvaluator.EvaluateAsync<object>(keyDefinition.Value.From, e, cancellationToken: cancellationToken).ConfigureAwait(false))?.ToString()
                : e.GetAttribute(keyDefinition.Value.From)?.ToString())
                ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(keyDefinition.Value.Expect))
            {
                if (keyDefinition.Value.Expect.IsRuntimeExpression())
                {
                    if (!await this.ExpressionEvaluator.EvaluateAsync<bool>(keyDefinition.Value.Expect, correlationTerm, cancellationToken: cancellationToken).ConfigureAwait(false)) return (false, null);
                }
                else if (!keyDefinition.Value.Expect.Equals(correlationTerm, StringComparison.OrdinalIgnoreCase)) return (false, null);
            }
            correlationKeys[keyDefinition.Key] = correlationTerm;
        }
        return (true, correlationKeys);
    }

    /// <summary>
    /// Creates or updates the specified <see cref="CorrelationContext"/>
    /// </summary>
    /// <param name="context">The <see cref="CorrelationContext"/> to create or update</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task CreateOrUpdateContextAsync(CorrelationContext context, CancellationToken cancellationToken =default)
    {
        ArgumentNullException.ThrowIfNull(context);
        JsonPatch patch;
        var originalResource = this.Correlation.Resource.Clone()!;
        var updatedResource = this.Correlation.Resource.Clone()!;
        updatedResource.Status ??= new();
        updatedResource.Status.LastModified = DateTimeOffset.Now;
        var existingContext = updatedResource.Status.Contexts.FirstOrDefault(c => c.Id == context.Id);
        if (existingContext == null) 
        {
            updatedResource.Status.Contexts.Add(context);
        }
        else
        {
            var index = updatedResource.Status.Contexts.IndexOf(existingContext);
            updatedResource.Status.Contexts.Remove(existingContext);
            updatedResource.Status.Contexts.Insert(index, context);
            if (context.Satisfies(updatedResource.Spec.Events))
            {
                context.Status = CorrelationContextStatus.Completed;
                if (updatedResource.Spec.Lifetime == CorrelationLifetime.Ephemeral) updatedResource.Status.Phase = CorrelationStatusPhase.Completed;
                WorkflowInstance? workflowInstance;
                switch (this.Correlation.Resource.Spec.Outcome.Type)
                {
                    case CorrelationOutcomeType.Correlate:
                        var qualifiedName = this.Correlation.Resource.Spec.Outcome.Correlate!.Instance.Trim().Split('.', StringSplitOptions.RemoveEmptyEntries);
                        var name = qualifiedName[0];
                        var @namespace = qualifiedName[1];
                        workflowInstance = await this.Resources.GetAsync<WorkflowInstance>(name, @namespace, cancellationToken).ConfigureAwait(false) ?? throw new ProblemDetailsException(ResourceProblemDetails.ResourceNotFound(new ResourceReference<WorkflowInstance>(name, @namespace)));
                        var updatedWorkflowInstance = workflowInstance.Clone()!;
                        updatedWorkflowInstance.Status ??= new();
                        updatedWorkflowInstance.Status.Correlations ??= [];
                        updatedWorkflowInstance.Status.Correlations[this.Correlation.Resource.Spec.Outcome.Correlate!.Task] = context;
                        patch = JsonPatchUtility.CreateJsonPatchFromDiff(workflowInstance, updatedWorkflowInstance);
                        await this.Resources.PatchStatusAsync<WorkflowInstance>(new(PatchType.JsonPatch, patch), workflowInstance.GetName(), workflowInstance.GetNamespace(), false, cancellationToken).ConfigureAwait(false);
                        break;
                    case CorrelationOutcomeType.Start:
                        var input = this.Correlation.Resource.Spec.Outcome.Start!.Input == null ? [] : await this.ExpressionEvaluator.EvaluateAsync<EquatableDictionary<string, object>>(this.Correlation.Resource.Spec.Outcome.Start!.Input!, context, cancellationToken: cancellationToken).ConfigureAwait(false);
                        workflowInstance = new()
                        {
                            Metadata = new()
                            {
                                Namespace = this.Correlation.Resource.Spec.Outcome.Start!.Workflow.Namespace,
                                Name = $"{this.Correlation.Resource.Spec.Outcome.Start!.Workflow.Namespace}-"
                            },
                            Spec = new()
                            {
                                Definition = this.Correlation.Resource.Spec.Outcome.Start!.Workflow,
                                Input = input
                            }
                        };
                        await this.Resources.AddAsync(workflowInstance, false, cancellationToken).ConfigureAwait(false);
                        break;
                    default: throw new NotSupportedException($"The specified correlation outcome type is not supported '{this.Correlation.Resource.Spec.Outcome.Type}'");
                }
            }
        }
        patch = JsonPatchUtility.CreateJsonPatchFromDiff(originalResource, updatedResource);
        await this.Resources.PatchStatusAsync<Correlation>(new(PatchType.JsonPatch, patch), originalResource.GetName(), originalResource.GetNamespace(), false, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes of the <see cref="CorrelationHandler"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="CorrelationHandler"/> is being disposed of</param>
    /// <returns>A new awaitable <see cref="ValueTask"/></returns>
    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (!disposing || this._disposed) return;
        await this.Correlation.DisposeAsync().ConfigureAwait(false);
        this.Subscription?.Dispose();
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
        if (!disposing || this._disposed) return;
        this.Correlation.Dispose();
        this.Subscription?.Dispose();
        this._disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

}