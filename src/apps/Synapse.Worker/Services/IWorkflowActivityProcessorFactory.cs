namespace Synapse.Worker.Services
{

    /// <summary>
    /// Defines the fundamentals of a service used to create <see cref="IWorkflowActivityProcessor"/>s
    /// </summary>
    public interface IWorkflowActivityProcessorFactory
    {

        /// <summary>
        /// Creates a new <see cref="IWorkflowActivityProcessor"/> for the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to create a new <see cref="IWorkflowActivityProcessor"/> for</param>
        /// <returns>A new <see cref="IWorkflowActivityProcessor"/></returns>
        IWorkflowActivityProcessor Create(V1WorkflowActivity activity);

        /// <summary>
        /// Creates a new <see cref="IWorkflowActivityProcessor"/> for the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <typeparam name="TActivity">The type of the <see cref="V1WorkflowActivity"/> to create a new <see cref="IWorkflowActivityProcessor"/> for</typeparam>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to create a new <see cref="IWorkflowActivityProcessor"/> for</param>
        /// <returns>A new <see cref="IWorkflowActivityProcessor"/></returns>
        IWorkflowActivityProcessor<TActivity> Create<TActivity>(TActivity activity)
            where TActivity : V1WorkflowActivity;

        /// <summary>
        /// Creates a new <see cref="IWorkflowActivityProcessor"/> for the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <typeparam name="TProcessor">The type of the <see cref="IWorkflowActivityProcessor"/> to create</typeparam>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to create a new <see cref="IWorkflowActivityProcessor"/> for</param>
        /// <returns>A new <see cref="IWorkflowActivityProcessor"/></returns>
        TProcessor Create<TProcessor>(V1WorkflowActivity activity);

        /// <summary>
        /// Creates a new <see cref="IWorkflowActivityProcessor"/> for the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <typeparam name="TActivity">The type of the <see cref="V1WorkflowActivity"/> to create a new <see cref="IWorkflowActivityProcessor"/> for</typeparam>
        /// <typeparam name="TProcessor">The type of the <see cref="IWorkflowActivityProcessor"/> to create</typeparam>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to create a new <see cref="IWorkflowActivityProcessor"/> for</param>
        /// <returns>A new <see cref="IWorkflowActivityProcessor"/></returns>
        TProcessor Create<TProcessor, TActivity>(TActivity activity)
            where TProcessor : IWorkflowActivityProcessor<TActivity>
            where TActivity : V1WorkflowActivity;

    }

}
