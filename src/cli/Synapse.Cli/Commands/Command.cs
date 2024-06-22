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

using Microsoft.Extensions.Options;

namespace Synapse.Cli.Commands;

/// <summary>
/// Represents the base class for all <see cref="System.CommandLine.Command"/> implementations
/// </summary>
public abstract class Command
    : System.CommandLine.Command
{

    /// <summary>
    /// Initializes a new <see cref="Command"/>
    /// </summary>
    /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
    /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
    /// <param name="api">The service used to interact with the remote Synapse API</param>
    /// <param name="name">The <see cref="Command"/>'s name</param>
    /// <param name="description">The <see cref="Command"/>'s description</param>
    protected Command(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api, string name, string description)
        : base(name, description)
    {
        this.ServiceProvider = serviceProvider;
        this.Logger = loggerFactory.CreateLogger(this.GetType());
        this.Api = api;
    }

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Gets the service used to interact with the remote Synapse API
    /// </summary>
    protected ISynapseApiClient Api { get; }

    /// <summary>
    /// Ensures that the CLI application has been properly configured
    /// </summary>
    protected virtual void EnsureConfigured()
    {
        var applicationOptions = this.ServiceProvider.GetRequiredService<IOptions<ApplicationOptions>>().Value;
        if (applicationOptions.Api.Configurations.Count < 1) throw new Exception(@"Error: The server is not configured. Please ensure that your server configuration is correctly set.

To configure the server, follow these steps:

1. Verify that your configuration file is present and correctly formatted.
2. Set the server configuration using the appropriate command, for example:
   synctl config set-api your-api-name --server=https://your-server-address --token=your_access_token

For more information on configuring the server, visit: https://your-docs-url.com/configuration

Hint: You can check your current configuration with:
   synctl config get-apis
");
    }

}