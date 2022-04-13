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
            : base("", "inject-node", null, 40)
        {
            this.Data = data;
            this.ComponentType = typeof(InjectNodeTemplate);
        }

        public string Data { get; set; }

    }
    

}
