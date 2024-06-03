using Synapse.Cli.Commands.WorkflowInstances;

namespace Synapse.Cli.Commands;

/// <summary>
/// Represents the <see cref="Command"/> used to manage <see cref="WorkflowInstance"/>s
/// </summary>
public class WorkflowInstanceCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="WorkflowInstanceCommand"/>'s name
    /// </summary>
    public const string CommandName = "workflow-instance";
    /// <summary>
    /// Gets the <see cref="WorkflowInstanceCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Manages workflow instances";

    /// <inheritdoc/>
    public WorkflowInstanceCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.AddAlias("workflow-instances");
        this.AddAlias("wfi");
        this.AddCommand(ActivatorUtilities.CreateInstance<GetWorkflowInstanceCommand>(this.ServiceProvider));
        this.AddCommand(ActivatorUtilities.CreateInstance<GetWorkflowInstanceOutputCommand>(this.ServiceProvider));
        this.AddCommand(ActivatorUtilities.CreateInstance<ListWorkflowInstancesCommand>(this.ServiceProvider));
        this.AddCommand(ActivatorUtilities.CreateInstance<DeleteWorkflowInstanceCommand>(this.ServiceProvider));
        //this.AddCommand(ActivatorUtilities.CreateInstance<SuspendWorkflowInstanceCommand>(this.ServiceProvider));
        //this.AddCommand(ActivatorUtilities.CreateInstance<ResumeWorkflowInstanceCommand>(this.ServiceProvider));
        //this.AddCommand(ActivatorUtilities.CreateInstance<CancelWorkflowInstanceCommand>(this.ServiceProvider));
    }

}