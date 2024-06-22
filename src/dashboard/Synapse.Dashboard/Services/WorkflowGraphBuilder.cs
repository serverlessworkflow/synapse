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

using Neuroglia.Blazor.Dagre.Models;
using ServerlessWorkflow.Sdk;
using ServerlessWorkflow.Sdk.Models;
using ServerlessWorkflow.Sdk.Models.Tasks;
using System.Diagnostics;

namespace Synapse.Dashboard.Services;

/// <summary>
/// Represents the default implementation of the <see cref="IWorkflowGraphBuilder"/> interface
/// </summary>
public class WorkflowGraphBuilder
    : IWorkflowGraphBuilder
{

    /// <summary>
    /// Gets the default radius for start and end nodes
    /// </summary>
    public const int StartEndNodeRadius = 30;

    /// <inheritdoc/>
    public async Task<IGraphViewModel> Build(WorkflowDefinition workflow)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        Stopwatch sw = Stopwatch.StartNew();
        ArgumentNullException.ThrowIfNull(workflow);
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
    /// <param name="workflow">The <see cref="WorkflowDefinition"/> that defines the <see cref="TaskDefinition"/> to create a new <see cref="TaskNodeViewModel"/> for</param>
    /// <param name="graph">The current <see cref="GraphViewModel"/></param>
    /// <param name="task">The name/definition mapping of the <see cref=""/> to create a new <see cref="TaskNodeViewModel"/> for</param>
    /// <param name="taskIndex">The index within its parent of the <see cref="TaskDefinition"/> to create a new <see cref="TaskNodeViewModel"/></param>
    /// <param name="endNode">The end node</param>
    /// <param name="previousNode">The previous node</param>
    /// <param name="parentReference">A reference to the parent, if any, of the task to build a new <see cref="TaskNodeViewModel"/> for</param>
    /// <returns>A new <see cref="TaskNodeViewModel"/></returns>
    protected async Task<NodeViewModel> BuildTaskNodeAsync(TaskNodeRenderingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.TaskDefinition switch
        {
            CallTaskDefinition callTask => await this.BuildCallTaskNodeAsync(context.OfType<CallTaskDefinition>()),
            DoTaskDefinition compositeTask => await this.BuildDoTaskNodeAsync(context.OfType<DoTaskDefinition>()),
            EmitTaskDefinition emitTask => await this.BuildEmitTaskNodeAsync(context.OfType<EmitTaskDefinition>()),
            ExtensionTaskDefinition extensionTask => await this.BuildExtensionTaskNodeAsync(context.OfType<ExtensionTaskDefinition>()),
            ForTaskDefinition forTask => await this.BuildForTaskNodeAsync(context.OfType<ForTaskDefinition>()),
            ListenTaskDefinition listenTask => await this.BuildListenTaskNodeAsync(context.OfType<ListenTaskDefinition>()),
            RaiseTaskDefinition raiseTask => await this.BuildRaiseTaskNodeAsync(context.OfType<RaiseTaskDefinition>()),
            RunTaskDefinition runTask => await this.BuildRunTaskNodeAsync(context.OfType<RunTaskDefinition>()),
            SetTaskDefinition setTask => await this.BuildSetTaskNodeAsync(context.OfType<SetTaskDefinition>()),
            SwitchTaskDefinition switchTask => await this.BuildSwitchTaskNodeAsync(context.OfType<SwitchTaskDefinition>()),
            TryTaskDefinition tryTask => await this.BuildTryTaskNodeAsync(context.OfType<TryTaskDefinition>()),
            WaitTaskDefinition waitTask => await this.BuildWaitTaskNodeAsync(context.OfType<WaitTaskDefinition>()),
            _ => throw new NotSupportedException($"The specified task type '{context.TaskDefinition.GetType()}' is not supported")
        } ?? throw new Exception($"Unable to define a last node for task '{context.TaskName}'");
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified task
    /// </summary>
    /// <param name="workflow">The <see cref="WorkflowDefinition"/> that defines the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="graph">The current <see cref="GraphViewModel"/></param>
    /// <param name="task">The name/definition mapping of the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="taskNodeGroup">The current task node group</param>
    /// <param name="endNode">The end node</param>
    /// <param name="previousNode">The previous node</param>
    /// <param name="parentReference">The reference of the task's parent</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildCallTaskNodeAsync(TaskNodeRenderingContext<CallTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var lastNode = new CallTaskNodeViewModel(context.TaskName);
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified task
    /// </summary>
    /// <param name="workflow">The <see cref="WorkflowDefinition"/> that defines the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="graph">The current <see cref="GraphViewModel"/></param>
    /// <param name="task">The name/definition mapping of the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="taskNodeGroup">The current task node group</param>
    /// <param name="endNode">The end node</param>
    /// <param name="previousNode">The previous node</param>
    /// <param name="parentReference">The reference of the task's parent</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildDoTaskNodeAsync(TaskNodeRenderingContext<DoTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var lastNode = new DoTaskNodeViewModel(context.TaskName);
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified task
    /// </summary>
    /// <param name="workflow">The <see cref="WorkflowDefinition"/> that defines the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="graph">The current <see cref="GraphViewModel"/></param>
    /// <param name="task">The name/definition mapping of the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="taskNodeGroup">The current task node group</param>
    /// <param name="endNode">The end node</param>
    /// <param name="previousNode">The previous node</param>
    /// <param name="parentReference">The reference of the task's parent</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildEmitTaskNodeAsync(TaskNodeRenderingContext<EmitTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var lastNode = new EmitTaskNodeViewModel(context.TaskName);
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified task
    /// </summary>
    /// <param name="workflow">The <see cref="WorkflowDefinition"/> that defines the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="graph">The current <see cref="GraphViewModel"/></param>
    /// <param name="task">The name/definition mapping of the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="taskNodeGroup">The current task node group</param>
    /// <param name="endNode">The end node</param>
    /// <param name="previousNode">The previous node</param>
    /// <param name="parentReference">The reference of the task's parent</param>
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
    /// Builds a new <see cref="NodeViewModel"/> for the specified task
    /// </summary>
    /// <param name="workflow">The <see cref="WorkflowDefinition"/> that defines the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="graph">The current <see cref="GraphViewModel"/></param>
    /// <param name="task">The name/definition mapping of the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="taskNodeGroup">The current task node group</param>
    /// <param name="endNode">The end node</param>
    /// <param name="previousNode">The previous node</param>
    /// <param name="parentReference">The reference of the task's parent</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildForTaskNodeAsync(TaskNodeRenderingContext<ForTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var lastNode = new ForTaskNodeViewModel(context.TaskName);
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified task
    /// </summary>
    /// <param name="workflow">The <see cref="WorkflowDefinition"/> that defines the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="graph">The current <see cref="GraphViewModel"/></param>
    /// <param name="task">The name/definition mapping of the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="taskNodeGroup">The current task node group</param>
    /// <param name="endNode">The end node</param>
    /// <param name="previousNode">The previous node</param>
    /// <param name="parentReference">The reference of the task's parent</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildListenTaskNodeAsync(TaskNodeRenderingContext<ListenTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var lastNode = new ListenTaskNodeViewModel(context.TaskName);
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified task
    /// </summary>
    /// <param name="workflow">The <see cref="WorkflowDefinition"/> that defines the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="graph">The current <see cref="GraphViewModel"/></param>
    /// <param name="task">The name/definition mapping of the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="taskNodeGroup">The current task node group</param>
    /// <param name="endNode">The end node</param>
    /// <param name="previousNode">The previous node</param>
    /// <param name="parentReference">The reference of the task's parent</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildRaiseTaskNodeAsync(TaskNodeRenderingContext<RaiseTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var lastNode = new RaiseTaskNodeViewModel(context.TaskName);
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified task
    /// </summary>
    /// <param name="workflow">The <see cref="WorkflowDefinition"/> that defines the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="graph">The current <see cref="GraphViewModel"/></param>
    /// <param name="task">The name/definition mapping of the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="taskNodeGroup">The current task node group</param>
    /// <param name="endNode">The end node</param>
    /// <param name="previousNode">The previous node</param>
    /// <param name="parentReference">The reference of the task's parent</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildRunTaskNodeAsync(TaskNodeRenderingContext<RunTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var lastNode = new RunTaskNodeViewModel(context.TaskName);
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified task
    /// </summary>
    /// <param name="workflow">The <see cref="WorkflowDefinition"/> that defines the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="graph">The current <see cref="GraphViewModel"/></param>
    /// <param name="task">The name/definition mapping of the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="taskNodeGroup">The current task node group</param>
    /// <param name="endNode">The end node</param>
    /// <param name="previousNode">The previous node</param>
    /// <param name="parentReference">The reference of the task's parent</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildSetTaskNodeAsync(TaskNodeRenderingContext<SetTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var lastNode = new SetTaskNodeViewModel(context.TaskName);
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified task
    /// </summary>
    /// <param name="workflow">The <see cref="WorkflowDefinition"/> that defines the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="graph">The current <see cref="GraphViewModel"/></param>
    /// <param name="task">The name/definition mapping of the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="taskNodeGroup">The current task node group</param>
    /// <param name="endNode">The end node</param>
    /// <param name="previousNode">The previous node</param>
    /// <param name="parentReference">The reference of the task's parent</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildSwitchTaskNodeAsync(TaskNodeRenderingContext<SwitchTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var lastNode = new SwitchTaskNodeViewModel(context.TaskName);
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified task
    /// </summary>
    /// <param name="workflow">The <see cref="WorkflowDefinition"/> that defines the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="graph">The current <see cref="GraphViewModel"/></param>
    /// <param name="task">The name/definition mapping of the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="taskNodeGroup">The current task node group</param>
    /// <param name="endNode">The end node</param>
    /// <param name="previousNode">The previous node</param>
    /// <param name="parentReference">The reference of the task's parent</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildTryTaskNodeAsync(TaskNodeRenderingContext<TryTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var lastNode = new TryTaskNodeViewModel(context.TaskName);
        if (context.TaskGroup == null) await context.Graph.AddElementAsync(lastNode);
        else await context.TaskGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(context.Graph, context.PreviousNode, lastNode);
        return lastNode;
    }

    /// <summary>
    /// Builds a new <see cref="NodeViewModel"/> for the specified task
    /// </summary>
    /// <param name="workflow">The <see cref="WorkflowDefinition"/> that defines the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="graph">The current <see cref="GraphViewModel"/></param>
    /// <param name="task">The name/definition mapping of the <see cref="TaskDefinition"/> to create a new <see cref="NodeViewModel"/> for</param>
    /// <param name="taskNodeGroup">The current task node group</param>
    /// <param name="endNode">The end node</param>
    /// <param name="previousNode">The previous node</param>
    /// <param name="parentReference">The reference of the task's parent</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual async Task<NodeViewModel> BuildWaitTaskNodeAsync(TaskNodeRenderingContext<WaitTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var lastNode = new WaitTaskNodeViewModel(context.TaskName);
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

    protected class TaskNodeRenderingContext(WorkflowDefinition workflow, GraphViewModel graph, int taskIndex, string taskName, TaskDefinition taskDefinition, TaskNodeViewModel? taskGroup, string parentReference, NodeViewModel endNode, NodeViewModel previousNode)
    {

        public virtual WorkflowDefinition Workflow { get; } = workflow;

        public virtual GraphViewModel Graph { get; } = graph;

        public virtual int TaskIndex { get; } = taskIndex;

        public virtual string TaskName { get; } = taskName;

        public virtual TaskDefinition TaskDefinition { get; } = taskDefinition;

        public virtual TaskNodeViewModel TaskGroup { get; } = taskGroup;

        public virtual string TaskReference => $"{this.ParentReference}/{this.TaskIndex}/{this.TaskName}";

        public virtual string ParentReference { get; } = parentReference;

        public virtual NodeViewModel EndNode { get; } = endNode;

        public virtual NodeViewModel PreviousNode { get; } = previousNode;

        public virtual TaskNodeRenderingContext<TDefinition> OfType<TDefinition>() where TDefinition : TaskDefinition => new(this.Workflow, this.Graph, this.TaskIndex, this.TaskName, this.TaskDefinition, this.TaskGroup, this.ParentReference, this.EndNode, this.PreviousNode);

    }

    protected class TaskNodeRenderingContext<TDefinition>(WorkflowDefinition workflow, GraphViewModel graph, int taskIndex, string taskName, TaskDefinition taskDefinition, TaskNodeViewModel? taskGroup, string parentReference, NodeViewModel endNode, NodeViewModel previousNode)
        : TaskNodeRenderingContext(workflow, graph, taskIndex, taskName, taskDefinition, taskGroup, parentReference, endNode, previousNode)
        where TDefinition : TaskDefinition
    {
        public new virtual TDefinition TaskDefinition => (TDefinition)base.TaskDefinition;

    }

}
