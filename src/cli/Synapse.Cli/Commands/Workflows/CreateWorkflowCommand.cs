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

using Neuroglia.Data;
using Neuroglia.Data.Infrastructure.ResourceOriented;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Cli.Commands.Workflows;

/// <summary>
/// Represents the <see cref="Command"/> used to create a new <see cref="Workflow"/>
/// </summary>
internal class CreateWorkflowCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="CreateWorkflowCommand"/>'s name
    /// </summary>
    public const string CommandName = "create";
    /// <summary>
    /// Gets the <see cref="CreateWorkflowCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Creates a new workflow.";

    /// <inheritdoc/>
    public CreateWorkflowCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api, IWorkflowDefinitionReader workflowDefinitionReader)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.WorkflowDefinitionReader = workflowDefinitionReader;
        this.Add(CommandOptions.File);
        this.Handler = CommandHandler.Create<string>(this.HandleAsync);
    }

    /// <summary>
    /// Gets the service used to read <see cref="WorkflowDefinition"/>s
    /// </summary>
    protected IWorkflowDefinitionReader WorkflowDefinitionReader { get; }

    /// <summary>
    /// Handles the <see cref="CreateWorkflowCommand"/>
    /// </summary>
    /// <param name="file">The path to the file that contains the definition of the workflow to create</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task HandleAsync(string file)
    {
        Stream? stream;
        if (!string.IsNullOrWhiteSpace(file)) stream = File.OpenRead(file);
        else throw new InvalidOperationException("You must specify exactly one of the following options: --file");
        var workflowDefinition = await this.WorkflowDefinitionReader.ReadAsync(stream);
        Workflow? workflow = null;
        try { workflow = await this.Api.Workflows.GetAsync(workflowDefinition.Document.Name, workflowDefinition.Document.Namespace); }
        catch { }
        if (workflow == null)
        {
            workflow = new() 
            { 
                Metadata = new() 
                { 
                    Namespace = workflowDefinition.Document.Namespace, 
                    Name = workflowDefinition.Document.Name
                }, 
                Spec = new() 
                { 
                    Versions = [workflowDefinition] 
                } 
            };
            workflow = await this.Api.Workflows.CreateAsync(workflow);
        }
        else
        {
            var originalWorkflow = workflow.Clone()!;
            workflow.Spec.Versions.Add(workflowDefinition);
            var patch = JsonPatchUtility.CreateJsonPatchFromDiff(originalWorkflow, workflow);
            workflow = await this.Api.Workflows.PatchAsync(workflowDefinition.Document.Name, workflowDefinition.Document.Namespace, new(PatchType.JsonPatch, patch));
        }
        Console.WriteLine($"workflow/{workflow.GetName()}:{workflowDefinition.Document.Version} created");
        await stream.DisposeAsync();
    }

    static class CommandOptions
    {

        public static Option<string> File
        {
            get
            {
                var option = new Option<string>("--file")
                {
                    Description = "The file that contains the definition of the workflow to create."
                };
                option.AddAlias("-f");
                return option;
            }
        }

    }

}
