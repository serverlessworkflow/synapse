// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Neuroglia;
using Neuroglia.Data.Infrastructure.ResourceOriented;

namespace Synapse.Cli.Commands.Workflows;

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
    /// <param name="api">The service used to interact with the remote Synapse API</param>
    /// <param name="jsonSerializer">The service used to serialize/deserialize to/from JSON</param>
    public RunWorkflowCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api, IJsonSerializer jsonSerializer)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.JsonSerializer = jsonSerializer;
        this.Add(new Argument<string>("name") { Description = "The name of the workflow to run" });
        this.Add(CommandOptions.Namespace);
        this.Add(CommandOptions.Version);
        this.Add(CommandOptions.Input);
        this.Handler = CommandHandler.Create<string, string, string, string>(this.HandleAsync);
    }

    /// <summary>
    /// Gets the service used to serialize/deserialize to/from JSON
    /// </summary>
    protected IJsonSerializer JsonSerializer { get; }

    /// <summary>
    /// Handles the <see cref="RunWorkflowCommand"/>
    /// </summary>
    /// <param name="name">The name of the workflow to run</param>
    /// <param name="namespace">The namespace the workflow to run belongs to. Defaults to 'default'.</param>
    /// <param name="version">The version of the workflow to run</param>
    /// <param name="input">The input data JSON</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task HandleAsync(string name, string @namespace, string version, string input)
    {
        var inputData = new EquatableDictionary<string, object> { };
        if (!string.IsNullOrWhiteSpace(input)) inputData = this.JsonSerializer.Deserialize<EquatableDictionary<string, object>>(input);
        var instance = await this.Api.WorkflowInstances.CreateAsync(new()
        {
            Metadata = new()
            {
                Namespace = @namespace,
                Name = $"{name}-"              
            },
            Spec = new()
            {
                Definition = new()
                {
                    Namespace = @namespace,
                    Name = name,
                    Version = version
                },
                Input = inputData
            }
        });
        Console.WriteLine($"workflow-instance/{instance.GetName()} created");
    }

    static class CommandOptions
    {

        public static Option<string> Namespace => new(["-n", "--namespace"], () => "default", "The namespace the workflow to run belongs to. Defaults to 'default'.");

        public static Option<string> Version => new(["-v", "--version"], "The version of the workflow to run.");

        public static Option<string> Input => new(["-i", "--input"], "The workflow's input data JSON.");

    }

}