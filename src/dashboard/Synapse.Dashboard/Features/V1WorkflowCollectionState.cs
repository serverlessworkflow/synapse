using Neuroglia.Data.Flux;
using Synapse.Integration.Models;

namespace Synapse.Dashboard
{

    [Feature]
    public class V1WorkflowCollectionState
        : List<V1Workflow>
    {

        public V1WorkflowCollectionState()
        {
            this.Add(new() { Id = "hello:0.1.0-alpha1", Definition = new() { Id= "hello", Name = "Hello", Version = "0.1.0" } });
            this.Add(new() { Id = "hello:0.2.0-alpha1", Definition = new() { Id = "hello", Name = "Hello", Version = "0.2.0" } });
            this.Add(new() { Id = "hello:0.3.0-alpha1", Definition = new() { Id = "hello", Name = "Hello", Version = "0.3.0" } });
        }

    }

    [Reducer]
    public static class V1WorkflowCollectionReducers
    {

        public static V1WorkflowCollectionState AddV1Workflow(V1WorkflowCollectionState state, AddV1Workflow action)
        {
            state.Add(action.Workflow);
            return state;
        }

    }

    public class AddV1Workflow
    {

        public AddV1Workflow(V1Workflow workflow)
        {
            this.Workflow = workflow;
        }

        public V1Workflow Workflow { get; }

    }

    public class RemoveV1Workflow
    {

        public RemoveV1Workflow(V1Workflow workflow)
        {
            this.Workflow = workflow;
        }

        public V1Workflow Workflow { get; }

    }

}
