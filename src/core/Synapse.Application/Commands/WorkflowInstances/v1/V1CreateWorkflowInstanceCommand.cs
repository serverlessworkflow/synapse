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

using Neuroglia.Data.Expressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Models;

namespace Synapse.Application.Commands.WorkflowInstances
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to create a new <see cref="V1Workflow"/>
    /// </summary>
    [DataTransferObjectType(typeof(V1CreateWorkflowInstanceCommandDto))]
    public class V1CreateWorkflowInstanceCommand
        : Command<V1WorkflowInstanceDto>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CreateWorkflowInstanceCommand"/>
        /// </summary>
        protected V1CreateWorkflowInstanceCommand()
        {
            this.WorkflowId = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1CreateWorkflowInstanceCommand"/>
        /// </summary>
        /// <param name="workflowId">The id of the <see cref="V1Workflow"/> to instanciate</param>
        /// <param name="activationType">The <see cref="V1Workflow"/>'s activation type</param>
        /// <param name="inputData">The input data of the <see cref="V1WorkflowInstance"/> to create</param>
        /// <param name="triggerEvents"></param>
        public V1CreateWorkflowInstanceCommand(string workflowId, V1WorkflowInstanceActivationType activationType, object? inputData, IReadOnlyCollection<V1CloudEvent>? triggerEvents)
        {
            this.WorkflowId = workflowId;
            this.ActivationType = activationType;
            this.InputData = inputData;
            this.TriggerEvents = triggerEvents;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1Workflow"/> to instanciate
        /// </summary>
        public virtual string WorkflowId { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1Workflow"/>'s activation type
        /// </summary>
        public virtual V1WorkflowInstanceActivationType ActivationType { get; protected set; }

        /// <summary>
        /// Gets the input data of the <see cref="V1WorkflowInstance"/> to create
        /// </summary>
        public virtual object? InputData { get; protected set; }

        /// <summary>
        /// Gets an <see cref="IReadOnlyCollection{T}"/> containing the descriptors of the <see cref="CloudEvent"/>s that have triggered the activation of the <see cref="V1WorkflowInstance"/> to create
        /// </summary>
        public virtual IReadOnlyCollection<V1CloudEvent>? TriggerEvents { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1CreateWorkflowInstanceCommand"/>s
    /// </summary>
    public class V1CreateWorkflowInstanceCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1CreateWorkflowInstanceCommand, V1WorkflowInstanceDto>
    {

        /// <inheritdoc/>
        public V1CreateWorkflowInstanceCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IHttpClientFactory httpClientFactory,
            IRepository<V1Workflow> workflows, IRepository<V1WorkflowInstance> workflowInstances, IExpressionEvaluatorProvider expressionEvaluatorProvider)
            : base(loggerFactory, mediator, mapper)
        {
            this.HttpClientFactory = httpClientFactory;
            this.Workflows = workflows;
            this.WorkflowInstances = workflowInstances;
            this.ExpressionEvaluatorProvider = expressionEvaluatorProvider;
        }

        /// <summary>
        /// Gets the service used to create <see cref="HttpClient"/>s
        /// </summary>
        protected IHttpClientFactory HttpClientFactory { get; }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1Workflow"/>s
        /// </summary>
        protected IRepository<V1Workflow> Workflows { get; }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<V1WorkflowInstance> WorkflowInstances { get;}

        /// <summary>
        /// Gets the service used to provide <see cref="IExpressionEvaluator"/>s
        /// </summary>
        protected IExpressionEvaluatorProvider ExpressionEvaluatorProvider { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<V1WorkflowInstanceDto>> HandleAsync(V1CreateWorkflowInstanceCommand command, CancellationToken cancellationToken = default)
        {
            var workflow = await this.Workflows.FindAsync(command.WorkflowId, cancellationToken);
            if(workflow == null)
                throw DomainException.NullReference(typeof(V1Workflow), command.WorkflowId);
            string? key = null;
            var dataInputSchema = workflow.Definition.DataInputSchema?.Schema;
            if (dataInputSchema == null
                && workflow.Definition.DataInputSchemaUri != null)
            {
                using var httpClient = this.HttpClientFactory.CreateClient();
                var json = await httpClient.GetStringAsync(workflow.Definition.DataInputSchemaUri, cancellationToken);
                dataInputSchema = JSchema.Parse(json);
            }
            if(dataInputSchema != null)
            {
                var input = command.InputData;
                var jobj = null as JObject;
                if (input == null)
                    jobj = new JObject();
                else
                    jobj = JObject.FromObject(input);
                if (!jobj.IsValid(dataInputSchema, out IList<string> errors))
                    throw new DomainArgumentException($"Invalid workflow input data:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}", nameof(command.InputData));
            }
            if (!string.IsNullOrWhiteSpace(workflow.Definition.Key)
                && command.InputData != null)
            {
                try
                {
                    key = this.ExpressionEvaluatorProvider.GetEvaluator(workflow.Definition.ExpressionLanguage)!.Evaluate(workflow.Definition.Key, command.InputData)?.ToString();
                }
                catch { }
            }
            if (string.IsNullOrWhiteSpace(key))
                key = Guid.NewGuid().ToBase64();
            while (await this.WorkflowInstances.ContainsAsync(V1WorkflowInstance.BuildUniqueIdentifier(key, workflow), cancellationToken))
            {
                key = Guid.NewGuid().ToBase64();
            }
            var workflowInstance = await this.WorkflowInstances.AddAsync(new(key, workflow, command.ActivationType, command.InputData, command.TriggerEvents), cancellationToken);
            await this.WorkflowInstances.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<V1WorkflowInstanceDto>(workflowInstance));
        }

    }

}
