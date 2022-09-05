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

using Neuroglia.Data.EventSourcing;
using Neuroglia.Serialization;
using Synapse.Application.Commands.WorkflowInstances;
using Synapse.Application.Queries.WorkflowInstances;
using Synapse.Application.Queries.Workflows;
using System.IO.Compression;

namespace Synapse.Application.Commands.Workflows
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to archive an existing <see cref="V1Workflow"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.Workflows.V1ArchiveWorkflowCommand))]
    public class V1ArchiveWorkflowCommand
        : Command<Stream>
    {

        /// <summary>
        /// Initializes a new <see cref="V1ArchiveWorkflowCommand"/>
        /// </summary>
        protected V1ArchiveWorkflowCommand()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1ArchiveWorkflowCommand"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Workflow"/> to archive</param>
        /// <param name="version">The version of the <see cref="V1Workflow"/> to archive</param>
        public V1ArchiveWorkflowCommand(string id, string? version)
        {
            this.Id = id;
            this.Version = version;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1Workflow"/> to archive
        /// </summary>
        public virtual string Id { get; protected set; } = null!;

        /// <summary>
        /// Gets the version of the <see cref="V1Workflow"/> to archive
        /// </summary>
        public virtual string? Version { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1ArchiveWorkflowCommand"/>s
    /// </summary>
    public class V1ArchiveWorkflowCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1ArchiveWorkflowCommand, Stream>
    {

        /// <summary>
        /// Initializes a new <see cref="V1ArchiveWorkflowCommandHandler"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="options">The service used to access the current <see cref="SynapseApplicationOptions"/></param>
        /// <param name="serializerProvider">The service used to provide <see cref="ISerializer"/>s</param>
        /// <param name="workflows">The <see cref="IRepository"/> used to manage <see cref="V1Workflow"/>s</param>
        /// <param name="workflowInstances">The <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s</param>
        /// <param name="workflowProcesses">The <see cref="IRepository"/> used to manage <see cref="V1WorkflowProcess"/>es</param>
        public V1ArchiveWorkflowCommandHandler(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IOptions<SynapseApplicationOptions> options,
            ISerializerProvider serializerProvider, IRepository<Integration.Models.V1Workflow> workflows, IRepository<Integration.Models.V1WorkflowInstance> workflowInstances, IRepository<Integration.Models.V1WorkflowProcess> workflowProcesses)
            : base(loggerFactory, mediator, mapper)
        {
            this.Options = options.Value;
            this.Serializer = serializerProvider.GetSerializer(this.Options.Archiving.SerializerType);
            this.EventStore = serviceProvider.GetService<IEventStore>();
            this.Workflows = workflows;
            this.WorkflowInstances = workflowInstances;
            this.WorkflowProcesses = workflowProcesses;
        }

        /// <summary>
        /// Gets the current <see cref="SynapseApplicationOptions"/>
        /// </summary>
        protected SynapseApplicationOptions Options { get; }

        /// <summary>
        /// Gets the service used to serialize the <see cref="V1WorkflowInstance"/> to archive and its components
        /// </summary>
        protected ISerializer Serializer { get; }

        /// <summary>
        /// Gets the service used to store events
        /// </summary>
        protected IEventStore? EventStore { get; }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1Workflow"/>s
        /// </summary>
        protected IRepository<Integration.Models.V1Workflow> Workflows { get; }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<Integration.Models.V1WorkflowInstance> WorkflowInstances { get; }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowProcess"/>es
        /// </summary>
        protected IRepository<Integration.Models.V1WorkflowProcess> WorkflowProcesses { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Stream>> HandleAsync(V1ArchiveWorkflowCommand command, CancellationToken cancellationToken = default)
        {
            List<Integration.Models.V1Workflow> workflowVersions;
            if (string.IsNullOrWhiteSpace(command.Version))
                workflowVersions = await this.Mediator.ExecuteAndUnwrapAsync(new V1GetWorkflowVersionsQuery(command.Id), cancellationToken);
            else
                workflowVersions = new() { await this.Mediator.ExecuteAndUnwrapAsync(new V1GetWorkflowByIdQuery(command.Id, command.Version), cancellationToken: cancellationToken) }; 
            if (!workflowVersions.Any())
                throw DomainException.NullReference(typeof(V1Workflow), command.Id);
            var definitionId = workflowVersions.First().Definition.Id!;
            var workflowDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), definitionId));
            if (workflowDirectory.Exists)
                workflowDirectory.Delete(true);
            workflowDirectory.Create();
            foreach (var workflowVersion in workflowVersions)
            {
                var versionDirectory = new DirectoryInfo(Path.Combine(workflowDirectory.FullName, workflowVersion.Definition.Version));
                if (versionDirectory.Exists)
                    versionDirectory.Delete(true);
                versionDirectory.Create();
                foreach(var workflowInstance in await this.Mediator.ExecuteAndUnwrapAsync(new V1GetWorkflowInstancesByDefinitionIdQuery(workflowVersion.Id), cancellationToken))
                {
                    using var instanceArchiveStream = await this.Mediator.ExecuteAndUnwrapAsync(new V1ArchiveWorkflowInstanceCommand(workflowInstance.Id), cancellationToken);
                    using var instanceArchive = new ZipArchive(instanceArchiveStream, ZipArchiveMode.Read);
                    var instanceArchiveDirectory = new DirectoryInfo(Path.GetTempPath());
                    instanceArchive.ExtractToDirectory(instanceArchiveDirectory.FullName, true);
                    instanceArchiveDirectory.GetDirectories(workflowInstance.Id).First().CopyTo(versionDirectory);
                }
            }
            var archiveFilePath = Path.Combine(Path.GetTempPath(), $"{definitionId}.zip");
            if (File.Exists(archiveFilePath))
                File.Delete(archiveFilePath);
            ZipFile.CreateFromDirectory(workflowDirectory.FullName, archiveFilePath, CompressionLevel.Fastest, true);
            using var fileStream = File.OpenRead(archiveFilePath);
            var stream = new MemoryStream();
            await fileStream.CopyToAsync(stream, cancellationToken);
            await stream.FlushAsync(cancellationToken);
            stream.Position = 0;
            return this.Ok(stream);
        }

    }

}
