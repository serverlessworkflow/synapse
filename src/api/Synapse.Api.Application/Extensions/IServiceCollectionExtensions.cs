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
using Neuroglia.Security;
using Synapse.Api.Application.Commands.Resources.Generic;
using Synapse.Api.Application.Commands.WorkflowDataDocuments;
using Synapse.Api.Application.Queries.Resources.Generic;
using Synapse.Api.Application.Queries.Users;
using Synapse.Api.Application.Queries.WorkflowDataDocuments;
using Synapse.Resources;

namespace Synapse.Api.Application;

/// <summary>
/// Defines extensions for <see cref="IServiceCollection"/>s
/// </summary>
public static class IServiceCollectionExtensions
{

    /// <summary>
    /// Adds and configures runtime infrastructure services
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
    /// <returns>The configured <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddSynapseApi(this IServiceCollection services)
    {
        services.AddApiCommands();
        services.AddApiQueries();

        return services;
    }

    /// <summary>
    /// Registers and configures generic query handlers
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
    /// <returns>The configured <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddApiQueries(this IServiceCollection services)
    {
        var resourceTypes = typeof(Workflow).Assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType && !t.IsInterface && typeof(Resource).IsAssignableFrom(t)).ToList();
        resourceTypes.Add(typeof(Namespace));
        foreach (var queryableType in resourceTypes)
        {
            var serviceLifetime = ServiceLifetime.Scoped;
            Type queryType;
            Type resultType;
            Type handlerServiceType;
            Type handlerImplementationType;

            queryType = typeof(GetResourceQuery<>).MakeGenericType(queryableType);
            resultType = typeof(IOperationResult<>).MakeGenericType(queryableType);
            handlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(queryType, resultType);
            handlerImplementationType = typeof(GetResourceQueryHandler<>).MakeGenericType(queryableType);
            services.Add(new ServiceDescriptor(handlerServiceType, handlerImplementationType, serviceLifetime));

            queryType = typeof(GetResourceDefinitionQuery<>).MakeGenericType(queryableType);
            resultType = typeof(IOperationResult<IResourceDefinition>);
            handlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(queryType, resultType);
            handlerImplementationType = typeof(GetResourceDefinitionQueryHandler<>).MakeGenericType(queryableType);
            services.Add(new ServiceDescriptor(handlerServiceType, handlerImplementationType, serviceLifetime));

            queryType = typeof(GetResourcesQuery<>).MakeGenericType(queryableType);
            resultType = typeof(IOperationResult<>).MakeGenericType(typeof(IAsyncEnumerable<>).MakeGenericType(queryableType));
            handlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(queryType, resultType);
            handlerImplementationType = typeof(GetResourcesQueryHandler<>).MakeGenericType(queryableType);
            services.Add(new ServiceDescriptor(handlerServiceType, handlerImplementationType, serviceLifetime));

            queryType = typeof(ListResourcesQuery<>).MakeGenericType(queryableType);
            resultType = typeof(IOperationResult<>).MakeGenericType(typeof(Neuroglia.Data.Infrastructure.ResourceOriented.ICollection<>).MakeGenericType(queryableType));
            handlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(queryType, resultType);
            handlerImplementationType = typeof(ListResourcesQueryHandler<>).MakeGenericType(queryableType);
            services.Add(new ServiceDescriptor(handlerServiceType, handlerImplementationType, serviceLifetime));

            queryType = typeof(WatchResourcesQuery<>).MakeGenericType(queryableType);
            resultType = typeof(IOperationResult<>).MakeGenericType(typeof(IAsyncEnumerable<>).MakeGenericType(typeof(IResourceWatchEvent<>).MakeGenericType(queryableType)));
            handlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(queryType, resultType);
            handlerImplementationType = typeof(WatchResourcesQueryHandler<>).MakeGenericType(queryableType);
            services.Add(new ServiceDescriptor(handlerServiceType, handlerImplementationType, serviceLifetime));

            queryType = typeof(MonitorResourceQuery<>).MakeGenericType(queryableType);
            resultType = typeof(IOperationResult<>).MakeGenericType(typeof(IAsyncEnumerable<>).MakeGenericType(typeof(IResourceWatchEvent<>).MakeGenericType(queryableType)));
            handlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(queryType, resultType);
            handlerImplementationType = typeof(MonitorResourceQueryHandler<>).MakeGenericType(queryableType);
            services.Add(new ServiceDescriptor(handlerServiceType, handlerImplementationType, serviceLifetime));
        }
        services.AddScoped<IRequestHandler<GetUserProfileQuery, IOperationResult<UserInfo>>, GetUserProfileQueryHandler>();
        services.AddScoped<IRequestHandler<GetWorkflowDataDocumentQuery, IOperationResult<Document>>, GetWorkflowDataQueryHandler>();
        return services;
    }

    /// <summary>
    /// Registers and configures generic command handlers
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
    /// <returns>The configured <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddApiCommands(this IServiceCollection services)
    {
        var resourceTypes = typeof(Workflow).Assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType && !t.IsInterface && typeof(Resource).IsAssignableFrom(t)).ToList();
        resourceTypes.Add(typeof(Namespace));
        foreach (var resourceType in resourceTypes)
        {
            var serviceLifetime = ServiceLifetime.Scoped;
            Type commandType;
            Type resultType;
            Type handlerServiceType;
            Type handlerImplementationType;

            commandType = typeof(CreateResourceCommand<>).MakeGenericType(resourceType);
            resultType = typeof(IOperationResult<>).MakeGenericType(resourceType);
            handlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(commandType, resultType);
            handlerImplementationType = typeof(CreateResourceCommandHandler<>).MakeGenericType(resourceType);
            services.Add(new ServiceDescriptor(handlerServiceType, handlerImplementationType, serviceLifetime));

            commandType = typeof(ReplaceResourceCommand<>).MakeGenericType(resourceType);
            resultType = typeof(IOperationResult<>).MakeGenericType(resourceType);
            handlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(commandType, resultType);
            handlerImplementationType = typeof(ReplaceResourceCommandHandler<>).MakeGenericType(resourceType);
            services.Add(new ServiceDescriptor(handlerServiceType, handlerImplementationType, serviceLifetime));

            commandType = typeof(PatchResourceCommand<>).MakeGenericType(resourceType);
            resultType = typeof(IOperationResult<>).MakeGenericType(resourceType);
            handlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(commandType, resultType);
            handlerImplementationType = typeof(PatchResourceCommandHandler<>).MakeGenericType(resourceType);
            services.Add(new ServiceDescriptor(handlerServiceType, handlerImplementationType, serviceLifetime));

            commandType = typeof(PatchResourceStatusCommand<>).MakeGenericType(resourceType);
            resultType = typeof(IOperationResult<>).MakeGenericType(resourceType);
            handlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(commandType, resultType);
            handlerImplementationType = typeof(PatchResourceStatusCommandHandler<>).MakeGenericType(resourceType);
            services.Add(new ServiceDescriptor(handlerServiceType, handlerImplementationType, serviceLifetime));

            commandType = typeof(DeleteResourceCommand<>).MakeGenericType(resourceType);
            resultType = typeof(IOperationResult<>).MakeGenericType(resourceType);
            handlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(commandType, resultType);
            handlerImplementationType = typeof(DeleteResourceCommandHandler<>).MakeGenericType(resourceType);
            services.Add(new ServiceDescriptor(handlerServiceType, handlerImplementationType, serviceLifetime));

        }
        services.AddScoped<IRequestHandler<CreateWorkflowDataDocumentCommand, IOperationResult<Document>>, CreateWorkflowDataDocumentCommandHandler>();
        return services;
    }

}
