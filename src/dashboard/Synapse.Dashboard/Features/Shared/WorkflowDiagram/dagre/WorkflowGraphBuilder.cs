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
            var startNode = this.BuildStartNode(graph);
            var endNode = this.BuildEndNode(graph);
            await graph.AddElement(startNode);
            await this.BuildStateNodes(definition, graph, startState, endNode, startNode);
            await graph.AddElement(endNode);
            return await Task.FromResult(graph);
        }

        private NodeViewModel BuildStartNode(GraphViewModel graph)
        {
            return new StartNodeViewModel();
        }

        private async Task BuildStateNodes(WorkflowDefinition definition, GraphViewModel graph, StateDefinition state, NodeViewModel endNode, NodeViewModel previousNode)
        {
            var stateNodeGroup = new StateNodeViewModel(state);
            await graph.AddElement(stateNodeGroup);
            List<NodeViewModel> childNodes = new();
            NodeViewModel node, firstNode, lastNode;
            switch (state)
            {
                case CallbackStateDefinition callbackState:
                    childNodes = await this.BuildActionNodes(graph, state, ((CallbackStateDefinition)state).Action!);
                    firstNode = childNodes.Last();
                    lastNode = this.BuildConsumeEventNode(state, ((CallbackStateDefinition)state).Event!);
                    childNodes.Add(lastNode);
                    await this.BuildEdgeBetween(graph, firstNode, lastNode);
                    break;
                case EventStateDefinition eventState:
                    var completionType = eventState.Exclusive ? ParallelCompletionType.Xor : ParallelCompletionType.And;
                    firstNode = this.BuildGatewayNode(state, completionType);
                    childNodes.Add(firstNode);
                    lastNode = this.BuildGatewayNode(state, completionType);
                    childNodes.Add(lastNode);
                    eventState.Triggers.ForEach(trigger =>
                    {
                        var refName = string.Join(" | ", trigger.Events);
                        var eventNode = this.BuildConsumeEventNode(eventState, refName);
                        //this.BuildLinkBetween(graph, firstNode, eventNode);

                    });
                    break;
                case ForEachStateDefinition foreachState:

                    break;
                case InjectStateDefinition injectState:

                    break;
                case OperationStateDefinition operationState:
                    node = previousNode;
                    switch (operationState.ActionMode)
                    {
                        case ActionExecutionMode.Parallel:
                            var startNode = this.BuildGatewayNode(state, ParallelCompletionType.And);
                            childNodes.Add(startNode);
                            var finalNode = this.BuildGatewayNode(state, ParallelCompletionType.And);
                            childNodes.Add(finalNode);
                            foreach(var action in operationState.Actions)
                            {
                                var actionNodes = await this.BuildActionNodes(graph, state, action);
                                await this.BuildEdgeBetween(graph, startNode, actionNodes.First());
                                await this.BuildEdgeBetween(graph, actionNodes.Last(), finalNode);
                                foreach(var n in actionNodes)
                                {
                                    await stateNodeGroup.AddChild(n);
                                }
                            }
                            break;
                        case ActionExecutionMode.Sequential:
                            var index = 0;
                            foreach (var action in operationState.Actions)
                            {
                                var actionNodes = await this.BuildActionNodes(graph, state, action);
                                if (index != 0)
                                    await this.BuildEdgeBetween(graph, node, actionNodes.First());
                                actionNodes.ForEach(n => childNodes.Add(n));
                                node = actionNodes.First();
                                index++;
                            }
                            break;
                        default:
                            throw new Exception($"The specified action execution mode '{operationState.ActionMode}' is not supported");
                    }
                    break;
                case ParallelStateDefinition parallelState:

                    break;
                case SleepStateDefinition sleepState:

                    break;
                case SwitchStateDefinition switchState:
                    firstNode = this.BuildGatewayNode(state, ParallelCompletionType.Xor);
                    childNodes.Add(firstNode);
                    switch (switchState.SwitchType)
                    {
                        case SwitchStateType.Data:
                            foreach(var condition in switchState.DataConditions)
                            {
                                var caseNode = this.BuildDataConditionNode(state, condition.Name!);
                                childNodes.Add(caseNode);
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
                                        await this.BuildStateNodes(definition, graph, nextState, endNode, caseNode);
                                        break;
                                    default:
                                        throw new Exception($"The specified condition type '${condition.Type}' is not supported");
                                }
                            }
                            node = this.BuildDataConditionNode(state, "default");
                            childNodes.Add(node);
                            if (switchState.DefaultCondition.IsEnd
                                || switchState.DefaultCondition.End != null)
                            {
                                await this.BuildEdgeBetween(graph, node, endNode);
                            }
                            else if (!string.IsNullOrWhiteSpace(switchState.DefaultCondition.TransitionToStateName)
                                || switchState.DefaultCondition.Transition != null)
                            {
                                var nextStateName = switchState.DefaultCondition.Transition == null ? switchState.DefaultCondition.TransitionToStateName : switchState.DefaultCondition.Transition.NextState;
                                var nextState = definition.GetState(nextStateName!);
                                if (nextState == null)
                                    throw new Exception($"Failed to find a state with name '{nextStateName}' in definition '{definition.GetUniqueIdentifier()}");
                            }
                            await this.BuildEdgeBetween(graph, firstNode, node);
                            break;
                        case SwitchStateType.Event:

                            break;
                        default:
                            throw new Exception($"The specified switch state type '{switchState.Type}' is not supported");
                    }
                    break;
                default:
                    throw new Exception($"The specified state type '{state.Type}' is not supported");
            }
            foreach(var n in childNodes)
            {
                await stateNodeGroup.AddChild(n);
            }
            /* ?
            if (stateNodeGroup.Children.Any())
            {
                node = stateNodeGroup.Children.First();
                var port = node.Ports.FirstOrDefault(p => p.Alignment == PortAlignment.Top);
                if (port != null)
                    node.RemovePort(port);
                node = stateNodeGroup.Children.Last();
                port = node.Ports.FirstOrDefault(p => p.Alignment == PortAlignment.Bottom);
                if (port != null)
                    node.RemovePort(port);
            }
            */
            lastNode = childNodes.Last();
            if (previousNode != null)
                await this.BuildEdgeBetween(graph, previousNode, stateNodeGroup);
            if (state.IsEnd
                || state.End != null)
            {
                await this.BuildEdgeBetween(graph, stateNodeGroup, endNode);
                return;
            }
            if (!string.IsNullOrWhiteSpace(state.TransitionToStateName)
                || state.Transition != null)
            {
                var nextStateName = state.Transition == null ? state.TransitionToStateName! : state.Transition!.NextState;
                var nextState = definition.GetState(nextStateName);
                if (nextState == null)
                    throw new Exception($"Failed to find a state with name '{nextStateName}' in definition '{definition.GetUniqueIdentifier()}'");
                await this.BuildStateNodes(definition, graph, nextState, endNode, lastNode);
                return;
            }
        }

        private async Task<List<NodeViewModel>> BuildActionNodes(GraphViewModel graph, StateDefinition state, ActionDefinition action)
        {
            switch (action.Type)
            {
                case ActionType.Function:
                    return new() { this.BuildFunctionNode(state, action, action.Function!) };
                case ActionType.Subflow:
                    return new() { this.BuildSubflowNode(state, action.Subflow!) };
                case ActionType.Trigger:
                    var triggerEventNode = this.BuildProduceEventNode(state, action.Event!.ProduceEvent);
                    var resultEventNode = this.BuildConsumeEventNode(state, action.Event!.ResultEvent);
                    await this.BuildEdgeBetween(graph, triggerEventNode, resultEventNode);
                    return new() { triggerEventNode, resultEventNode };
                default:
                    throw new NotSupportedException($"The specified action type '{action.Type}' is not supported");
            }
        }

        private FunctionRefNodeViewModel BuildFunctionNode(StateDefinition state, ActionDefinition action, FunctionReference function)
        {
            return new(action, function);
        }

        private SubflowRefNodeViewModel BuildSubflowNode(StateDefinition state, SubflowReference subflowRef)
        {
            return new(subflowRef);
        }

        private EventNodeViewModel BuildProduceEventNode(StateDefinition state, string refName)
        {
            return new(EventKind.Produced, refName);
        }

        private EventNodeViewModel BuildConsumeEventNode(StateDefinition state, string refName)
        {
            return new(EventKind.Consumed, refName);
        }

        private GatewayNodeViewModel BuildGatewayNode(StateDefinition state, ParallelCompletionType completionType)
        {
            return new(completionType);
        }

        private DataCaseNodeViewModel BuildDataConditionNode(StateDefinition state, string caseDefinitionName)
        {
            return new(caseDefinitionName);
        }

        private NodeViewModel BuildEndNode(GraphViewModel graph)
        {
            return new EndNodeViewModel();
        }

        private async Task BuildEdgeBetween(GraphViewModel graph, NodeViewModel node1, NodeViewModel node2)
        {
            await graph.AddElement(new EdgeViewModel(node1.Id, node2.Id));
        }


        /// <inheritdoc/>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

}
