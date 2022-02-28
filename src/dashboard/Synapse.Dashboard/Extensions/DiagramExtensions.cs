using Blazor.Diagrams.Core;
using Synapse.Dashboard.Models;
using Synapse.Integration.Models;

namespace Synapse.Dashboard
{
    public static class DiagramExtensions
    {

        public static void DisplayActivityStatusFor(this Diagram diagram, IEnumerable<V1WorkflowInstanceDto> instances)
        {
            diagram.ClearActivityStatus();
            foreach (var instance in instances)
            {
                if(instance.Status == V1WorkflowInstanceStatus.Starting)
                {
                    var node = diagram.Nodes.OfType<StartNodeModel>().FirstOrDefault();
                    if(node != null)
                        node.ActiveInstances.Add(instance);
                    continue;
                }
                foreach(var activity in instance.Activities.Where(a => a.Status == V1WorkflowActivityStatus.Running))
                {
                    var node = diagram.GetNodeFor(activity);
                    if(node != null)
                        node.ActiveInstances.Add(instance);    
                }
                foreach (var activity in instance.Activities.Where(a => a.Status == V1WorkflowActivityStatus.Faulted))
                {
                    var node = diagram.GetNodeFor(activity);
                    if (node != null)
                        node.FaultedInstances.Add(instance);
                }
            }
        }

        public static void ClearActivityStatus(this Diagram diagram)
        {
            var nodes = diagram.Nodes.ToList();
            nodes.AddRange(diagram.Groups);
            nodes.AddRange(diagram.Groups.SelectMany(g => g.Children));
            foreach (var node in nodes.OfType<IWorkflowNodeModel>())
            {
                node.ActiveInstances.Clear();
                node.FaultedInstances.Clear();
            }
        }

        public static IWorkflowNodeModel? GetNodeFor(this Diagram diagram, V1WorkflowActivityDto activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            switch (activity.Type)
            {
                case V1WorkflowActivityType.Action:
                    return null;
                case V1WorkflowActivityType.Function:
                    return diagram.GetActionNodeFor(activity);
                case V1WorkflowActivityType.Branch:
                    throw new NotImplementedException(); //todo
                case V1WorkflowActivityType.ConsumeEvent:
                    throw new NotImplementedException(); //todo
                case V1WorkflowActivityType.End:
                    return diagram.Nodes.OfType<EndNodeModel>().FirstOrDefault();
                case V1WorkflowActivityType.Error:
                    throw new NotImplementedException(); //todo
                case V1WorkflowActivityType.EventTrigger:
                    throw new NotImplementedException(); //todo
                case V1WorkflowActivityType.Iteration:
                    throw new NotImplementedException(); //todo
                case V1WorkflowActivityType.ProduceEvent:
                    throw new NotImplementedException(); //todo
                case V1WorkflowActivityType.Start:
                    return diagram.Nodes.OfType<StartNodeModel>().FirstOrDefault();
                case V1WorkflowActivityType.State:
                    return diagram.GetStateNodeFor(activity);
                case V1WorkflowActivityType.SubFlow:
                    throw new NotImplementedException(); //todo
                default:
                    throw new NotSupportedException($"The specified {nameof(V1WorkflowActivityType)} '{activity.Type}' is not supported");
            }
        }

        private static IWorkflowNodeModel? GetStateNodeFor(this Diagram diagram, V1WorkflowActivityDto activity)
        {
            if (!activity.Metadata.TryGetValue("state", out var stateName))
                throw new InvalidDataException($"The specified activity's metadata does not define a 'state' value");
            return diagram.Groups.OfType<StateNodeModel>().FirstOrDefault(g => g.State.Name == stateName);
        }

        private static IWorkflowNodeModel? GetActionNodeFor(this Diagram diagram, V1WorkflowActivityDto activity)
        {
            var stateNode = (StateNodeModel)diagram.GetStateNodeFor(activity)!;
            if (!activity.Metadata.TryGetValue("action", out var actionName))
                throw new InvalidDataException($"The specified activity's metadata does not define a 'action' value");
            return stateNode.Children.OfType<ActionNodeModel>().FirstOrDefault(a => a.Action.Name == actionName);
        }

    }

}
