/*
 * Copyright © 2022-Present The Synapse Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

namespace Synapse.Dashboard
{

    /// <summary>
    /// Defines the fundamentals of a service used to manage toasts
    /// </summary>
    public interface IToastManager
        : IDisposable
    {

        /// <summary>
        /// Represents the event fired whenever a toast has been shown
        /// </summary>
        public event Action OnShowToast;
        /// <summary>
        /// Represents the event fired whenever a toast has been hidden
        /// </summary>
        public event Action OnHideToast;

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> containing 
        /// </summary>
        IEnumerable<Toast> Toasts { get; }

        void ShowToast(Action<IToastBuilder> setup);

    }

}
