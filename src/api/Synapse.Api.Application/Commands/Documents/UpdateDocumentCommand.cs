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

using Neuroglia.Data.Infrastructure.Services;
using Synapse.Resources;

namespace Synapse.Api.Application.Commands.Documents;

/// <summary>
/// Represents the <see cref="ICommand"/> used to update the content of an existing <see cref="Document"/>
/// </summary>
/// <param name="id">The id of the document to update</param>
/// <param name="content">The specified document's updated content</param>
public class UpdateDocumentCommand(string id, object content)
    : Command<Document>
{

    /// <summary>
    /// Gets the id of the document to update the content of
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// Gets the updated content of the specified document
    /// </summary>
    public object Content { get; } = content;

}

    /// <summary>
/// Represents the service used to handle <see cref="UpdateDocumentCommand"/>s
/// </summary>
/// <param name="documents">The service used to manage <see cref="Document"/>s</param>
public class UpdateDocumentCommandHandler(IRepository<Document, string> documents)
    : ICommandHandler<UpdateDocumentCommand, Document>
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult<Document>> HandleAsync(UpdateDocumentCommand command, CancellationToken cancellationToken = default)
    {
        var document = await documents.GetAsync(command.Id, cancellationToken).ConfigureAwait(false);
        if (document == null) return this.NotFound();
        document.Content = command.Content;
        document = await documents.UpdateAsync(document, cancellationToken).ConfigureAwait(false);
        await documents.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return this.Ok(document);
    }

}