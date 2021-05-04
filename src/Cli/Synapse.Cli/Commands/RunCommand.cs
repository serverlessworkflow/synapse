using k8s.Models;
using Newtonsoft.Json.Linq;
using ServerlessWorkflow.Sdk;
using Synapse.Cli.Services;
using Synapse.Domain.Models;
using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace Synapse.Cli.Commands
{

    /// <summary>
    /// Represents the <see cref="Command"/> used to run a workflow definition
    /// </summary>
    public class RunCommand
        : Command
    {

        /// <summary>
        /// Initializes a new <see cref="RunCommand"/>
        /// </summary>
        /// <param name="synapse">The service used to interact with Synapse</param>
        public RunCommand(ISynapseService synapse)
            : base(synapse, "run", "Runs the specified workflow.")
        {
            this.CreateHandler();
            this.Add(CommandOptions.Input);
            this.Add(CommandOptions.File);
            this.Add(CommandOptions.Yaml);
            this.Add(CommandOptions.Json);
            this.Add(CommandOptions.Namespace);
            this.Add(CommandOptions.Wait);
            this.Add(CommandOptions.Clean);
        }

        /// <summary>
        /// Handles a <see cref="RunCommand"/>
        /// </summary>
        /// <param name="input">The input data of the workflow to run</param>
        /// <param name="file">The definition's file path</param>
        /// <param name="yaml">The YAML of the definition to run</param>
        /// <param name="json">The JSON of the definition to run</param>
        /// <param name="namespace">The namespace in which to deploy and run the specified workflow</param>
        /// <param name="reference">The reference of the workflow to run (ex: 'myworkflow:latest')</param>
        /// <param name="wait">A boolean indicating whether or not to wait for the execution to be completed. Defaults to false.</param>
        /// <param name="clean">A boolean indicating whether or not to clean all related resources when the workflow ran to completion</param>
        /// <returns>A new int representing the program's return code</returns>
        public async Task<int> HandleAsync(string input = null, string file = null, string yaml = null, string json = null, string @namespace = "synapse", string reference = null, bool wait = false, bool clean = false)
        {
            V1Workflow workflow;
            if (!string.IsNullOrWhiteSpace(file))
                workflow = await this.Synapse.ReadWorkflowFromFileAsync(file);
            else if (!string.IsNullOrWhiteSpace(yaml))
                workflow = await this.Synapse.ReadWorkflowAsync(yaml, WorkflowDefinitionFormat.Yaml);
            else if (!string.IsNullOrWhiteSpace(json))
                workflow = await this.Synapse.ReadWorkflowAsync(json, WorkflowDefinitionFormat.Json);
            else if (!string.IsNullOrWhiteSpace(reference))
                workflow = await this.Synapse.FindWorkflowByReferenceAsync(reference);
            else
                throw new InvalidOperationException("You must specifiy exactly one of the following options: --file, --yaml or --json");
            if (workflow.Metadata?.NamespaceProperty == null)
                workflow.Metadata.NamespaceProperty = @namespace;
            if (string.IsNullOrWhiteSpace(reference))
                workflow = await this.Synapse.DeployWorkflowAsync(workflow, true);
            V1WorkflowInstance workflowInstance = await this.Synapse.RunWorkflowAsync(workflow, string.IsNullOrWhiteSpace(input) ? null : JObject.Parse(input), wait);
            if(!wait)
            {
                Console.WriteLine($"Workflow instance '{workflowInstance.Name()}' successfully initialized.");
                return 0;
            }
            Console.WriteLine($"Workflow instance '{workflowInstance.Name()}' has been executed.");
            Console.WriteLine($"Status: '{EnumHelper.Stringify(workflowInstance.Status.Type)}'");
            Console.WriteLine($"Output: {workflowInstance.Status.Output}");
            if(!clean)
                return 0;
            Console.WriteLine("Cleaning up...");
            await this.Synapse.DeleteWorkflowAsync(workflow.Name(), workflow.Namespace());
            Console.WriteLine("Clean up completed");
            return 0;
        }

        private static class CommandOptions
        {

            public static Option<string> Input
            {
                get
                {
                    Option<string> option = new Option<string>("--input");
                    option.Description = "The input data of the workflow instance to run.";
                    option.AddAlias("i");
                    return option;
                }
            }

            public static Option<string> File
            {
                get
                {
                    Option<string> option = new Option<string>("--file");
                    option.Description = "The file that contains the workflow definition.";
                    option.AddAlias("f");
                    return option;
                }
            }

            public static Option<string> Yaml
            {
                get
                {
                    Option<string> option = new Option<string>("--yaml");
                    option.Description = "The YAML of the workflow definition to run.";
                    return option;
                }
            }

            public static Option<string> Json
            {
                get
                {
                    Option<string> option = new Option<string>("--json");
                    option.Description = "The JSON of the workflow definition to run.";
                    return option;
                }
            }

            public static Option<string> Namespace
            {
                get
                {
                    Option<string> option = new Option<string>("--namespace");
                    option.AddAlias("-n");
                    option.Description = "The namespace to deploy the workflow to. Defaults to 'synapse'";
                    return option;
                }
            }

            public static Option<bool> Wait
            {
                get
                {
                    Option<bool> option = new Option<bool>("--wait");
                    option.Description = "A boolean indicating whether or not to wait for the workflow's execution. Defaults to False.";
                    return option;
                }
            }

            public static Option<bool> Clean
            {
                get
                {
                    Option<bool> option = new Option<bool>("--clean");
                    option.Description = "A boolean indicating whether or not to clean up after the workflow's execution. Defaults to False.";
                    return option;
                }
            }

        }

    }

}
