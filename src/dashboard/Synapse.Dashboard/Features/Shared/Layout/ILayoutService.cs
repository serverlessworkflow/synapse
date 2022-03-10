using System.ComponentModel;

namespace Synapse.Dashboard
{

    public interface ILayoutService
        : INotifyPropertyChanged
    {

        AppHeader? Header { get; set; }
        AppRightSidebar? RightSidebar { get; set; }
        AppLeftSidebar? LeftSidebar { get; set; }

        void UpdateHeader();

        void UpdateRightSidebar();

        void UpdateLeftSidebar();

    }

}
