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

using System.Collections.Concurrent;

namespace Synapse.Dashboard
{

    public class ToastManager
        : IToastManager
    {

        /// <inheritdoc/>
        public event Action OnShowToast;
        /// <inheritdoc/>
        public event Action OnHideToast;

        protected ConcurrentDictionary<Guid, Toast> ToastMap { get; } = new();

        /// <inheritdoc/>
        public IEnumerable<Toast> Toasts => this.ToastMap.Values;

        /// <inheritdoc/>
        public virtual void ShowToast(Action<IToastBuilder> setup)
        {
            ToastBuilder builder = new();
            setup?.Invoke(builder);
            Toast toast = builder.Build();
            toast.OnHide += this.OnToastHidden;
            this.ToastMap.TryAdd(toast.Id, toast);
            toast.Show();
            this.OnShowToast?.Invoke();
        }

        /// <inheritdoc/>
        public virtual void OnToastHidden(Toast toast)
        {
            toast.OnHide -= this.OnToastHidden;
            this.ToastMap.TryRemove(toast.Id, out _);
            toast.Dispose();
            toast.OnHideCallback?.Invoke();
            this.OnHideToast?.Invoke();
        }

        private bool _Disposed;
        /// <summary>
        /// Disposes of the <see cref="Toast"/>
        /// </summary>
        /// <param name="disposing">A boolean indicating whether or not the <see cref="Toast"/> is being disposed of</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._Disposed)
            {
                if (disposing)
                {
                    this.ToastMap.Values.ToList().ForEach(t => t.Dispose());
                    this.ToastMap.Clear();
                }
                this._Disposed = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }

}
