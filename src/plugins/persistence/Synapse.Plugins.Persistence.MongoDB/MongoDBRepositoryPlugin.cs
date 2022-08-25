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
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Neuroglia;
using Neuroglia.Data;
using Neuroglia.Serialization;
using Newtonsoft.Json;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Infrastructure.Plugins;
using Synapse.Plugins.Persistence.MongoDB.Services;
using System.Reflection;

namespace Synapse.Plugins.Persistence.MongoDB
{

    /// <summary>
    /// Represents the official <see cref="IRepositoryPlugin"/> implementation for MongoDB
    /// </summary>
    public class MongoDBRepositoryPlugin
        : Plugin, IRepositoryPlugin
    {

        /// <summary>
        /// Gets the <see cref="MongoDBRepositoryPlugin"/>'s <see cref="IServiceProvider"/>
        /// </summary>
        protected IServiceProvider ServiceProvider { get; private set; } = null!;

        /// <inheritdoc/>
        protected override async ValueTask InitializeAsync(CancellationToken stoppingToken)
        {
            await base.InitializeAsync(stoppingToken);
            ConfigureBsonSerialization();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Path.GetDirectoryName(typeof(MongoDBRepositoryPlugin).Assembly.Location)!, "settings.plugin.json"), true, true)
                .AddEnvironmentVariables()
                .Build();
            var mongoSettings = MongoClientSettings.FromConnectionString(configuration.GetConnectionString("MongoDB"));
            mongoSettings.LinqProvider = LinqProvider.V3;
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddMongoClient(mongoSettings, ServiceLifetime.Singleton);
            services.AddMongoDatabase("synapse", ServiceLifetime.Singleton);
            this.ServiceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Configures Synapse's BSON serialization
        /// </summary>
        protected static void ConfigureBsonSerialization()
        {
            BsonClassMap.RegisterClassMap<StateDefinition>(map =>
            {
                map.AutoMap();
                map.SetIsRootClass(true);
            });
            foreach(var t in typeof(StateDefinition).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType && typeof(StateDefinition).IsAssignableFrom(t)))
            {
                var classMap = new BsonClassMap(t);
                classMap.AutoMap();
                BsonClassMap.RegisterClassMap(classMap);
            }

            BsonClassMap.RegisterClassMap<AuthenticationProperties>(map =>
            {
                map.AutoMap();
                map.SetIsRootClass(true);
            });
            foreach (var t in typeof(AuthenticationProperties).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType && typeof(AuthenticationProperties).IsAssignableFrom(t)))
            {
                var classMap = new BsonClassMap(t);
                classMap.AutoMap();
                BsonClassMap.RegisterClassMap(classMap);
            }

            BsonClassMap.RegisterClassMap<StateOutcomeDefinition>(map =>
            {
                map.AutoMap();
                map.SetIsRootClass(true);
            });
            foreach (var t in typeof(StateOutcomeDefinition).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType && typeof(StateOutcomeDefinition).IsAssignableFrom(t)))
            {
                var classMap = new BsonClassMap(t);
                classMap.AutoMap();
                BsonClassMap.RegisterClassMap(classMap);
            }

            BsonClassMap.RegisterClassMap<SwitchCaseDefinition>(map =>
            {
                map.AutoMap();
                map.SetIsRootClass(true);
            });
            foreach (var t in typeof(SwitchCaseDefinition).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType && typeof(SwitchCaseDefinition).IsAssignableFrom(t)))
            {
                var classMap = new BsonClassMap(t);
                classMap.AutoMap();
                BsonClassMap.RegisterClassMap(classMap);
            }

            BsonSerializer.RegisterGenericSerializerDefinition(typeof(OneOf<,>), typeof(OneOfSerializer<,>));
            BsonSerializer.RegisterSerializer(typeof(Dynamic), new DynamicSerializer());
            BsonSerializer.RegisterSerializer(typeof(DynamicArray), new DynamicArraySerializer());
            BsonSerializer.RegisterSerializer(typeof(DynamicObject), new DynamicObjectSerializer());

            var conventionPack = new ConventionPack
            {
                new DelegateClassMapConvention("protected-properties", cm =>
                {
                    var properties = cm.ClassType.GetProperties(BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach(var property in properties
                        .Where(p => p.TryGetCustomAttribute<JsonPropertyAttribute>(out _)))
                    {
                        var jsonpropertyAttribute = property.GetCustomAttribute<JsonPropertyAttribute>()!;
                        cm.MapMember(property).SetElementName(jsonpropertyAttribute.PropertyName);
                    }
                    foreach(var property in properties
                        .Where(p => p.TryGetCustomAttribute<JsonIgnoreAttribute>(out _)))
                    {
                        cm.UnmapMember(property);
                    }
                }),
                new EnumRepresentationConvention(BsonType.Int32)
            };
            ConventionRegistry.Remove("synapse");
            ConventionRegistry.Register("synapse", conventionPack, t => true);
        }

        /// <inheritdoc/>
        public IRepository CreateRepository(Type entityType, Type keyType)
        {
            if(entityType == null)
                throw new ArgumentNullException(nameof(entityType));
            if (keyType == null)
                throw new ArgumentNullException(nameof(keyType));
            var repositoryType = typeof(MongoRepository<,>).MakeGenericType(entityType, keyType);
            var repository = (IRepository)ActivatorUtilities.CreateInstance(this.ServiceProvider, repositoryType);
            return repository;
        }

    }

}
