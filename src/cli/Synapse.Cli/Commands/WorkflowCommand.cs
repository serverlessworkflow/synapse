using Synapse.Cli.Commands.Workflows;

namespace Synapse.Cli.Commands;

/// <summary>
/// Represents the <see cref="Command"/> used to manage <see cref="Workflow"/>s
/// </summary>
public class WorkflowCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="WorkflowCommand"/>'s name
    /// </summary>
    public const string CommandName = "workflow";
    /// <summary>
    /// Gets the <see cref="WorkflowCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Manages workflows";

    /// <inheritdoc/>
    public WorkflowCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.AddAlias("workflows");
        this.AddAlias("wf");
        this.AddCommand(ActivatorUtilities.CreateInstance<CreateWorkflowCommand>(this.ServiceProvider));
        this.AddCommand(ActivatorUtilities.CreateInstance<RunWorkflowCommand>(this.ServiceProvider));
        this.AddCommand(ActivatorUtilities.CreateInstance<GetWorkflowCommand>(this.ServiceProvider));
        this.AddCommand(ActivatorUtilities.CreateInstance<ListWorkflowsCommand>(this.ServiceProvider));
        this.AddCommand(ActivatorUtilities.CreateInstance<DeleteWorkflowCommand>(this.ServiceProvider));
    }

}
