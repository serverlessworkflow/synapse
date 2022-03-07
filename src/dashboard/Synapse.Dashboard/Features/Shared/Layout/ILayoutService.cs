using System.ComponentModel;

namespace Synapse.Dashboard
{

    public interface ILayoutService
        : INotifyPropertyChanged
    {

        AppHeader Header { get; set; }

        void UpdateHeader();

    }

}
