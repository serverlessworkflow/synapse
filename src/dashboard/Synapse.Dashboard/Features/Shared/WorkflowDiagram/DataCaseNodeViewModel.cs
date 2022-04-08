using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard
{
    /// <summary>
    /// Represents a data case <see cref="NodeViewModel"/>
    /// </summary>
    public class DataCaseNodeViewModel
        : WorkflowNodeViewModel
    {

        /// <summary>
        /// Initializes a new <see cref="DataCaseNodeViewModel"/>
        /// </summary>
        /// <param name="dataCase">The name of the <see cref="DataCaseDefinition"/> the <see cref="NodeViewModel"/> represents</param>
        public DataCaseNodeViewModel(string dataCaseName)
            : base("", "datacase-node")
        {
            this.DataCaseName = dataCaseName;
        }

        /// <summary>
        /// Gets the name of the <see cref="DataCaseDefinition"/> the <see cref="NodeViewModel"/> represents
        /// </summary>
        public string DataCaseName { get; }

    }
    

}
