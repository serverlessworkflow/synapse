﻿using k8s;
using k8s.Models;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerlessWorkflow.Sdk;
using ServerlessWorkflow.Sdk.Models;
using ServerlessWorkflow.Sdk.Services.IO;
using Synapse.Domain.Models;
using Synapse.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Cli.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="ISynapseService"/>
    /// </summary>
    public class SynapseService
        : ISynapseService
    {

        /// <summary>
        /// Initializes a new <see cref="SynapseService"/>
        /// </summary>
        /// <param name="kubernetes">The service used to interact with Kubernetes</param>
        /// <param name="workflowReader">The service used to read <see cref="WorkflowDefinition"/>s</param>
        /// <param name="workflows">The <see cref="IRepository{TResource}"/> used to manage <see cref="V1Workflow"/>s</param>
        /// <param name="workflowInstances">The <see cref="IRepository{TResource}"/> used to manage <see cref="V1WorkflowInstance"/>s</param>
        public SynapseService(IKubernetes kubernetes, IWorkflowReader workflowReader, IRepository<V1Workflow> workflows, IRepository<V1WorkflowInstance> workflowInstances)
        {
            this.Kubernetes = kubernetes;
            this.WorkflowReader = workflowReader;
            this.Workflows = workflows;
            this.WorkflowInstances = workflowInstances;
        }

        /// <summary>
        /// Gets the service used to interact with Kubernetes
        /// </summary>
        protected IKubernetes Kubernetes { get; }

        /// <summary>
        /// Gets the service used to read <see cref="WorkflowDefinition"/>s
        /// </summary>
        protected IWorkflowReader WorkflowReader { get; }

        /// <summary>
        /// Gets the <see cref="IRepository{TResource}"/> used to manage <see cref="V1Workflow"/>s
        /// </summary>
        protected IRepository<V1Workflow> Workflows { get; }

        /// <summary>
        /// Gets the <see cref="IRepository{TResource}"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<V1WorkflowInstance> WorkflowInstances { get; }

        /// <inheritdoc/>
        public virtual async Task InstallAsync(string @namespace = "synapse", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(@namespace))
                throw new ArgumentNullException(nameof(@namespace));
            Console.WriteLine("Installing Synapse...");
            V1Namespace ns = (await this.Kubernetes.ListNamespaceAsync(cancellationToken: cancellationToken)).Items.SingleOrDefault(n => n.Name() == @namespace);
            if (ns == null)
            {
                Console.WriteLine("Namespace '{namespace}' does not exist. Creating it...", @namespace);
                ns = await this.Kubernetes.CreateNamespaceAsync(new V1Namespace() { Metadata = new V1ObjectMeta() { Name = @namespace } }, cancellationToken: cancellationToken);
                Console.WriteLine("Namespace '{namespace}' created.", @namespace);
            }
            await this.DeployCustomResourceDefinitionsAsync(cancellationToken);
            await this.DeployOperatorAsync(@namespace, cancellationToken);
            Console.WriteLine("Synapse has been successfully installed.");
        }

        /// <inheritdoc/>
        public virtual async Task<V1Workflow> FindWorkflowByReferenceAsync(string reference, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reference))
                throw new ArgumentNullException(nameof(reference));
            string id = reference.Split(":", StringSplitOptions.RemoveEmptyEntries).First();
            string version = reference.Substring(id.Length + 1);
            return await this.Workflows.FindAsync(id, version, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1Workflow> ReadWorkflowAsync(Stream stream, WorkflowDefinitionFormat definitionFormat = WorkflowDefinitionFormat.Yaml, CancellationToken cancellationToken = default)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            return await Task.Run(() => 
            {
                V1Workflow workflow = new V1Workflow(new V1WorkflowSpec(this.WorkflowReader.Read(stream, definitionFormat)));
                workflow.Metadata = new V1ObjectMeta() { Name = $"{workflow.Spec.Definition.Id}-{workflow.Spec.Definition.Version.Slugify("-")}".ToLower() };
                return workflow;
            }, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1Workflow> DeployWorkflowAsync(V1Workflow workflow, bool wait = false, CancellationToken cancellationToken = default)
        {
            if (workflow == null)
                throw new ArgumentNullException(nameof(workflow));
            Console.WriteLine($"Deploying workflow '{workflow.Spec.Definition.Id}:{workflow.Spec.Definition.Version}'...");
            workflow = await this.Workflows.AddAsync(workflow, cancellationToken);
            Console.WriteLine("Workflow deployed.");
            if (!wait)
                return workflow;
            Console.WriteLine("Waiting for Synapse Operator feedback...");
            while ((workflow.Status == null || workflow.Status.Type < V1WorkflowDefinitionStatus.Valid)
                && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(50);
                workflow = await this.Workflows.FindByNameAsync(workflow.Name(), workflow.Namespace(), cancellationToken);
            }
            if(cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("The operation has been cancelled");
                throw new TaskCanceledException();
            }
            Console.WriteLine("The Synapse Operator has finished processing the workflow:");
            Console.WriteLine($"Status: {EnumHelper.Stringify(workflow.Status.Type)}");
            Console.WriteLine($"Errors: {JsonConvert.SerializeObject(workflow.Status.Errors)}");
            return workflow;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstance> RunWorkflowAsync(V1Workflow workflow, JObject input = null, bool wait = false, CancellationToken cancellationToken = default)
        {
            if (workflow == null)
                throw new ArgumentNullException(nameof(workflow));
            if (input == null)
                input = new JObject();
            V1WorkflowInstance workflowInstance;
            try
            {
                workflowInstance = await this.WorkflowInstances.AddAsync(new V1WorkflowInstance()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        GenerateName = $"{workflow.Spec.Definition.Id}-".ToLower(),
                        NamespaceProperty = workflow.Namespace()
                    },
                    Spec = new V1WorkflowInstanceSpec(new V1WorkflowReference(workflow.Spec.Definition.Id, workflow.Spec.Definition.Version), input)
                }, cancellationToken);
            }
            catch(HttpOperationException ex)
            {
                Console.WriteLine(ex.Response.Content);
                throw;
            }
          
            if (!wait)
                return workflowInstance;
            while ((workflowInstance.Status == null || workflowInstance.Status.Type < V1WorkflowActivityStatus.Faulted)
               && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(50);
                workflowInstance = await this.WorkflowInstances.FindByNameAsync(workflowInstance.Name(), workflowInstance.Namespace(), cancellationToken);
            }
            if (cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("The operation has been cancelled");
                throw new TaskCanceledException();
            }
            return workflowInstance;
        }

        /// <inheritdoc/>
        public virtual async Task DeleteWorkflowAsync(string name, string @namespace, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            V1Workflow workflow;
            workflow = await this.Workflows.FindByNameAsync(name, @namespace, cancellationToken);
            if (workflow == null)
                throw new NullReferenceException($"Failed to find a workflow with the specified name '{name}'");
            await this.Workflows.RemoveAsync(workflow, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task DeleteAllWorkflowsAsync(string @namespace, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(@namespace))
                throw new ArgumentNullException(nameof(@namespace));
            try
            {
                await this.Workflows.ClearAsync(@namespace, cancellationToken);
            }
            catch(HttpOperationException ex)
            {
                Console.WriteLine(ex.Request.RequestUri);
                Console.WriteLine(ex.Response.Content);
                throw;
            }
          
        }

        protected virtual async Task DeployCustomResourceDefinitionsAsync(CancellationToken cancellationToken = default)
        {
            
        }

        protected virtual async Task DeployOperatorAsync(string @namespace, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(@namespace))
                throw new ArgumentNullException(nameof(@namespace));

        }

    }

}
