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

using Microsoft.Extensions.DependencyInjection.Extensions;
using Synapse.Application.Commands.Generic;
using Synapse.Application.Queries.Generic;
using Synapse.Domain;
using Synapse.Infrastructure;
using System.Reflection;

namespace Synapse.Application.Configuration
{

    /// <summary>
    /// Defines extensions for <see cref="IServiceCollection"/>s
    /// </summary>
    public static class IServiceCollectionExtensions
    {

        /// <summary>
        /// Adds and configures the services required by the Synapse Serverless Workflow runtime
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
        /// <param name="configuration">The current <see cref="IConfiguration"/></param>
        /// <param name="setup">An <see cref="Action{T}"/> used to configure Synapse</param>
        /// <returns>The configured <see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddSynapse(this IServiceCollection services, IConfiguration configuration, Action<ISynapseApplicationBuilder> setup)
        {
            var applicationOptions = new SynapseApplicationOptions();
            configuration.Bind(applicationOptions);
            services.Configure<SynapseApplicationOptions>(configuration);
            var appBuilder = new SynapseApplicationBuilder(configuration, services);
            setup(appBuilder);
            appBuilder.Build();
            return services;
        }

        /// <summary>
        /// Adds and configures the <see cref="IIntegrationEventBus"/> service
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
        /// <returns>The configured <see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddIntegrationEventBus(this IServiceCollection services)
        {
            services.TryAddSingleton<IIntegrationEventBusFactory, IntegrationBusPipelineFactory>();
            services.TryAddSingleton(provider => provider.GetRequiredService<IIntegrationEventBusFactory>().Create());
            return services;
        }

        /// <summary>
        /// Adds the specified <see cref="PublishIntegrationEventDelegate"/> to the <see cref="IIntegrationEventBus"/> pipeline
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
        /// <param name="middleware">The <see cref="PublishIntegrationEventDelegate"/> middleware to add to the pipeline</param>
        /// <returns>The configured <see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddIntegrationEventBus(this IServiceCollection services, PublishIntegrationEventDelegate middleware)
        {
            services.AddIntegrationEventBus();
            services.Configure<IntegrationEventBusPipelineOptions>(options =>
            {
                options.Middlewares.Add(middleware);
            });
            return services;
        }

        internal static IServiceCollection AddGenericQueryHandlers(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            foreach (Type queryableType in TypeCacheUtil.FindFilteredTypes("nqt", t => t.TryGetCustomAttribute<QueryableAttribute>(out _), typeof(QueryableAttribute).Assembly))
            {
                var keyType = queryableType.GetGenericType(typeof(IIdentifiable<>)).GetGenericArguments().First();
                var queryType = typeof(V1FindByIdQuery<,>).MakeGenericType(queryableType, keyType);
                var resultType = typeof(IOperationResult<>).MakeGenericType(queryableType);
                var handlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(queryType, resultType);
                var handlerImplementationType = typeof(V1FindByKeyQueryHandler<,>).MakeGenericType(queryableType, keyType);
                services.Add(new ServiceDescriptor(handlerServiceType, handlerImplementationType, serviceLifetime));
                services.Add(new ServiceDescriptor(typeof(IMiddleware<,>).MakeGenericType(queryType, resultType), typeof(DomainExceptionHandlingMiddleware<,>).MakeGenericType(queryType, resultType), serviceLifetime));

                queryType = typeof(V1ListQuery<>).MakeGenericType(queryableType);
                resultType = typeof(IOperationResult<>).MakeGenericType(typeof(IQueryable<>).MakeGenericType(queryableType));
                handlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(queryType, resultType);
                handlerImplementationType = typeof(V1ListQueryHandler<>).MakeGenericType(queryableType);
                services.Add(new ServiceDescriptor(handlerServiceType, handlerImplementationType, serviceLifetime));
                services.Add(new ServiceDescriptor(typeof(IMiddleware<,>).MakeGenericType(queryType, resultType), typeof(DomainExceptionHandlingMiddleware<,>).MakeGenericType(queryType, resultType), serviceLifetime));

                queryType = typeof(V1FilterQuery<>).MakeGenericType(queryableType);
                resultType = typeof(IOperationResult<>).MakeGenericType(typeof(List<>).MakeGenericType(queryableType));
                handlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(queryType, resultType);
                handlerImplementationType = typeof(V1FilterQueryHandler<>).MakeGenericType(queryableType);
                services.Add(new ServiceDescriptor(handlerServiceType, handlerImplementationType, serviceLifetime));
                services.Add(new ServiceDescriptor(typeof(IMiddleware<,>).MakeGenericType(queryType, resultType), typeof(DomainExceptionHandlingMiddleware<,>).MakeGenericType(queryType, resultType), serviceLifetime));
            }
            return services;
        }

        internal static IServiceCollection AddGenericCommandHandlers(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            foreach (Type aggregateType in TypeCacheUtil.FindFilteredTypes("domain:aggregates", t => t.IsClass && !t.IsAbstract && typeof(IAggregateRoot).IsAssignableFrom(t), typeof(V1Workflow).Assembly))
            {
                Type dtoType = aggregateType.GetCustomAttribute<DataTransferObjectTypeAttribute>()!.Type;
                Type keyType = aggregateType.GetGenericType(typeof(IIdentifiable<>)).GetGenericArguments().First();
                Type commandType;
                Type resultType;
                Type handlerServiceType;
                Type handlerImplementationType;
                if (aggregateType.TryGetCustomAttribute(out PatchableAttribute patchableAttribute))
                {
                    commandType = typeof(V1PatchCommand<,,>).MakeGenericType(aggregateType, dtoType, keyType);
                    resultType = typeof(IOperationResult<>).MakeGenericType(dtoType);
                    handlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(commandType, resultType);
                    handlerImplementationType = typeof(PatchCommandHandler<,,>).MakeGenericType(aggregateType, dtoType, keyType);
                    services.Add(new ServiceDescriptor(handlerServiceType, handlerImplementationType, serviceLifetime));
                    services.Add(new ServiceDescriptor(typeof(IMiddleware<,>).MakeGenericType(commandType, resultType), typeof(DomainExceptionHandlingMiddleware<,>).MakeGenericType(commandType, resultType), serviceLifetime));
                }

                if (typeof(IDeletable).IsAssignableFrom(aggregateType))
                {
                    commandType = typeof(V1DeleteCommand<,>).MakeGenericType(aggregateType, keyType);
                    resultType = typeof(IOperationResult);
                    handlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(commandType, resultType);
                    handlerImplementationType = typeof(DeleteCommandHandler<,>).MakeGenericType(aggregateType, keyType);
                    services.Add(new ServiceDescriptor(handlerServiceType, handlerImplementationType, serviceLifetime));
                    services.Add(new ServiceDescriptor(typeof(IMiddleware<,>).MakeGenericType(commandType, resultType), typeof(DomainExceptionHandlingMiddleware<,>).MakeGenericType(commandType, resultType), serviceLifetime));
                }
            }
            return services;
        }

        static IServiceCollection AddRepository(this IServiceCollection services, Type entityType, ServiceLifetime lifetime, ApplicationModelType modelType)
        {
            var keyType = entityType.GetGenericType(typeof(IIdentifiable<>)).GetGenericArguments()[0];
            var genericRepositoryType = typeof(IRepository<,>).MakeGenericType(entityType, keyType);
            services.Add(new(genericRepositoryType, provider => provider.GetRequiredService<IRepositoryFactory>().CreateRepository(entityType, modelType), lifetime));
            services.Add(new(typeof(IRepository<>).MakeGenericType(entityType), provider => provider.GetRequiredService(genericRepositoryType), lifetime));
            return services;
        }

        internal static IServiceCollection AddRepositories(this IServiceCollection services, IEnumerable<Type> entityTypes, ServiceLifetime lifetime, ApplicationModelType modelType)
        {
            foreach (Type entityType in entityTypes)
            {
                services.AddRepository(entityType, lifetime, modelType);
            }
            return services;
        }

    }

}
