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
/// <param name="yamlSerializer">The service to serialize and deserialize YAML</param>
/// <param name="jsonSerializer">The service to serialize and deserialize YAML</param>
public class WorkflowGraphBuilder(IYamlSerializer yamlSerializer, IJsonSerializer jsonSerializer)
    : IWorkflowGraphBuilder
{

    const string _portSuffix = "-port";
    const string _trySuffix = "-try";
    const string _catchSuffix = "-catch";
    const double characterSize = 8d;

    /// <summary>
    /// Gets the default radius for start and end nodes
    /// </summary>
    public const int StartEndNodeRadius = 50;

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
        Stopwatch sw = Stopwatch.StartNew();
        var isEmpty = workflow.Do.Count < 1;
        var graph = new GraphViewModel();
        //graph.EnableProfiling = true;
        var startNode = this.BuildStartNode(!isEmpty);
        var endNode = this.BuildEndNode();
        graph.AddNode(startNode);
        graph.AddNode(endNode);
        var nextNode = endNode;
        if (!isEmpty)
        {
            nextNode = this.BuildTaskNode(new(workflow, graph, 0, workflow.Do.First().Key, workflow.Do.First().Value, null, "/do", null, endNode, startNode));
            if (nextNode is ClusterViewModel clusterViewModel)
            {
                nextNode = (NodeViewModel)clusterViewModel.AllNodes.Values.First();
            }
        }
        this.BuildEdge(graph, startNode, nextNode);
        sw.Stop();
        Console.WriteLine($"WorkflowGraphBuilder.Build took {sw.ElapsedMilliseconds} ms");
        return graph;
    }

    /// <summary>
    /// Builds a new start <see cref="NodeViewModel"/>
    /// </summary>
    /// <param name="hasSuccessor">A boolean indicating whether or not the node has successor</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual NodeViewModel BuildStartNode(bool hasSuccessor = false) => new StartNodeViewModel(hasSuccessor);

    /// <summary>
    /// Returns the name, index and reference of the next node
    /// </summary>
    /// <param name="context">The rendering context for the task nodes</param>
    /// <param name="ignoreConditionalTasks">If true, skips <see cref="TaskDefinition"/> with an if clause</param>
    /// <param name="transition">A transition, if different from the context task definition's</param>
    /// <returns>The next task <see cref="TaskIdentity"/></returns>
    protected TaskIdentity? GetNextTaskIdentity(TaskNodeRenderingContext context, bool ignoreConditionalTasks, string? transition = null)
    {
        transition = !string.IsNullOrWhiteSpace(transition) ? transition : context.TaskDefinition.Then;
        if (transition == FlowDirective.End) return null;
        if (transition == FlowDirective.Exit)
        {
            if (context.ParentContext == null)
            {
                return null;
            }
            return this.GetNextTaskIdentity(context.ParentContext, ignoreConditionalTasks);
        }
        var nextTaskName = string.IsNullOrWhiteSpace(transition) || transition == FlowDirective.Continue
                ? context.Workflow.GetTaskAfter(new(context.TaskName, context.TaskDefinition), context.ParentReference, ignoreConditionalTasks)?.Key
                : transition;
        if (string.IsNullOrWhiteSpace(nextTaskName))
        {
            if (context.ParentContext == null)
            {
                return null;
            }
            return this.GetNextTaskIdentity(context.ParentContext, ignoreConditionalTasks);
        }
        var nextTaskIndex = context.Workflow.IndexOf(nextTaskName, context.ParentReference);
        var nextTaskReference = $"{context.ParentReference}/{nextTaskIndex}/{nextTaskName}";
        return new(nextTaskName, nextTaskIndex, nextTaskReference, context);
    }

    /// <summary>
    /// Gets a <see cref="NodeViewModel"/> by reference in the provided <see cref="TaskNodeRenderingContext"/>
    /// </summary>
    /// <param name="context">The source <see cref="TaskNodeRenderingContext"/></param>
    /// <param name="reference">The reference to look for</param>
    /// <returns></returns>
    protected NodeViewModel GetNodeByReference(TaskNodeRenderingContext context, string reference)
    {
        if (context.Graph.AllClusters.ContainsKey(reference))
        {
            return (NodeViewModel)context.Graph.AllClusters[reference].AllNodes.First().Value;
        }
        if (context.Graph.AllNodes.ContainsKey(reference))
        {
            return (NodeViewModel)context.Graph.AllNodes[reference];
        }
        throw new IndexOutOfRangeException($"Unabled to find the task with reference '{reference}' in the provided context.");
    }

    /// <summary>
    /// Gets the next <see cref="NodeViewModel"/> in the graph
    /// </summary>
    /// <param name="context">The rendering context for the task nodes</param>
    /// <param name="currentNode">The current task node</param>
    /// <param name="ignoreConditionalTasks">If true, skips <see cref="TaskDefinition"/> with an if clause</param>
    /// <param name="transition">A transition, if different from the context task definition's</param>
    /// <returns>The next task <see cref="NodeViewModel"/></returns>
    /// <exception cref="Exception"></exception>
    protected NodeViewModel GetNextNode(TaskNodeRenderingContext context, NodeViewModel currentNode, bool ignoreConditionalTasks = false, string? transition = null)
    {
        var nextTaskIdentity = this.GetNextTaskIdentity(context, ignoreConditionalTasks, transition);
        if (nextTaskIdentity == null)
        {
            return context.EndNode;
        }
        var nextTask = context.Workflow.GetComponent<TaskDefinition>(nextTaskIdentity.Reference) ?? throw new Exception($"Failed to find the task at '{nextTaskIdentity.Reference}' in workflow '{context.Workflow.Document.Name}.{context.Workflow.Document.Namespace}:{context.Workflow.Document.Version}'");
        if (!context.Graph.AllNodes.ContainsKey(nextTaskIdentity.Reference) && !context.Graph.AllClusters.ContainsKey(nextTaskIdentity.Reference))
        {
            this.BuildTaskNode(new(nextTaskIdentity.Context.Workflow, nextTaskIdentity.Context.Graph, nextTaskIdentity.Index, nextTaskIdentity.Name, nextTask, nextTaskIdentity.Context.TaskGroup, nextTaskIdentity.Context.ParentReference, nextTaskIdentity.Context.ParentContext, nextTaskIdentity.Context.EndNode, currentNode));
        }
        if (string.IsNullOrEmpty(nextTask.If))
        {
            return this.GetNodeByReference(context, nextTaskIdentity.Reference);
        }
        var nextNode = this.GetNodeByReference(context, nextTaskIdentity.Reference);
        this.BuildEdge(context.Graph, currentNode, nextNode);
        return this.GetNextNode(context, currentNode, true, transition);
    }

    /// <summary>
    /// Builds a new <see cref="WorkflowClusterViewModel"/> for the specified task
    /// </summary>
    /// <param name="context">The rendering context for the task node</param>
    /// <returns>A new <see cref="WorkflowClusterViewModel"/></returns>
    protected NodeViewModel BuildTaskNode(TaskNodeRenderingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
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
            _ => throw new NotSupportedException($"The specified task type '{context.TaskDefinition.GetType()}' is not supported")
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
                    // todo
                    //var definition = (HttpCallDefinition)this.JsonSerializer.Convert(context.TaskDefinition.With, typeof(HttpCallDefinition))!;
                    callType = context.TaskDefinition.Call.ToLower();
                    //content = definition.Endpoint.Uri.ToString();
                    break;
                }
            case "openapi":
                {
                    //var definition = (OpenApiCallDefinition)this.JsonSerializer.Convert(context.TaskDefinition.With, typeof(OpenApiCallDefinition))!;
                    callType = context.TaskDefinition.Call.ToLower();
                    //content = definition.OperationId;
                    break;
                }
            default:
                callType = string.Empty; 
                break;
        }
        var node = new CallTaskNodeViewModel(context.TaskReference, context.TaskName, content, callType);
        if (context.TaskGroup == null) context.Graph.AddNode(node);
        else context.TaskGroup.AddChild(node);
        this.BuildEdge(context.Graph, node, this.GetNextNode(context, node));
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
        var cluster = new DoTaskNodeViewModel(context.TaskReference, context.TaskName, $"{taskCount} task{(taskCount > 1 ? "s" : "")}");
        var port = new PortNodeViewModel(context.TaskReference + _portSuffix);
        cluster.AddChild(port);
        if (context.TaskGroup == null) context.Graph.AddCluster(cluster);
        else context.TaskGroup.AddChild(cluster);
        var innerContext = new TaskNodeRenderingContext(context.Workflow, context.Graph, 0, context.TaskDefinition.Do.First().Key, context.TaskDefinition.Do.First().Value, cluster, context.TaskReference + "/do", context, context.EndNode, context.PreviousNode);
        this.BuildTaskNode(innerContext);
        if (taskCount > 0)
        {
            var firstDoNode = (NodeViewModel)cluster.AllNodes.Skip(1).First().Value;
            this.BuildEdge(context.Graph, port, firstDoNode);
            if (taskCount > 1 && !string.IsNullOrWhiteSpace(context.TaskDefinition.Do.First().Value.If))
            {
                this.BuildEdge(context.Graph, port, this.GetNextNode(innerContext, firstDoNode));
            }
        }
        else
        {
            this.BuildEdge(context.Graph, port, this.GetNextNode(context, cluster));
        }
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
        var node = new EmitTaskNodeViewModel(context.TaskReference, context.TaskName, this.YamlSerializer.SerializeToText(context.TaskDefinition.Emit.Event.With));
        if (context.TaskGroup == null) context.Graph.AddNode(node);
        else context.TaskGroup.AddChild(node);
        this.BuildEdge(context.Graph, node, this.GetNextNode(context, node));
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
        var node = new ExtensionTaskNodeViewModel(context.TaskReference, context.TaskName);
        if (context.TaskGroup == null) context.Graph.AddNode(node);
        else context.TaskGroup.AddChild(node);
        this.BuildEdge(context.Graph, node, this.GetNextNode(context, node));
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
        var cluster = new ForTaskNodeViewModel(context.TaskReference, context.TaskName, this.YamlSerializer.SerializeToText(context.TaskDefinition.For));
        var port = new PortNodeViewModel(context.TaskReference + _portSuffix);
        cluster.AddChild(port);
        if (context.TaskGroup == null) context.Graph.AddCluster(cluster);
        else context.TaskGroup.AddChild(cluster);
        this.BuildTaskNode(new(context.Workflow, context.Graph, 0, context.TaskDefinition.Do.First().Key, context.TaskDefinition.Do.First().Value, cluster, context.TaskReference + "/do", context, context.EndNode, context.PreviousNode));
        if (context.TaskDefinition.Do.Count > 0)
        {
            this.BuildEdge(context.Graph, port, (NodeViewModel)cluster.AllNodes.Skip(1).First().Value);
        }
        else
        {
            this.BuildEdge(context.Graph, port, this.GetNextNode(context, cluster));
        }
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
        var cluster = new ForkTaskNodeViewModel(context.TaskReference, context.TaskName, this.YamlSerializer.SerializeToText(context.TaskDefinition.Fork));
        var entryPort = new PortNodeViewModel(context.TaskReference + _portSuffix);
        var exitPort = new PortNodeViewModel(context.TaskReference + "-exit" + _portSuffix);
        cluster.AddChild(entryPort);
        if (context.TaskGroup == null) context.Graph.AddCluster(cluster);
        else context.TaskGroup.AddChild(cluster);
        for (int i = 0, c = context.TaskDefinition.Fork.Branches.Count; i<c; i++)
        {
            var branch = context.TaskDefinition.Fork.Branches.ElementAt(i);
            var branchNode = this.BuildTaskNode(new(context.Workflow, context.Graph, i, branch.Key, branch.Value, cluster, context.TaskReference + "/fork/branches", context, context.EndNode, context.PreviousNode));
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
        cluster.AddChild(exitPort);
        this.BuildEdge(context.Graph, exitPort, this.GetNextNode(context, cluster));
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
        var node = new ListenTaskNodeViewModel(context.TaskReference, context.TaskName, this.YamlSerializer.SerializeToText(context.TaskDefinition.Listen));
        if (context.TaskGroup == null) context.Graph.AddNode(node);
        else context.TaskGroup.AddChild(node);
        this.BuildEdge(context.Graph, node, this.GetNextNode(context, node));
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
        var node = new RaiseTaskNodeViewModel(context.TaskReference, context.TaskName, this.YamlSerializer.SerializeToText(context.TaskDefinition.Raise.Error));
        if (context.TaskGroup == null) context.Graph.AddNode(node);
        else context.TaskGroup.AddChild(node);
        this.BuildEdge(context.Graph, node, this.GetNextNode(context, node));
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
        var node = new RunTaskNodeViewModel(context.TaskReference, context.TaskName, content, runType);
        if (context.TaskGroup == null) context.Graph.AddNode(node);
        else context.TaskGroup.AddChild(node);
        this.BuildEdge(context.Graph, node, this.GetNextNode(context, node));
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
        var node = new SetTaskNodeViewModel(context.TaskReference, context.TaskName, this.YamlSerializer.SerializeToText(context.TaskDefinition.Set));
        if (context.TaskGroup == null) context.Graph.AddNode(node);
        else context.TaskGroup.AddChild(node);
        this.BuildEdge(context.Graph, node, this.GetNextNode(context, node));
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
        var node = new SwitchTaskNodeViewModel(context.TaskReference, context.TaskName, this.YamlSerializer.SerializeToText(context.TaskDefinition.Switch));
        if (context.TaskGroup == null) context.Graph.AddNode(node);
        else context.TaskGroup.AddChild(node);
        foreach (var switchCase in context.TaskDefinition.Switch)
        {
            var switchCaseNode = this.GetNextNode(context, node, false, switchCase.Value.Then);
            this.BuildEdge(context.Graph, node, switchCaseNode, switchCase.Key);
        }
        if (!context.TaskDefinition.Switch.Any(switchCase => string.IsNullOrEmpty(switchCase.Value.When)))
        {
            this.BuildEdge(context.Graph, node, this.GetNextNode(context, node));
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
        var containerCluster = new TryTaskNodeViewModel(context.TaskReference, context.TaskName, $"{taskCount} task{(taskCount > 1 ? "s" : "")}");
        var containerPort = new PortNodeViewModel(context.TaskReference + _portSuffix);
        containerCluster.AddChild(containerPort);
        if (context.TaskGroup == null) context.Graph.AddCluster(containerCluster);
        else context.TaskGroup.AddChild(containerCluster);

        var tryCluster = new TryNodeViewModel(context.TaskReference + _trySuffix, context.TaskName, string.Empty);
        var tryPort = new PortNodeViewModel(context.TaskReference + _trySuffix + _portSuffix);
        tryCluster.AddChild(tryPort);
        containerCluster.AddChild(tryCluster);
        this.BuildEdge(context.Graph, containerPort, tryPort);
        var innerContext = new TaskNodeRenderingContext(context.Workflow, context.Graph, 0, context.TaskDefinition.Try.First().Key, context.TaskDefinition.Try.First().Value, tryCluster, context.TaskReference + "/try", context, context.EndNode, context.PreviousNode);
        this.BuildTaskNode(innerContext);
        if (taskCount > 0)
        {
            var firstNode = (NodeViewModel)tryCluster.AllNodes.Skip(1).First().Value;
            this.BuildEdge(context.Graph, tryPort, firstNode);
            if (taskCount > 1 && !string.IsNullOrWhiteSpace(context.TaskDefinition.Try.First().Value.If))
            {
                this.BuildEdge(context.Graph, tryPort, this.GetNextNode(innerContext, firstNode));
            }
        }
        var catchContent = this.YamlSerializer.SerializeToText(context.TaskDefinition.Catch);
        if (context.TaskDefinition.Catch.Do == null || context.TaskDefinition.Catch.Do.Count == 0)
        {
            var catchNode = new CatchNodeViewModel(context.TaskReference + _catchSuffix, context.TaskName, catchContent);
            containerCluster.AddChild(catchNode);
            this.BuildEdge(context.Graph, tryCluster.AllNodes.Values.Last(), catchNode);
            this.BuildEdge(context.Graph, catchNode, this.GetNextNode(context, containerCluster));
        }
        else
        {
            var catchCluster = new CatchDoNodeViewModel(context.TaskReference + _catchSuffix, context.TaskName, catchContent);
            var catchPort = new PortNodeViewModel(context.TaskReference + _catchSuffix + _portSuffix);
            catchCluster.AddChild(catchPort);
            containerCluster.AddChild(catchCluster);
            this.BuildEdge(context.Graph, tryCluster.AllNodes.Values.Last(), catchPort);
            this.BuildTaskNode(new(context.Workflow, context.Graph, 0, context.TaskDefinition.Catch.Do.First().Key, context.TaskDefinition.Catch.Do.First().Value, catchCluster, context.TaskReference + "/catch/do", context, context.EndNode, context.PreviousNode));
            this.BuildEdge(context.Graph, catchPort, catchCluster.AllNodes.Values.Skip(1).First());
            this.BuildEdge(context.Graph, catchCluster.AllNodes.Values.Last(), this.GetNextNode(context, containerCluster));
        }
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
        var node = new WaitTaskNodeViewModel(context.TaskReference, context.TaskName, context.TaskDefinition.Wait.ToTimeSpan().ToString("hh\\:mm\\:ss\\.fff"));
        if (context.TaskGroup == null) context.Graph.AddNode(node);
        else context.TaskGroup.AddChild(node);
        this.BuildEdge(context.Graph, node, this.GetNextNode(context, node));
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
            if (!string.IsNullOrEmpty(label)) { 
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
        if (target.Id.EndsWith(_portSuffix))
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
    /// <param name="taskIndex">The index of the task.</param>
    /// <param name="taskName">The name of the task.</param>
    /// <param name="taskDefinition">The definition of the task.</param>
    /// <param name="taskGroup">The optional task group.</param>
    /// <param name="parentReference">The reference to the parent task node.</param>
    /// <param name="parentContext">The parent rendering context of the task node.</param>
    /// <param name="endNode">The end node view model.</param>
    /// <param name="previousNode">The previous node view model.</param>
    protected class TaskNodeRenderingContext(WorkflowDefinition workflow, GraphViewModel graph, int taskIndex, string taskName, TaskDefinition taskDefinition, WorkflowClusterViewModel? taskGroup, string parentReference, TaskNodeRenderingContext? parentContext, NodeViewModel endNode, NodeViewModel previousNode)
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
        /// Gets the index of the task.
        /// </summary>
        public virtual int TaskIndex { get; } = taskIndex;

        /// <summary>
        /// Gets the name of the task.
        /// </summary>
        public virtual string TaskName { get; } = taskName;

        /// <summary>
        /// Gets the definition of the task.
        /// </summary>
        public virtual TaskDefinition TaskDefinition { get; } = taskDefinition;

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
        /// Gets the end node view model.
        /// </summary>
        public virtual NodeViewModel EndNode { get; } = endNode;

        /// <summary>
        /// Gets the previous node view model.
        /// </summary>
        public virtual NodeViewModel PreviousNode { get; } = previousNode;

        /// <summary>
        /// Creates a new instance of <see cref="TaskNodeRenderingContext{TDefinition}"/> with the specified task definition type.
        /// </summary>
        /// <typeparam name="TDefinition">The type of the task definition.</typeparam>
        /// <returns>A new instance of <see cref="TaskNodeRenderingContext{TDefinition}"/>.</returns>
        public virtual TaskNodeRenderingContext<TDefinition> OfType<TDefinition>() where TDefinition : TaskDefinition => new(this.Workflow, this.Graph, this.TaskIndex, this.TaskName, this.TaskDefinition, this.TaskGroup, this.ParentReference, this.ParentContext, this.EndNode, this.PreviousNode);

    }

    /// <summary>
    /// Represents the context for rendering a task node within a workflow, with a specific task definition type.
    /// </summary>
    /// <typeparam name="TDefinition">The type of the task definition.</typeparam>
    /// <param name="workflow">The workflow definition.</param>
    /// <param name="graph">The graph view model.</param>
    /// <param name="taskIndex">The index of the task.</param>
    /// <param name="taskName">The name of the task.</param>
    /// <param name="taskDefinition">The definition of the task.</param>
    /// <param name="taskGroup">The optional task group.</param>
    /// <param name="parentReference">The reference to the parent task node.</param>
    /// <param name="parentContext">The parent rendering context of the task node.</param>
    /// <param name="endNode">The end node view model.</param>
    /// <param name="previousNode">The previous node view model.</param>
    protected class TaskNodeRenderingContext<TDefinition>(WorkflowDefinition workflow, GraphViewModel graph, int taskIndex, string taskName, TaskDefinition taskDefinition, WorkflowClusterViewModel? taskGroup, string parentReference, TaskNodeRenderingContext? parentContext, NodeViewModel endNode, NodeViewModel previousNode)
        : TaskNodeRenderingContext(workflow, graph, taskIndex, taskName, taskDefinition, taskGroup, parentReference, parentContext, endNode, previousNode)
        where TDefinition : TaskDefinition
    {
        /// <summary>
        /// Gets the task definition of type <typeparamref name="TDefinition"/>.
        /// </summary>
        public new virtual TDefinition TaskDefinition => (TDefinition)base.TaskDefinition;

    }

    /// <summary>
    /// Represents the identity of a task
    /// </summary>
    /// <param name="name">The task name</param>
    /// <param name="index">The task index</param>
    /// <param name="reference">The task reference</param>
    /// <param name="context">The task rendering context</param>
    protected class TaskIdentity(string name, int index, string reference, TaskNodeRenderingContext context)
    {
        /// <summary>
        /// Gets the task name
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Gets the task index
        /// </summary>
        public int Index { get; } = index;

        /// <summary>
        /// Gets the task reference
        /// </summary>
        public string Reference { get; } = reference;

        /// <summary>
        /// Get the task rendering context
        /// </summary>
        public TaskNodeRenderingContext Context { get; } = context;
    }
}
