namespace Synapse.Dashboard
{
    /// <summary>
    /// Represents a sleep state node
    /// </summary>
    public class SleepNodeViewModel
        : WorkflowNodeViewModel
    {

        /// <summary>
        /// Initializes a new <see cref="SleepNodeViewModel"/>
        /// </summary>
        /// <param name="data"></param>
        public SleepNodeViewModel(TimeSpan delay)
            : base(System.Xml.XmlConvert.ToString(delay), "sleep-node")
        {
            this.Delay = delay;
            this.ComponentType = typeof(SleepNodeTemplate);
        }

        public TimeSpan Delay { get; set; }

    }
    

}
