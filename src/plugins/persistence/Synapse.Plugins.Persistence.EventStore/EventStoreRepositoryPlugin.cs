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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neuroglia.Data;
using Neuroglia.Data.EventSourcing;
using Neuroglia.Data.EventSourcing.Services;
using Neuroglia.Mediation;
using Neuroglia.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Synapse.Infrastructure.Plugins;

namespace Synapse.Plugins.Persistence.EventStore
{

    /// <summary>
    /// Represents the official <see cref="IRepositoryPlugin"/> implementation for EventStore
    /// </summary>
    public class EventStoreRepositoryPlugin
        : Plugin, IRepositoryPlugin
    {

        /// <summary>
        /// Initializes a new <see cref="EventStoreRepositoryPlugin"/>
        /// </summary>
        /// <param name="mediator">The service used to mediate calls</param>
        public EventStoreRepositoryPlugin(IMediator mediator)
        {
            this.Mediator = mediator;
        }

        /// <summary>
        /// Gets the service used to mediate calls
        /// </summary>
        protected IMediator Mediator { get; }

        /// <summary>
        /// Gets the <see cref="EventStoreRepositoryPlugin"/>'s <see cref="IServiceProvider"/>
        /// </summary>
        protected IServiceProvider ServiceProvider { get; private set; } = null!;

        /// <inheritdoc/>
        protected override async ValueTask InitializeAsync(CancellationToken stoppingToken)
        {
            await base.InitializeAsync(stoppingToken);
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Path.GetDirectoryName(typeof(EventStoreRepositoryPlugin).Assembly.Location)!, "settings.plugin.json"), true, true)
                .AddEnvironmentVariables()
                .Build();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(this.Mediator);
            services.AddEventStore(es =>
            {
                es.ConfigureClient(client =>
                {
                    client.UseConnectionString(configuration.GetConnectionString("EventStore"));
                });
            });
            services.AddNewtonsoftJsonSerializer(options =>
            {
                options.ContractResolver = new NonPublicSetterContractResolver();
                options.NullValueHandling = NullValueHandling.Ignore;
            });
            this.ServiceProvider = services.BuildServiceProvider();
        }

        /// <inheritdoc/>
        public IRepository CreateRepository(Type entityType, Type keyType)
        {
            if(entityType == null)
                throw new ArgumentNullException(nameof(entityType));
            if (keyType == null)
                throw new ArgumentNullException(nameof(keyType));
            var repositoryType = typeof(EventSourcingRepository<,>).MakeGenericType(entityType, keyType);
            var repository = (IRepository)ActivatorUtilities.CreateInstance(this.ServiceProvider, repositoryType);
            return repository;
        }

    }

}
