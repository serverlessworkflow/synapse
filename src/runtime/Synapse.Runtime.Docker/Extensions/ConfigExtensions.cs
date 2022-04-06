namespace Synapse.Runtime.Docker
{
    /// <summary>
    /// Defines extensions for <see cref="Config"/>s
    /// </summary>
    public static class ConfigExtensions
    {

        /// <summary>
        /// Adds the specified environment variable
        /// </summary>
        /// <param name="config">The extended <see cref="Config"/></param>
        /// <param name="name">The name of the environment variable to set</param>
        /// <param name="value">The new value of the environment variable to set</param>
        public static void AddOrUpdateEnvironmentVariable(this Config config, string name, string value)
        {
            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (config.Env == null)
                config.Env = new List<string>();
            var variable = config.Env.FirstOrDefault(s => s.Split('=', StringSplitOptions.RemoveEmptyEntries)[0] == name);
            if (!string.IsNullOrEmpty(variable))
                config.Env.Remove(variable);
            variable = $"{name}={value}";
            config.Env.Add(variable);
        }

        /// <summary>
        /// Removes the environment variable with the specified name
        /// </summary>
        /// <param name="config">The extended <see cref="Config"/></param>
        /// <param name="name">The name of the environment variable to remove</param>
        /// <returns>A boolean indicating whether or not the specified environment variable has been removed</returns>
        public static bool RemoveEnvironmentVariable(this Config config, string name)
        {
            if (config.Env == null)
                return false;
            var variable = config.Env.FirstOrDefault(s => s.Split('=', StringSplitOptions.RemoveEmptyEntries)[0] == name);
            if (!string.IsNullOrEmpty(variable))
                return config.Env.Remove(variable);
            return false;
        }

    }

}
