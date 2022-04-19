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

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Synapse.Cli.Commands.Systems.Installs
{

    /// <summary>
    /// Represents the <see cref="Command"/> used to perform a Docker Synapse install
    /// </summary>
    internal class DockerInstallCommand
        : Command
    {

        /// <summary>
        /// Gets the <see cref="DockerInstallCommand"/>'s name
        /// </summary>
        public const string CommandName = "docker";
        /// <summary>
        /// Gets the <see cref="DockerInstallCommand"/>'s description
        /// </summary>
        public const string CommandDescription = "Installs Synapse on Docker";

        /// <inheritdoc/>
        public DockerInstallCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseManagementApi synapseManagementApi, IHttpClientFactory httpClientFactory)
            : base(serviceProvider, loggerFactory, synapseManagementApi, CommandName, CommandDescription)
        {
            this.HttpClient = httpClientFactory.CreateClient();
            this.AddOption(CommandOptions.Name);
            this.AddOption(CommandOptions.HostName);
            this.AddOption(CommandOptions.CloudEventSinkUri);
            this.AddOption(CommandOptions.SkipCertificateValidation);
            this.Handler = CommandHandler.Create<string, string, Uri, bool>(this.HandleAsync);
        }

        /// <summary>
        /// Gets the service used to perform HTTP requests
        /// </summary>
        protected HttpClient HttpClient { get; }

        /// <summary>
        /// Handles the <see cref="DockerInstallCommand"/>
        /// </summary>
        /// <param name="name">The Synapse container name"</param>
        /// <param name="hostName">Synapse's host name</param>
        /// <param name="ceSink">The Synapse cloud event sink uri</param>
        /// <param name="skipCertificateValidation">A boolean indicating whether or not to skip certificate validation</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public async Task HandleAsync(string name, string hostName, Uri ceSink, bool skipCertificateValidation)
        {
            var environmentVariables = new List<string>();
            environmentVariables.Add(@$"-e ""{EnvironmentVariables.Api.HostName.Name} = {hostName}""");
            environmentVariables.Add(@$"-e ""{EnvironmentVariables.CloudEvents.Sink.Uri.Name} = {ceSink}""");
            if(skipCertificateValidation)
                environmentVariables.Add(@$"-e ""{EnvironmentVariables.SkipCertificateValidation.Name} = {skipCertificateValidation}""");
            var args = @$"run --name {name} -v /var/run/docker.sock:/var/run/docker.sock --add-host=host.docker.internal:host-gateway -p 42286:42286 -p 41387:41387 -d --restart unless-stopped {string.Join(" ", environmentVariables)} ghcr.io/serverlessworkflow/synapse:latest";
            var process = Process.Start("docker", args);
            await process.WaitForExitAsync();
            await Task.Delay(500); //wait for the server to run
            var uri = "http://localhost:42286";
            try
            {
                Process.Start(uri);
            }
            catch
            {
                try
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        Process.Start(new ProcessStartInfo("cmd", $"/c start {uri.Replace("&", "^&")}") { CreateNoWindow = true });
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        Process.Start("xdg-open", uri);
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        Process.Start("open", uri);
                    else
                        throw new NotSupportedException();
                }
                catch (NotSupportedException)
                {
                    throw;
                }
            }
        }

        private static class CommandOptions
        {

            public static Option<string> Name
            {
                get
                {
                    var option = new Option<string>("--name")
                    {
                        Description = "The Synapse container name"
                    };
                    option.SetDefaultValue("synapse");
                    option.AddAlias("-n");
                    return option;
                }
            }

            public static Option<string> HostName
            {
                get
                {
                    var option = new Option<string>("--hostname")
                    {
                        Description = "Synapse's host name"
                    };
                    option.SetDefaultValue("synapse");
                    option.AddAlias("-hn");
                    return option;
                }
            }

            public static Option<Uri> CloudEventSinkUri
            {
                get
                {
                    var option = new Option<Uri>("--ce-sink")
                    {
                        Description = "The Synapse cloud event sink uri"
                    };
                    option.SetDefaultValue(new Uri("https://en37uhd2he6t4.x.pipedream.net"));
                    option.AddAlias("-ces");
                    return option;
                }
            }

            public static Option<bool> SkipCertificateValidation
            {
                get
                {
                    var option = new Option<bool>("--skip-certificate-validation")
                    {
                        Description = "Skips certificate validation when performing http requests"
                    };
                    option.AddAlias("-scv");
                    return option;
                }
            }

        }

    }

}
