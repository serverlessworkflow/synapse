using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models;
using GraphShape.Algorithms.Layout;
using ServerlessWorkflow.Sdk;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Dashboard.Components;
using Synapse.Dashboard.Models;

namespace Synapse.Dashboard.Services
{
    public class WorkflowDiagramBuilder
        : IWorkflowDiagramBuilder
    {

        /// <inheritdoc/>
        public async Task<Diagram> BuildDiagram(WorkflowDefinition definition)
        {
            await Task.CompletedTask; //??? => if anything, should be at the end or should be return Task.FromResult(diagram) or return await Task.Run(() =>  ...)
            /*            
            diagram = new Diagram(new DiagramOptions());
            diagram.RegisterModelComponent<WorkflowStartNodeModel, WorkflowStartNode>();
            diagram.RegisterModelComponent<WorkflowStateNodeModel, WorkflowStateNode>();
            diagram.RegisterModelComponent<WorkflowEndNodeModel, WorkflowEndNode>();

            var startNode = this.BuildStartNode();
            diagram.Nodes.Add(startNode);
            var endNode = this.BuildEndNode();
            diagram.Nodes.Add(endNode);
            var state = this.Definition.GetStartState();
            this.BuildStateNodes(state, endNode, startNode);
            this.ComputeNodePositions();
            diagram.SelectionChanged += this.OnNodeSelectionChanged;
            */
            var diagram = new Diagram(new DiagramOptions());
            diagram.RegisterModelComponent<WorkflowStartNodeModel, WorkflowStartNode>();
            diagram.RegisterModelComponent<WorkflowStateNodeModel, WorkflowStateNode>();
            diagram.RegisterModelComponent<FunctionRefNodeModel, FunctionRefNode>();
            diagram.RegisterModelComponent<WorkflowEndNodeModel, WorkflowEndNode>();
            var startState = definition.GetStartState();
            var startNode = this.BuildStartNode(diagram);            
            var endNode = this.BuildEndNode(diagram);
            diagram.Nodes.Add(startNode);
            this.BuildStateNodes(definition, diagram, startState, endNode, startNode);
            diagram.Nodes.Add(endNode);
            this.ComputeNodePositions(diagram);
            return diagram;
        }

        private NodeModel BuildStartNode(Diagram diagram)
        {
            return new WorkflowStartNodeModel();
        }

        private void BuildStateNodes(WorkflowDefinition definition, Diagram diagram, StateDefinition state, NodeModel endNode, NodeModel previousNode)
        {
            var stateNodeGroup = new GroupModel(Array.Empty<NodeModel>())
            {
                Title = state.Name
            };
            diagram.AddGroup(stateNodeGroup);
            List<NodeModel> childNodes = new();
            NodeModel node, firstNode, lastNode;
            switch (state)
            {
                case CallbackStateDefinition callbackState:
                    childNodes = this.BuildActionNodes(diagram, state, ((CallbackStateDefinition)state).Action!);
                    firstNode = childNodes.Last();
                    lastNode = this.BuildConsumeEventNode(state, ((CallbackStateDefinition)state).Event!);
                    childNodes.Add(lastNode);
                    this.BuildLinkBetween(diagram, firstNode, lastNode);
                    break;
                case EventStateDefinition eventState:

                    break;
                case ForEachStateDefinition foreachState:

                    break;
                case InjectStateDefinition injectState:
                    node = new WorkflowStateNodeModel(state);
                    childNodes.Add(node);
                    this.BuildLinkBetween(diagram, node, previousNode);
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
                            operationState.Actions.ForEach(action =>
                            {
                                var actionNodes = this.BuildActionNodes(diagram, state, action);
                                this.BuildLinkBetween(diagram, startNode, actionNodes.First());
                                this.BuildLinkBetween(diagram, actionNodes.Last(), finalNode);
                                actionNodes.ForEach(n => stateNodeGroup.AddChild(n));
                            });
                            break;
                        case ActionExecutionMode.Sequential:
                            var index = 0;
                            foreach (var action in operationState.Actions)
                            {
                                var actionNodes = this.BuildActionNodes(diagram, state, action);
                                if (index != 0)
                                    this.BuildLinkBetween(diagram, node, actionNodes.First());
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
                            switchState.DataConditions.ForEach(condition =>
                            {
                                var caseNode = this.BuildDataConditionNode(state, condition.Name!);
                                childNodes.Add(caseNode);
                                this.BuildLinkBetween(diagram, firstNode, caseNode);
                                switch (condition.Type)
                                {
                                    case ConditionType.End:
                                        this.BuildLinkBetween(diagram, caseNode, endNode);
                                        break;
                                    case ConditionType.Transition:
                                        var nextStateName = condition.Transition == null ? condition.TransitionToStateName : condition.Transition.NextState;
                                        var nextState = definition.GetState(nextStateName!);
                                        if (nextState == null)
                                            throw new Exception($"Failed to find a state with name '{nextStateName}' in definition '{definition.GetUniqueIdentifier()}");
                                        this.BuildStateNodes(definition, diagram, nextState, endNode, caseNode);
                                        break;
                                    default:
                                        throw new Exception($"The specified condition type '${condition.Type}' is not supported");
                                }
                            });
                            node = this.BuildDataConditionNode(state, "default");
                            childNodes.Add(node);
                            if (switchState.DefaultCondition.IsEnd
                                || switchState.DefaultCondition.End != null)
                            {
                                this.BuildLinkBetween(diagram, node, endNode);
                            }
                            else if (!string.IsNullOrWhiteSpace(switchState.DefaultCondition.TransitionToStateName)
                                || switchState.DefaultCondition.Transition != null)
                            {
                                var nextStateName = switchState.DefaultCondition.Transition == null ? switchState.DefaultCondition.TransitionToStateName : switchState.DefaultCondition.Transition.NextState;
                                var nextState = definition.GetState(nextStateName!);
                                if (nextState == null)
                                    throw new Exception($"Failed to find a state with name '{nextStateName}' in definition '{definition.GetUniqueIdentifier()}");
                            }
                            this.BuildLinkBetween(diagram, firstNode, node);
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
            childNodes.ForEach(n => stateNodeGroup.AddChild(n));
            lastNode = childNodes.Last();
            if (previousNode != null)
                this.BuildLinkBetween(diagram, previousNode, childNodes.First());
            if (state.IsEnd
                || state.End != null)
            {
                this.BuildLinkBetween(diagram, childNodes.Last(), endNode);
                return;
            }
            if (!string.IsNullOrWhiteSpace(state.TransitionToStateName)
                || state.Transition != null)
            {
                var nextStateName = state.Transition == null ? state.TransitionToStateName! : state.Transition!.NextState;
                var nextState = definition.GetState(nextStateName);
                if (nextState == null)
                    throw new Exception($"Failed to find a state with name '{nextStateName}' in definition '{definition.GetUniqueIdentifier()}'");
                this.BuildStateNodes(definition, diagram, nextState, endNode, lastNode);
                return;
            }
        }

        private List<NodeModel> BuildActionNodes(Diagram diagram, StateDefinition state, ActionDefinition action)
        {
            switch (action.Type)
            {
                case ActionType.Function:
                    return new() { this.BuildFunctionNode(state, action.Function!) };
                case ActionType.Subflow:
                    return new() { this.BuildSubflowNode(state, action.Subflow!) };
                case ActionType.Trigger:
                    var triggerEventNode = this.BuildProduceEventNode(state, action.Event!.ProduceEvent);
                    var resultEventNode = this.BuildConsumeEventNode(state, action.Event!.ResultEvent);
                    this.BuildLinkBetween(diagram, triggerEventNode, resultEventNode);
                    return new() { triggerEventNode, resultEventNode };
                default:
                    throw new NotSupportedException($"The specified action type '{action.Type}' is not supported");
            }
        }

        private FunctionRefNodeModel BuildFunctionNode(StateDefinition state, FunctionReference functionRef)
        {
            return new(functionRef);
        }

        private SubflowRefNodeModel BuildSubflowNode(StateDefinition state, SubflowReference subflowRef)
        {
            return new(subflowRef);
        }

        private EventNodeModel BuildProduceEventNode(StateDefinition state, string refName)
        {
            return new(EventKind.Produced, refName);
        }

        private EventNodeModel BuildConsumeEventNode(StateDefinition state, string refName)
        {
            return new(EventKind.Consumed, refName);
        }

        private GatewayNodeModel BuildGatewayNode(StateDefinition state, ParallelCompletionType completionType)
        {
            return new(completionType);
        }

        private DataCaseNodeModel BuildDataConditionNode(StateDefinition state, string caseDefinitionName)
        {
            return new(caseDefinitionName);
        }

        private NodeModel BuildEndNode(Diagram diagram)
        {
            return new WorkflowEndNodeModel();
        }

        private void BuildLinkBetween(Diagram diagram, NodeModel node1, NodeModel node2)
        {
            diagram.Links.Add(new LinkModel(node1.GetPort(PortAlignment.Bottom), node2.GetPort(PortAlignment.Top)) { Locked = true });
        }

        private void ComputeNodePositions(Diagram diagram)
        {
            var graph = new QuikGraph.BidirectionalGraph<NodeModel, QuikGraph.Edge<NodeModel>>();
            var nodes = diagram.Nodes.ToList();
            nodes.AddRange(diagram.Groups.SelectMany(g => g.Children));
            var edges = diagram.Links.OfType<LinkModel>()
                .Select(lm =>
                {
                    var source = nodes.Single(dn => dn.Id == lm.SourceNode.Id);
                    var target = nodes.Single(dn => dn.Id == lm?.TargetNode?.Id);
                    return new QuikGraph.Edge<NodeModel>(source, target);
                })
                .ToList();
            graph.AddVertexRange(nodes);
            graph.AddEdgeRange(edges);
            var positions = nodes.ToDictionary(nm => nm, dn => new GraphShape.Point(dn.Position.X, dn.Position.Y));
            var sizes = nodes.ToDictionary(nm => nm, dn => new GraphShape.Size(dn.Size?.Width ?? 75, dn.Size?.Height ?? 75));
            var context = new LayoutContext<NodeModel, QuikGraph.Edge<NodeModel>, QuikGraph.BidirectionalGraph<NodeModel, QuikGraph.Edge<NodeModel>>>(graph, positions, sizes, LayoutMode.Simple);
            var algorithmFactory = new StandardLayoutAlgorithmFactory<NodeModel, QuikGraph.Edge<NodeModel>, QuikGraph.BidirectionalGraph<NodeModel, QuikGraph.Edge<NodeModel>>>();
            var algorithm = algorithmFactory.CreateAlgorithm("Tree", context, null);
            algorithm.Compute();
            try
            {
                diagram.SuspendRefresh = true;
                foreach (var vertex in algorithm.VerticesPositions)
                {
                    vertex.Key.SetPosition(vertex.Value.X, vertex.Value.Y);
                }
            }
            finally
            {
                diagram.SuspendRefresh = false;
            }
        }


        /// <inheritdoc/>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
