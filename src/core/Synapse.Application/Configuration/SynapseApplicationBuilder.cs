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

using CloudNative.CloudEvents.NewtonsoftJson;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Adapters;
using Neuroglia.Data.Expressions;
using Neuroglia.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ServerlessWorkflow.Sdk;
using System.Reflection;

namespace Synapse.Application.Configuration
{

    /// <summary>
    /// Represents the default implementation of the <see cref="ISynapseApplicationBuilder"/> interface
    /// </summary>
    public class SynapseApplicationBuilder
        : ISynapseApplicationBuilder
    {

        /// <summary>
        /// Initializes a new <see cref="SynapseApplicationBuilder"/>
        /// </summary>
        /// <param name="configuration">The current <see cref="IConfiguration"/></param>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
        public SynapseApplicationBuilder(IConfiguration configuration, IServiceCollection services)
        {
            this.Configuration = configuration;
            this.Services = services;
        }

        /// <summary>
        /// Gets the current <see cref="IConfiguration"/>
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> to configure
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Gets the type of <see cref="IRepository"/> to use for write models
        /// </summary>
        protected Type WriteModelRepositoryType { get; set; } = typeof(DistributedCacheRepository<,>);

        /// <summary>
        /// Gets the type of <see cref="IRepository"/> to use for read models
        /// </summary>
        protected Type ReadModelRepositoryType { get; set; }= typeof(DistributedCacheRepository<,>);

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing the assemblies to scan to automatically register mapping-related services
        /// </summary>
        protected List<Assembly> MapperAssemblies { get; } = new() { typeof(SynapseApplicationBuilder).Assembly };

        /// <inheritdoc/>
        public virtual ISynapseApplicationBuilder AddMappingProfile<TProfile>() 
            where TProfile : AutoMapper.Profile
        {
            var assembly = typeof(TProfile).Assembly;
            if(!this.MapperAssemblies.Contains(assembly))
                this.MapperAssemblies.Add(assembly);
            return this;
        }

        /// <inheritdoc/>
        public virtual void Build()
        {
            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings()
                {
                    ContractResolver = new NonPublicSetterContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore
                };
                return settings;
            };

            List<Type> writeModelTypes = TypeCacheUtil.FindFilteredTypes("domain:aggregates", t => t.IsClass && !t.IsAbstract && typeof(IAggregateRoot).IsAssignableFrom(t), typeof(V1Workflow).Assembly).ToList();
            List<Type> readModelTypes = writeModelTypes
                .Where(t => t.TryGetCustomAttribute<DataTransferObjectTypeAttribute>(out _))
                .Select(t => t.GetCustomAttribute<DataTransferObjectTypeAttribute>()!.Type)
                .ToList();

            SynapseApplicationOptions options = new();
            this.Configuration.Bind(options);

            this.Services.AddSingleton(Options.Create(options));
            this.Services.AddLogging();
            this.Services.AddMediator(builder =>
            {
                builder.ScanAssembly(typeof(SynapseApplicationBuilder).Assembly);
                builder.UseDefaultPipelineBehavior(typeof(DomainExceptionHandlingMiddleware<,>));
                builder.UseDefaultPipelineBehavior(typeof(FluentValidationMiddleware<,>));
            });
            this.Services.AddGenericQueryHandlers();
            this.Services.AddGenericCommandHandlers();
            this.Services.AddMapper(this.MapperAssemblies.ToArray());
            this.Services.AddSingleton<IJsonPatchMetadataProvider, JsonPatchMetadataProvider>();
            this.Services.AddScoped<IObjectAdapter, AggregateObjectAdapter>();
            this.Services.AddTransient<IEdmModelBuilder, EdmModelBuilder>();
            this.Services.AddTransient<IODataQueryOptionsParser, ODataQueryOptionsParser>();
            this.Services.AddSingleton<IWorkflowRuntimeProxyFactory, WorkflowRuntimeProxyFactory>();
            this.Services.AddSingleton<IWorkflowRuntimeProxyManager, WorkflowRuntimeProxyManager>();
            this.Services.AddSingleton<ICronJobScheduler, CronJobScheduler>();
            this.Services.AddHostedService<WorkflowScheduler>();
            this.Services.AddTransient(provider => provider.GetRequiredService<IEdmModelBuilder>().Build());
            this.Services.AddNewtonsoftJsonSerializer(options =>
            {
                options.ContractResolver = new NonPublicSetterContractResolver();
                options.NullValueHandling = NullValueHandling.Include;
                options.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
                options.Converters.Add(new AbstractClassConverterFactory());
            });
            this.Services.AddSingleton<CloudEventFormatter, JsonEventFormatter>();
            this.Services.AddCloudEventBus(builder =>
            {
                builder.WithBrokerUri(options.CloudEvents.Broker.Uri);
            });
            this.Services.AddServerlessWorkflow();
            this.Services.AddSingleton<CloudEventCorrelator>();
            this.Services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<CloudEventCorrelator>());
            this.Services.AddIntegrationEventBus(async (provider, e, cancellationToken) =>
            {
                await provider.GetRequiredService<ICloudEventBus>().PublishAsync(e, cancellationToken);
            });
            this.Services.AddAuthorization();
            this.Services.AddSingleton<IExpressionEvaluatorProvider, ExpressionEvaluatorProvider>();
            this.Services.AddRepositories(writeModelTypes, this.WriteModelRepositoryType, ServiceLifetime.Scoped);
            this.Services.AddRepositories(readModelTypes, this.ReadModelRepositoryType, ServiceLifetime.Scoped);
        }

    }

}
