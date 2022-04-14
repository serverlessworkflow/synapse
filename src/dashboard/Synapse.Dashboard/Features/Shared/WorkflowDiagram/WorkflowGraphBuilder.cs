/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Neuroglia.Blazor.Dagre.Models;
using ServerlessWorkflow.Sdk;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard
{
    public class WorkflowGraphBuilder
    //: IWorkflowGraphViewModelBuilder
    {

        /// <inheritdoc/>
        public async Task<IGraphViewModel> BuildGraph(WorkflowDefinition definition)
        {
            var graph = new GraphViewModel();
            var startState = definition.GetStartState();
            var startNode = this.BuildStartNode();
            var endNode = this.BuildEndNode();
            await graph.AddElementAsync(startNode);
            await this.BuildStateNodes(definition, graph, startState, endNode, startNode);
            await graph.AddElementAsync(endNode);
            return await Task.FromResult(graph);
        }

        protected NodeViewModel BuildStartNode()
        {
            return new StartNodeViewModel();
        }

        protected async Task<NodeViewModel> BuildStateNodes(WorkflowDefinition definition, GraphViewModel graph, StateDefinition state, NodeViewModel endNode, NodeViewModel previousNode)
        {
            var stateNodeGroup = new StateNodeViewModel(state);
            await graph.AddElementAsync(stateNodeGroup);
            //List<NodeViewModel> childNodes = new();
            NodeViewModel? firstNode, lastNode = null;
            switch (state)
            {
                case CallbackStateDefinition callbackState:
                    { 
                        var actionNodes = await this.BuildActionNodes(graph, callbackState.Action!);
                        lastNode = this.BuildConsumeEventNode(callbackState.Event!);
                        foreach (var actionNode in actionNodes)
                        {
                            await stateNodeGroup.AddChildAsync(actionNode);
                        }
                        await this.BuildEdgeBetween(graph, previousNode, actionNodes.First());
                        await this.BuildEdgeBetween(graph, actionNodes.Last(), lastNode);
                        await stateNodeGroup.AddChildAsync(lastNode);
                        break;
                    }
                case EventStateDefinition eventState:
                    { 
                        firstNode = eventState.Exclusive ? this.BuildGatewayNode(GatewayNodeType.Xor) : this.BuildJunctionNode();
                        lastNode = eventState.Exclusive ? this.BuildGatewayNode(GatewayNodeType.Xor) : this.BuildJunctionNode();
                        await stateNodeGroup.AddChildAsync(firstNode);
                        await this.BuildEdgeBetween(graph, previousNode, firstNode);
                        var andNode = this.BuildGatewayNode(GatewayNodeType.And);
                        if (!eventState.Exclusive)
                        {
                            await stateNodeGroup.AddChildAsync(andNode);
                        }
                        foreach (var trigger in eventState.Triggers)
                        {
                            var refName = string.Join(" | ", trigger.Events);
                            var eventNode = this.BuildConsumeEventNode(refName);
                            await stateNodeGroup.AddChildAsync(eventNode);
                            await this.BuildEdgeBetween(graph, firstNode, eventNode);
                            if (eventState.Exclusive) { 
                                foreach (var action in trigger.Actions)
                                {
                                    var actionsNodes = await this.BuildActionNodes(graph, action);
                                    foreach(var actionNode in actionsNodes)
                                    {
                                        await stateNodeGroup.AddChildAsync(actionNode);
                                    }
                                    await this.BuildEdgeBetween(graph, eventNode, actionsNodes.First());
                                    await this.BuildEdgeBetween(graph, actionsNodes.Last(), lastNode);
                                }
                            }
                            else
                            {
                                await this.BuildEdgeBetween(graph, eventNode, andNode);
                            }
                        }
                        if (!eventState.Exclusive)
                        {
                            foreach (var action in eventState.Triggers.SelectMany(trigger => trigger.Actions))
                            {
                                var actionsNodes = await this.BuildActionNodes(graph, action);
                                foreach (var actionNode in actionsNodes)
                                {
                                    await stateNodeGroup.AddChildAsync(actionNode);
                                }
                                await this.BuildEdgeBetween(graph, andNode, actionsNodes.First());
                                await this.BuildEdgeBetween(graph, actionsNodes.Last(), lastNode);
                            }
                        }
                        await stateNodeGroup.AddChildAsync(lastNode);
                        break;
                    }
                case ForEachStateDefinition foreachState:
                    {
                        firstNode = this.BuildForEachNode(foreachState);
                        lastNode = this.BuildForEachNode(foreachState);
                        await stateNodeGroup.AddChildAsync(firstNode);
                        await this.BuildEdgeBetween(graph, previousNode, firstNode);
                        foreach (var action in foreachState.Actions)
                        {
                            var actionNodes = await this.BuildActionNodes(graph, action);
                            foreach (var actionNode in actionNodes)
                            {
                                await stateNodeGroup.AddChildAsync(actionNode);
                            }
                            await this.BuildEdgeBetween(graph, firstNode, actionNodes.First());
                            await this.BuildJunctionBetween(graph, actionNodes.Last(), lastNode);
                        }
                        await stateNodeGroup.AddChildAsync(lastNode);
                        break;
                    }
                case InjectStateDefinition injectState:
                    {
                        lastNode = this.BuildInjectNode(injectState);
                        await stateNodeGroup.AddChildAsync(lastNode);
                        await this.BuildEdgeBetween(graph, previousNode, lastNode);
                        break;
                    }
                case OperationStateDefinition operationState:
                    { 
                        switch (operationState.ActionMode)
                        {
                            case ActionExecutionMode.Parallel:
                                firstNode = this.BuildGatewayNode(GatewayNodeType.And);
                                lastNode = this.BuildJunctionNode();
                                await stateNodeGroup.AddChildAsync(firstNode);
                                await this.BuildEdgeBetween(graph, previousNode, firstNode);
                                foreach(var action in operationState.Actions)
                                {
                                    var actionNodes = await this.BuildActionNodes(graph, action);
                                    foreach (var actionNode in actionNodes)
                                    {
                                        await stateNodeGroup.AddChildAsync(actionNode);
                                    }
                                    await this.BuildEdgeBetween(graph, firstNode, actionNodes.First());
                                    await this.BuildJunctionBetween(graph, actionNodes.Last(), lastNode);
                                }
                                await stateNodeGroup.AddChildAsync(lastNode);
                                break;
                            case ActionExecutionMode.Sequential:
                                lastNode = previousNode;
                                foreach (var action in operationState.Actions)
                                {
                                    var actionNodes = await this.BuildActionNodes(graph, action);
                                    foreach (var actionNode in actionNodes)
                                    {
                                        await stateNodeGroup.AddChildAsync(actionNode);
                                    }
                                    await this.BuildEdgeBetween(graph, lastNode, actionNodes.First());
                                    lastNode = actionNodes.Last();
                                }
                                break;
                            default:
                                throw new Exception($"The specified action execution mode '{operationState.ActionMode}' is not supported");
                        }
                        break;
                    }
                case ParallelStateDefinition parallelState:
                    {
                        firstNode = this.BuildGatewayNode(parallelState.CompletionType == ParallelCompletionType.AllOf ? GatewayNodeType.And : GatewayNodeType.N);
                        lastNode = this.BuildJunctionNode();
                        await stateNodeGroup.AddChildAsync(firstNode);
                        await this.BuildEdgeBetween(graph, previousNode, firstNode);
                        foreach(var branch in parallelState.Branches)
                        {
                            foreach(var action in branch.Actions)
                            {
                                var actionNodes = await this.BuildActionNodes(graph, action);
                                foreach (var actionNode in actionNodes)
                                {
                                    await stateNodeGroup.AddChildAsync(actionNode);
                                }
                                await this.BuildEdgeBetween(graph, firstNode, actionNodes.First());
                                await this.BuildJunctionBetween(graph, actionNodes.Last(), lastNode);
                            }
                        }
                        await stateNodeGroup.AddChildAsync(lastNode);
                        break;
                    }
                case SleepStateDefinition sleepState:
                    { 
                        lastNode = this.BuildSleepNode(sleepState);
                        await stateNodeGroup.AddChildAsync(lastNode);
                        await this.BuildEdgeBetween(graph, previousNode, lastNode);
                        break;
                    }
                case SwitchStateDefinition switchState:
                    {
                        firstNode = this.BuildGatewayNode(GatewayNodeType.Xor);
                        await stateNodeGroup.AddChildAsync(firstNode);
                        await this.BuildEdgeBetween(graph, previousNode, firstNode);
                        switch (switchState.SwitchType)
                        {
                            case SwitchStateType.Data:
                                foreach(var condition in switchState.DataConditions)
                                {
                                    var caseNode = this.BuildDataConditionNode(condition.Name!); // todo: should be a labeled edge, not a node?
                                    await stateNodeGroup.AddChildAsync(caseNode);
                                    await this.BuildEdgeBetween(graph, firstNode, caseNode);
                                    switch (condition.Type)
                                    {
                                        case ConditionType.End:
                                            await this.BuildEdgeBetween(graph, caseNode, endNode);
                                            break;
                                        case ConditionType.Transition:
                                            var nextStateName = condition.Transition == null ? condition.TransitionToStateName : condition.Transition.NextState;
                                            var nextState = definition.GetState(nextStateName!);
                                            if (nextState == null)
                                                throw new Exception($"Failed to find a state with name '{nextStateName}' in definition '{definition.GetUniqueIdentifier()}");
                                            lastNode = await this.BuildStateNodes(definition, graph, nextState, endNode, caseNode);
                                            break;
                                        default:
                                            throw new Exception($"The specified condition type '${condition.Type}' is not supported");
                                    }
                                }
                                var defaultCaseNode = this.BuildDataConditionNode("default");
                                await stateNodeGroup.AddChildAsync(defaultCaseNode);
                                await this.BuildEdgeBetween(graph, firstNode, defaultCaseNode);
                                if (switchState.DefaultCondition.IsEnd
                                    || switchState.DefaultCondition.End != null)
                                {
                                    lastNode = defaultCaseNode;
                                }
                                else if (!string.IsNullOrWhiteSpace(switchState.DefaultCondition.TransitionToStateName)
                                    || switchState.DefaultCondition.Transition != null)
                                {
                                    var nextStateName = switchState.DefaultCondition.Transition == null ? switchState.DefaultCondition.TransitionToStateName : switchState.DefaultCondition.Transition.NextState;
                                    var nextState = definition.GetState(nextStateName!);
                                    if (nextState == null)
                                        throw new Exception($"Failed to find a state with name '{nextStateName}' in definition '{definition.GetUniqueIdentifier()}");
                                    lastNode = await this.BuildStateNodes(definition, graph, nextState, endNode, defaultCaseNode);
                                }
                                break;
                            case SwitchStateType.Event:
                                throw new NotImplementedException();
                                //break;
                            default:
                                throw new Exception($"The specified switch state type '{switchState.Type}' is not supported");
                        }
                        break;
                    }
                default:
                    throw new Exception($"The specified state type '{state.Type}' is not supported");
            }
            if (lastNode == null)
            {
                throw new Exception($"Unable to define a last node for state '{state.Name}'. Every switch case should provide a last node.");
            }
            if (state.IsEnd
                || state.End != null)
            {
                //Console.WriteLine($"State '{state.Name}' ends");
                await this.BuildEdgeBetween(graph, lastNode, endNode);
                return lastNode;
            }
            if (!string.IsNullOrWhiteSpace(state.TransitionToStateName)
                || state.Transition != null)
            {
                var nextStateName = state.Transition == null ? state.TransitionToStateName! : state.Transition!.NextState;
                //Console.WriteLine($"State '{state.Name}' transitions to '{nextStateName}'");
                var nextState = definition.GetState(nextStateName);
                if (nextState == null)
                    throw new Exception($"Failed to find a state with name '{nextStateName}' in definition '{definition.GetUniqueIdentifier()}'");
                await this.BuildStateNodes(definition, graph, nextState, endNode, lastNode);
                return lastNode;
            }
            //Console.WriteLine($"No transition for state '{state.Name}'");
            //Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(state));
            return lastNode;
        }

        protected async Task<List<NodeViewModel>> BuildActionNodes(GraphViewModel graph, ActionDefinition action)
        {
            switch (action.Type)
            {
                case ActionType.Function:
                    return new() { this.BuildFunctionNode(action, action.Function!) };
                case ActionType.Subflow:
                    return new() { this.BuildSubflowNode(action.Subflow!) };
                case ActionType.Trigger:
                    var triggerEventNode = this.BuildProduceEventNode(action.Event!.ProduceEvent);
                    var resultEventNode = this.BuildConsumeEventNode(action.Event!.ResultEvent);
                    await this.BuildEdgeBetween(graph, triggerEventNode, resultEventNode);
                    return new() { triggerEventNode, resultEventNode };
                default:
                    throw new NotSupportedException($"The specified action type '{action.Type}' is not supported");
            }
        }

        protected FunctionRefNodeViewModel BuildFunctionNode(ActionDefinition action, FunctionReference function)
        {
            return new(action, function);
        }

        protected SubflowRefNodeViewModel BuildSubflowNode(SubflowReference subflowRef)
        {
            return new(subflowRef);
        }

        protected EventNodeViewModel BuildProduceEventNode(string refName)
        {
            return new(EventKind.Produced, refName);
        }

        protected EventNodeViewModel BuildConsumeEventNode(string refName)
        {
            return new(EventKind.Consumed, refName);
        }

        protected GatewayNodeViewModel BuildGatewayNode(GatewayNodeType gatewayNodeType)
        {
            return new(gatewayNodeType);
        }

        protected JunctionNodeViewModel BuildJunctionNode()
        {
            return new();
        }

        protected InjectNodeViewModel BuildInjectNode(InjectStateDefinition injectState)
        {
            return new(Newtonsoft.Json.JsonConvert.SerializeObject(injectState.Data, Newtonsoft.Json.Formatting.Indented));
        }

        protected ForEachNodeViewModel BuildForEachNode(ForEachStateDefinition forEachState)
        {
            return new();
        }

        protected SleepNodeViewModel BuildSleepNode(SleepStateDefinition sleepState)
        {
            return new(sleepState.Duration);
        }

        protected DataCaseNodeViewModel BuildDataConditionNode(string caseDefinitionName)
        {
            return new(caseDefinitionName);
        }

        protected NodeViewModel BuildEndNode()
        {
            return new EndNodeViewModel();
        }


        /// <summary>
        /// Builds an edge between two nodes
        /// </summary>
        /// <param name="graph">The graph instance hosting the edge</param>
        /// <param name="source">The edge's source node</param>
        /// <param name="target">The edge's target node</param>
        /// <returns></returns>
        protected async Task BuildEdgeBetween(GraphViewModel graph, NodeViewModel source, NodeViewModel target)
        {
            await graph.AddElementAsync(new EdgeViewModel(source.Id, target.Id));
        }

        /// <summary>
        /// Builds an edge between two nodes without the end arrow
        /// </summary>
        /// <param name="graph">The graph instance hosting the edge</param>
        /// <param name="source">The edge's source node</param>
        /// <param name="target">The edge's target node</param>
        /// <returns></returns>
        protected async Task BuildJunctionBetween(GraphViewModel graph, NodeViewModel source, NodeViewModel target)
        {
            await graph.AddElementAsync(new EdgeViewModel(source.Id, target.Id) { EndMarkerId = null });
        }


        /// <inheritdoc/>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

}
