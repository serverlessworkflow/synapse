using Synapse.Dashboard.Pages.Workflows.View.State;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Pages.Workflows.View.Actions
{
    /// <summary>
    /// Triggers state initialization
    /// </summary>
    public class InitializeState {}

    /// <summary>
    /// Returns the initial state
    /// </summary>
    public class InitializeStateSuccessful
    {
        public InitializeStateSuccessful(WorkflowViewState initialState)
        {
            this.InitialState = initialState ?? throw new ArgumentNullException(nameof(initialState));
        }

        /// <summary>
        /// Gets the initial state
        /// </summary>
        public WorkflowViewState InitialState { get; }
    }

    /// <summary>
    /// The action triggered to get a workflow by id
    /// </summary>
    public class GetWorkflowById
    {
        /// <summary>
        /// Creates a new <see cref="GetWorkflowById"/>
        /// </summary>
        /// <param name="workflowId">The id of the workflow to get</param>
        /// <exception cref="ArgumentNullException"></exception>
        public GetWorkflowById(string workflowId)
        {
            this.WorkflowId = workflowId ?? throw new ArgumentNullException(nameof(workflowId));
        }

        /// <summary>
        /// Gets the workflow id
        /// </summary>
        public string WorkflowId { get; }
    }

    /// <summary>
    /// The action to set the state's workflow
    /// </summary>
    public class SetWorkflow
    {
        /// <summary>
        /// Creates a new <see cref="SetWorkflow"/>
        /// </summary>
        /// <param name="workflow">The new workflow value</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SetWorkflow(V1Workflow workflow)
        {
            this.Workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
        }

        /// <summary>
        /// Gets the workflow
        /// </summary>
        public V1Workflow Workflow { get; }
    }

    /// <summary>
    /// The action to set the state's workflow instances
    /// </summary>
    public class SetWorkflowInstances
    {
        /// <summary>
        /// Creates a new <see cref="SetWorkflowInstances"/>
        /// </summary>
        /// <param name="workflow">The workflow instances</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SetWorkflowInstances(ICollection<V1WorkflowInstance> workflowInstances)
        {
            this.WorkflowInstances = workflowInstances ?? throw new ArgumentNullException(nameof(workflowInstances));
        }

        /// <summary>
        /// Gets the workflow instances
        /// </summary>
        public ICollection<V1WorkflowInstance> WorkflowInstances { get; }
    }

    /// <summary>
    /// The action to set the active instance
    /// </summary>
    public class SetActiveInstance
    {
        /// <summary>
        /// Creates a new <see cref="SetActiveInstance"/>
        /// </summary>
        /// <param name="workflow">The new workflow value</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SetActiveInstance(V1WorkflowInstance? instance)
        {
            this.Instance = instance;
        }

        /// <summary>
        /// Gets the active instance
        /// </summary>
        public V1WorkflowInstance? Instance { get; }
    }
}
