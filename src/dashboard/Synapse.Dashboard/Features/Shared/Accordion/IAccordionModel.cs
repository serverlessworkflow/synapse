using Microsoft.AspNetCore.Components;

namespace Synapse.Dashboard
{
    /// <summary>
    /// Represents the state of an accordion component
    /// </summary>
    public interface IAccordionModel
    {
        /// <summary>
        /// Gets/sets the header of the accordion
        /// </summary>
        public RenderFragment Header { get; set; }
        /// <summary>
        /// Gets/sets the content of the accordion
        /// </summary>
        public RenderFragment Body { get; set; }
        /// <summary>
        /// Gets/sets if the accordion is opened
        /// </summary>
        public bool IsExpanded { get; set; }
        /// <summary>
        /// Gets/sets if the accordion can be opened at the same time than others
        /// </summary>
        public bool AllowsMultiple { get; set; }
    }
}
