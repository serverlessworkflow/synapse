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
        var isEmpty = workflow.Do.Count < 1;
        var graph = new GraphViewModel(enableProfiling: true);
        //graph.RegisterBehavior(new DragAndDropNodeBehavior(graph, this.jSRuntime));
        var startNode = this.BuildStartNode(!isEmpty);
        var endNode = this.BuildEndNode();
        await graph.AddElementAsync(startNode);
        if (isEmpty) await this.BuildEdgeAsync(graph, startNode, endNode);
        else await this.BuildTaskNodeAsync(workflow, graph, workflow.Do.First(), endNode, startNode, "/do"); 
        await graph.AddElementAsync(endNode);
        return graph;
    }

    /// <summary>
    /// Builds a new start <see cref="NodeViewModel"/>
    /// </summary>
    /// <param name="hasSuccessor">A boolean indicating whether or not the node has successor</param>
    /// <returns>A new <see cref="NodeViewModel"/></returns>
    protected virtual NodeViewModel BuildStartNode(bool hasSuccessor = false) => new StartNodeViewModel(hasSuccessor);

    /// <summary>
    /// Builds a new <see cref="TaskNodeViewModel"/> for the specified task
    /// </summary>
    /// <param name="workflow">The <see cref="WorkflowDefinition"/> that defines the <see cref="TaskDefinition"/> to create a new <see cref="TaskNodeViewModel"/> for</param>
    /// <param name="graph">The current <see cref="GraphViewModel"/></param>
    /// <param name="task">The name/definition mapping of the <see cref="TaskDefinition"/> to create a new <see cref="TaskNodeViewModel"/> for</param>
    /// <param name="endNode">The end node</param>
    /// <param name="previousNode">The previous node</param>
    /// <param name="parentReference">A reference to the parent, if any, of the task to build a new <see cref="TaskNodeViewModel"/> for</param>
    /// <returns>A new <see cref="TaskNodeViewModel"/></returns>
    protected async Task<NodeViewModel> BuildTaskNodeAsync(WorkflowDefinition workflow, GraphViewModel graph, MapEntry<string, TaskDefinition> task, NodeViewModel endNode, NodeViewModel previousNode, string parentReference)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(endNode);
        ArgumentNullException.ThrowIfNull(previousNode);
        var taskNodeGroup = graph.AllClusters.Values.OfType<TaskNodeViewModel>().FirstOrDefault(cluster => cluster.Task.Key == task.Key);
        if (taskNodeGroup != null) return taskNodeGroup;
        var lastNode = task.Value switch
        {
            CallTaskDefinition callTask => await this.BuildCallTaskNodeAsync(workflow, graph, new(task.Key, callTask), taskNodeGroup, endNode, previousNode, parentReference),
            DoTaskDefinition compositeTask => await this.BuildDoTaskNodeAsync(workflow, graph, new(task.Key, compositeTask), taskNodeGroup, endNode, previousNode, parentReference),
            EmitTaskDefinition emitTask => await this.BuildEmitTaskNodeAsync(workflow, graph, new(task.Key, emitTask), taskNodeGroup, endNode, previousNode, parentReference),
            ExtensionTaskDefinition extensionTask => await this.BuildExtensionTaskNodeAsync(workflow, graph, new(task.Key, extensionTask), taskNodeGroup, endNode, previousNode, parentReference),
            ForTaskDefinition forTask => await this.BuildForTaskNodeAsync(workflow, graph, new(task.Key, forTask), taskNodeGroup, endNode, previousNode, parentReference),
            ListenTaskDefinition listenTask => await this.BuildListenTaskNodeAsync(workflow, graph, new(task.Key, listenTask), taskNodeGroup, endNode, previousNode, parentReference),
            RaiseTaskDefinition raiseTask => await this.BuildRaiseTaskNodeAsync(workflow, graph, new(task.Key, raiseTask), taskNodeGroup, endNode, previousNode, parentReference),
            RunTaskDefinition runTask => await this.BuildRunTaskNodeAsync(workflow, graph, new(task.Key, runTask), taskNodeGroup, endNode, previousNode, parentReference),
            SetTaskDefinition setTask => await this.BuildSetTaskNodeAsync(workflow, graph, new(task.Key, setTask), taskNodeGroup, endNode, previousNode, parentReference),
            SwitchTaskDefinition switchTask => await this.BuildSwitchTaskNodeAsync(workflow, graph, new(task.Key, switchTask), taskNodeGroup, endNode, previousNode, parentReference),
            TryTaskDefinition tryTask => await this.BuildTryTaskNodeAsync(workflow, graph, new(task.Key, tryTask), taskNodeGroup, endNode, previousNode, parentReference),
            WaitTaskDefinition waitTask => await this.BuildWaitTaskNodeAsync(workflow, graph, new(task.Key, waitTask), taskNodeGroup, endNode, previousNode, parentReference),
            _ => throw new NotSupportedException($"The specified task type '{task.Value.GetType()}' is not supported")
        } ?? throw new Exception($"Unable to define a last node for task '{task.Key}'");
        if (task.Value.Then == FlowDirective.End || task.Value.Then == FlowDirective.Exit) await this.BuildEdgeAsync(graph, lastNode, endNode);
        else
        {
            var nextTaskName = string.IsNullOrWhiteSpace(task.Value.Then) || task.Value.Then == FlowDirective.Continue
                ? workflow.GetTaskAfter(task, parentReference)?.Key
                : task.Value.Then;
            if (string.IsNullOrWhiteSpace(nextTaskName)) await this.BuildEdgeAsync(graph, lastNode, endNode);
            else
            {
                var nextTaskIndex = workflow.IndexOf(nextTaskName, parentReference);
                var nextTaskReference = $"{parentReference}/{nextTaskIndex}/{nextTaskName}";
                var nextTask = workflow.GetComponent<TaskDefinition>(nextTaskReference) ?? throw new Exception($"Failed to find the task at '{nextTaskReference}' in workflow '{workflow.Document.Name}.{workflow.Document.Namespace}:{workflow.Document.Version}'");
                var nextTaskNode = await this.BuildTaskNodeAsync(workflow, graph, new(nextTaskName, nextTask), endNode, lastNode, parentReference);
                await this.BuildEdgeAsync(graph, lastNode, nextTaskNode);
            }
        }
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
    protected virtual async Task<NodeViewModel> BuildCallTaskNodeAsync(WorkflowDefinition workflow, GraphViewModel graph, MapEntry<string, CallTaskDefinition> task, TaskNodeViewModel? taskNodeGroup, NodeViewModel endNode, NodeViewModel previousNode, string parentReference)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(endNode);
        ArgumentNullException.ThrowIfNull(previousNode);
        ArgumentException.ThrowIfNullOrWhiteSpace(parentReference);
        var lastNode = new CallTaskNodeViewModel(task);
        if (taskNodeGroup == null) await graph.AddElementAsync(lastNode);
        else await taskNodeGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(graph, previousNode, lastNode);
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
    protected virtual async Task<NodeViewModel> BuildDoTaskNodeAsync(WorkflowDefinition workflow, GraphViewModel graph, MapEntry<string, DoTaskDefinition> task, TaskNodeViewModel? taskNodeGroup, NodeViewModel endNode, NodeViewModel previousNode, string parentReference)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(endNode);
        ArgumentNullException.ThrowIfNull(previousNode);
        ArgumentException.ThrowIfNullOrWhiteSpace(parentReference);
        var lastNode = new DoTaskNodeViewModel(task);
        if (taskNodeGroup == null) await graph.AddElementAsync(lastNode);
        else await taskNodeGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(graph, previousNode, lastNode);
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
    protected virtual async Task<NodeViewModel> BuildEmitTaskNodeAsync(WorkflowDefinition workflow, GraphViewModel graph, MapEntry<string, EmitTaskDefinition> task, TaskNodeViewModel? taskNodeGroup, NodeViewModel endNode, NodeViewModel previousNode, string parentReference)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(endNode);
        ArgumentNullException.ThrowIfNull(previousNode);
        ArgumentException.ThrowIfNullOrWhiteSpace(parentReference);
        var lastNode = new EmitTaskNodeViewModel(task);
        if (taskNodeGroup == null) await graph.AddElementAsync(lastNode);
        else await taskNodeGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(graph, previousNode, lastNode);
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
    protected virtual async Task<NodeViewModel> BuildExtensionTaskNodeAsync(WorkflowDefinition workflow, GraphViewModel graph, MapEntry<string, ExtensionTaskDefinition> task, TaskNodeViewModel? taskNodeGroup, NodeViewModel endNode, NodeViewModel previousNode, string parentReference)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(endNode);
        ArgumentNullException.ThrowIfNull(previousNode);
        ArgumentException.ThrowIfNullOrWhiteSpace(parentReference);
        var lastNode = new ExtensionTaskNodeViewModel(task);
        if (taskNodeGroup == null) await graph.AddElementAsync(lastNode);
        else await taskNodeGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(graph, previousNode, lastNode);
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
    protected virtual async Task<NodeViewModel> BuildForTaskNodeAsync(WorkflowDefinition workflow, GraphViewModel graph, MapEntry<string, ForTaskDefinition> task, TaskNodeViewModel? taskNodeGroup, NodeViewModel endNode, NodeViewModel previousNode, string parentReference)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(endNode);
        ArgumentNullException.ThrowIfNull(previousNode);
        ArgumentException.ThrowIfNullOrWhiteSpace(parentReference);
        var lastNode = new ForTaskNodeViewModel(task);
        if (taskNodeGroup == null) await graph.AddElementAsync(lastNode);
        else await taskNodeGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(graph, previousNode, lastNode);
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
    protected virtual async Task<NodeViewModel> BuildListenTaskNodeAsync(WorkflowDefinition workflow, GraphViewModel graph, MapEntry<string, ListenTaskDefinition> task, TaskNodeViewModel? taskNodeGroup, NodeViewModel endNode, NodeViewModel previousNode, string parentReference)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(endNode);
        ArgumentNullException.ThrowIfNull(previousNode);
        ArgumentException.ThrowIfNullOrWhiteSpace(parentReference);
        var lastNode = new ListenTaskNodeViewModel(task);
        if (taskNodeGroup == null) await graph.AddElementAsync(lastNode);
        else await taskNodeGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(graph, previousNode, lastNode);
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
    protected virtual async Task<NodeViewModel> BuildRaiseTaskNodeAsync(WorkflowDefinition workflow, GraphViewModel graph, MapEntry<string, RaiseTaskDefinition> task, TaskNodeViewModel? taskNodeGroup, NodeViewModel endNode, NodeViewModel previousNode, string parentReference)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(endNode);
        ArgumentNullException.ThrowIfNull(previousNode);
        ArgumentException.ThrowIfNullOrWhiteSpace(parentReference);
        var lastNode = new RaiseTaskNodeViewModel(task);
        if (taskNodeGroup == null) await graph.AddElementAsync(lastNode);
        else await taskNodeGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(graph, previousNode, lastNode);
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
    protected virtual async Task<NodeViewModel> BuildRunTaskNodeAsync(WorkflowDefinition workflow, GraphViewModel graph, MapEntry<string, RunTaskDefinition> task, TaskNodeViewModel? taskNodeGroup, NodeViewModel endNode, NodeViewModel previousNode, string parentReference)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(endNode);
        ArgumentNullException.ThrowIfNull(previousNode);
        ArgumentException.ThrowIfNullOrWhiteSpace(parentReference);
        var lastNode = new RunTaskNodeViewModel(task);
        if (taskNodeGroup == null) await graph.AddElementAsync(lastNode);
        else await taskNodeGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(graph, previousNode, lastNode);
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
    protected virtual async Task<NodeViewModel> BuildSetTaskNodeAsync(WorkflowDefinition workflow, GraphViewModel graph, MapEntry<string, SetTaskDefinition> task, TaskNodeViewModel? taskNodeGroup, NodeViewModel endNode, NodeViewModel previousNode, string parentReference)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(endNode);
        ArgumentNullException.ThrowIfNull(previousNode);
        ArgumentException.ThrowIfNullOrWhiteSpace(parentReference);
        var lastNode = new SetTaskNodeViewModel(task);
        if (taskNodeGroup == null) await graph.AddElementAsync(lastNode);
        else await taskNodeGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(graph, previousNode, lastNode);
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
    protected virtual async Task<NodeViewModel> BuildSwitchTaskNodeAsync(WorkflowDefinition workflow, GraphViewModel graph, MapEntry<string, SwitchTaskDefinition> task, TaskNodeViewModel? taskNodeGroup, NodeViewModel endNode, NodeViewModel previousNode, string parentReference)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(endNode);
        ArgumentNullException.ThrowIfNull(previousNode);
        ArgumentException.ThrowIfNullOrWhiteSpace(parentReference);
        var lastNode = new SwitchTaskNodeViewModel(task);
        if (taskNodeGroup == null) await graph.AddElementAsync(lastNode);
        else await taskNodeGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(graph, previousNode, lastNode);
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
    protected virtual async Task<NodeViewModel> BuildTryTaskNodeAsync(WorkflowDefinition workflow, GraphViewModel graph, MapEntry<string, TryTaskDefinition> task, TaskNodeViewModel? taskNodeGroup, NodeViewModel endNode, NodeViewModel previousNode, string parentReference)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(endNode);
        ArgumentNullException.ThrowIfNull(previousNode);
        ArgumentException.ThrowIfNullOrWhiteSpace(parentReference);
        var lastNode = new TryTaskNodeViewModel(task);
        if (taskNodeGroup == null) await graph.AddElementAsync(lastNode);
        else await taskNodeGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(graph, previousNode, lastNode);
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
    protected virtual async Task<NodeViewModel> BuildWaitTaskNodeAsync(WorkflowDefinition workflow, GraphViewModel graph, MapEntry<string, WaitTaskDefinition> task, TaskNodeViewModel? taskNodeGroup, NodeViewModel endNode, NodeViewModel previousNode, string parentReference)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(endNode);
        ArgumentNullException.ThrowIfNull(previousNode);
        ArgumentException.ThrowIfNullOrWhiteSpace(parentReference);
        var lastNode = new WaitTaskNodeViewModel(task);
        if (taskNodeGroup == null) await graph.AddElementAsync(lastNode);
        else await taskNodeGroup.AddChildAsync(lastNode);
        await this.BuildEdgeAsync(graph, previousNode, lastNode);
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

}
