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
                if (instance.Status == V1WorkflowInstanceStatus.Pending
                    || instance.Status == V1WorkflowInstanceStatus.Starting)
                {
                    var node = graph.Nodes.Values.OfType<StartNodeViewModel>().FirstOrDefault();
                    if (node != null)
                        node.ActiveInstances.Add(instance);
                    continue;
                }
                if (instance.Activities != null)
                {
                    foreach (var activity in instance.Activities)
                    {
                        var nodes = graph.GetNodesFor(activity);
                        if (nodes != null && nodes.Any())
                        {
                            if (activity.Status == V1WorkflowActivityStatus.Pending || activity.Status == V1WorkflowActivityStatus.Running)
                            {
                                nodes.ToList().ForEach(node => node.ActiveInstances.Add(instance));
                            }
                            else if(activity.Status == V1WorkflowActivityStatus.Compensating)
                            {
                                nodes.ToList().ForEach(node => node.CompensatedInstances.Add(instance));
                            }
                            else if (activity.Status == V1WorkflowActivityStatus.Faulted)
                            {
                                nodes.ToList().ForEach(node => node.FaultedInstances.Add(instance));
                            }
                            if (highlightPath)
                            {
                                nodes.ToList().ForEach(node =>
                                {
                                    ((INodeViewModel)node).CssClass = (((INodeViewModel)node).CssClass ?? "") + " active";
                                });
                            }
                        }
                    }
                }
                if (instance.Status == V1WorkflowInstanceStatus.Completed)
                {
                    var node = graph.Nodes.Values.OfType<EndNodeViewModel>().FirstOrDefault();
                    if (node != null)
                        node.ActiveInstances.Add(instance);
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
                    wfNode.CompensatedInstances.Clear();
                }
                node.CssClass = node.CssClass?.Replace(" active", "");
            }
        }

        public static IEnumerable<IWorkflowNodeViewModel>? GetNodesFor(this IGraphViewModel graph, V1WorkflowActivity activity)
        {
            if (activity == null)
                return null;
            switch (activity.Type)
            {
                case V1WorkflowActivityType.Action:
                    return graph.GetActionNodesFor(activity);
                case V1WorkflowActivityType.Function:
                    return graph.GetActionNodesFor(activity);
                case V1WorkflowActivityType.Transition:
                    return graph.GetTransitionNodesFor(activity);
                case V1WorkflowActivityType.Branch:
                    throw new NotImplementedException(); //todo
                case V1WorkflowActivityType.ConsumeEvent:
                    return graph.GetConsumeEventNodesFor(activity);
                case V1WorkflowActivityType.End:
                    return graph.Nodes.Values.OfType<EndNodeViewModel>();
                case V1WorkflowActivityType.Error:
                    throw new NotImplementedException(); //todo
                case V1WorkflowActivityType.EventTrigger:
                    return graph.GetEventTriggerNodesFor(activity);
                case V1WorkflowActivityType.Iteration:
                    return graph.GetIterationNodesFor(activity);
                case V1WorkflowActivityType.ProduceEvent:
                    throw new NotImplementedException(); //todo
                case V1WorkflowActivityType.Start:
                    return graph.Nodes.Values.OfType<StartNodeViewModel>();
                case V1WorkflowActivityType.State:
                    return graph.GetInnerStateNodesFor(activity);
                case V1WorkflowActivityType.SubFlow:
                    throw new NotImplementedException(); //todo
                default:
                    return null;
            }
        }

        private static IEnumerable<StateNodeViewModel> GetStateNodesFor(this IGraphViewModel graph, V1WorkflowActivity activity)
        {
            if (!activity.Metadata.TryGetValue("state", out var stateName))
                throw new InvalidDataException($"The specified activity's metadata does not define a 'state' value");
            return graph.Clusters.Values.OfType<StateNodeViewModel>().Where(g => g.State.Name == stateName);
        }

        private static IEnumerable<IWorkflowNodeViewModel> GetInnerStateNodesFor(this IGraphViewModel graph, V1WorkflowActivity activity)
        {
            var stateNodes = graph.GetStateNodesFor(activity);
            return stateNodes
                .Concat(stateNodes.SelectMany(node => node.Children.Values.OfType<InjectNodeViewModel>()) as IEnumerable<IWorkflowNodeViewModel>)
                .Concat(stateNodes.SelectMany(node => node.Children.Values.OfType<SleepNodeViewModel>()) as IEnumerable<IWorkflowNodeViewModel>)
            ;
        }

        private static IEnumerable<IWorkflowNodeViewModel>? GetActionNodesFor(this IGraphViewModel graph, V1WorkflowActivity activity)
        {
            var stateNodes = graph.GetStateNodesFor(activity);
            if (!activity.Metadata.TryGetValue("action", out var actionName))
                throw new InvalidDataException($"The specified activity's metadata does not define a 'action' value");
            return stateNodes.SelectMany(node => node.Children.Values
                .Where(node => typeof(IActionNodeViewModel).IsAssignableFrom(node.GetType()))
                .Select(node => node as IActionNodeViewModel)
                .Where(node => node != null && node.Action.Name == actionName)
            );
        }

        private static IEnumerable<IWorkflowNodeViewModel>? GetTransitionNodesFor(this IGraphViewModel graph, V1WorkflowActivity activity)
        {
            var stateNodes = graph.GetStateNodesFor(activity);
            if (!activity.Metadata.TryGetValue("case", out var caseName))
                return null;
            return stateNodes.SelectMany(node => 
                node.Children.Values.OfType<DataCaseNodeViewModel>().Where(node => node != null && node.DataCaseName == caseName)
            );
        }

        private static IEnumerable<IWorkflowNodeViewModel>? GetIterationNodesFor(this IGraphViewModel graph, V1WorkflowActivity activity)
        {
            var stateNodes = graph.GetStateNodesFor(activity);
            return stateNodes.SelectMany(node => node.Children.Values.OfType<ForEachNodeViewModel>());
        }

        private static IEnumerable<IWorkflowNodeViewModel>? GetEventTriggerNodesFor(this IGraphViewModel graph, V1WorkflowActivity activity)
        {
            var stateNodes = graph.GetStateNodesFor(activity);
            if (!activity.Metadata.TryGetValue("trigger", out var eventName))
                throw new InvalidDataException($"The specified activity's metadata does not define a 'trigger' value");
            return stateNodes.SelectMany(node => node.Children.Values.OfType<EventNodeViewModel>()
                .Where(eventNode => eventNode.RefName == eventName /* && eventNode.Kind == ServerlessWorkflow.Sdk.EventKind.Consumed */)
            );
        }

        private static IEnumerable<IWorkflowNodeViewModel>? GetConsumeEventNodesFor(this IGraphViewModel graph, V1WorkflowActivity activity)
        {
            var stateNodes = graph.GetStateNodesFor(activity);
            if (!activity.Metadata.TryGetValue("event", out var eventName))
                throw new InvalidDataException($"The specified activity's metadata does not define a 'event' value");
            return stateNodes.SelectMany(node => node.Children.Values.OfType<EventNodeViewModel>()
                .Where(eventNode => eventNode.RefName == eventName /* && eventNode.Kind == ServerlessWorkflow.Sdk.EventKind.Consumed */)
            );
        }


    }

}
