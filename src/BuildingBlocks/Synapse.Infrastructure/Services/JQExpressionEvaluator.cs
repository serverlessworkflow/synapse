using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerlessWorkflow.Sdk;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Models;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Synapse.Services
{
    /// <summary>
    /// Represents the default jq implementation of the <see cref="IExpressionEvaluator"/> interface
    /// </summary>
    public class JQExpressionEvaluator
        : IExpressionEvaluator
    {

        /// <inheritdoc/>
        public virtual string Language => "jq";

        /// <summary>
        /// Initializes a new <see cref="JQExpressionEvaluator"/>
        /// </summary>
        /// <param name="workflow">The <see cref="V1Workflow"/> the <see cref="JQExpressionEvaluator"/> belongs to</param>
        public JQExpressionEvaluator(V1Workflow workflow)
        {
            this.Workflow = workflow;
        }

        /// <summary>
        /// Gets the <see cref="V1Workflow"/> the <see cref="JQExpressionEvaluator"/> belongs to
        /// </summary>
        protected V1Workflow Workflow { get; }

        /// <inheritdoc/>
        public virtual JToken Evaluate(string expression, JToken data)
        {
            expression = expression.Trim();
            if (expression.StartsWith("${"))
                expression = expression[2..^1].Trim();
            foreach (Match functionMatch in Regex.Matches(expression, @"(fn:\w*)"))
            {
                string functionName = functionMatch.Value.Trim();
                functionName = functionName[3..];
                if (!this.Workflow.Spec.Definition.TryGetFunction(functionName, out FunctionDefinition function))
                    throw new NullReferenceException($"Failed to find a function with the specified name '{functionName}' in the workflow '{this.Workflow}'");
                if (function.Type != FunctionType.Expression)
                    throw new InvalidOperationException($"The function with name '{function.Name}' is of type '{EnumHelper.Stringify(function.Type)}' and cannot be called in an expression");
                JToken token = this.Evaluate(function.Operation, data);
                expression = expression.Replace(functionMatch.Value, token?.ToString(Formatting.None));
            }
            string fileName;
            string args;
            using Process process = new();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                fileName = "cmd.exe";
                args = @$"/c echo {data.ToString(Formatting.None)} | jq ""{this.EscapeJson(expression)}""";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                fileName = "bash";
                args = @$"-c ""echo '{this.EscapeJson(data.ToString(Formatting.None))}' | jq '{this.EscapeJson(expression)}'""";
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0)
                throw new Exception($"An error occured while evaluting the specified expression:{Environment.NewLine}Details: {error}");
            if (string.IsNullOrWhiteSpace(output))
                return null;
            else
                return JToken.Parse(output);
        }

        /// <summary>
        /// Escapes doubles quotes in the supplied json
        /// </summary>
        /// <param name="json">The json string to escape</param>
        /// <returns>The escaped json</returns>
        protected virtual string EscapeJson(string json)
        {
            if (!json.Contains(@"\"""))
                json = json.Replace("\"", @"\""");
            if (!json.Contains("^&"))
                json = json.Replace("&", "^&");
            return json;
        }

    }

}
