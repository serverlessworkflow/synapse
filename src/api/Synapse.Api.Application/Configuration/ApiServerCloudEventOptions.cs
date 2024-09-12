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

using Neuroglia.Eventing.CloudEvents;

namespace Synapse.Api.Application.Configuration;

/// <summary>
/// Represents the options used to configure the Cloud Events published by the Synapse API server
/// </summary>
public class ApiServerCloudEventOptions
{

    /// <summary>
    /// Initializes a new <see cref="CloudEvent"/>
    /// </summary>
    /// <exception cref="Exception"></exception>
    public ApiServerCloudEventOptions()
    {
        var env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Api.CloudEvents.Endpoint);
        if (!string.IsNullOrWhiteSpace(env))
        {
            if (!Uri.TryCreate(env, UriKind.Absolute, out var endpoint)) throw new Exception("The Cloud Events endpoint must be a valid absolute URI");
            this.Endpoint = endpoint;
        }
    }

    /// <summary>
    /// Gets/sets the uri, if any, to which to publish <see cref="CloudEvent"/>s
    /// </summary>
    public virtual Uri? Endpoint { get; set; }

}