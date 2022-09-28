using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Synapse.Application.Commands.Generic;
using Synapse.Application.Mapping.Converters;
using System.Reflection;

namespace Synapse.Application.Mapping
{

    /// <summary>
    /// Represents the application mapping <see cref="Profile"/>
    /// </summary>
    public class MappingProfile
        : Profile
    {

        /// <summary>
        /// Initializes a new <see cref="MappingProfile"/>
        /// </summary>
        public MappingProfile()
        {
            this.AllowNullCollections = true;
            this.MappingConfigurationTypes = new List<Type>();
            this.ApplyMappingConfigurations();
            this.ConfigureEntities();
            this.ConfigureCommands();
            this.ConfigureQueries();
            this.ConfigureEvents();
            this.CreateMap(typeof(JsonPatchDocument), typeof(JsonPatchDocument<>))
                .ConvertUsing(typeof(JsonPatchDocumentConverter<>));
        }

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing the types of all existing <see cref="IMappingConfiguration"/>s
        /// </summary>
        protected List<Type> MappingConfigurationTypes { get; }

        /// <summary>
        /// Initializes the <see cref="MappingProfile"/>
        /// </summary>
        protected void ApplyMappingConfigurations()
        {
            foreach (Type mappingConfigurationType in this.GetType().Assembly
                .GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && t.IsClass && typeof(IMappingConfiguration).IsAssignableFrom(t)))
            {
                this.MappingConfigurationTypes.Add(mappingConfigurationType);
                this.ApplyConfiguration((IMappingConfiguration)Activator.CreateInstance(mappingConfigurationType, new object[] { })!);
            }
        }

        /// <summary>
        /// Configures entity mappings
        /// </summary>
        protected void ConfigureEntities()
        {
            foreach (Type entityType in new Assembly[] { typeof(V1Workflow).Assembly }
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsInterface && t.IsClass))
            {
                DataTransferObjectTypeAttribute? dtoTypeAttribute = entityType.GetCustomAttribute<DataTransferObjectTypeAttribute>();
                if (dtoTypeAttribute != null && !this.MappingConfigurationTypes.Any(t => typeof(IMappingConfiguration<,>).MakeGenericType(entityType, dtoTypeAttribute.Type).IsAssignableFrom(t)))
                    this.CreateMap(entityType, dtoTypeAttribute.Type);
            }
        }

        /// <summary>
        /// Configures command mappings
        /// </summary>
        protected void ConfigureCommands()
        {
            foreach (var commandType in typeof(MappingProfile).Assembly
                .GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && t.IsClass && typeof(ICommand).IsAssignableFrom(t)))
            {
                var dtoTypeAttribute = commandType.GetCustomAttribute<DataTransferObjectTypeAttribute>();
                if (dtoTypeAttribute != null && !this.MappingConfigurationTypes.Any(t => typeof(IMappingConfiguration<,>).MakeGenericType(dtoTypeAttribute.Type, commandType).IsAssignableFrom(t)))
                    this.CreateMap(dtoTypeAttribute.Type, commandType);
            }
            foreach (var patchableAggregateType in typeof(V1Workflow).Assembly
                .GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && t.IsClass && typeof(IAggregateRoot).IsAssignableFrom(t) && t.TryGetCustomAttribute<PatchableAttribute>(out _)))
            {
                var keyType = patchableAggregateType.GetGenericType(typeof(IIdentifiable<>)).GetGenericArguments()[0];
                var dtoType = patchableAggregateType.GetCustomAttribute<DataTransferObjectTypeAttribute>()!.Type;
                var patchCommandType = typeof(V1PatchCommand<,,>).MakeGenericType(patchableAggregateType, dtoType, keyType);
                this.CreateMap(typeof(Integration.Commands.Generic.V1PatchCommand), patchCommandType);
            }
        }

        /// <summary>
        /// Configures query mappings
        /// </summary>
        protected void ConfigureQueries()
        {
            foreach (Type queryType in typeof(MappingProfile).Assembly
                .GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && t.IsClass && typeof(IQuery).IsAssignableFrom(t)))
            {
                DataTransferObjectTypeAttribute? dtoTypeAttribute = queryType.GetCustomAttribute<DataTransferObjectTypeAttribute>();
                if (dtoTypeAttribute != null && !this.MappingConfigurationTypes.Any(t => typeof(IMappingConfiguration<,>).MakeGenericType(dtoTypeAttribute.Type, queryType).IsAssignableFrom(t)))
                    this.CreateMap(dtoTypeAttribute.Type, queryType);
            }
        }

        /// <summary>
        /// Configures event mappings
        /// </summary>
        protected void ConfigureEvents()
        {
            foreach (Type eventType in typeof(MappingProfile).Assembly
               .GetTypes()
               .Where(t => !t.IsAbstract && !t.IsInterface && t.IsClass && typeof(IDomainEvent).IsAssignableFrom(t)))
            {
                DataTransferObjectTypeAttribute? dtoTypeAttribute = eventType.GetCustomAttribute<DataTransferObjectTypeAttribute>();
                if (dtoTypeAttribute != null && !this.MappingConfigurationTypes.Any(t => typeof(IMappingConfiguration<,>).MakeGenericType(eventType, dtoTypeAttribute.Type).IsAssignableFrom(t)))
                    this.CreateMap(eventType, dtoTypeAttribute.Type);
            }
            this.CreateMap(typeof(IDomainEvent), typeof(IIntegrationEvent))
                .IncludeAllDerived();
        }

    }

}
