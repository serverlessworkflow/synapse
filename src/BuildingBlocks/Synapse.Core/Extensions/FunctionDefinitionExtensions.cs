using Newtonsoft.Json.Linq;
using ServerlessWorkflow.Sdk.Models;
using System.Text.RegularExpressions;

namespace Synapse
{

    /// <summary>
    /// Defines extensions for <see cref="FunctionDefinition"/>s
    /// </summary>
    public static class FunctionDefinitionExtensions
    {

        /// <summary>
        /// Builds the parameters to pass to the <see cref="FunctionReference"/> based on the specified inpit
        /// </summary>
        /// <param name="function">The <see cref="FunctionReference"/> to build parameters for</param>
        /// <param name="input">The input data to build the parameters from</param>
        /// <returns>A new <see cref="JObject"/> that contains the <see cref="FunctionReference"/>'s parameters</returns>
        public static JObject BuildParameters(this FunctionReference function, JToken input)
        {
            if (input == null)
                return new JObject();
            string json = function.Arguments.ToString();
            foreach (Match match in Regex.Matches(json, @"\{\{.+?\}\}"))
            {
                string jsonPath = match.Value.Replace("{{", "").Replace("}}", "").Trim();
                JToken valueToken = input.SelectToken(jsonPath);
                string value = valueToken == null ? "null" : valueToken.ToString();
                json = json.Replace(match.Value, value);
            }
            return JObject.Parse(json);
        }

    }

}
