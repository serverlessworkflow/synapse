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
        var workflow = await this.Api.Workflows.GetAsync(workflowDefinition.Document.Name, workflowDefinition.Document.Namespace);
        if (workflow == null)
        {
            workflow = new() 
            { 
                Metadata = new() 
                { 
                    Namespace = "default", 
                    Name = "test" 
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
        Console.WriteLine($"The workflow '{workflow.GetQualifiedName()}:{workflowDefinition.Document.Version}' has been successfully created");
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
