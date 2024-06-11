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

using System.Collections;
using System.Collections.Concurrent;

namespace Synapse;

/// <summary>
/// Represents a concurrent <see cref="HashSet{T}"/>
/// </summary>
/// <typeparam name="T">The type of value contained by the <see cref="ConcurrentHashSet{T}"/></typeparam>
public class ConcurrentHashSet<T>
    : ICollection<T>
    where T : notnull
{

    readonly ConcurrentDictionary<T, bool> _dictionary = new();

    /// <inheritdoc/>
    public virtual int Count => this._dictionary.Count;

    /// <inheritdoc/>
    public virtual bool IsReadOnly => false;

    /// <inheritdoc/>
    public virtual bool Contains(T item) => this._dictionary.ContainsKey(item);

    /// <inheritdoc/>
    public virtual void Add(T item)
    {
        if (!this._dictionary.TryAdd(item, false)) throw new InvalidOperationException("The specified item already exists in the collection");
    }

    /// <summary>
    /// Attempts to add the specified item to the hashset
    /// </summary>
    /// <param name="item">The item to add</param>
    /// <returns>A boolean indicating whether or not the item has been added or if it was already present in the set</returns>
    public virtual bool TryAdd(T item) => this._dictionary.TryAdd(item, false);

    /// <inheritdoc/>
    public virtual bool Remove(T item) => this._dictionary.Remove(item, out _);

    /// <inheritdoc/>
    public virtual void Clear() => this._dictionary.Clear();

    /// <inheritdoc/>
    public virtual void CopyTo(T[] array, int arrayIndex) => this._dictionary.Keys.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public virtual IEnumerator<T> GetEnumerator() => this._dictionary.Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

}
