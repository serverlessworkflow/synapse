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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neuroglia.Reactive;
using System.Reactive.Subjects;

namespace Synapse.Dashboard.StateManagement;

/// <summary>
/// Represents the default implementation of the <see cref="IStore"/> interface
/// </summary>
public class Store
    : IStore
{

    /// <summary>
    /// Initializes a new <see cref="IStore"/>
    /// </summary>
    /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
    /// <param name="logger">The service used to perform logging</param>
    /// <param name="dispatcher">The service used to dispatch actions</param>
    public Store(IServiceProvider serviceProvider, ILogger<Store> logger, IDispatcher dispatcher)
    {
        this.ServiceProvider = serviceProvider;
        this.Logger = logger;
        this.Dispatcher = dispatcher;
        this.Dispatcher.SubscribeAsync(this.DispatchAsync);
    }

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Gets the service used to dispatch actions
    /// </summary>
    protected IDispatcher Dispatcher { get; }

    /// <summary>
    /// Gets the <see cref="BehaviorSubject{T}"/> used to stream actions
    /// </summary>
    protected BehaviorSubject<object> Stream { get; } = new(default!);

    /// <summary>
    /// Gets a <see cref="List{T}"/> containing the <see cref="Store"/>'s <see cref="IFeature"/>s
    /// </summary>
    protected List<IFeature> Features { get; } = [];

    /// <summary>
    /// Gets a <see cref="List{T}"/> containing the types of the <see cref="Store"/>'s <see cref="IMiddleware"/>s
    /// </summary>
    protected List<Type> Middlewares { get; } = [];

    /// <summary>
    /// Gets a <see cref="List{T}"/> containing the <see cref="Store"/>'s <see cref="IEffect"/>s
    /// </summary>
    protected List<IEffect> Effects { get; } = [];

    /// <inheritdoc/>
    public virtual object State => this.Features.ToDictionary(f => f.State.GetType().Name, f => f.State);

    IDisposable IObservable<object>.Subscribe(IObserver<object> observer)
    {
        return observer == null ? throw new ArgumentNullException(nameof(observer)) : this.Stream.Subscribe(observer);
    }

    /// <inheritdoc/>
    public virtual void AddFeature<TState>(IFeature<TState> feature)
    {
        ArgumentNullException.ThrowIfNull(feature);
        this.Features.Add(feature);
        feature.Subscribe(this.OnNextState);
    }

    /// <inheritdoc/>
    public virtual void AddMiddleware(Type middlewareType)
    {
        this.Middlewares.Add(middlewareType);
    }

    /// <inheritdoc/>
    public virtual void AddEffect(IEffect effect)
    {
        ArgumentNullException.ThrowIfNull(effect);
        this.Effects.Add(effect);
    }

    /// <inheritdoc/>
    public virtual IFeature<TState> GetFeature<TState>()
    {
        return (IFeature<TState>)this.Features.First(f => f.State.GetType() == typeof(TState));
    }

    /// <summary>
    /// Dispatches the specified action
    /// </summary>
    /// <param name="action">The action to dispatch</param>
    protected virtual async Task DispatchAsync(object action)
    {
        try
        {
            DispatchDelegate pipelineBuilder(DispatchDelegate reducer) => this.Middlewares
                .AsEnumerable()
                .Reverse()
                .Aggregate(reducer, (next, type) => this.InstantiateMiddleware(type, next).InvokeAsync);
            var context = new ActionContext(this.ServiceProvider, this, action);
            foreach (var feature in this.Features
                .Where(f => f.ShouldReduceStateFor(action)))
            {
                await feature.ReduceStateAsync(context, pipelineBuilder);
            }
            this.OnApplyEffects(action);
        }
        catch(Exception ex)
        {
            this.Logger.LogError("An error occurred while dispatching an action of type '{actionType}': {ex}", action.GetType().Name, ex.ToString());
            throw;
        }
    }

    /// <summary>
    /// Creates a new instance of the specified <see cref="IMiddleware"/>
    /// </summary>
    /// <param name="type">The type of <see cref="IMiddleware"/> to instantiate</param>
    /// <param name="next">The next <see cref="DispatchDelegate"/> in the pipeline</param>
    /// <returns>A new <see cref="IMiddleware"/></returns>
    protected virtual IMiddleware InstantiateMiddleware(Type type, DispatchDelegate next)
    {
        var constructor = type.GetConstructor([]);
        if (constructor != null) return (IMiddleware)constructor.Invoke([]);
        var parameters = new List<object>(1);
        constructor = type.GetConstructors().First();
        if (constructor.GetParameters().Any(p => p.ParameterType == typeof(DispatchDelegate))) parameters.Add(next);
        return (IMiddleware)ActivatorUtilities.CreateInstance(this.ServiceProvider, type, [.. parameters]);
    }

    /// <summary>
    /// Applies <see cref="IEffect"/>s
    /// </summary>
    /// <param name="action">The action to apply <see cref="IEffect"/>s to</param>
    protected virtual void OnApplyEffects(object action)
    {
        var applyEffectTasks = new List<Task>();
        foreach (var effect in this.Effects) applyEffectTasks.Add(effect.ApplyAsync(action, new EffectContext(this.ServiceProvider, this.Dispatcher)));
        var exceptions = new List<Exception>();
        Task.Run(async () =>
        {
            try
            {
                await Task.WhenAll(applyEffectTasks);
            }
            catch(Exception ex)
            {
                exceptions.Add(ex);
            }
            if(exceptions.Count != 0) throw new AggregateException(exceptions);
        });
    }

    /// <summary>
    /// Handles the next feature value
    /// </summary>
    /// <typeparam name="TState">The type of <see cref="IFeature"/> to handle the next value for</typeparam>
    /// <param name="feature">The next <see cref="IFeature"/> value</param>
    protected virtual void OnNextState<TState>(TState feature)
    {
        this.Stream.OnNext(feature!);
    }

}
