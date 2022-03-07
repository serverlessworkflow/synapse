using Blazor.Diagrams.Core;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard
{
    public interface IWorkflowDiagramBuilder
        : IDisposable
    {
        /// <summary>
        /// Builds a <see cref="Diagram"/> from the provided <see cref="WorkflowDefinition"/>
        /// </summary>
        /// <param name="definition">The <see cref="WorkflowDefinition"/> to build <see cref="Diagram"/> for</param>
        /// <returns>The <see cref="WorkflowDefinition"/> <see cref="Diagram"/></returns>
        Task<Diagram> BuildDiagram(WorkflowDefinition definition);

    }
}
