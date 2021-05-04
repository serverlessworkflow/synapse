using FluentValidation.Results;
using k8s.Models;
using Microsoft.AspNetCore.JsonPatch;
using Synapse.Domain.Events.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Domain.Models
{

    public class V1Workflow
        : CustomResourceAggregate<V1WorkflowSpec, V1WorkflowStatus>
    {

        public V1Workflow() 
            : base(new V1WorkflowDefinition())
        {

        }

        public V1Workflow(V1WorkflowSpec spec)
            : this()
        {
            this.Spec = spec;
        }

        /// <inheritdoc/>
        protected new JsonPatchDocument<V1Workflow> Patch
        {
            get
            {
                return (JsonPatchDocument<V1Workflow>)base.Patch;
            }
        }

        /// <inheritdoc/>
        protected new JsonPatchDocument<V1Workflow> StatusPatch
        {
            get
            {
                return (JsonPatchDocument<V1Workflow>)base.StatusPatch;
            }
        }

        public virtual void StartProcessing()
        {
            if (this.Status != null && this.Status.Type != V1WorkflowDefinitionStatus.Pending)
                throw DomainException.UnexpectedState(typeof(V1Workflow), this.Name(), this.Status.Type);
            this.On(this.RegisterEvent(new V1WorkflowProcessingStarted(this.Id)));
        }

        public virtual bool Label()
        {
            if (this.Status.Type != V1WorkflowDefinitionStatus.Processing)
                throw DomainException.UnexpectedState(typeof(V1Workflow), this.Name(), this.Status.Type);
            bool updated = false;
            IDictionary<string, string> labels = this.Metadata.Labels;
            if(labels == null || !labels.ContainsKey(SynapseConstants.Labels.Workflows.Id))
            {
                this.SetLabel(SynapseConstants.Labels.Workflows.Id, this.Spec.Definition.Id);
                updated = true;
            }
            if (labels == null || !labels.ContainsKey(SynapseConstants.Labels.Workflows.Version))
            {
                this.SetLabel(SynapseConstants.Labels.Workflows.Version, this.Spec.Definition.Version);
                updated = true;
            }
            if (!updated)
                return false;
            this.Patch.Replace(w => w.Metadata.Labels, this.Metadata.Labels);
            return true;
        }

        public virtual void SetValidationResult(ValidationResult validationResult)
        {
            if (this.Status.Type != V1WorkflowDefinitionStatus.Processing)
                throw DomainException.UnexpectedState(typeof(V1Workflow), this.Name(), this.Status.Type);
            if (validationResult == null)
                throw DomainException.ArgumentNull(nameof(validationResult));
            
            this.On(this.RegisterEvent(new V1WorkflowValidationCompleted(this.Id, validationResult.IsValid, validationResult.Errors.Select(e => new V1Error(e.ErrorCode, e.ErrorMessage)))));
        }

        public virtual void Fault(Exception ex)
        {
            if (this.Status.Type != V1WorkflowDefinitionStatus.Processing)
                throw DomainException.UnexpectedState(typeof(V1Workflow), this.Name(), this.Status.Type);
            this.On(this.RegisterEvent(new V1WorkflowProcessingFaulted(this.Id, new V1Error[] { ex.ToV1Error() })));
        }

        protected virtual void On(V1WorkflowProcessingStarted e)
        {
            this.StatusPatch.Replace(w => w.Status, new V1WorkflowStatus() { Type = V1WorkflowDefinitionStatus.Processing });
            this.StatusPatch.ApplyTo(this);
        }

        protected virtual void On(V1WorkflowValidationCompleted e)
        {
            if (e.IsValid)
                this.StatusPatch.Replace(w => w.Status.Type, V1WorkflowDefinitionStatus.Valid);
            else
                this.StatusPatch.Replace(w => w.Status.Type, V1WorkflowDefinitionStatus.Invalid);
            this.StatusPatch.Replace(w => w.Status.Errors, e.ValidationErrors.ToList());
            this.StatusPatch.ApplyTo(this);
        }

        protected virtual void On(V1WorkflowProcessingFaulted e)
        {
            this.StatusPatch.Replace(w => w.Status.Type, V1WorkflowDefinitionStatus.Error);
            this.StatusPatch.Replace(w => w.Status.Errors, e.Errors.ToList());
            this.StatusPatch.ApplyTo(this);
        }

    }

}
