// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Synapse.Api.Application.Configuration;

/// <summary>
/// Represents the options used to configure the seeding, if any, of Synapse's database
/// </summary>
public class DatabaseSeedingOptions
{

    /// <summary>
    /// Gets the path to the directory from which to load the static resources used to seed the database
    /// </summary>
    public static readonly string DefaultDirectory = Path.Combine(AppContext.BaseDirectory, "data", "seed");
    /// <summary>
    /// Gets the default GLOB pattern used to match the static resource files to use to seed the database
    /// </summary>
    public const string DefaultFilePattern = "*.*";

    /// <summary>
    /// Initializes a new <see cref="DatabaseSeedingOptions"/>
    /// </summary>
    public DatabaseSeedingOptions()
    {
        var env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Database.Seeding.Reset);
        if (!string.IsNullOrWhiteSpace(env))
        {
            if (!bool.TryParse(env, out var reset)) throw new Exception($"Failed to parse the value specified as '{SynapseDefaults.EnvironmentVariables.Database.Seeding.Reset}' environment variable into a boolean");
            this.Reset = reset;
        }
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Database.Seeding.Directory);
        if (!string.IsNullOrWhiteSpace(env))
        {
            this.Directory = env;
            if (!System.IO.Directory.Exists(this.Directory)) System.IO.Directory.CreateDirectory(this.Directory);
        }
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Database.Seeding.Overwrite);
        if (!string.IsNullOrWhiteSpace(env))
        {
            if (!bool.TryParse(env, out var overwrite)) throw new Exception($"Failed to parse the value specified as '{SynapseDefaults.EnvironmentVariables.Database.Seeding.Overwrite}' environment variable into a boolean");
            this.Overwrite = overwrite;
        }
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Database.Seeding.FilePattern);
        if (!string.IsNullOrWhiteSpace(env)) this.FilePattern = env;
    }

    /// <summary>
    /// Gets/sets a boolean indicating whether or not to reset the database upon starting up the API server
    /// </summary>
    public virtual bool Reset { get; set; }

    /// <summary>
    /// Gets/sets the directory from which to load the static resources used to seed the database
    /// </summary>
    public virtual string Directory { get; set; } = DefaultDirectory;

    /// <summary>
    /// Gets/sets a boolean indicating whether or not to overwrite existing resources
    /// </summary>
    public virtual bool Overwrite { get; set; }

    /// <summary>
    /// Gets/sets the GLOB pattern used to match the static resource files to use to seed the database
    /// </summary>
    public virtual string FilePattern { get; set; } = DefaultFilePattern;

}
