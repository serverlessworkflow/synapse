namespace Synapse.Runtime.Services
{

    /// <summary>
    /// Defines the fundamentals of a service used to process <see cref="StateDefinition"/> activities
    /// </summary>
    public interface IStateProcessor
        : IWorkflowActivityProcessor
    {

        /// <summary>
        /// Gets the <see cref="StateDefinition"/> to process
        /// </summary>
        StateDefinition State { get; }

    }

    /// <summary>
    /// Defines the fundamentals of a service used to process <see cref="StateDefinition"/> activities
    /// </summary>
    public interface IStateProcessor<TState>
        : IStateProcessor
        where TState : StateDefinition
    {

        /// <summary>
        /// Gets the <see cref="StateDefinition"/> to process
        /// </summary>
        new TState State { get; }

    }

}
