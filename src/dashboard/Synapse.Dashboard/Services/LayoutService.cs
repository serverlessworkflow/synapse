using Microsoft.AspNetCore.Components;
using Synapse.Dashboard.Components;
using System.ComponentModel;

namespace Synapse.Dashboard.Services
{
    public class LayoutService
        : ILayoutService
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public RenderFragment HeaderFragment => this.Header?.ChildContent;

        private AppHeader _Header;
        public AppHeader Header
        {
            get => _Header;
            set
            {
                if (this._Header == value) return;
                this._Header = value;
                this.UpdateHeader();
            }
        }

        public void UpdateHeader() => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Header)));

    }

}
