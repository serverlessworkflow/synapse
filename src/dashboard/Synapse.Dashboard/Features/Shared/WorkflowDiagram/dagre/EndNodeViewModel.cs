using Neuroglia.Blazor.Dagre;
using Neuroglia.Blazor.Dagre.Models;

namespace Synapse.Dashboard
{
    public class EndNodeViewModel
        : NodeViewModel
    {
        public EndNodeViewModel()
            :base("", "end-node", NodeShape.Circle, 20, 20)
        {

        }
    }
}
