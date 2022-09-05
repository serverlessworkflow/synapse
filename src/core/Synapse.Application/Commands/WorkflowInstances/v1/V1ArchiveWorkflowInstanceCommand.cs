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
using System.IO.Compression;

namespace Synapse.Application.Commands.WorkflowInstances
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to archive an existing <see cref="V1WorkflowInstance"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.WorkflowInstances.V1ArchiveWorkflowInstanceCommand))]
    public class V1ArchiveWorkflowInstanceCommand
        : Command<Stream>
    {

        /// <summary>
        /// Initializes a new <see cref="V1ArchiveWorkflowInstanceCommand"/>
        /// </summary>
        protected V1ArchiveWorkflowInstanceCommand()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1ArchiveWorkflowInstanceCommand"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> to archive</param>
        public V1ArchiveWorkflowInstanceCommand(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/> to archive
        /// </summary>
        public virtual string Id { get; protected set; } = null!;

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1ArchiveWorkflowInstanceCommand"/>s
    /// </summary>
    public class V1ArchiveWorkflowInstanceCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1ArchiveWorkflowInstanceCommand, Stream>
    {

        /// <summary>
        /// Initializes a new <see cref="V1ArchiveWorkflowInstanceCommandHandler"/>
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
        public V1ArchiveWorkflowInstanceCommandHandler(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IOptions<SynapseApplicationOptions> options, 
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
        public virtual async Task<IOperationResult<Stream>> HandleAsync(V1ArchiveWorkflowInstanceCommand command, CancellationToken cancellationToken = default)
        {
            var workflowInstance = await this.WorkflowInstances.FindAsync(command.Id, cancellationToken);
            if (workflowInstance == null)
                throw DomainException.NullReference(typeof(V1WorkflowInstance), command.Id);
            var workflow = await this.Workflows.FindAsync(workflowInstance.WorkflowId, cancellationToken);
            if(workflow == null)
                throw DomainException.NullReference(typeof(V1Workflow), workflowInstance.WorkflowId);
            var directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), workflowInstance.Id));
            if (directory.Exists)
                directory.Delete(true);
            directory.Create();
            var buffer = await this.Serializer.SerializeAsync(workflow.Definition, cancellationToken);
            await File.WriteAllBytesAsync(Path.Combine(directory.FullName, $"definition{this.Options.Archiving.FileExtension}"), buffer, cancellationToken);
            buffer = await this.Serializer.SerializeAsync(this.Mapper.Map<Integration.Models.V1WorkflowInstance>(workflowInstance), cancellationToken);
            await File.WriteAllBytesAsync(Path.Combine(directory.FullName, $"snapshot{this.Options.Archiving.FileExtension}"), buffer, cancellationToken);
            if(this.EventStore != null)
            {
                var eventStream = await this.EventStore.GetStreamAsync($"{typeof(V1WorkflowInstance).Name.ToLower()}-{workflowInstance.Id}", cancellationToken); //todo: ATTENTION: we should not be building that key ourselves here. A service should take care of that. Possibly fix/change the way the EventSourcingRepository works
                if(eventStream != null)
                {
                    var events = await eventStream.ToListAsync(cancellationToken);
                    buffer = await this.Serializer.SerializeAsync(events, cancellationToken);
                    await File.WriteAllBytesAsync(Path.Combine(directory.FullName, $"stream{this.Options.Archiving.FileExtension}"), buffer, cancellationToken);
                }
            }
            var processesDirectory = new DirectoryInfo(Path.Combine(directory.FullName, "processes"));
            if (processesDirectory.Exists)
                processesDirectory.Delete(true);
            processesDirectory.Create();
            foreach (var processId in workflowInstance.Sessions.Select(s => s.ProcessId))
            {
                var process = await this.WorkflowProcesses.FindAsync(processId, cancellationToken);
                if (process == null)
                    throw DomainException.NullReference(typeof(V1WorkflowProcess), processId);
                buffer = await this.Serializer.SerializeAsync(this.Mapper.Map<Integration.Models.V1WorkflowProcess>(process), cancellationToken);
                await File.WriteAllBytesAsync(Path.Combine(processesDirectory.FullName, $"{processId}{this.Options.Archiving.FileExtension}"), buffer, cancellationToken);
            }
            var archiveFilePath = Path.Combine(Path.GetTempPath(), $"{workflowInstance.Id}.zip");
            ZipFile.CreateFromDirectory(directory.FullName, archiveFilePath, CompressionLevel.Fastest, true);
            using var fileStream = File.OpenRead(archiveFilePath);
            var stream = new MemoryStream();
            await fileStream.CopyToAsync(stream, cancellationToken);
            await stream.FlushAsync(cancellationToken);
            stream.Position = 0;
            return this.Ok(stream);
        }

    }
}
