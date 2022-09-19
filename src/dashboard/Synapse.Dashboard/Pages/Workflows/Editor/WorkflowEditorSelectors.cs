using Neuroglia.Data.Flux;
using Synapse.Dashboard.Pages.Workflows.Editor.State;
using System.Reactive.Linq;

namespace Synapse.Dashboard.Pages.Workflows.Editor
{
    public static class WorkflowEditorSelectors
    {
        /// <summary>
        /// Selects the workflow definition text
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public static IObservable<string?> SelectWorkflowDefinitionText(IStore store)
        {
            return store.GetFeature<WorkflowEditorState>()
                .Select(featureState => featureState.WorkflowDefinitionText)
                .DistinctUntilChanged();
        }
    }
}
