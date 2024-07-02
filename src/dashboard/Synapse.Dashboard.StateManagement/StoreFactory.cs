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

using Synapse.Dashboard.StateManagement.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neuroglia;
using System.Linq.Expressions;
using System.Reflection;

namespace Synapse.Dashboard.StateManagement;

/// <summary>
/// Represents the default implementation of the <see cref="IStoreFactory"/> interface
/// </summary>
/// <remarks>
/// Initializes a new <see cref="StoreFactory"/>
/// </remarks>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="fluxOptions">The current <see cref="Configuration.FluxOptions"/></param>
public class StoreFactory(IServiceProvider serviceProvider, IOptions<FluxOptions> fluxOptions)
            : IStoreFactory
{

    static readonly MethodInfo AddFeatureMethod = typeof(IStoreExtensions).GetMethods().First(m => m.Name == nameof(IStoreExtensions.AddFeature) && m.GetParameters().Length == 2);

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    /// Gets the current <see cref="Configuration.FluxOptions"/>
    /// </summary>
    protected FluxOptions FluxOptions { get; } = fluxOptions.Value;

    /// <inheritdoc/>
    public virtual IStore CreateStore()
    {
        var store = (IStore)ActivatorUtilities.CreateInstance(this.ServiceProvider, this.FluxOptions.StoreType);
        this.ConfigureStoreFeatures(store);
        this.ConfigureStoreEffects(store);
        this.ConfigureStoreMiddlewares(store);
        return store;
    }

    /// <summary>
    /// Finds and configures <see cref="IFeature"/>s for the specified <see cref="IStore"/>
    /// </summary>
    /// <param name="store">The <see cref="IStore"/> to add <see cref="IFeature"/>s for</param>
    protected virtual void ConfigureStoreFeatures(IStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        foreach (var feature in this.FluxOptions.Features) store.AddFeature(feature);
        if (!this.FluxOptions.AutoRegisterFeatures) return;
        var reducersPerState = this.FindAndMapReducersPerState();
        foreach (var stateType in TypeCacheUtil.FindFilteredTypes("nflux-features",
          t => t.IsClass && !t.IsAbstract && !t.IsInterface && !t.IsGenericType && t.TryGetCustomAttribute<FeatureAttribute>(out _)))
        {
            if (!reducersPerState.TryGetValue(stateType, out var reducersPerFeature)) continue;
            AddFeatureMethod.MakeGenericMethod(stateType).Invoke(null, [store, reducersPerFeature.OfType(typeof(IReducer<>).MakeGenericType(stateType)).OfType<object>().ToArray()]);
        }
    }

    /// <summary>
    /// Finds and configures <see cref="IEffect"/>s for the specified <see cref="IStore"/>
    /// </summary>
    /// <param name="store">The <see cref="IStore"/> to add <see cref="IEffect"/>s for</param>
    protected virtual void ConfigureStoreEffects(IStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        foreach (var effect in this.FluxOptions.Effects) store.AddEffect(effect);
        if (!this.FluxOptions.AutoRegisterEffects)return;
        foreach (var effectDeclaringType in TypeCacheUtil.FindFilteredTypes("nflux-effects",
            t => t.TryGetCustomAttribute< EffectAttribute>(out _) || t.GetMethods().Any(m => m.TryGetCustomAttribute<EffectAttribute>(out _)), this.FluxOptions.AssembliesToScan?.ToArray()!))
        {
            foreach (var effectMethod in effectDeclaringType.GetMethods()
               .Where(m => (effectDeclaringType.TryGetCustomAttribute<EffectAttribute>(out _) && m.IsStatic && m.GetParameters().Length == 2 && m.GetParameters()[1].ParameterType == typeof(IEffectContext)) || m.TryGetCustomAttribute<EffectAttribute>(out _)))
            {
                if(effectMethod.ReturnType != typeof(Task)) throw new Exception($"The method '{effectMethod.Name}' in type '{effectMethod.DeclaringType!.FullName}' must return a task to be used as a Flux effect");
                if (!effectMethod.IsStatic) throw new Exception($"The method '{effectMethod.Name}' in type '{effectMethod.DeclaringType!.FullName}' must be static to be used as a Flux effect");
                if (effectMethod.GetParameters().Length != 2) throw new Exception($"The method '{effectMethod.Name}' in type '{effectMethod.DeclaringType!.FullName}' must declare exactly 2 parameters to be used as a Flux effect");
                var actionType = effectMethod.GetParameters()[0].ParameterType;
                var effectType = typeof(Effect<>).MakeGenericType(actionType);
                var effectLambda = this.BuildEffectLambda(actionType, effectMethod);
                var effectFunction = effectLambda.Compile();
                var effect = (IEffect)ActivatorUtilities.CreateInstance(this.ServiceProvider, effectType, effectFunction);
                store.AddEffect(effect);
            }
        }
    }

    /// <summary>
    /// Finds and configures <see cref="IMiddleware"/>s for the specified <see cref="IStore"/>
    /// </summary>
    /// <param name="store">The <see cref="IStore"/> to add <see cref="IMiddleware"/>s for</param>
    protected virtual void ConfigureStoreMiddlewares(IStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        foreach (var middlewareType in this.FluxOptions.Middlewares) store.AddMiddleware(middlewareType);
        if (!this.FluxOptions.AutoRegisterMiddlewares) return;
        foreach (var middlewareType in TypeCacheUtil.FindFilteredTypes("nflux-middlewares",
             t => t.IsClass && !t.IsInterface && !t.IsAbstract && !t.IsGenericType && typeof(IMiddleware).IsAssignableFrom(t), this.FluxOptions.AssembliesToScan?.ToArray()!))
        {
            store.AddMiddleware(middlewareType);
        }
    }

    /// <summary>
    /// Finds and maps scanned <see cref="IReducer"/>s per state type
    /// </summary>
    /// <returns>A new <see cref="IDictionary{TKey, TValue}"/> containing scanned <see cref="IReducer"/>s mapped by type</returns>
    protected virtual IDictionary<Type, List<IReducer>> FindAndMapReducersPerState()
    {
        var reducersPerState = new Dictionary<Type, List<IReducer>>();
        foreach (var reducerDeclaringType in TypeCacheUtil.FindFilteredTypes("nflux-reducers",
            t => t.TryGetCustomAttribute<ReducerAttribute>(out _) || t.GetMethods().Any(m => m.TryGetCustomAttribute<ReducerAttribute>(out _)), this.FluxOptions.AssembliesToScan?.ToArray()!))
        {
            foreach (var reducerMethod in reducerDeclaringType.GetMethods()
                .Where(m => (reducerDeclaringType.TryGetCustomAttribute<ReducerAttribute>(out _) && m.IsStatic && m.GetParameters().Length == 2 && m.ReturnType == m.GetParameters()[0].ParameterType) || m.TryGetCustomAttribute<ReducerAttribute>(out _)))
            {
                if (!reducerMethod.IsStatic) throw new Exception($"The method '{reducerMethod.Name}' in type '{reducerMethod.DeclaringType!.FullName}' must be static to be used as a Flux reducer");
                if (reducerMethod.GetParameters().Length != 2) throw new Exception($"The method '{reducerMethod.Name}' in type '{reducerMethod.DeclaringType!.FullName}' must declare exactly 2 parameters to be used as a Flux reducer");
                if (reducerMethod.ReturnType != reducerMethod.GetParameters()[0].ParameterType) throw new Exception($"The method '{reducerMethod.Name}' in type '{reducerMethod.DeclaringType!.FullName}' must return a type matching its first parameter's to be used as a Flux reducer");
                var stateType = reducerMethod.GetParameters()[0].ParameterType;
                var actionType = reducerMethod.GetParameters()[1].ParameterType;
                var reducerType = typeof(Reducer<,>).MakeGenericType(stateType, actionType);
                var reducerLambda = this.BuildReducerLambda(stateType, actionType, reducerMethod);
                var reducerFunction = reducerLambda.Compile();
                var reducer = (IReducer)ActivatorUtilities.CreateInstance(this.ServiceProvider, reducerType, reducerFunction);
                if (reducersPerState.TryGetValue(stateType, out var reducers)) reducers.Add(reducer);
                else reducersPerState.Add(stateType, [reducer]);
            }
        }
        return reducersPerState;
    }

    /// <summary>
    /// Builds a new reducer <see cref="LambdaExpression"/> for the specified state type, action type and reducer method
    /// </summary>
    /// <param name="stateType">The type of state to create the reducer <see cref="LambdaExpression"/> for</param>
    /// <param name="actionType">The type of action to create the reducer <see cref="LambdaExpression"/> for</param>
    /// <param name="reducerMethod">The reducer method</param>
    /// <returns>A new reducer <see cref="LambdaExpression"/></returns>
    protected virtual LambdaExpression BuildReducerLambda(Type stateType, Type actionType, MethodInfo reducerMethod)
    {
        ArgumentNullException.ThrowIfNull(stateType);
        ArgumentNullException.ThrowIfNull(actionType);
        ArgumentNullException.ThrowIfNull(reducerMethod);
        var stateParam = Expression.Parameter(stateType, "state");
        var actionParam = Expression.Parameter(actionType, "action");
        var methodCall = Expression.Call(null, reducerMethod, stateParam, actionParam);
        var lambda = Expression.Lambda(methodCall, stateParam, actionParam);
        return lambda;
    }

    /// <summary>
    /// Builds a new effect <see cref="LambdaExpression"/> for the specified action type and reducer method
    /// </summary>
    /// <param name="actionType">The type of action to create the effect <see cref="LambdaExpression"/> for</param>
    /// <param name="effectMethod">The effect method</param>
    /// <returns>A new effect <see cref="LambdaExpression"/></returns>
    protected virtual LambdaExpression BuildEffectLambda(Type actionType, MethodInfo effectMethod)
    {
        ArgumentNullException.ThrowIfNull(actionType);
        ArgumentNullException.ThrowIfNull(effectMethod);
        var actionParam = Expression.Parameter(actionType, "action");
        var contextParam = Expression.Parameter(typeof(IEffectContext), "context");
        var methodCall = Expression.Call(null, effectMethod, actionParam, contextParam);
        var lambda = Expression.Lambda(methodCall, actionParam, contextParam);
        return lambda;
    }

}
