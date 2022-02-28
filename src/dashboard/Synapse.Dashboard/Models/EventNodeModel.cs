using Blazor.Diagrams.Core.Models;
using ServerlessWorkflow.Sdk;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard.Models
{
    /// <summary>
    /// Represents an <see cref="EventDefinition"/> reference <see cref="NodeModel"/>
    /// </summary>
    public class EventNodeModel
        : WorkflowNodeModel
    {

        /// <summary>
        /// Initializes a new <see cref="EventNodeModel"/>
        /// </summary>
        /// <param name="kind">The kind of the <see cref="EventDefinition"/> the <see cref="EventNodeModel"/> represents</param>
        /// <param name="refName">The name of the <see cref="EventDefinition"/> the <see cref="EventNodeModel"/> represents</param>
        public EventNodeModel(EventKind kind, string refName)
        {
            this.Kind = kind;
            this.RefName = refName;
            this.Title = refName;
            this.AddPort(PortAlignment.Top);
            this.AddPort(PortAlignment.Bottom);
        }

        /// <summary>
        /// Gets the kind of the <see cref="EventDefinition"/> the <see cref="EventNodeModel"/> represents
        /// </summary>
        public EventKind Kind { get; }

        /// <summary>
        /// Gets the name of the <see cref="EventDefinition"/> the <see cref="EventNodeModel"/> represents
        /// </summary>
        public string RefName { get; }

    }

}
