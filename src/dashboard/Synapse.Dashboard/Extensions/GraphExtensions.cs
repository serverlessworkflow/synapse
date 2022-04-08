using Neuroglia.Blazor.Dagre.Models;
using Synapse.Integration.Models;

namespace Synapse.Dashboard
{
    public static class GraphExtensions
    {

        public static void DisplayActivityStatusFor(this IGraphViewModel graph, IEnumerable<V1WorkflowInstance> instances)
        {
            graph.ClearActivityStatus();
            foreach (var instance in instances)
            {
                if(instance.Status == V1WorkflowInstanceStatus.Pending 
                    || instance.Status == V1WorkflowInstanceStatus.Starting)
                {
                    var node = graph.Nodes.OfType<StartNodeViewModel>().FirstOrDefault();
                    if(node != null)
                        node.ActiveInstances.Add(instance);
                    continue;
                }
                foreach(var activity in instance.Activities.Where(a => a.Status == V1WorkflowActivityStatus.Running))
                {
                    var node = graph.GetNodeFor(activity);
                    if(node != null)
                        node.ActiveInstances.Add(instance);    
                }
                foreach (var activity in instance.Activities.Where(a => a.Status == V1WorkflowActivityStatus.Faulted))
                {
                    var node = graph.GetNodeFor(activity);
                    if (node != null)
                        node.FaultedInstances.Add(instance);
                }
            }
        }

        public static void ClearActivityStatus(this IGraphViewModel graph)
        {
            var nodes = graph.AllNodes.Values.ToList();
            nodes.AddRange(graph.AllClusters.Values);
            foreach (var node in nodes.OfType<IWorkflowNodeViewModel>())
            {
                node.ActiveInstances.Clear();
                node.FaultedInstances.Clear();
            }
        }

        public static IWorkflowNodeViewModel? GetNodeFor(this IGraphViewModel graph, V1WorkflowActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            switch (activity.Type)
            {
                case V1WorkflowActivityType.Action:
                    return null;
                case V1WorkflowActivityType.Function:
                    return graph.GetActionNodeFor(activity);
                case V1WorkflowActivityType.Branch:
                    throw new NotImplementedException(); //todo
                case V1WorkflowActivityType.ConsumeEvent:
                    throw new NotImplementedException(); //todo
                case V1WorkflowActivityType.End:
                    return graph.Nodes.OfType<EndNodeViewModel>().FirstOrDefault();
                case V1WorkflowActivityType.Error:
                    throw new NotImplementedException(); //todo
                case V1WorkflowActivityType.EventTrigger:
                    throw new NotImplementedException(); //todo
                case V1WorkflowActivityType.Iteration:
                    throw new NotImplementedException(); //todo
                case V1WorkflowActivityType.ProduceEvent:
                    throw new NotImplementedException(); //todo
                case V1WorkflowActivityType.Start:
                    return graph.Nodes.OfType<StartNodeViewModel>().FirstOrDefault();
                case V1WorkflowActivityType.State:
                    return graph.GetStateNodeFor(activity);
                case V1WorkflowActivityType.SubFlow:
                    throw new NotImplementedException(); //todo
                default:
                    throw new NotSupportedException($"The specified {nameof(V1WorkflowActivityType)} '{activity.Type}' is not supported");
            }
        }

        private static IWorkflowNodeViewModel? GetStateNodeFor(this IGraphViewModel graph, V1WorkflowActivity activity)
        {
            if (!activity.Metadata.TryGetValue("state", out var stateName))
                throw new InvalidDataException($"The specified activity's metadata does not define a 'state' value");
            return graph.Clusters.Values.OfType<StateNodeViewModel>().FirstOrDefault(g => g.State.Name == stateName);
        }

        private static IWorkflowNodeViewModel? GetActionNodeFor(this IGraphViewModel graph, V1WorkflowActivity activity)
        {
            var stateNode = (StateNodeViewModel)graph.GetStateNodeFor(activity)!;
            if (!activity.Metadata.TryGetValue("action", out var actionName))
                throw new InvalidDataException($"The specified activity's metadata does not define a 'action' value");
            return stateNode.Children.OfType<ActionNodeViewModel>().FirstOrDefault(a => a.Action.Name == actionName);
        }

    }

}
