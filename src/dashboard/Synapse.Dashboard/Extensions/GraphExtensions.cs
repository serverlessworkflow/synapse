/*
 * Copyright © 2022-Present The Synapse Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Neuroglia.Blazor.Dagre.Models;
using Synapse.Integration.Models;

namespace Synapse.Dashboard
{
    public static class GraphExtensions
    {

        public static void DisplayActivityStatusFor(this IGraphViewModel graph, IEnumerable<V1WorkflowInstance> instances, bool highlightPath = false)
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
                if (instance.Activities != null) { 
                    foreach(var activity in instance.Activities)
                    {
                        var node = graph.GetNodeFor(activity);
                        if(node != null)
                        {
                            if (activity.Status == V1WorkflowActivityStatus.Pending || activity.Status == V1WorkflowActivityStatus.Running)
                            {
                                node.ActiveInstances.Add(instance);
                            }
                            else if (activity.Status == V1WorkflowActivityStatus.Faulted)
                            {
                                node.FaultedInstances.Add(instance);
                            }
                            if (highlightPath)
                            {
                                ((INodeViewModel)node).CssClass = (((INodeViewModel)node).CssClass ?? "") + " active" ;
                            }
                        }
                    }
                }
            }
        }

        public static void ClearActivityStatus(this IGraphViewModel graph)
        {
            var nodes = graph.AllNodes.Values.ToList();
            nodes.AddRange(graph.AllClusters.Values);
            foreach (var node in nodes)
            {
                if (node is IWorkflowNodeViewModel wfNode)
                {
                    wfNode.ActiveInstances.Clear();
                    wfNode.FaultedInstances.Clear();
                }
                node.CssClass = node.CssClass?.Replace(" active", "");
            }
        }

        public static IWorkflowNodeViewModel? GetNodeFor(this IGraphViewModel graph, V1WorkflowActivity activity)
        {
            if (activity == null)
                return null;
            switch (activity.Type)
            {
                case V1WorkflowActivityType.Action:
                    return graph.GetActionNodeFor(activity);
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
                    return null;
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
            return stateNode.Children.Values.Where(node => typeof(IActionNodeViewModel).IsAssignableFrom(node.GetType())).Select(node => node as IActionNodeViewModel).FirstOrDefault(node => node != null && node.Action.Name == actionName);
        }

    }

}
