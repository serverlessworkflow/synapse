using Blazor.Diagrams.Core.Models;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard.Models
{
    /// <summary>
    /// Represents a data case <see cref="NodeModel"/>
    /// </summary>
    public class DataCaseNodeModel
        : NodeModel
    {

        /// <summary>
        /// Initializes a new <see cref="DataCaseNodeModel"/>
        /// </summary>
        /// <param name="dataCase">The name of the <see cref="DataCaseDefinition"/> the <see cref="NodeModel"/> represents</param>
        public DataCaseNodeModel(string dataCaseName)
        {
            this.DataCaseName = dataCaseName;
            this.AddPort(PortAlignment.Top);
            this.AddPort(PortAlignment.Bottom);
        }

        /// <summary>
        /// Gets the name of the <see cref="DataCaseDefinition"/> the <see cref="NodeModel"/> represents
        /// </summary>
        public string DataCaseName { get; }

    }
    

}
