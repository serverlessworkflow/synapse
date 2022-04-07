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

        private NodeViewModel BuildStartNode()
        {
            return new StartNodeViewModel();
        }

        private async Task BuildStateNodes(WorkflowDefinition definition, GraphViewModel graph, StateDefinition state, NodeViewModel endNode, NodeViewModel previousNode)
        {
            var stateNodeGroup = new StateNodeViewModel(state);
            //List<NodeViewModel> childNodes = new();
            NodeViewModel? firstNode, lastNode = null;
            switch (state)
            {
                case CallbackStateDefinition callbackState:
                    // todo: refactor?
                    /*
                    childNodes = await this.BuildActionNodes(graph, ((CallbackStateDefinition)state).Action!);
                    firstNode = childNodes.Last();
                    lastNode = this.BuildConsumeEventNode(((CallbackStateDefinition)state).Event!);
                    childNodes.Add(lastNode);
                    await this.BuildEdgeBetween(graph, firstNode, lastNode);
                    */
                    break;
                case EventStateDefinition eventState:
                    firstNode = eventState.Exclusive ? this.BuildGatewayNode(ParallelCompletionType.Xor) : this.BuildJunctionNode();
                    lastNode = this.BuildJunctionNode();
                    await stateNodeGroup.AddChildAsync(firstNode);
                    await this.BuildEdgeBetween(graph, previousNode, firstNode);
                    var andNode = this.BuildGatewayNode(ParallelCompletionType.And);
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
                case ForEachStateDefinition foreachState:

                    break;
                case InjectStateDefinition injectState:

                    break;
                case OperationStateDefinition operationState:
                    switch (operationState.ActionMode)
                    {
                        case ActionExecutionMode.Parallel:
                            firstNode = this.BuildGatewayNode(ParallelCompletionType.And);
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
                                await this.BuildEdgeBetween(graph, actionNodes.Last(), lastNode);
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
                case ParallelStateDefinition parallelState:

                    break;
                case SleepStateDefinition sleepState:

                    break;
                case SwitchStateDefinition switchState:
                    /*firstNode = this.BuildGatewayNode(ParallelCompletionType.Xor);
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
                    }*/
                    break;
                default:
                    throw new Exception($"The specified state type '{state.Type}' is not supported");
            }
            /*foreach(var n in childNodes)
            {
                await stateNodeGroup.AddChildAsync(n);
            }*/
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
            /*lastNode = childNodes.Last();
            if (previousNode != null)
                await this.BuildEdgeBetween(graph, previousNode, stateNodeGroup);
            if (state.IsEnd
                || state.End != null)
            {
                await this.BuildEdgeBetween(graph, stateNodeGroup, endNode);
                return;
            }*/
            await graph.AddElementAsync(stateNodeGroup);
            if (lastNode == null)
            {
                throw new Exception("Every switch case should provide a last node.");
            }
            if (state.IsEnd
                || state.End != null)
            {
                await this.BuildEdgeBetween(graph, lastNode, endNode);
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

        private async Task<List<NodeViewModel>> BuildActionNodes(GraphViewModel graph, ActionDefinition action)
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

        private FunctionRefNodeViewModel BuildFunctionNode(ActionDefinition action, FunctionReference function)
        {
            return new(action, function);
        }

        private SubflowRefNodeViewModel BuildSubflowNode(SubflowReference subflowRef)
        {
            return new(subflowRef);
        }

        private EventNodeViewModel BuildProduceEventNode(string refName)
        {
            return new(EventKind.Produced, refName);
        }

        private EventNodeViewModel BuildConsumeEventNode(string refName)
        {
            return new(EventKind.Consumed, refName);
        }

        private GatewayNodeViewModel BuildGatewayNode( ParallelCompletionType completionType)
        {
            return new(completionType);
        }

        private JunctionNodeViewModel BuildJunctionNode()
        {
            return new();
        }

        private DataCaseNodeViewModel BuildDataConditionNode(string caseDefinitionName)
        {
            return new(caseDefinitionName);
        }

        private NodeViewModel BuildEndNode()
        {
            return new EndNodeViewModel();
        }

        private async Task BuildEdgeBetween(GraphViewModel graph, NodeViewModel node1, NodeViewModel node2)
        {
            await graph.AddElementAsync(new EdgeViewModel(node1.Id, node2.Id));
        }


        /// <inheritdoc/>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

}
