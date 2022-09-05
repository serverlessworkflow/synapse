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

using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Neuroglia.Mapping;
using Neuroglia.Mediation;
using Synapse.Application.Services;
using Synapse.Integration.Commands.Correlations;
using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Commands.Workflows;
using Synapse.Integration.Models;

namespace Synapse.Apis.Management.Ipc
{

    /// <summary>
    /// Represents the Intra-Process Communication (IPC) implementation of the <see cref="ISynapseManagementApi"/>
    /// </summary>
    public class SynapseIpcManagementApiClient
        : ISynapseManagementApi
    {

        /// <summary>
        /// Initializes a new <see cref="SynapseIpcManagementApiClient"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="queryOptionsParser">The service used to parse <see cref="ODataQueryOptions"/></param>
        protected SynapseIpcManagementApiClient(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IODataQueryOptionsParser queryOptionsParser)
        {
            this.Logger = loggerFactory.CreateLogger(this.GetType());
            this.Mediator = mediator;
            this.Mapper = mapper;
            this.QueryOptionsParser = queryOptionsParser;
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to mediate calls
        /// </summary>
        protected IMediator Mediator { get; }

        /// <summary>
        /// Gets the service used to map objects
        /// </summary>
        protected IMapper Mapper { get; }

        /// <summary>
        /// Gets the service used to parse <see cref="ODataQueryOptions"/>
        /// </summary>
        protected IODataQueryOptionsParser QueryOptionsParser { get; }

        #region Workflows

        /// <inheritdoc/>
        public virtual async Task<V1Workflow> CreateWorkflowAsync(V1CreateWorkflowCommand command, CancellationToken cancellationToken = default)
        {
            if(command == null)
                throw new ArgumentNullException(nameof(command));
            return await this.Mediator.ExecuteAndUnwrapAsync(this.Mapper.Map<Application.Commands.Workflows.V1CreateWorkflowCommand>(command), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1Workflow> UploadWorkflowAsync(V1UploadWorkflowCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            return await this.Mediator.ExecuteAndUnwrapAsync(this.Mapper.Map<Application.Commands.Workflows.V1UploadWorkflowCommand>(command), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1Workflow>> GetWorkflowsAsync(string? query, CancellationToken cancellationToken = default)
        {
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Queries.Generic.V1FilterQuery<V1Workflow>(this.QueryOptionsParser.Parse<V1Workflow>(query)), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1Workflow>> GetWorkflowsAsync(CancellationToken cancellationToken = default)
        {
            return await this.GetWorkflowsAsync(null!, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1Workflow> GetWorkflowByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Queries.Generic.V1FindByIdQuery<V1Workflow, string>(id), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<Stream> ArchiveWorkflowAsync(string id, string? version = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Commands.Workflows.V1ArchiveWorkflowCommand(id, version), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task DeleteWorkflowAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            await this.Mediator.ExecuteAndUnwrapAsync(new Application.Commands.Generic.V1DeleteCommand<V1Workflow, string>(id), cancellationToken);
        }

        #endregion region

        #region WorkflowInstances

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstance> CreateWorkflowInstanceAsync(V1CreateWorkflowInstanceCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            return await this.Mediator.ExecuteAndUnwrapAsync(this.Mapper.Map<Application.Commands.WorkflowInstances.V1CreateWorkflowInstanceCommand>(command), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstance> StartWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Commands.WorkflowInstances.V1StartWorkflowInstanceCommand(id), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstance> GetWorkflowInstanceByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Queries.Generic.V1FindByIdQuery<V1WorkflowInstance, string>(id), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowInstance>> GetWorkflowInstancesAsync(string? query, CancellationToken cancellationToken = default)
        {
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Queries.Generic.V1FilterQuery<V1WorkflowInstance>(this.QueryOptionsParser.Parse<V1WorkflowInstance>(query)), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowInstance>> GetWorkflowInstancesAsync(CancellationToken cancellationToken = default)
        {
            return await this.GetWorkflowInstancesAsync(null, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task SuspendWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            await this.Mediator.ExecuteAndUnwrapAsync(new Application.Commands.WorkflowInstances.V1SuspendWorkflowInstanceCommand(id), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task ResumeWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            await this.Mediator.ExecuteAndUnwrapAsync(new Application.Commands.WorkflowInstances.V1ResumeWorkflowInstanceCommand(id), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task CancelWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            await this.Mediator.ExecuteAndUnwrapAsync(new Application.Commands.WorkflowInstances.V1CancelWorkflowInstanceCommand(id), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<string> GetWorkflowInstanceLogsAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Queries.WorkflowInstances.V1GetWorkflowInstanceLogsQuery(id), cancellationToken);
        }


        /// <inheritdoc/>
        public virtual async Task<Stream> ArchiveWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Commands.WorkflowInstances.V1ArchiveWorkflowInstanceCommand(id), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task DeleteWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            await this.Mediator.ExecuteAndUnwrapAsync(new Application.Commands.WorkflowInstances.V1DeleteWorkflowInstanceCommand(id), cancellationToken);
        }

        #endregion

        #region Correlations

        /// <inheritdoc/>
        public virtual async Task<V1Correlation> CreateCorrelationAsync(V1CreateCorrelationCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            return await this.Mediator.ExecuteAndUnwrapAsync(this.Mapper.Map<Application.Commands.Correlations.V1CreateCorrelationCommand>(command), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1Correlation>> GetCorrelationsAsync(string? query, CancellationToken cancellationToken = default)
        {
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Queries.Generic.V1FilterQuery<V1Correlation>(this.QueryOptionsParser.Parse<V1Correlation>(query)), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1Correlation>> GetCorrelationsAsync(CancellationToken cancellationToken = default)
        {
            return await this.GetCorrelationsAsync(null!, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1Correlation> GetCorrelationByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Queries.Generic.V1FindByIdQuery<V1Correlation, string>(id), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task DeleteCorrelationAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            await this.Mediator.ExecuteAndUnwrapAsync(new Application.Commands.Generic.V1DeleteCommand<V1Correlation, string>(id), cancellationToken);
        }

        #endregion region

        #region OperationalReports

        /// <inheritdoc/>
        public virtual async Task<V1OperationalReport> GetOperationalReportAsync(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            if (!date.HasValue)
                date = DateTime.Now;
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Queries.Generic.V1FindByIdQuery<V1OperationalReport, string>(V1OperationalReport.GetIdFor(date.Value)), cancellationToken);
        }

        #endregion

    }

}
