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

namespace Synapse.Cli.Commands.Workflows
{

    /// <summary>
    /// Represents the <see cref="Command"/> used to create a new <see cref="V1Workflow"/>
    /// </summary>
    internal class DeployWorkflowCommand
        : Command
    {

        /// <summary>
        /// Gets the <see cref="DeployWorkflowCommand"/>'s name
        /// </summary>
        public const string CommandName = "deploy";
        /// <summary>
        /// Gets the <see cref="DeployWorkflowCommand"/>'s description
        /// </summary>
        public const string CommandDescription = "Deploys a new workflow";

        /// <summary>
        /// Initializes a new <see cref="DeployWorkflowCommand"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="synapseManagementApi">The service used to interact with the remote Synapse Management API</param>
        /// <param name="workflowReader">The service used to read <see cref="WorkflowDefinition"/>s</param>
        public DeployWorkflowCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseManagementApi synapseManagementApi, IWorkflowReader workflowReader) 
            : base(serviceProvider, loggerFactory, synapseManagementApi, CommandName, CommandDescription)
        {
            this.WorkflowReader = workflowReader;
            this.Add(CommandOptions.File);
            this.Handler = CommandHandler.Create<string>(this.HandleAsync);
        }

        /// <summary>
        /// Gets the service used to read <see cref="WorkflowDefinition"/>s
        /// </summary>
        protected IWorkflowReader WorkflowReader { get; }

        /// <summary>
        /// Handles the <see cref="DeployWorkflowCommand"/>
        /// </summary>
        /// <param name="file">The definition's file path</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public async Task HandleAsync(string file)
        {
            var workflowDefinition = null as WorkflowDefinition;
            var stream = null as Stream;
            if (!string.IsNullOrWhiteSpace(file))
                stream = File.OpenRead(file);
            else
                throw new InvalidOperationException("You must specifiy exactly one of the following options: --file");
            try
            {
                workflowDefinition = await this.WorkflowReader.ReadAsync(stream);
                var workflow = await this.SynapseManagementApi.CreateWorkflowAsync(new() { Collection = workflowDefinition });
                Console.WriteLine($"The workflow with id '{workflow.Id}' has been successfully deployed");
            }
            catch
            {
                throw;
            }
            finally
            {
                await stream.DisposeAsync();
            }
        }

        private static class CommandOptions
        {

            public static Option<string> File
            {
                get
                {
                    var option = new Option<string>("--file")
                    {
                        Description = "The file that contains the workflow definition to deploy"
                    };
                    option.AddAlias("-f");
                    return option;
                }
            }

        }

    }

}
