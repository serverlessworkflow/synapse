using Synapse.Dashboard.Components;
using System.ComponentModel;

namespace Synapse.Dashboard.Services
{

    public interface ILayoutService
        : INotifyPropertyChanged
    {

        AppHeader Header { get; set; }

        void UpdateHeader();

    }

}
