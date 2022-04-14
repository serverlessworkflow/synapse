using Neuroglia.Data.Flux;

namespace Synapse.Dashboard.Features.Test
{
    public static class CounterStateReducers
    {

        [Reducer]
        public static CounterState Increment(CounterState state, Increment action) => new(state.Count + action.Amount);

        [Reducer]
        public static CounterState Decrement(CounterState state, Decrement action) => new(state.Count - action.Amount);

    }

}
