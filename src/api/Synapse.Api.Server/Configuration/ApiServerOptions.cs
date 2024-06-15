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

namespace Synapse.Api.Server.Configuration;

/// <summary>
/// Represents the options used to configure a Synapse API server
/// </summary>
public class ApiServerOptions
{

    /// <summary>
    /// Initializes a new <see cref="ApiServerOptions"/>
    /// </summary>
    public ApiServerOptions()
    {
        var env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Dashboard.Serve);
        if (!string.IsNullOrWhiteSpace(env) && bool.TryParse(env, out var serveDashboard)) this.ServeDashboard = serveDashboard;
    }

    /// <summary>
    /// Gets/sets a boolean indicating whether or not to serve the Synapse Dashboard
    /// </summary>
    public bool ServeDashboard { get; set; } = true;

    /// <summary>
    /// Gets/sets the application's authentication policy
    /// </summary>
    public AuthenticationPolicyOptions Authentication { get; set; } = new();

}
