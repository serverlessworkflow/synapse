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

using Google.Protobuf.Reflection;
using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;

namespace Synapse.Runtime.Executor.Services.Processors
{

    /// <summary>
    /// Represents the <see cref="IWorkflowActivityProcessor"/> used to process GRPC <see cref="FunctionDefinition"/>s
    /// </summary>
    public class GrpcFunctionProcessor
        : FunctionProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="GrpcFunctionProcessor"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivityDto"/> to process</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to process</param>
        /// <param name="function">The <see cref="FunctionDefinition"/> to process</param>
        public GrpcFunctionProcessor(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory,
            IOptions<ApplicationOptions> options, V1WorkflowActivityDto activity,
            ActionDefinition action, FunctionDefinition function)
            : base(serviceProvider, loggerFactory, context, activityProcessorFactory, options, activity, action, function)
        {

        }

        /// <inheritdoc/>
        protected override Task InitializeAsync(CancellationToken cancellationToken)
        {
            var proto = ""; //todo

            var fileDescriptorSet = new FileDescriptorSet();
            fileDescriptorSet.Add("" /* todo */, true, new StringReader(proto));
            fileDescriptorSet.Process();
            var errors = fileDescriptorSet.GetErrors();

            foreach(var file in fileDescriptorSet.Files)
            {
                foreach(var service in file.Services)
                {
                    foreach(var method in service.Methods)
                    {
                        //todo: if method.Deprecated => display warning
                        
                    }
                }
            }

            var channel = GrpcChannel.ForAddress($"{EnvironmentVariables.Api.Host.Value}:8080");
            var serviceName = "";
            var client = new GrpcClient(channel, serviceName);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            await base.ProcessAsync(cancellationToken);
            if (this.Activity.Status == V1WorkflowActivityStatus.Skipped)
                return;

            //await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, output), cancellationToken);
            //await this.OnCompletedAsync(cancellationToken);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                
            }
        }

    }

}
