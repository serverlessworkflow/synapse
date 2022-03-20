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

        public Type? WriteModelRepositoryType { get; protected set; } = typeof(DistributedCacheRepository<,>);

        public Type? ReadModelRepositoryType { get; protected set; }= typeof(DistributedCacheRepository<,>);

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
            //todo: re-establish
            //this.Services.AddIntegrationEventBus(async (provider, e, cancellationToken) =>
            //{
            //    await provider.GetRequiredService<ICloudEventBus>().PublishAsync(e, cancellationToken);
            //});
            this.Services.AddAuthorization();
            this.Services.AddSingleton<IExpressionEvaluatorProvider, ExpressionEvaluatorProvider>();
            this.Services.AddRepositories(writeModelTypes, this.WriteModelRepositoryType, ServiceLifetime.Scoped);
            this.Services.AddRepositories(readModelTypes, this.ReadModelRepositoryType, ServiceLifetime.Scoped);
        }

    }

}
