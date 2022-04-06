using Neuroglia.Blazor.Dagre;
using Neuroglia.Blazor.Dagre.Models;

namespace Synapse.Dashboard
{
    public class StartNodeViewModel
        : NodeViewModel
    {
        public StartNodeViewModel()
            :base("", "start-node", NodeShape.Circle, 20, 20)
        {

        }
    }
}
