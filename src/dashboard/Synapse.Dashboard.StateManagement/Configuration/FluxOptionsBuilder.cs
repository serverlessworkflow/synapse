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

/*
 * Copyright © 2021 Neuroglia SPRL. All rights reserved.
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

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Synapse.Dashboard.StateManagement.Configuration
{

    /// <summary>
    /// Represents the default implementation of the <see cref="IFluxOptionsBuilder"/> interface
    /// </summary>
    /// <remarks>
    /// Initializes a new <see cref="FluxOptionsBuilder"/>
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
    public class FluxOptionsBuilder(IServiceCollection services)
                : IFluxOptionsBuilder
    {

        /// <inheritdoc/>
        public IServiceCollection Services { get; } = services;

        /// <summary>
        /// Gets the <see cref="FluxOptions"/> to configure
        /// </summary>
        protected FluxOptions Options { get; } = new();

        /// <inheritdoc/>
        public virtual IFluxOptionsBuilder ScanAssembly(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);
            this.Options.AssembliesToScan.Add(assembly);
            return this;
        }

        /// <inheritdoc/>
        public virtual IFluxOptionsBuilder AutoRegisterFeatures(bool autoRegister = true)
        {
            this.Options.AutoRegisterFeatures = autoRegister;
            return this;
        }

        /// <inheritdoc/>
        public virtual IFluxOptionsBuilder AutoRegisterEffects(bool autoRegister = true)
        {
            this.Options.AutoRegisterEffects = autoRegister;
            return this;
        }

        /// <inheritdoc/>
        public virtual IFluxOptionsBuilder AutoRegisterMiddlewares(bool autoRegister = true)
        {
            this.Options.AutoRegisterMiddlewares = autoRegister;
            return this;
        }

        /// <inheritdoc/>
        public virtual IFluxOptionsBuilder UseDispatcher(Type dispatcherType)
        {
            ArgumentNullException.ThrowIfNull(dispatcherType);
            if (!typeof(IDispatcher).IsAssignableFrom(dispatcherType)) throw new ArgumentException($"The specified type must implement the {nameof(IDispatcher)} interface", nameof(dispatcherType));
            this.Options.DispatcherType = dispatcherType;
            return this;
        }

        /// <inheritdoc/>
        public virtual IFluxOptionsBuilder UseStoreFactory(Type storeFactoryType)
        {
            ArgumentNullException.ThrowIfNull(storeFactoryType);
            if (!typeof(IStoreFactory).IsAssignableFrom(storeFactoryType)) throw new ArgumentException($"The specified type must implement the {nameof(IStoreFactory)} interface", nameof(storeFactoryType));
            this.Options.StoreFactoryType = storeFactoryType;
            return this;
        }

        /// <inheritdoc/>
        public virtual IFluxOptionsBuilder UseStore(Type storeType)
        {
            ArgumentNullException.ThrowIfNull(storeType);
            if (!typeof(IStore).IsAssignableFrom(storeType)) throw new ArgumentException($"The specified type must implement the {nameof(IStore)} interface", nameof(storeType));
            this.Options.StoreType = storeType;
            return this;
        }

        /// <inheritdoc/>
        public virtual IFluxOptionsBuilder WithServiceLifetime(ServiceLifetime lifetime)
        {
            this.Options.ServiceLifetime = lifetime;
            return this;
        }

        /// <inheritdoc/>
        public virtual IFluxOptionsBuilder AddFeature<TState>(TState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            this.Options.Features.Add(new Feature<TState>(state));
            return this;
        }

        /// <inheritdoc/>
        public virtual IFluxOptionsBuilder AddFeature<TState>()
            where TState : new()
        {
            return this.AddFeature(new TState());
        }

        /// <inheritdoc/>
        public virtual IFluxOptionsBuilder AddMiddleware<TMiddleware>() 
            where TMiddleware : IMiddleware
        {
            this.Options.Middlewares.Add(typeof(TMiddleware));
            return this;
        }

        /// <inheritdoc/>
        public virtual IFluxOptionsBuilder AddEffect(IEffect effect) 
        {
            ArgumentNullException.ThrowIfNull(effect);
            this.Options.Effects.Add(effect);
            return this;
        }

        /// <inheritdoc/>
        public virtual FluxOptions Build()
        {
            this.Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(this.Options));
            this.Services.Add(new(typeof(IDispatcher), this.Options.DispatcherType, this.Options.ServiceLifetime));
            this.Services.Add(new(typeof(IStoreFactory), this.Options.StoreFactoryType, this.Options.ServiceLifetime));
            this.Services.Add(new(typeof(IStore), provider => provider.GetRequiredService<IStoreFactory>().CreateStore(), this.Options.ServiceLifetime));
            return this.Options;
        }

    }

}
