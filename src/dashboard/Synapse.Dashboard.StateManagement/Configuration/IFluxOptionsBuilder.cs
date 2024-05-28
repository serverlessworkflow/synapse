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
    /// Defines the fundamentals of a service used to build <see cref="FluxOptions"/>
    /// </summary>
    public interface IFluxOptionsBuilder
    {

        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> to configure
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Configures the <see cref="IFluxOptionsBuilder"/> to scan the specified <see cref="Assembly"/> in order to find Flux components
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to scan</param>
        /// <returns>The configured <see cref="IFluxOptionsBuilder"/></returns>
        IFluxOptionsBuilder ScanAssembly(Assembly assembly);

        /// <summary>
        /// Configures the <see cref="IFluxOptionsBuilder"/> to automatically register scanned <see cref="IFeature"/>s and <see cref="IReducer"/>s
        /// </summary>
        /// <param name="autoRegister">A boolean indicating whether or not to automatically register scanned <see cref="IFeature"/>s and <see cref="IReducer"/>s</param>
        /// <returns>The configured <see cref="IFluxOptionsBuilder"/></returns>
        IFluxOptionsBuilder AutoRegisterFeatures(bool autoRegister = true);

        /// <summary>
        /// Configures the <see cref="IFluxOptionsBuilder"/> to automatically register scanned <see cref="IEffect"/>s
        /// </summary>
        /// <param name="autoRegister">A boolean indicating whether or not to automatically register scanned <see cref="IEffect"/>s</param>
        /// <returns>The configured <see cref="IFluxOptionsBuilder"/></returns>
        IFluxOptionsBuilder AutoRegisterEffects(bool autoRegister = true);

        /// <summary>
        /// Configures the <see cref="IFluxOptionsBuilder"/> to automatically register scanned <see cref="IMiddleware"/>s
        /// </summary>
        /// <param name="autoRegister">A boolean indicating whether or not to automatically register scanned <see cref="IMiddleware"/>s</param>
        /// <returns>The configured <see cref="IFluxOptionsBuilder"/></returns>
        IFluxOptionsBuilder AutoRegisterMiddlewares(bool autoRegister = true);

        /// <summary>
        /// Configures the <see cref="IFluxOptionsBuilder"/> to use the specified <see cref="IDispatcher"/> type
        /// </summary>
        /// <param name="dispatcherType">The type of the <see cref="IDispatcher"/> to use</param>
        /// <returns>The configured <see cref="IFluxOptionsBuilder"/></returns>
        IFluxOptionsBuilder UseDispatcher(Type dispatcherType);

        /// <summary>
        /// Configures the <see cref="IFluxOptionsBuilder"/> to use the specified <see cref="IDispatcher"/> type
        /// </summary>
        /// <param name="storeFactoryType">The type of the <see cref="IStoreFactory"/> to use</param>
        /// <returns>The configured <see cref="IFluxOptionsBuilder"/></returns>
        IFluxOptionsBuilder UseStoreFactory(Type storeFactoryType);

        /// <summary>
        /// Configures the <see cref="IFluxOptionsBuilder"/> to use the specified <see cref="IDispatcher"/> type
        /// </summary>
        /// <param name="storeType">The type of the <see cref="IStore"/> to use</param>
        /// <returns>The configured <see cref="IFluxOptionsBuilder"/></returns>
        IFluxOptionsBuilder UseStore(Type storeType);

        /// <summary>
        /// Configures the lifetime of all Flux services
        /// </summary>
        /// <param name="lifetime">The lifetime of all Flux services</param>
        /// <returns>The configures <see cref="IFluxOptionsBuilder"/></returns>
        IFluxOptionsBuilder WithServiceLifetime(ServiceLifetime lifetime);

        /// <summary>
        /// Configures the <see cref="IFluxOptionsBuilder"/> to use the specified <see cref="IFeature"/>
        /// </summary>
        /// <typeparam name="TState">The type of the state of the <see cref="IFeature"/> to use</typeparam>
        /// <param name="state">The initial state of the <see cref="IFeature"/> to add</param>
        /// <returns>The configured <see cref="IFluxOptionsBuilder"/></returns>
        IFluxOptionsBuilder AddFeature<TState>(TState state);

        /// <summary>
        /// Configures the <see cref="IFluxOptionsBuilder"/> to use the specified <see cref="IFeature"/>
        /// </summary>
        /// <typeparam name="TState">The type of the state of the <see cref="IFeature"/> to use</typeparam>
        /// <returns>The configured <see cref="IFluxOptionsBuilder"/></returns>
        IFluxOptionsBuilder AddFeature<TState>()
            where TState : new();

        /// <summary>
        /// Configures the <see cref="IFluxOptionsBuilder"/> to use the specified <see cref="IMiddleware"/>
        /// </summary>
        /// <typeparam name="TMiddleware">The type of <see cref="IMiddleware"/> to use</typeparam>
        /// <returns>The configured <see cref="IFluxOptionsBuilder"/></returns>
        IFluxOptionsBuilder AddMiddleware<TMiddleware>()
            where TMiddleware : IMiddleware;

        /// <summary>
        /// Configures the <see cref="IFluxOptionsBuilder"/> to use the specified <see cref="IEffect"/>
        /// </summary>
        /// <param name="effect">The <see cref="IEffect"/> to add</param>
        /// <returns>The configured <see cref="IFluxOptionsBuilder"/></returns>
        IFluxOptionsBuilder AddEffect(IEffect effect);

        /// <summary>
        /// Builds new <see cref="FluxOptions"/>
        /// </summary>
        /// <returns>New <see cref="FluxOptions"/></returns>
        FluxOptions Build();

    }

}
