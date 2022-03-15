using Neuroglia.Data.Flux;

namespace Synapse.Dashboard.Features.Test
{
    public static class CounterStateReducers
    {

        [Reducer]
        public static CounterState Increment(CounterState state, IncrementAction action) => new(state.Count + action.Amount);

        [Reducer]
        public static CounterState Decrement(CounterState state, DecrementAction action) => new(state.Count - action.Amount);

    }

}
