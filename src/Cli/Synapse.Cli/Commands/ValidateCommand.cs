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
    /// Represents the <see cref="Command"/> used to validate a workflow definition
    /// </summary>
    public class ValidateCommand
        : Command
    {

        /// <summary>
        /// Initializes a new <see cref="ValidateCommand"/>
        /// </summary>
        /// <param name="synapse">The service used to interact with Synapse</param>
        public ValidateCommand(ISynapseService synapse)
            : base(synapse, "validate", "Validates the specified workflow.")
        {
            this.Handler = CommandHandler.Create<string, string, string>(this.HandleAsync);
            this.AddAlias("val");
            this.Add(CommandOptions.File);
            this.Add(CommandOptions.Yaml);
            this.Add(CommandOptions.Json);
        }

        /// <summary>
        /// Handles the <see cref="ValidateCommand"/>
        /// </summary>
        /// <param name="file">The definition's file path</param>
        /// <param name="yaml">The YAML of the definition to validate</param>
        /// <param name="json">The JSON of the definition to validate</param>
        /// <returns>A new int representing the program's return code</returns>
        public async Task<int> HandleAsync(string file = null, string yaml = null, string json = null)
        {
            V1Workflow workflow;
            if (!string.IsNullOrWhiteSpace(file))
                workflow = await this.Synapse.ReadWorkflowFromFileAsync(file);
            else if (!string.IsNullOrWhiteSpace(yaml))
                workflow = await this.Synapse.ReadWorkflowAsync(yaml, WorkflowDefinitionFormat.Yaml);
            else if (!string.IsNullOrWhiteSpace(json))
                workflow = await this.Synapse.ReadWorkflowAsync(json, WorkflowDefinitionFormat.Json);
            else
                throw new InvalidOperationException("You must specifiy exactly one of the following options: --file, --yaml or --json");
            //TODO: validate with SDK services
            Console.WriteLine($"workflow: {workflow.Spec.Definition.Id}:{workflow.Spec.Definition.Version}");
            Console.WriteLine($"status: VALID");
            Console.WriteLine($"errors:");
            return 0;
        }

        private static class CommandOptions
        {

            public static Option<string> File
            {
                get
                {
                    Option<string> option = new Option<string>("--file");
                    option.Description = "The file that contains the workflow definition to validate.";
                    option.AddAlias("f");
                    return option;
                }
            }

            public static Option<string> Yaml
            {
                get
                {
                    Option<string> option = new Option<string>("--yaml");
                    option.Description = "The YAML of the workflow definition to validate.";
                    return option;
                }
            }

            public static Option<string> Json
            {
                get
                {
                    Option<string> option = new Option<string>("--json");
                    option.Description = "The JSON of the workflow definition to validate.";
                    return option;
                }
            }

        }

    }

}
