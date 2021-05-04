using ServerlessWorkflow.Sdk.Models;
using System.Linq;

namespace Synapse
{

    /// <summary>
    /// Defines extensions for <see cref="BranchDefinition"/>s
    /// </summary>
    public static class BranchDefinitionExtensions
    {

        /// <summary>
        /// Gets the <see cref="ActionDefinition"/> with the specified name
        /// </summary>
        /// <param name="branch">The <see cref="BranchDefinition"/> to search for the specified <see cref="ActionDefinition"/></param>
        /// <param name="name">The name of the <see cref="ActionDefinition"/> to get</param>
        /// <returns>The <see cref="ActionDefinition"/> with the specified name</returns>
        public static ActionDefinition GetAction(this BranchDefinition branch, string name)
        {
            return branch.Actions.FirstOrDefault(s => s.Name == name);
        }

        /// <summary>
        /// Attempts to get the <see cref="ActionDefinition"/> with the specified name
        /// </summary>
        /// <param name="branch">The <see cref="BranchDefinition"/> to search for the specified <see cref="ActionDefinition"/></param>
        /// <param name="name">The name of the <see cref="ActionDefinition"/> to get</param>
        /// <param name="action">The <see cref="ActionDefinition"/> with the specified name</param>
        /// <returns>A boolean indicating whether or not a <see cref="ActionDefinition"/> with the specified name could be found</returns>
        public static bool TryGetAction(this BranchDefinition branch, string name, out ActionDefinition action)
        {
            action = branch.GetAction(name);
            return action != null;
        }

        /// <summary>
        /// Attempts to get the next <see cref="ActionDefinition"/> in the pipeline
        /// </summary>
        /// <param name="branch">The <see cref="BranchDefinition"/> to search</param>
        /// <param name="previousActionName">The name of the <see cref="ActionDefinition"/> to get the next <see cref="ActionDefinition"/> for</param>
        /// <param name="action">The next <see cref="ActionDefinition"/>, if any</param>
        /// <returns>A boolean indicating whether or not there is a next <see cref="ActionDefinition"/> in the pipeline</returns>
        public static bool TryGetNextAction(this BranchDefinition branch, string previousActionName, out ActionDefinition action)
        {
            action = null;
            ActionDefinition previousAction = branch.Actions.FirstOrDefault(a => a.Name == previousActionName);
            int previousActionIndex = branch.Actions.ToList().IndexOf(previousAction);
            int nextIndex = previousActionIndex + 1;
            if (nextIndex >= branch.Actions.Count())
                return false;
            action = branch.Actions.ElementAt(nextIndex);
            return true;
        }

    }

}
