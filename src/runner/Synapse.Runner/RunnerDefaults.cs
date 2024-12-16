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

namespace Synapse;

/// <summary>
/// Exposes constants about the Synapse Runner application
/// </summary>
public static class RunnerDefaults
{

    /// <summary>
    /// Exposes constants about the Synapse Runner command line
    /// </summary>
    public static class CommandLine
    {

        /// <summary>
        /// Gets the Runner's command line arguments switch mappings
        /// </summary>
        public static readonly IDictionary<string, string> SwitchMappings = new Dictionary<string, string>()
        {
            { "--workflow", $"{nameof(RunnerOptions.Workflow)}:{nameof(WorkflowOptions.DefinitionFilePath)}" },
            { "-w", $"{nameof(RunnerOptions.Workflow)}:{nameof(WorkflowOptions.DefinitionFilePath)}" },
            { "--input", $"{nameof(RunnerOptions.Workflow)}:{nameof(WorkflowOptions.InputFilePath)}" },
            { "-i", $"{nameof(RunnerOptions.Workflow)}:{nameof(WorkflowOptions.InputFilePath)}" },
            { "--format", $"{nameof(RunnerOptions.Workflow)}:{nameof(WorkflowOptions.OutputFormat)}" },
            { "-f", $"{nameof(RunnerOptions.Workflow)}:{nameof(WorkflowOptions.OutputFormat)}" },
            { "--output", $"{nameof(RunnerOptions.Workflow)}:{nameof(WorkflowOptions.OutputFilePath)}" },
            { "-o", $"{nameof(RunnerOptions.Workflow)}:{nameof(WorkflowOptions.OutputFilePath)}" },
            { "--logs", $"{nameof(RunnerOptions.Logging)}:{nameof(RunnerLoggingOptions.OutputFilePath)}" },
            { "-l", $"{nameof(RunnerOptions.Logging)}:{nameof(RunnerLoggingOptions.OutputFilePath)}" }
        };

    }

}
