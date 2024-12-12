// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Neuroglia.Blazor.Dagre;
using Neuroglia.Blazor.Dagre.Models;
using ServerlessWorkflow.Sdk;
using ServerlessWorkflow.Sdk.Models;
using ServerlessWorkflow.Sdk.Models.Calls;
using ServerlessWorkflow.Sdk.Models.Tasks;
using System.Diagnostics;

namespace Synapse.Dashboard.Services;

/// <summary>
/// Represents the default implementation of the <see cref="IWorkflowGraphBuilder"/> interface
/// </summary>
/// <param name="logger">The service used to perform logging</param>
/// <param name="yamlSerializer">The service to serialize and deserialize YAML</param>
/// <param name="jsonSerializer">The service to serialize and deserialize YAML</param>
public class WorkflowGraphBuilder(ILogger<WorkflowGraphBuilder> logger, IYamlSerializer yamlSerializer, IJsonSerializer jsonSerializer)
    : IWorkflowGraphBuilder
{

    const string _clusterEntrySuffix = "-cluster-entry-port";
    const string _clusterExitSuffix = "-cluster-exit-port";
    const string _trySuffix = "-try";
    const string _catchSuffix = "-catch";
    const double characterSize = 8d;

    /// <summary>
    /// Gets the default radius for start and end nodes
    /// </summary>
    public const int StartEndNodeRadius = 50;

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger<WorkflowGraphBuilder> Logger { get; } = logger;

    /// <summary>
    /// Gets the service used to serialize and deserialize YAML
    /// </summary>
    protected IYamlSerializer YamlSerializer { get; } = yamlSerializer;

    /// <summary>
    /// Gets the service used to serialize and deserialize YAML
    /// </summary>
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;

    /// <inheritdoc/>
    public IGraphViewModel Build(WorkflowDefinition workflow)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        this.Logger.LogTrace("Starting WorkflowGraphBuilder.Build");
        Stopwatch sw = Stopwatch.StartNew();
        var graph = new GraphViewModel();
        var startNode = this.BuildStartNode();
        var endNode = this.BuildEndNode();
        graph.AddNode(startNode);
        graph.AddNode(endNode);
        this.BuildTransitions(startNode, new(workflow, graph, workflow.Do, null, null, null, null, "/do", null, startNode, endNode));
        sw.Stop();
        this.Logger.LogTrace("WorkflowGraphBuilder.Build took {elapsedTime} ms", sw.ElapsedMilliseconds);
        return graph;
    }

    /// <summary>
    /// Gets the anchor used to attach the provided node.
    /// </summary>
    /// <param name="node">The node to get the anchor of</param>
    /// <param name="portType">The type of port the anchor should be</param>
    /// <returns>If the node is a cluster, the corresponding port, the node itself otherwise</returns>
    protected virtual INodeViewModel GetNodeAnchor(INodeViewModel node, NodePortType portType)
    {
        if (node is IClusterViewModel cluster)
        {
            return portType == NodePortType.Entry ? cluster.Children.First().Value : cluster.Children.Skip(1).First().Value;
        }
        return node;
    }

    /// <summary>
    /// Gets the identify of the next task for the provided task/transition
    /// </summary>
    /// <param name="tasksList">The list of tasks to fetch the next task in</param>
    /// <param name="currentTask">The current task</param>
    /// <param name="transition">A specific transition, if any (use for switch cases)</param>
    /// <returns>The next task identity</returns>
    protected virtual TaskIdentity GetNextTask(Map<string, TaskDefinition> tasksList, string? currentTask, string? transition = null)
    {
        ArgumentNullException.ThrowIfNull(tasksList);
        var taskDefinition = tasksList.FirstOrDefault(taskEntry => taskEntry.Key == currentTask)?.Value;
        transition = !string.IsNullOrWhiteSpace(transition) ? transition : taskDefinition?.Then;
        if (transition == FlowDirective.End || transition == FlowDirective.Exit)
        {
            return new TaskIdentity(transition, -1, null);
        }
        int index;
        if (!string.IsNullOrWhiteSpace(transition) && transition != FlowDirective.Continue)
        {
            index = tasksList.Keys.ToList().IndexOf(transition);
        }
        else if (!string.IsNullOrWhiteSpace(currentTask))
        {
            index = tasksList.Keys.ToList().IndexOf(currentTask) + 1;
            if (index >= tasksList.Count)
            {
                return new TaskIdentity(FlowDirective.Exit, -1, null);
            }
        }
        else
        {
            index = 0;
        }
        var taskEntry = tasksList.ElementAt(index);
        return new TaskIdentity(taskEntry.Key, index, taskEntry.Value);
    }

    /// <summary>
    /// Builds all possible transitions from the specified node
    /// </summary>
    /// <param name="node">The node to transition from</param>
    /// <param name="context">The rendering context of the provided node</param>
    protected virtual void BuildTransitions(INodeViewModel node, TaskNodeRenderingContext context)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(context);
        this.Logger.LogTrace("Starting WorkflowGraphBuilder.BuildTransitions from '{nodeId}'", node.Id);
        List<TaskIdentity> transitions = [];
        TaskIdentity nextTask = this.GetNextTask(context.TasksList, context.TaskName);
        transitions.Add(nextTask);
        this.Logger.LogTrace("[WorkflowGraphBuilder.BuildTransitions][{nodeId}] found transition to '{nextTaskName}'", node.Id, nextTask?.Name);
        while (!string.IsNullOrWhiteSpace(nextTask?.Definition?.If))
        {
            this.Logger.LogTrace("[WorkflowGraphBuilder.BuildTransitions][{nodeId}] if clause found, looking up next task.", node.Id);
            nextTask = this.GetNextTask(context.TasksList, nextTask.Name);
            transitions.Add(nextTask);
            this.Logger.LogTrace("[WorkflowGraphBuilder.BuildTransitions][{nodeId}] found transition to '{nextTaskName}'", node.Id, nextTask?.Name);
        }
        foreach (var transition in transitions.Distinct(new TaskIdentityComparer()))
        {
            if (transition.Index != -1)
            {
                this.Logger.LogTrace("[WorkflowGraphBuilder.BuildTransitions][{nodeId}] Building node '{transitionName}'", node.Id, transition.Name);
                var transitionNode = this.BuildTaskNode(new(context.Workflow, context.Graph, context.TasksList, transition.Index, transition.Name, transition.Definition, context.TaskGroup, context.ParentReference, context.ParentContext, context.EntryNode, context.ExitNode));
                this.Logger.LogTrace("[WorkflowGraphBuilder.BuildTransitions][{nodeId}] Building edge to node '{transitionName}'", node.Id, transition.Name);
                this.BuildEdge(context.Graph, this.GetNodeAnchor(node, NodePortType.Exit), GetNodeAnchor(transitionNode, NodePortType.Entry));
            }
            else if (transition.Name == FlowDirective.Exit)
            {
                this.Logger.LogTrace("[WorkflowGraphBuilder.BuildTransitions][{nodeId}] Exit transition, building edge to node '{contextExitNode}'", node.Id, context.ExitNode);
                this.BuildEdge(context.Graph, this.GetNodeAnchor(node, NodePortType.Exit), context.ExitNode);
            }
            else if (transition.Name == FlowDirective.End)
            {
                this.Logger.LogTrace("[WorkflowGraphBuilder.BuildTransitions][{nodeId}] End transition, building edge to node '{contextExitNode}'", node.Id, context.ExitNode);
                this.BuildEdge(context.Graph, this.GetNodeAnchor(node, NodePortType.Exit), context.Graph.AllNodes.Skip(1).First().Value);
            }
            else
            {
                throw new IndexOutOfRangeException("Invalid transition");
            }
        }
        this.Logger.LogTrace("Exiting WorkflowGraphBuilder.BuildTransitions from '{nodeId}'", node.Id);
    }

    /// <summary>
    /// Builds a new start <see cref="NodeViewModel"/>
    /// </summary>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual NodeViewModel BuildStartNode() => new StartNodeViewModel();

    /// <summary>
    /// Builds a new <see cref="WorkflowClusterViewModel"/> for the specified task
    /// </summary>
    /// <param name="context">The rendering context for the task node</param>
    /// <returns>A new <see cref="WorkflowClusterViewModel"/></returns>
    protected INodeViewModel BuildTaskNode(TaskNodeRenderingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        this.Logger.LogTrace("Starting WorkflowGraphBuilder.BuildTaskNode for '{contextTaskName}'", context.TaskName);
        if (context.Graph.AllNodes.ContainsKey(context.TaskReference))
        {
            this.Logger.LogTrace("Exiting WorkflowGraphBuilder.BuildTaskNode for '{contextTaskName}', found existing node.", context.TaskName);
            return context.Graph.AllNodes[context.TaskReference];
        }
        if (context.Graph.AllClusters.ContainsKey(context.TaskReference))
        {
            this.Logger.LogTrace("Exiting WorkflowGraphBuilder.BuildTaskNode for '{contextTaskName}', found existing cluster.", context.TaskName);
            return context.Graph.AllClusters[context.TaskReference];
        }
        return context.TaskDefinition switch
        {
            CallTaskDefinition => this.BuildCallTaskNode(context.OfType<CallTaskDefinition>()),
            DoTaskDefinition => this.BuildDoTaskNode(context.OfType<DoTaskDefinition>()),
            EmitTaskDefinition => this.BuildEmitTaskNode(context.OfType<EmitTaskDefinition>()),
            ExtensionTaskDefinition => this.BuildExtensionTaskNode(context.OfType<ExtensionTaskDefinition>()),
            ForTaskDefinition => this.BuildForTaskNode(context.OfType<ForTaskDefinition>()),
            ForkTaskDefinition => this.BuildForkTaskNode(context.OfType<ForkTaskDefinition>()),
            ListenTaskDefinition => this.BuildListenTaskNode(context.OfType<ListenTaskDefinition>()),
            RaiseTaskDefinition => this.BuildRaiseTaskNode(context.OfType<RaiseTaskDefinition>()),
            RunTaskDefinition => this.BuildRunTaskNode(context.OfType<RunTaskDefinition>()),
            SetTaskDefinition => this.BuildSetTaskNode(context.OfType<SetTaskDefinition>()),
            SwitchTaskDefinition => this.BuildSwitchTaskNode(context.OfType<SwitchTaskDefinition>()),
            TryTaskDefinition => this.BuildTryTaskNode(context.OfType<TryTaskDefinition>()),
            WaitTaskDefinition => this.BuildWaitTaskNode(context.OfType<WaitTaskDefinition>()),
            _ => throw new NotSupportedException($"The specified task type '{context.TaskDefinition?.GetType()}' is not supported")
        } ?? throw new Exception($"Unable to define a last node for task '{context.TaskName}'");
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified call task
    /// </summary>
    /// <param name="context">The rendering context for the call task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual NodeViewModel BuildCallTaskNode(TaskNodeRenderingContext<CallTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var content = string.Empty;
        string callType;
        switch (context.TaskDefinition.Call.ToLower())
        {
            case "asyncapi":
                {
                    var definition = (AsyncApiCallDefinition)this.JsonSerializer.Convert(context.TaskDefinition.With, typeof(AsyncApiCallDefinition))!;
                    callType = context.TaskDefinition.Call.ToLower();
                    content = definition.OperationRef;
                    break;
                }
            case "grpc":
                {
                    var definition = (GrpcCallDefinition)this.JsonSerializer.Convert(context.TaskDefinition.With, typeof(GrpcCallDefinition))!;
                    callType = context.TaskDefinition.Call.ToLower();
                    content = definition.Service.Name;
                    break;
                }
            case "http":
                {
                    var definition = (HttpCallDefinition)this.JsonSerializer.Convert(context.TaskDefinition.With, typeof(HttpCallDefinition))!;
                    callType = context.TaskDefinition.Call.ToLower();
                    content = definition.Endpoint.Uri.ToString();
                    break;
                }
            case "openapi":
                {
                    var definition = (OpenApiCallDefinition)this.JsonSerializer.Convert(context.TaskDefinition.With, typeof(OpenApiCallDefinition))!;
                    callType = context.TaskDefinition.Call.ToLower();
                    content = definition.OperationId;
                    break;
                }
            default:
                callType = context.TaskDefinition.Call.ToLower();
                break;
        }
        var node = new CallTaskNodeViewModel(context.TaskReference, context.TaskName!, content, callType);
        if (context.TaskGroup == null) context.Graph.AddNode(node);
        else context.TaskGroup.AddChild(node);
        this.BuildTransitions(node, context);
        return node;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified do task
    /// </summary>
    /// <param name="context">The rendering context for the do task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual NodeViewModel BuildDoTaskNode(TaskNodeRenderingContext<DoTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var taskCount = context.TaskDefinition.Do.Count;
        var cluster = new DoTaskNodeViewModel(context.TaskReference, context.TaskName!, $"{taskCount} task{(taskCount > 1 ? "s" : "")}");
        var entryPort = new PortNodeViewModel(context.TaskReference + _clusterEntrySuffix);
        var exitPort = new PortNodeViewModel(context.TaskReference + _clusterExitSuffix);
        cluster.AddChild(entryPort);
        cluster.AddChild(exitPort);
        if (context.TaskGroup == null) context.Graph.AddCluster(cluster);
        else context.TaskGroup.AddChild(cluster);
        var innerContext = new TaskNodeRenderingContext(context.Workflow, context.Graph, context.TaskDefinition.Do, null, null, null, cluster, context.TaskReference + "/do", context, entryPort, exitPort);
        this.BuildTransitions(entryPort, innerContext);
        this.BuildTransitions(cluster, context);
        return cluster;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified emit task
    /// </summary>
    /// <param name="context">The rendering context for the emit task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual NodeViewModel BuildEmitTaskNode(TaskNodeRenderingContext<EmitTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var node = new EmitTaskNodeViewModel(context.TaskReference, context.TaskName!, this.YamlSerializer.SerializeToText(context.TaskDefinition.Emit.Event.With));
        if (context.TaskGroup == null) context.Graph.AddNode(node);
        else context.TaskGroup.AddChild(node);
        this.BuildTransitions(node, context);
        return node;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified extension task
    /// </summary>
    /// <param name="context">The rendering context for the extension task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual NodeViewModel BuildExtensionTaskNode(TaskNodeRenderingContext<ExtensionTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var node = new ExtensionTaskNodeViewModel(context.TaskReference, context.TaskName!);
        if (context.TaskGroup == null) context.Graph.AddNode(node);
        else context.TaskGroup.AddChild(node);
        this.BuildTransitions(node, context);
        return node;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified for task
    /// </summary>
    /// <param name="context">The rendering context for the for task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual NodeViewModel BuildForTaskNode(TaskNodeRenderingContext<ForTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var cluster = new ForTaskNodeViewModel(context.TaskReference, context.TaskName!, this.YamlSerializer.SerializeToText(context.TaskDefinition.For));
        var entryPort = new PortNodeViewModel(context.TaskReference + _clusterEntrySuffix);
        var exitPort = new PortNodeViewModel(context.TaskReference + _clusterExitSuffix);
        cluster.AddChild(entryPort);
        cluster.AddChild(exitPort);
        if (context.TaskGroup == null) context.Graph.AddCluster(cluster);
        else context.TaskGroup.AddChild(cluster);
        var innerContext = new TaskNodeRenderingContext(context.Workflow, context.Graph, context.TaskDefinition.Do, null, null, null, cluster, context.TaskReference + "/do", context, entryPort, exitPort);
        this.BuildTransitions(entryPort, innerContext);
        this.BuildTransitions(cluster, context);
        return cluster;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified fork task
    /// </summary>
    /// <param name="context">The rendering context for the fork task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual NodeViewModel BuildForkTaskNode(TaskNodeRenderingContext<ForkTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var cluster = new ForkTaskNodeViewModel(context.TaskReference, context.TaskName!, this.YamlSerializer.SerializeToText(context.TaskDefinition.Fork));
        var entryPort = new PortNodeViewModel(context.TaskReference + _clusterEntrySuffix);
        var exitPort = new PortNodeViewModel(context.TaskReference + _clusterExitSuffix);
        cluster.AddChild(entryPort);
        cluster.AddChild(exitPort);
        if (context.TaskGroup == null) context.Graph.AddCluster(cluster);
        else context.TaskGroup.AddChild(cluster);
        for (int i = 0, c = context.TaskDefinition.Fork.Branches.Count; i < c; i++)
        {
            var branch = context.TaskDefinition.Fork.Branches.ElementAt(i);
            var branchNode = this.BuildTaskNode(new(context.Workflow, context.Graph, [], i, branch.Key, branch.Value, cluster, context.TaskReference + "/fork/branches", context, entryPort, exitPort));
            if (branchNode is WorkflowClusterViewModel branchCluster)
            {
                this.BuildEdge(context.Graph, entryPort, branchCluster.AllNodes.Values.First());
                this.BuildEdge(context.Graph, branchCluster.AllNodes.Values.Last(), exitPort);
            }
            else
            {
                this.BuildEdge(context.Graph, entryPort, branchNode);
                this.BuildEdge(context.Graph, branchNode, exitPort);
            }
        }
        this.BuildTransitions(cluster, context);
        return cluster;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified listen task
    /// </summary>
    /// <param name="context">The rendering context for the listen task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual NodeViewModel BuildListenTaskNode(TaskNodeRenderingContext<ListenTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var node = new ListenTaskNodeViewModel(context.TaskReference, context.TaskName!, this.YamlSerializer.SerializeToText(context.TaskDefinition.Listen));
        if (context.TaskGroup == null) context.Graph.AddNode(node);
        else context.TaskGroup.AddChild(node);
        this.BuildTransitions(node, context);
        return node;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified raise task
    /// </summary>
    /// <param name="context">The rendering context for the raise task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual NodeViewModel BuildRaiseTaskNode(TaskNodeRenderingContext<RaiseTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var node = new RaiseTaskNodeViewModel(context.TaskReference, context.TaskName!, this.YamlSerializer.SerializeToText(context.TaskDefinition.Raise.Error));
        if (context.TaskGroup == null) context.Graph.AddNode(node);
        else context.TaskGroup.AddChild(node);
        this.BuildTransitions(node, context);
        return node;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified run task
    /// </summary>
    /// <param name="context">The rendering context for the run task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual NodeViewModel BuildRunTaskNode(TaskNodeRenderingContext<RunTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        string content = string.Empty;
        string runType;
        switch (context.TaskDefinition.Run.ProcessType)
        {
            case ProcessType.Container:
                {
                    runType = ProcessType.Container;
                    content = context.TaskDefinition.Run.Container!.Image;
                    break;
                }
            case ProcessType.Shell:
                {
                    runType = ProcessType.Shell;
                    content = context.TaskDefinition.Run.Shell!.Command;
                    break;
                }
            case ProcessType.Script:
                {
                    runType = ProcessType.Script;
                    content = context.TaskDefinition.Run.Script!.Code ?? string.Empty;
                    break;
                }
            case ProcessType.Workflow:
                {
                    runType = ProcessType.Workflow;
                    content = context.TaskDefinition.Run.Workflow!.Name;
                    break;
                }
            default:
                runType = string.Empty;
                break;
        }
        var node = new RunTaskNodeViewModel(context.TaskReference, context.TaskName!, content, runType);
        if (context.TaskGroup == null) context.Graph.AddNode(node);
        else context.TaskGroup.AddChild(node);
        this.BuildTransitions(node, context);
        return node;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified set task
    /// </summary>
    /// <param name="context">The rendering context for the set task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual NodeViewModel BuildSetTaskNode(TaskNodeRenderingContext<SetTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var node = new SetTaskNodeViewModel(context.TaskReference, context.TaskName!, this.YamlSerializer.SerializeToText(context.TaskDefinition.Set));
        if (context.TaskGroup == null) context.Graph.AddNode(node);
        else context.TaskGroup.AddChild(node);
        this.BuildTransitions(node, context);
        return node;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified switch task
    /// </summary>
    /// <param name="context">The rendering context for the switch task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual NodeViewModel BuildSwitchTaskNode(TaskNodeRenderingContext<SwitchTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var node = new SwitchTaskNodeViewModel(context.TaskReference, context.TaskName!, this.YamlSerializer.SerializeToText(context.TaskDefinition.Switch));
        if (context.TaskGroup == null) context.Graph.AddNode(node);
        else context.TaskGroup.AddChild(node);
        foreach (var switchCase in context.TaskDefinition.Switch)
        {
            var switchCaseTask = this.GetNextTask(context.TasksList, context.TaskName, switchCase.Value.Then)!;
            var switchCaseNode = this.BuildTaskNode(new(context.Workflow, context.Graph, context.TasksList, switchCaseTask.Index, switchCaseTask.Name, switchCaseTask.Definition, context.TaskGroup, context.ParentReference, context.ParentContext, context.EntryNode, context.ExitNode));
            this.BuildEdge(context.Graph, this.GetNodeAnchor(node, NodePortType.Exit), GetNodeAnchor(switchCaseNode, NodePortType.Entry));
        }
        if (!context.TaskDefinition.Switch.Any(switchCase => string.IsNullOrEmpty(switchCase.Value.When)))
        {
            this.BuildTransitions(node, context);
        }
        return node;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified try task
    /// </summary>
    /// <param name="context">The rendering context for the try task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual NodeViewModel BuildTryTaskNode(TaskNodeRenderingContext<TryTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var taskCount = context.TaskDefinition.Try.Count;
        var containerCluster = new TryTaskNodeViewModel(context.TaskReference, context.TaskName!, $"{taskCount} task{(taskCount > 1 ? "s" : "")}");
        var containerEntryPort = new PortNodeViewModel(context.TaskReference + _clusterEntrySuffix);
        var containerExitPort = new PortNodeViewModel(context.TaskReference + _clusterExitSuffix);
        containerCluster.AddChild(containerEntryPort);
        containerCluster.AddChild(containerExitPort);
        if (context.TaskGroup == null) context.Graph.AddCluster(containerCluster);
        else context.TaskGroup.AddChild(containerCluster);

        var tryCluster = new TryNodeViewModel(context.TaskReference + _trySuffix, context.TaskName!, string.Empty);
        var tryEntryPort = new PortNodeViewModel(context.TaskReference + _trySuffix + _clusterEntrySuffix);
        var tryExitPort = new PortNodeViewModel(context.TaskReference + _trySuffix + _clusterExitSuffix);
        tryCluster.AddChild(tryEntryPort);
        tryCluster.AddChild(tryExitPort);
        containerCluster.AddChild(tryCluster);
        this.BuildEdge(context.Graph, containerEntryPort, tryEntryPort);
        var innerContext = new TaskNodeRenderingContext(context.Workflow, context.Graph, context.TaskDefinition.Try, null, null, null, tryCluster, context.TaskReference + "/try", context, tryEntryPort, tryExitPort);
        this.BuildTransitions(tryEntryPort, innerContext);

        var catchContent = this.YamlSerializer.SerializeToText(context.TaskDefinition.Catch);
        if (context.TaskDefinition.Catch.Do == null || context.TaskDefinition.Catch.Do.Count == 0)
        {
            var catchNode = new CatchNodeViewModel(context.TaskReference + _catchSuffix, context.TaskName!, catchContent);
            containerCluster.AddChild(catchNode);
            this.BuildEdge(context.Graph, tryExitPort, catchNode);
            this.BuildEdge(context.Graph, catchNode, containerExitPort);
        }
        else
        {
            var catchCluster = new CatchDoNodeViewModel(context.TaskReference + _catchSuffix, context.TaskName!, catchContent);
            var catchEntryPort = new PortNodeViewModel(context.TaskReference + _catchSuffix + _clusterEntrySuffix);
            var catchExitPort = new PortNodeViewModel(context.TaskReference + _catchSuffix + _clusterExitSuffix);
            catchCluster.AddChild(catchEntryPort);
            catchCluster.AddChild(catchExitPort);
            containerCluster.AddChild(catchCluster);
            this.BuildEdge(context.Graph, tryExitPort, catchEntryPort);
            var catchContext = new TaskNodeRenderingContext(context.Workflow, context.Graph, context.TaskDefinition.Catch.Do, null, null, null, catchCluster, context.TaskReference + "/catch/do", context, catchEntryPort, catchExitPort);
            this.BuildTransitions(catchEntryPort, catchContext);
            this.BuildEdge(context.Graph, catchExitPort, containerExitPort);
        }
        this.BuildTransitions(containerCluster, context);
        return containerCluster;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified wait task
    /// </summary>
    /// <param name="context">The rendering context for the wait task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual NodeViewModel BuildWaitTaskNode(TaskNodeRenderingContext<WaitTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var node = new WaitTaskNodeViewModel(context.TaskReference, context.TaskName!, context.TaskDefinition.Wait.ToTimeSpan().ToString("hh\\:mm\\:ss\\.fff"));
        if (context.TaskGroup == null) context.Graph.AddNode(node);
        else context.TaskGroup.AddChild(node);
        this.BuildTransitions(node, context);
        return node;
    }

    /// <summary>
    /// Builds a new end <see cref="NodeViewModel"/>
    /// </summary>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual NodeViewModel BuildEndNode() => new EndNodeViewModel();

    /// <summary>
    /// Builds an edge between two nodes
    /// </summary>
    /// <param name="graph">The current <see cref="GraphViewModel"/></param>
    /// <param name="source">The node to draw the edge from</param>
    /// <param name="target">The node to draw the edge to</param>
    /// <param name="label">The edge label, if any</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual IEdgeViewModel BuildEdge(IGraphViewModel graph, INodeViewModel source, INodeViewModel target, string? label = null)
    {
        var edge = graph.Edges.Select(keyValuePair => keyValuePair.Value).FirstOrDefault(edge => edge.SourceId == source.Id && edge.TargetId == target.Id);
        if (edge != null)
        {
            if (!string.IsNullOrEmpty(label))
            {
                edge.Label = edge.Label + " / " + label;
                edge.Width = edge.Label.Length * characterSize;
            }
            return edge;
        }
        edge = new EdgeViewModel(source.Id, target.Id, label);
        if (!string.IsNullOrEmpty(edge.Label))
        {
            edge.LabelPosition = EdgeLabelPosition.Center;
            edge.Width = edge.Label.Length * characterSize;
        }
        if (target.Id.EndsWith(_clusterEntrySuffix) || target.Id.EndsWith(_clusterExitSuffix))
        {
            edge.EndMarkerId = null;
        }
        return graph.AddEdge(edge);
    }

    /// <summary>
    /// Represents the context for rendering a task node within a workflow.
    /// </summary>
    /// <param name="workflow">The workflow definition.</param>
    /// <param name="graph">The graph view model.</param>
    /// <param name="tasksList">The list of tasks in the rendering context.</param>
    /// <param name="taskIndex">The index of the task.</param>
    /// <param name="taskName">The name of the task.</param>
    /// <param name="taskDefinition">The definition of the task.</param>
    /// <param name="taskGroup">The optional task group.</param>
    /// <param name="parentReference">The reference to the parent task node.</param>
    /// <param name="parentContext">The parent rendering context of the task node.</param>
    /// <param name="entryNode">The entry node view model of the context.</param>
    /// <param name="exitNode">The exit node view model of the context.</param>
    protected class TaskNodeRenderingContext(WorkflowDefinition workflow, GraphViewModel graph, Map<string, TaskDefinition> tasksList, int? taskIndex, string? taskName, TaskDefinition? taskDefinition, WorkflowClusterViewModel? taskGroup, string parentReference, TaskNodeRenderingContext? parentContext, NodeViewModel entryNode, NodeViewModel exitNode)
    {

        /// <summary>
        /// Gets the workflow definition.
        /// </summary>
        public virtual WorkflowDefinition Workflow { get; } = workflow;

        /// <summary>
        /// Gets the graph view model.
        /// </summary>
        public virtual GraphViewModel Graph { get; } = graph;

        /// <summary>
        /// Gets the list of tasks in the rendering context.
        /// </summary>
        public virtual Map<string, TaskDefinition> TasksList { get; } = tasksList;

        /// <summary>
        /// Gets the index of the task.
        /// </summary>
        public virtual int? TaskIndex { get; } = taskIndex;

        /// <summary>
        /// Gets the name of the task.
        /// </summary>
        public virtual string? TaskName { get; } = taskName;

        /// <summary>
        /// Gets the definition of the task.
        /// </summary>
        public virtual TaskDefinition? TaskDefinition { get; } = taskDefinition;

        /// <summary>
        /// Gets the optional task group.
        /// </summary>
        public virtual WorkflowClusterViewModel? TaskGroup { get; } = taskGroup;

        /// <summary>
        /// Gets the reference of the task node in the context of the parent task node.
        /// </summary>
        public virtual string TaskReference => $"{this.ParentReference}/{this.TaskIndex}/{this.TaskName}";

        /// <summary>
        /// Gets the reference to the parent task node.
        /// </summary>
        public virtual string ParentReference { get; } = parentReference;

        /// <summary>
        /// Gets the rendering context of the parent task node.
        /// </summary>
        public virtual TaskNodeRenderingContext? ParentContext { get; } = parentContext;

        /// <summary>
        /// Gets the entry node view model.
        /// </summary>
        public virtual NodeViewModel EntryNode { get; } = entryNode;

        /// <summary>
        /// Gets the exit node view model.
        /// </summary>
        public virtual NodeViewModel ExitNode { get; } = exitNode;


        /// <summary>
        /// Creates a new instance of <see cref="TaskNodeRenderingContext{TDefinition}"/> with the specified task definition type.
        /// </summary>
        /// <typeparam name="TDefinition">The type of the task definition.</typeparam>
        /// <returns>A new instance of <see cref="TaskNodeRenderingContext{TDefinition}"/>.</returns>
        public virtual TaskNodeRenderingContext<TDefinition> OfType<TDefinition>() where TDefinition : TaskDefinition => new(this.Workflow, this.Graph, this.TasksList, this.TaskIndex, this.TaskName, this.TaskDefinition, this.TaskGroup, this.ParentReference, this.ParentContext, this.EntryNode, this.ExitNode);

    }

    /// <summary>
    /// Represents the context for rendering a task node within a workflow, with a specific task definition type.
    /// </summary>
    /// <typeparam name="TDefinition">The type of the task definition.</typeparam>
    /// <param name="workflow">The workflow definition.</param>
    /// <param name="graph">The graph view model.</param>
    /// <param name="tasksList">The list of tasks in the rendering context.</param>
    /// <param name="taskIndex">The index of the task.</param>
    /// <param name="taskName">The name of the task.</param>
    /// <param name="taskDefinition">The definition of the task.</param>
    /// <param name="taskGroup">The optional task group.</param>
    /// <param name="parentReference">The reference to the parent task node.</param>
    /// <param name="parentContext">The parent rendering context of the task node.</param>
    /// <param name="exitNode">The end node view model.</param>
    /// <param name="entryNode">The previous node view model.</param>
    protected class TaskNodeRenderingContext<TDefinition>(WorkflowDefinition workflow, GraphViewModel graph, Map<string, TaskDefinition> tasksList, int? taskIndex, string? taskName, TaskDefinition? taskDefinition, WorkflowClusterViewModel? taskGroup, string parentReference, TaskNodeRenderingContext? parentContext, NodeViewModel entryNode, NodeViewModel exitNode)
        : TaskNodeRenderingContext(workflow, graph, tasksList, taskIndex, taskName, taskDefinition, taskGroup, parentReference, parentContext, entryNode, exitNode)
        where TDefinition : TaskDefinition
    {
        /// <summary>
        /// Gets the task definition of type <typeparamref name="TDefinition"/>.
        /// </summary>
        public new virtual TDefinition TaskDefinition => (TDefinition)base.TaskDefinition!;

    }

    /// <summary>
    /// Represents the identity of a task
    /// </summary>
    /// <param name="Name">The task name</param>
    /// <param name="Index">The task index</param>
    /// <param name="Definition">The task definition</param>
    protected record TaskIdentity(string Name, int Index, TaskDefinition? Definition)
    {
    }

    /// <summary>
    /// Represents a port type
    /// </summary>
    protected enum NodePortType
    {
        /// <summary>
        /// The entry port of a cluster
        /// </summary>
        Entry = 0,
        /// <summary>
        /// The exit port of a cluster
        /// </summary>
        Exit = 1
    }

    /// <summary>
    /// The object used to compare <see cref="TaskIdentity"/>
    /// </summary>
    protected class TaskIdentityComparer : IEqualityComparer<TaskIdentity>
    {
        /// <inheritdoc/>
        public bool Equals(TaskIdentity? identity1, TaskIdentity? identity2)
        {
            if (ReferenceEquals(identity1, identity2))
                return true;

            if (identity1 is null || identity2 is null)
                return false;

            return identity1.Name == identity2.Name &&
                identity1.Index == identity2.Index &&
                identity1.Definition == identity2.Definition;
        }

        /// <inheritdoc/>
        public int GetHashCode(TaskIdentity identity) => identity.Name.GetHashCode();
    }
}
