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

using ServerlessWorkflow.Sdk.Models;
using ServerlessWorkflow.Sdk.Services.IO;
using System.ComponentModel.DataAnnotations;

namespace Synapse.Application.Commands.Workflows
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to upload a new <see cref="V1UploadWorkflowCommand"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.Workflows.V1UploadWorkflowCommand))]
    public class V1UploadWorkflowCommand
        : Command<Integration.Models.V1Workflow>
    {

        /// <summary>
        /// Initializes a new <see cref="V1UploadWorkflowCommand"/>
        /// </summary>
        protected V1UploadWorkflowCommand()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1UploadWorkflowCommand"/>
        /// </summary>
        /// <param name="definitionFile">The <see cref="IFormFile"/> that contains the <see cref="WorkflowDefinition"/> to read</param>
        /// <param name="resourceFiles">An <see cref="IEnumerable{T}"/> of the <see cref="IFormFile"/>s that contain the resources of the <see cref="WorkflowDefinition"/> to read</param>
        public V1UploadWorkflowCommand(IFormFile definitionFile, IEnumerable<IFormFile>? resourceFiles)
        {
            this.DefinitionFile = definitionFile;
            this.ResourceFiles = resourceFiles;
        }

        /// <summary>
        /// Gets the <see cref="IFormFile"/> that contains the <see cref="WorkflowDefinition"/> to read
        /// </summary>
        [Required]
        public virtual IFormFile DefinitionFile { get; protected set; } = null!;

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> of the <see cref="IFormFile"/>s that contain the resources of the <see cref="WorkflowDefinition"/> to read
        /// </summary>
        public virtual IEnumerable<IFormFile>? ResourceFiles { get; protected set; } = null!;

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1UploadWorkflowCommand"/>s
    /// </summary>
    public class V1UploadWorkflowCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1UploadWorkflowCommand, Integration.Models.V1Workflow>
    {

        /// <summary>
        /// Initializes a new <see cref="V1UploadWorkflowCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="workflowReader">The service used to read <see cref="WorkflowDefinition"/>s</param>
        public V1UploadWorkflowCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IWorkflowReader workflowReader) 
            : base(loggerFactory, mediator, mapper)
        {
            this.WorkflowReader = workflowReader;
        }

        /// <summary>
        /// Gets the service used to read <see cref="WorkflowDefinition"/>s
        /// </summary>
        protected IWorkflowReader WorkflowReader { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Integration.Models.V1Workflow>> HandleAsync(V1UploadWorkflowCommand command, CancellationToken cancellationToken = default)
        {
            var directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            if (directory.Exists)
                directory.Delete(true);
            directory.Create();
            var filePath = Path.Combine(directory.FullName, command.DefinitionFile.FileName);
            using var definitionFileStream = new FileStream(filePath, FileMode.Create);
            await command.DefinitionFile.CopyToAsync(definitionFileStream, cancellationToken);
            await definitionFileStream.FlushAsync(cancellationToken);
            definitionFileStream.Position = 0;
            if(command.ResourceFiles != null)
            {
                foreach (var resourceFile in command.ResourceFiles)
                {
                    filePath = Path.Combine(directory.FullName, resourceFile.FileName);
                    using var resourceFileStream = new FileStream(filePath, FileMode.Create);
                    await resourceFile.CopyToAsync(resourceFileStream, cancellationToken);
                    await resourceFileStream.FlushAsync(cancellationToken);
                }
            }
            var definition = await this.WorkflowReader.ReadAsync(definitionFileStream, new() { BaseDirectory = directory.FullName }, cancellationToken);
            var workflow = await this.Mediator.ExecuteAndUnwrapAsync(new V1CreateWorkflowCommand(definition, false), cancellationToken);
            return this.Ok(workflow);
        }

    }

}
