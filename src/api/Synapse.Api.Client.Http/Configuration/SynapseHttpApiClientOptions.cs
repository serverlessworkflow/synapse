// Copyright © 2024-Present Neuroglia SRL. All rights reserved.
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

namespace Synapse.Api.Client.Http.Configuration;

/// <summary>
/// Represents the options used to configure a Synapse HTTP API client
/// </summary>
public class SynapseHttpApiClientOptions
{

    /// <summary>
    /// Initializes a new <see cref="SynapseHttpApiClientOptions"/>
    /// </summary>
    public SynapseHttpApiClientOptions()
    {
        var uri = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Api.Uri);
        this.BaseAddress = string.IsNullOrWhiteSpace(uri) ? null! : new(uri, UriKind.RelativeOrAbsolute);
    }

    /// <summary>
    /// Gets/sets the base address of the Cloud Streams API to connect to
    /// </summary>
    public virtual Uri BaseAddress { get; set; }

}
