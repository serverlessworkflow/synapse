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

namespace Synapse.Api.Client.Services;

/// <summary>
/// Defines the fundamentals of the Synapse API used to manage <see cref="Document"/>s
/// </summary>
public interface IDocumentApiClient
{

    /// <summary>
    /// Creates a new <see cref="Document"/>
    /// </summary>
    /// <param name="name">The <see cref="Document"/>'s name</param>
    /// <param name="content">The <see cref="Document"/>'s content</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The newly created <see cref="Document"/></returns>
    Task<Document> CreateAsync(string name, object content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the <see cref="Document"/> with the specified id
    /// </summary>
    /// <param name="id">The id of the <see cref="Document"/> to get</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="Document"/> with the specified id</returns>
    Task<Document> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the contents of the <see cref="Document"/> with the specified id
    /// </summary>
    /// <param name="id">The id of the <see cref="Document"/> to update the content of</param>
    /// <param name="content">The <see cref="Document"/>'s content</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="Document"/> with the specified id</returns>
    Task UpdateAsync(string id, object content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the <see cref="Document"/> with the specified id
    /// </summary>
    /// <param name="id">The id of the <see cref="Document"/> to delete</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="Document"/> with the specified id</returns>
    Task DeletesAsync(string id, CancellationToken cancellationToken = default);

}
