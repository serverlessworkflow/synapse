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

using Neuroglia.Data.Infrastructure;
using Neuroglia.Data.Infrastructure.Services;
using System.Runtime.CompilerServices;

namespace Synapse.UnitTests.Services;

internal class MockTextDocumentRepository<TKey>
    : ITextDocumentRepository<TKey>
    where TKey : IEquatable<TKey>
{
    public Task AppendAsync(TKey key, string text, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task AppendAsync(object key, string text, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task DeleteAsync(TKey key, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task DeleteAsync(object key, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public async IAsyncEnumerable<ITextDocument<TKey>> GetAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        yield break;
    }

    public Task<ITextDocument<TKey>?> GetAsync(TKey key, CancellationToken cancellationToken = default) => Task.FromResult((ITextDocument<TKey>?)null);

    public Task<ITextDocument?> GetAsync(object key, CancellationToken cancellationToken = default) => Task.FromResult((ITextDocument?)null);

    public Task<System.Collections.Generic.ICollection<ITextDocument<TKey>>> ListAsync(int? max = null, int? skip = null, CancellationToken cancellationToken = default) => Task.FromResult((System.Collections.Generic.ICollection<ITextDocument<TKey>>)[]);

    public async IAsyncEnumerable<string> ReadAsync(TKey key, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        yield break;
    }

    public async IAsyncEnumerable<string> ReadAsync(object key, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        yield break;
    }

    public Task<string> ReadToEndAsync(TKey key, CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);

    public Task<string> ReadToEndAsync(object key, CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);

    public Task ReplaceAsync(TKey key, string text, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task ReplaceAsync(object key, string text, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<ITextDocumentWatch> WatchAsync(TKey key, CancellationToken cancellationToken = default) => Task.FromResult((ITextDocumentWatch)null!);

    public Task<ITextDocumentWatch> WatchAsync(object key, CancellationToken cancellationToken = default) => Task.FromResult((ITextDocumentWatch)null!);

    async IAsyncEnumerable<ITextDocument> ITextDocumentRepository.GetAllAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        yield break;
    }

    Task<System.Collections.Generic.ICollection<ITextDocument>> ITextDocumentRepository.ListAsync(int? max, int? skip, CancellationToken cancellationToken) => Task.FromResult((System.Collections.Generic.ICollection<ITextDocument>)[]);

}