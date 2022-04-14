namespace Synapse.Dashboard
{
    /// <summary>
    /// Represents a sleep state node
    /// </summary>
    public class SleepNodeViewModel
        : WorkflowNodeViewModel
    {

        /// <summary>
        /// Initializes a new <see cref="InjectNodeViewModel"/>
        /// </summary>
        /// <param name="data"></param>
        public SleepNodeViewModel(TimeSpan delay)
            : base("", "sleep-node")
        {
            this.Delay = delay;
        }

        public TimeSpan Delay { get; set; }

    }
    

}
