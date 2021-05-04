using ServerlessWorkflow.Sdk;
using Synapse.Cli.Services;
using Synapse.Domain.Models;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Synapse.Cli.Commands
{

    /// <summary>
    /// Represents the <see cref="Command"/> used to deploy a workflow definition
    /// </summary>
    public class DeployCommand
        : Command
    {

        /// <summary>
        /// Initializes a new <see cref="DeployCommand"/>
        /// </summary>
        /// <param name="synapse">The service used to interact with Synapse</param>
        public DeployCommand(ISynapseService synapse)
            : base(synapse, "deploy", "Deploys the specified workflow.")
        {
            this.Handler = CommandHandler.Create<string, string, string, string, bool>(this.HandleAsync);
            this.Add(CommandOptions.File);
            this.Add(CommandOptions.Yaml);
            this.Add(CommandOptions.Json);
            this.Add(CommandOptions.Wait);
            this.Add(CommandOptions.Namespace);
        }

        /// <summary>
        /// Handles the <see cref="DeployCommand"/>
        /// </summary>
        /// <param name="file">The definition's file path</param>
        /// <param name="yaml">The YAML of the definition to deploy</param>
        /// <param name="json">The JSON of the definition to deploy</param>
        /// <param name="wait">A boolean indicating whether or not to wait for the deployment to be completed. Defaults to false.</param>
        /// <returns>A new int representing the program's return code</returns>
        public async Task<int> HandleAsync(string file = null, string yaml = null, string json = null, string @namespace = "synapse", bool wait = false)
        {
            V1Workflow workflow;
            if (string.IsNullOrWhiteSpace(@namespace))
                throw new ArgumentNullException(nameof(@namespace));
            if (!string.IsNullOrWhiteSpace(file))
                workflow = await this.Synapse.ReadWorkflowFromFileAsync(file);
            else if (!string.IsNullOrWhiteSpace(yaml))
                workflow = await this.Synapse.ReadWorkflowAsync(yaml, WorkflowDefinitionFormat.Yaml);
            else if (!string.IsNullOrWhiteSpace(json))
                workflow = await this.Synapse.ReadWorkflowAsync(json, WorkflowDefinitionFormat.Json);
            else
                throw new InvalidOperationException("You must specifiy exactly one of the following options: --file, --yaml or --json");
            workflow.Metadata.NamespaceProperty = @namespace;
            await this.Synapse.DeployWorkflowAsync(workflow, wait);
            return 0;
        }

        private static class CommandOptions
        {

            public static Option<string> File
            {
                get
                {
                    Option<string> option = new Option<string>("--file");
                    option.Description = "The file that contains the workflow definition to deploy.";
                    option.AddAlias("f");
                    return option;
                }
            }

            public static Option<string> Yaml
            {
                get
                {
                    Option<string> option = new Option<string>("--yaml");
                    option.Description = "The YAML of the workflow definition to deploy.";
                    return option;
                }
            }

            public static Option<string> Json
            {
                get
                {
                    Option<string> option = new Option<string>("--json");
                    option.Description = "The JSON of the workflow definition to deploy.";
                    return option;
                }
            }

            public static Option<string> Namespace
            {
                get
                {
                    Option<string> option = new Option<string>("--namespace");
                    option.AddAlias("-n");
                    option.Description = "The namespace to deploy the workflow to";
                    return option;
                }
            }

            public static Option<bool> Wait
            {
                get
                {
                    Option<bool> option = new Option<bool>("--wait");
                    option.Description = "A boolean indicating whether or not to wait for the workflow's deployment. Defaults to False.";
                    return option;
                }
            }

        }

    }

}
