using Neuroglia.Blazor.Dagre;
using Neuroglia.Blazor.Dagre.Models;

namespace Synapse.Dashboard
{
    public class JunctionNodeViewModel
        : NodeViewModel
    {
        public JunctionNodeViewModel()
            : base("", "junction-node", NodeShape.Circle, 2, 2)
        {

        }
    }
}
