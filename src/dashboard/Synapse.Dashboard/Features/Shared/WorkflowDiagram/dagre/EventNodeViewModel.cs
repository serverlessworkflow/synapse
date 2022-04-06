using Blazor.Diagrams.Core.Models;
using ServerlessWorkflow.Sdk;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard
{
    /// <summary>
    /// Represents an <see cref="EventDefinition"/> reference <see cref="NodeViewModel"/>
    /// </summary>
    public class EventNodeViewModel
        : WorkflowNodeViewModel
    {

        /// <summary>
        /// Initializes a new <see cref="EventNodeViewModel"/>
        /// </summary>
        /// <param name="kind">The kind of the <see cref="EventDefinition"/> the <see cref="EventNodeViewModel"/> represents</param>
        /// <param name="refName">The name of the <see cref="EventDefinition"/> the <see cref="EventNodeViewModel"/> represents</param>
        public EventNodeViewModel(EventKind kind, string refName)
            :base(refName)
        {
            this.Kind = kind;
            this.RefName = refName;
        }

        /// <summary>
        /// Gets the kind of the <see cref="EventDefinition"/> the <see cref="EventNodeViewModel"/> represents
        /// </summary>
        public EventKind Kind { get; }

        /// <summary>
        /// Gets the name of the <see cref="EventDefinition"/> the <see cref="EventNodeViewModel"/> represents
        /// </summary>
        public string RefName { get; }

    }

}
