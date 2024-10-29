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

namespace Synapse.Dashboard.Services.Interfaces;

/// <summary>
/// Defines a contract for interacting with the browser's localStorage API, 
/// providing both asynchronous and synchronous methods for managing key-value pairs.
/// </summary>
public interface ILocalStorage
{
    /// <summary>
    /// Asynchronously retrieves a value from localStorage by key.
    /// </summary>
    /// <param name="key">The key of the item to retrieve.</param>
    /// <returns>A ValueTask containing the string value, or null if the key does not exist.</returns>
    ValueTask<string?> GetItemAsync(string key);

    /// <summary>
    /// Asynchronously sets a key-value pair in localStorage.
    /// </summary>
    /// <param name="key">The key to store.</param>
    /// <param name="value">The value associated with the key.</param>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    ValueTask SetItemAsync(string key, string value);

    /// <summary>
    /// Asynchronously removes an item from localStorage by key.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    ValueTask RemoveItemAsync(string key);

    /// <summary>
    /// Asynchronously clears all key-value pairs in localStorage.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    ValueTask ClearAsync();

    /// <summary>
    /// Asynchronously gets the number of items in localStorage.
    /// </summary>
    /// <returns>A ValueTask containing the number of items stored.</returns>
    ValueTask<int> LengthAsync();

    /// <summary>
    /// Asynchronously retrieves the key at the specified index in localStorage.
    /// </summary>
    /// <param name="index">The zero-based index of the key to retrieve.</param>
    /// <returns>A ValueTask containing the key at the specified index, or null if not found.</returns>
    ValueTask<string?> KeyAsync(int index);

    /// <summary>
    /// Asynchronously retrieves the keys in localStorage.
    /// </summary>
    /// <returns>A ValueTask containing the keys of the localStorage</returns>
    ValueTask<IEnumerable<string>> KeysAsync();
}