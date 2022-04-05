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

using Neuroglia.Serialization;

namespace Synapse.Cli.Commands.Workflows
{

    /// <summary>
    /// Represents the <see cref="Command"/> used to run a workflow
    /// </summary>
    internal class RunWorkflowCommand
        : Command
    {

        /// <summary>
        /// Gets the <see cref="RunWorkflowCommand"/>'s name
        /// </summary>
        public const string CommandName = "run";
        /// <summary>
        /// Gets the <see cref="RunWorkflowCommand"/>'s description
        /// </summary>
        public const string CommandDescription = "Runs a workflow";

        /// <summary>
        /// Initializes a new <see cref="Command"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="synapseManagementApi">The service used to interact with the remote Synapse Management API</param>
        /// <param name="jsonSerializer">The service used to serialize/deserialize to/from JSON</param>
        public RunWorkflowCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseManagementApi synapseManagementApi, IJsonSerializer jsonSerializer) 
            : base(serviceProvider, loggerFactory, synapseManagementApi, CommandName, CommandDescription)
        {
            this.JsonSerializer = jsonSerializer;
            this.Add(new Argument<string>("id") { Description = "The id of the workflow to run (ex: myworkflow:1.0). Note that failing to specify the version will run the latest version of the specified workflow" });
            this.Add(CommandOptions.Input);
            this.Handler = CommandHandler.Create<string, string>(this.HandleAsync);
        }

        /// <summary>
        /// Gets the service used to serialize/deserialize to/from JSON
        /// </summary>
        protected IJsonSerializer JsonSerializer { get; }

        /// <summary>
        /// Handles the <see cref="DeployWorkflowCommand"/>
        /// </summary>
        /// <param name="id">The id of the workflow to run. Failing to specify the version will run the latest version of the specified workflow</param>
        /// <param name="input">The input data JSON</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public async Task HandleAsync(string id, string input)
        {
            var inputData = null as Dynamic;
            if (!string.IsNullOrWhiteSpace(input))
                inputData = await this.JsonSerializer.DeserializeAsync<Dynamic>(input);
            var instance = await this.SynapseManagementApi.CreateWorkflowInstanceAsync(new()
            {
                WorkflowId = id,
                ActivationType = V1WorkflowInstanceActivationType.Manual,
                AutoStart = true,
                InputData = inputData
            });
            Console.WriteLine($"The workflow instance with id '{instance.Id}' has been successfully created and started");
        }

        private static class CommandOptions
        {

            public static Option<string> Input
            {
                get
                {
                    var option = new Option<string>("--input")
                    {
                        Description = "The workflow's input data "
                    };
                    option.AddAlias("-i");
                    return option;
                }
            }

        }

    }

}
