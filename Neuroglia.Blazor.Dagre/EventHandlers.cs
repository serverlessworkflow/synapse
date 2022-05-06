// todo: remove with .NET >7.0, the event handles have been added
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Neuroglia.Blazor.Dagre
{
    [EventHandler("onmouseleave", typeof(MouseEventArgs), true, true)]
    [EventHandler("onmouseenter", typeof(MouseEventArgs), true, true)]
    public static class EventHandlers
    {
    }
}
