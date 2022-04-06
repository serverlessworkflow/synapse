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
