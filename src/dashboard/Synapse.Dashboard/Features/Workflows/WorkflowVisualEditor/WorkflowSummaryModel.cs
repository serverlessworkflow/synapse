using ServerlessWorkflow.Sdk.Models;
using System.ComponentModel.DataAnnotations;

namespace Synapse.Dashboard
{
    public class WorkflowSummaryModel
    {
        /// <summary>
        /// Gets/sets the System.Version of the Serverless Workflow schema to use
        /// </summary>
        [Required]
        [Display(Name = "Specification version")]
        public virtual string SpecVersion { get; } = Constants.SpecVersion;
        /// <summary>
        /// Gets/sets the ServerlessWorkflow.Sdk.Models.WorkflowDefinition's version
        /// </summary>
        [Required]
        [Display(Name = "Version")]
        public virtual string Version { get; set; } = Constants.DefinitionVersion;
        /// <summary>
        /// Gets/sets the ServerlessWorkflow.Sdk.Models.WorkflowDefinition's description
        /// </summary>
        [Display(Name = "Description")]
        public virtual string? Description { get; set; }
        /// <summary>
        /// Gets/sets the ServerlessWorkflow.Sdk.Models.WorkflowDefinition's name
        /// </summary>
        [Required]
        [Display(Name = "Name")]
        public virtual string Name { get; set; }
        /// <summary>
        /// Gets/sets the ServerlessWorkflow.Sdk.Models.WorkflowDefinition's domain-specific
        /// </summary>
        [Display(Name = "Instance key expression")]
        public virtual string? Key { get; set; }
        /// <summary>
        /// Gets/sets the ServerlessWorkflow.Sdk.Models.WorkflowDefinition's unique identifier
        /// </summary>
        [Display(Name = "Id")]
        [Required]
        public virtual string Id { get; set; }
        /// <summary>
        /// Gets/sets the name of the ServerlessWorkflow.Sdk.Models.WorkflowDefinition's
        /// start ServerlessWorkflow.Sdk.Models.StateDefinition
        /// </summary>
        public virtual string StartStateName { get; set; }
    }
}
