using Neuroglia.Data.Flux;
using Synapse.Integration.Models;
using System.Reactive.Linq;

namespace Synapse.Dashboard.Pages.Schedules.List
{

    /// <summary>
    /// Defines selectors for <see cref="ScheduleCollectionState"/>s
    /// </summary>
    public static class ScheduleCollectionStateSelectors
    {

        /// <summary>
        /// Selects all available schedules
        /// </summary>
        /// <param name="store">The global <see cref="IStore"/></param>
        /// <returns>A new <see cref="IObservable{T}"/></returns>
        public static IObservable<List<V1Schedule>?> SelectedSchedules(IStore store)
        {
            return store.GetFeature<ScheduleCollectionState>()
                .Select(state => state.Schedules)
                .DistinctUntilChanged();
        }

    }

}
