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

using Microsoft.AspNetCore.Components;

namespace Synapse.Dashboard
{

    public class Toast
        : IDisposable
    {

        public event Action<Toast> OnHide;

        public Guid Id { get; } = Guid.NewGuid();

        public DateTime CreatedAt { get; } = DateTime.Now;

        public LogLevel Level { get; set; } = LogLevel.Information;

        public bool HasHeader { get; set; }

        public MarkupString Header { get; set; }

        public MarkupString Body { get; set; }

        public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(30);

        public Action OnHideCallback { get; set; }

        protected System.Timers.Timer Timer { get; private set; }

        public void Show()
        {
            this.Timer = new(this.Duration.TotalMilliseconds) { AutoReset = false };
            this.Timer.Elapsed += (sender, e) => this.OnTimerElapsed();
            this.Timer.Start();
        }

        public void Hide()
        {
            this.OnTimerElapsed();
        }

        private void OnTimerElapsed()
        {
            this.Timer.Stop();
            this.Timer.Dispose();
            this.Timer = null;
            this.OnHide?.Invoke(this);
        }

        private bool _Disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!this._Disposed)
            {
                if (disposing)
                    this.Timer?.Dispose();
                this._Disposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }

}
