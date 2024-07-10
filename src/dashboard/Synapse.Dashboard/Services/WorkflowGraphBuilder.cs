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

using Microsoft.VisualBasic;
using Neuroglia.Blazor.Dagre.Models;
using Neuroglia.Eventing.CloudEvents;
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
    public async Task<IGraphViewModel> Build(WorkflowDefinition workflow)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        Stopwatch sw = Stopwatch.StartNew();
        var isEmpty = workflow.Do.Count < 1;
        var graph = new GraphViewModel();
        var startNode = this.BuildStartNode(!isEmpty);
        var endNode = this.BuildEndNode();
        await graph.AddElementAsync(startNode);
        await graph.AddElementAsync(endNode);
        if (isEmpty) await this.BuildEdgeAsync(graph, startNode, endNode);
        else await this.BuildTaskNodesAsync(new(workflow, graph, 0, workflow.Do.First().Key, workflow.Do.First().Value, null, "/do", endNode, startNode));
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
    /// Recursively builds task nodes and their edges
    /// </summary>
    /// <param name="context">The rendering context for the task nodes</param>
    /// <returns>The last built task <see cref="NodeViewModel"/></returns>
    protected async Task<NodeViewModel> BuildTaskNodesAsync(TaskNodeRenderingContext context)
    {
        var lastNode = await this.BuildTaskNodeAsync(context);
        if (context.TaskDefinition.Then == FlowDirective.End || context.TaskDefinition.Then == FlowDirective.Exit) await this.BuildEdgeAsync(context.Graph, lastNode, context.EndNode);
        else
        {
            var nextTaskName = string.IsNullOrWhiteSpace(context.TaskDefinition.Then) || context.TaskDefinition.Then == FlowDirective.Continue
                ? context.Workflow.GetTaskAfter(new(context.TaskName, context.TaskDefinition), context.ParentReference)?.Key
                : context.TaskDefinition.Then;
            if (string.IsNullOrWhiteSpace(nextTaskName)) await this.BuildEdgeAsync(context.Graph, lastNode, context.EndNode);
            else
            {
                var nextTaskIndex = context.Workflow.IndexOf(nextTaskName, context.ParentReference);
                var nextTaskReference = $"{context.ParentReference}/{nextTaskIndex}/{nextTaskName}";
                var nextTask = context.Workflow.GetComponent<TaskDefinition>(nextTaskReference) ?? throw new Exception($"Failed to find the task at '{nextTaskReference}' in workflow '{context.Workflow.Document.Name}.{context.Workflow.Document.Namespace}:{context.Workflow.Document.Version}'");
                var nextTaskNode = await this.BuildTaskNodesAsync(new(context.Workflow, context.Graph, nextTaskIndex, nextTaskName, nextTask, context.TaskGroup, context.ParentReference, context.EndNode, lastNode));
                await this.BuildEdgeAsync(context.Graph, lastNode, nextTaskNode);
            }
        }
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="TaskNodeViewModel"/> for the specified task
    /// </summary>
    /// <param name="context">The rendering context for the task node</param>
    /// <returns>A new <see cref="TaskNodeViewModel"/></returns>
    protected async Task<NodeViewModel> BuildTaskNodeAsync(TaskNodeRenderingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.TaskDefinition switch
        {
            CallTaskDefinition => await this.BuildCallTaskNodeAsync(context.OfType<CallTaskDefinition>()),
            DoTaskDefinition => await this.BuildDoTaskNodeAsync(context.OfType<DoTaskDefinition>()),
            EmitTaskDefinition => await this.BuildEmitTaskNodeAsync(context.OfType<EmitTaskDefinition>()),
            ExtensionTaskDefinition => await this.BuildExtensionTaskNodeAsync(context.OfType<ExtensionTaskDefinition>()),
            ForTaskDefinition => await this.BuildForTaskNodeAsync(context.OfType<ForTaskDefinition>()),
            ListenTaskDefinition => await this.BuildListenTaskNodeAsync(context.OfType<ListenTaskDefinition>()),
            RaiseTaskDefinition => await this.BuildRaiseTaskNodeAsync(context.OfType<RaiseTaskDefinition>()),
            RunTaskDefinition => await this.BuildRunTaskNodeAsync(context.OfType<RunTaskDefinition>()),
            SetTaskDefinition => await this.BuildSetTaskNodeAsync(context.OfType<SetTaskDefinition>()),
            SwitchTaskDefinition => await this.BuildSwitchTaskNodeAsync(context.OfType<SwitchTaskDefinition>()),
            TryTaskDefinition => await this.BuildTryTaskNodeAsync(context.OfType<TryTaskDefinition>()),
            WaitTaskDefinition => await this.BuildWaitTaskNodeAsync(context.OfType<WaitTaskDefinition>()),
            _ => throw new NotSupportedException($"The specified task type '{context.TaskDefinition.GetType()}' is not supported")
        } ?? throw new Exception($"Unable to define a last node for task '{context.TaskName}'");
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified call task
    /// </summary>
    /// <param name="context">The rendering context for the call task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildCallTaskNodeAsync(TaskNodeRenderingContext<CallTaskDefinition> context)
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
                    var definition = (OpenApiCallDefinition)this.JsonSerializer.Convert(context.TaskDefinition.With, typeof(OpenApiCallDefinition))!;
                    callType = context.TaskDefinition.Call.ToLower();
                    content = definition.OperationId;
                    break;
                }
            default:
                callType = string.Empty; 
                break;
        }
        var lastNode = new CallTaskNodeViewModel(context.TaskName, content, callType);
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified do task
    /// </summary>
    /// <param name="context">The rendering context for the do task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildDoTaskNodeAsync(TaskNodeRenderingContext<DoTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var taskCount = context.TaskDefinition.Do.Count;
        var lastNode = new DoTaskNodeViewModel(context.TaskName, $"{taskCount} task{(taskCount > 1 ? "s": "")}");
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified emit task
    /// </summary>
    /// <param name="context">The rendering context for the emit task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildEmitTaskNodeAsync(TaskNodeRenderingContext<EmitTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var lastNode = new EmitTaskNodeViewModel(context.TaskName, this.YamlSerializer.SerializeToText(context.TaskDefinition.Emit.Event.With));
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified extension task
    /// </summary>
    /// <param name="context">The rendering context for the extension task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildExtensionTaskNodeAsync(TaskNodeRenderingContext<ExtensionTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var lastNode = new ExtensionTaskNodeViewModel(context.TaskName);
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified for task
    /// </summary>
    /// <param name="context">The rendering context for the for task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildForTaskNodeAsync(TaskNodeRenderingContext<ForTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var lastNode = new ForTaskNodeViewModel(context.TaskName, this.YamlSerializer.SerializeToText(context.TaskDefinition.For));
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified listen task
    /// </summary>
    /// <param name="context">The rendering context for the listen task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildListenTaskNodeAsync(TaskNodeRenderingContext<ListenTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var lastNode = new ListenTaskNodeViewModel(context.TaskName, this.YamlSerializer.SerializeToText(context.TaskDefinition.Listen));
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified raise task
    /// </summary>
    /// <param name="context">The rendering context for the raise task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildRaiseTaskNodeAsync(TaskNodeRenderingContext<RaiseTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var lastNode = new RaiseTaskNodeViewModel(context.TaskName, this.YamlSerializer.SerializeToText(context.TaskDefinition.Raise.Error));
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified run task
    /// </summary>
    /// <param name="context">The rendering context for the run task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildRunTaskNodeAsync(TaskNodeRenderingContext<RunTaskDefinition> context)
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
        var lastNode = new RunTaskNodeViewModel(context.TaskName, content, runType);
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified set task
    /// </summary>
    /// <param name="context">The rendering context for the set task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildSetTaskNodeAsync(TaskNodeRenderingContext<SetTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var lastNode = new SetTaskNodeViewModel(context.TaskName, this.YamlSerializer.SerializeToText(context.TaskDefinition.Set));
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified switch task
    /// </summary>
    /// <param name="context">The rendering context for the switch task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildSwitchTaskNodeAsync(TaskNodeRenderingContext<SwitchTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var lastNode = new SwitchTaskNodeViewModel(context.TaskName, this.YamlSerializer.SerializeToText(context.TaskDefinition.Switch));
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified try task
    /// </summary>
    /// <param name="context">The rendering context for the try task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildTryTaskNodeAsync(TaskNodeRenderingContext<TryTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var taskCount = context.TaskDefinition.Try.Count;
        var lastNode = new TryTaskNodeViewModel(context.TaskName, $"{taskCount} task{(taskCount > 1 ? "s" : "")}");
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified wait task
    /// </summary>
    /// <param name="context">The rendering context for the wait task node</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildWaitTaskNodeAsync(TaskNodeRenderingContext<WaitTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var lastNode = new WaitTaskNodeViewModel(context.TaskName, context.TaskDefinition.Wait.ToTimeSpan().ToString("hh\\:mm\\:ss\\.fff"));
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
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
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual Task BuildEdgeAsync(GraphViewModel graph, NodeViewModel source, NodeViewModel target) => graph.AddElementAsync(new EdgeViewModel(source.Id, target.Id, null));

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
    /// <param name="endNode">The end node view model.</param>
    /// <param name="previousNode">The previous node view model.</param>
    protected class TaskNodeRenderingContext(WorkflowDefinition workflow, GraphViewModel graph, int taskIndex, string taskName, TaskDefinition taskDefinition, TaskNodeViewModel? taskGroup, string parentReference, NodeViewModel endNode, NodeViewModel previousNode)
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
        public virtual TaskNodeViewModel? TaskGroup { get; } = taskGroup;

        /// <summary>
        /// Gets the reference of the task node in the context of the parent task node.
        /// </summary>
        public virtual string TaskReference => $"{this.ParentReference}/{this.TaskIndex}/{this.TaskName}";

        /// <summary>
        /// Gets the reference to the parent task node.
        /// </summary>
        public virtual string ParentReference { get; } = parentReference;

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
        public virtual TaskNodeRenderingContext<TDefinition> OfType<TDefinition>() where TDefinition : TaskDefinition => new(this.Workflow, this.Graph, this.TaskIndex, this.TaskName, this.TaskDefinition, this.TaskGroup, this.ParentReference, this.EndNode, this.PreviousNode);

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
    /// <param name="endNode">The end node view model.</param>
    /// <param name="previousNode">The previous node view model.</param>
    protected class TaskNodeRenderingContext<TDefinition>(WorkflowDefinition workflow, GraphViewModel graph, int taskIndex, string taskName, TaskDefinition taskDefinition, TaskNodeViewModel? taskGroup, string parentReference, NodeViewModel endNode, NodeViewModel previousNode)
        : TaskNodeRenderingContext(workflow, graph, taskIndex, taskName, taskDefinition, taskGroup, parentReference, endNode, previousNode)
        where TDefinition : TaskDefinition
    {
        /// <summary>
        /// Gets the task definition of type <typeparamref name="TDefinition"/>.
        /// </summary>
        public new virtual TDefinition TaskDefinition => (TDefinition)base.TaskDefinition;

    }

}
