namespace Synapse.Dashboard
{
    /// <summary>
    /// Represents a inject state node
    /// </summary>
    public class InjectNodeViewMode
        : WorkflowNodeViewModel
    {

        /// <summary>
        /// Initializes a new <see cref="InjectNodeViewMode"/>
        /// </summary>
        /// <param name="data"></param>
        public InjectNodeViewMode(string data)
            : base("", "inject-node")
        {
            this.Data = data;
        }

        public string Data { get; set; }

    }
    

}
