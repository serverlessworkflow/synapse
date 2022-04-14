using Neuroglia.Data.Flux;

namespace Synapse.Dashboard.Features.Test
{

    [Feature]
    public class CounterState
    {

        public CounterState()
        {

        }

        public CounterState(int count)
        {
            this.Count = count;
        }

        public int Count { get; }

    }

}
