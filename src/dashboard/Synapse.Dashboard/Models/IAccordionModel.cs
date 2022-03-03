using Microsoft.AspNetCore.Components;

namespace Synapse.Dashboard.Models
{
    /// <summary>
    /// Represents the state of an accordion component
    /// </summary>
    public interface IAccordionModel
    {
        /// <summary>
        /// Gets/sets the title of the accordion
        /// </summary>
        public RenderFragment Title { get; set; }
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
