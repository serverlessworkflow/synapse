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

namespace Synapse.Runner;

/// <summary>
/// Defines extensions for <see cref="IDictionary{TKey, TValue}"/> instances
/// </summary>
public static class IDictionaryExtensions
{

    /// <summary>
    /// Attempts to get the specified value
    /// </summary>
    /// <typeparam name="TValue">The type of values contained by the <see cref="IDictionary{TKey, TValue}"/></typeparam>
    /// <param name="dictionary">The extended <see cref="IDictionary{TKey, TValue}"/></param>
    /// <param name="key">The key of the value to get</param>
    /// <param name="value">The value with the specified key, if any</param>
    /// <param name="comparison">The <see cref="StringComparison"/> to use when comparing the keys</param>
    /// <returns>A boolean indicating whether or not the <see cref="IDictionary{TKey, TValue}"/> contains the specified key</returns>
    public static bool TryGetValue<TValue>(this IDictionary<string, TValue> dictionary, string key, out TValue? value, StringComparison comparison)
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var kvp = dictionary.FirstOrDefault(p => p.Key.Equals(key, comparison));
        value = kvp.Value;
        return kvp.Key != null;
    }

}
