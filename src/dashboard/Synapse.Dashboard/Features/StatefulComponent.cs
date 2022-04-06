using Microsoft.AspNetCore.Components;
using Neuroglia.Data.Flux;

namespace Synapse.Dashboard
{

    public abstract class StatefulComponent<TState>
        : ComponentBase, IDisposable
    {

        private bool _Disposed;
        private IDisposable? _Subscription;

        [Inject]
        public IDispatcher Dispatcher { get; set; } = null!;

        [Inject]
        public IStore Store { get; set; } = null!;

        public IFeature<TState> Feature { get; private set; } = null!;

        public TState State => this.Feature.State;

        protected virtual void Dispose(bool disposing)
        {
            if (!this._Disposed)
            {
                if (disposing)
                {
                    this._Subscription?.Dispose();
                    this._Subscription = null;
                }
                this._Disposed = true;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            this.Feature = this.Store.GetFeature<TState>();
            this._Subscription = this.Feature.Subscribe(_ => this.StateHasChanged());
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    
    }

}
