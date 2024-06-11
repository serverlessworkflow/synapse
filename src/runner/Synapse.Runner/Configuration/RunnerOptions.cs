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

using Synapse.Api.Client.Http.Configuration;

namespace Synapse.Runner.Configuration;

/// <summary>
/// Represents the options used to configure a Synapse Runner application
/// </summary>
public class RunnerOptions
{

    /// <summary>
    /// Gets/sets the options used to configure the Synapse API the runner must use
    /// </summary>
    public virtual SynapseHttpApiClientOptions Api { get; set; } = new();

    /// <summary>
    /// Gets/sets the options used to configure the workflow the Synapse Runner must run and how
    /// </summary>
    public virtual WorkflowOptions Workflow { get; set; } = new();

}
