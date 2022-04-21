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

using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Synapse.Infrastructure.Plugins
{
    /// <summary>
    /// Represents an object used to describe an <see cref="IPlugin"/>
    /// </summary>
    public class PluginMetadata
    {

        /// <summary>
        /// Gets the <see cref="IPlugin"/>'s name
        /// </summary>
        [Required, MinLength(1)]
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        [Newtonsoft.Json.JsonProperty("name", Required = Newtonsoft.Json.Required.Always)]
        public virtual string Name { get; protected set; } = null!;

        /// <summary>
        /// Gets the <see cref="IPlugin"/>'s description
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("description")]
        [Newtonsoft.Json.JsonProperty("description")]
        public virtual string? Description { get; protected set; }

        /// <summary>
        /// Gets the <see cref="IPlugin"/>'s version
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("version")]
        [Newtonsoft.Json.JsonProperty("version")]
        public virtual string? Version { get; protected set; }

        /// <summary>
        /// Gets the <see cref="IPlugin"/>'s authors
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("authors")]
        [Newtonsoft.Json.JsonProperty("authors")]
        public virtual string? Authors { get; protected set; }

        /// <summary>
        /// Gets the <see cref="IPlugin"/>'s copyright
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("copyright")]
        [Newtonsoft.Json.JsonProperty("copyright")]
        public virtual string? Copyright { get; protected set; }

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing the <see cref="IPlugin"/>'s tags
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("tags")]
        [Newtonsoft.Json.JsonProperty("tags")]
        public virtual List<string> Tags { get; protected set; } = new();

        /// <summary>
        /// Gets the <see cref="IPlugin"/>'s license file <see cref="Uri"/>
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("licenseUri")]
        [Newtonsoft.Json.JsonProperty("licenseUri")]
        public virtual Uri? LicenseUri { get; protected set; }

        /// <summary>
        /// Gets the <see cref="IPlugin"/>'s readme file <see cref="Uri"/>
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("readmeUri")]
        [Newtonsoft.Json.JsonProperty("readmeUri")]
        public virtual Uri? ReadmeUri { get; protected set; }

        /// <summary>
        /// Gets the <see cref="IPlugin"/>'s website <see cref="Uri"/>
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("websiteUri")]
        [Newtonsoft.Json.JsonProperty("websiteUri")]
        public virtual Uri? WebsiteUri { get; protected set; }

        /// <summary>
        /// Gets the <see cref="IPlugin"/>'s repository <see cref="Uri"/>
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("repositoryUri")]
        [Newtonsoft.Json.JsonProperty("repositoryUri")]
        public virtual Uri? RepositoryUri { get; protected set; }

        /// <summary>
        /// Gets the <see cref="IPlugin"/>'s <see cref="Assembly"/> file name
        /// </summary>
        [Required, MinLength(1)]
        [System.Text.Json.Serialization.JsonPropertyName("assemblyFile")]
        [Newtonsoft.Json.JsonProperty("assemblyFile", Required = Newtonsoft.Json.Required.Always)]
        public virtual string AssemblyFileName { get; protected set; } = null!;

    }

}
