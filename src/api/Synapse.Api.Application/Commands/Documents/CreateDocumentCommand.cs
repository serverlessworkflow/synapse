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

using Synapse.Resources;
using Neuroglia.Data.Infrastructure.Services;

namespace Synapse.Api.Application.Commands.Documents;

/// <summary>
/// Represents the <see cref="ICommand"/> used to create a new <see cref="Synapse.Resources.Document"/>
/// </summary>
/// <param name="document">The  document to create</param>
public class CreateDocumentCommand(Document document)
    : Command<Document>
{

    /// <summary>
    /// Gets the document to create
    /// </summary>
    public Document Document { get; } = document;

}

/// <summary>
/// Represents the service used to handle <see cref="CreateDocumentCommand"/>s
/// </summary>
/// <param name="documents">The service used to manage <see cref="Document"/>s</param>
public class CreateDocumentCommandHandler(IRepository<Document> documents)
    : ICommandHandler<CreateDocumentCommand, Document>
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult<Document>> HandleAsync(CreateDocumentCommand command, CancellationToken cancellationToken = default)
    {
        var document = await documents.AddAsync(command.Document, cancellationToken).ConfigureAwait(false);
        await documents.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return this.Ok(document);
    }

}
