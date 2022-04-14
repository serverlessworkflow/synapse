/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using System.Runtime.InteropServices;

namespace Synapse.Runtime.Docker.Configuration
{

    /// <summary>
    /// Represents the options used to configure a Synapse Docker-based runtime
    /// </summary>
    public class NativeRuntimeOptions
    {

        /// <summary>
        /// Gets the name of the default worker file
        /// </summary>
        public const string DefaultWorkerFileName = "Synapse.Worker";

        /// <summary>
        /// Gets/sets the name of the worker file to run
        /// </summary>
        public virtual string WorkerFileName { get; set; } = DefaultWorkerFileName;

        /// <summary>
        /// Gets/sets the directory in which to run the worker process
        /// </summary>
        public virtual string WorkingDirectory { get; set; } = Path.Combine(AppContext.BaseDirectory, "bin", "worker");

        /// <summary>
        /// Gets the full name of the worker file to run
        /// </summary>
        /// <returns>The full name of the worker file to run</returns>
        public virtual string GetWorkerFileName()
        {
            var directory = this.WorkingDirectory;
            var fileName = this.WorkerFileName;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                fileName += ".exe";
            return Path.Combine(directory, fileName);
        }

    }

}
