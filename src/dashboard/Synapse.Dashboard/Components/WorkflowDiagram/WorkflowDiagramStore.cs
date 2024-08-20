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
using ServerlessWorkflow.Sdk.Models;
using Synapse.Resources;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Synapse.Dashboard.Components.WorkflowDiagramStateManagement;

/// <summary>
/// Represents the <see cref="ComponentStore{TState}" /> of a <see cref="WorkflowDiagram"/> component
/// </summary>
/// <param name="workflowGraphBuilder">The service used build the workflow graph</param>
public class WorkflowDiagramStore(
    IWorkflowGraphBuilder workflowGraphBuilder
)
    : ComponentStore<WorkflowDiagramState>(new())
{

    /// <summary>
    /// Gets the service used build the workflow graph
    /// </summary>
    protected IWorkflowGraphBuilder WorkflowGraphBuilder { get; } = workflowGraphBuilder;

    /// <summary>
    /// Gets/sets a reference to the <see cref="Modal"/> component used to display the legend
    /// </summary>
    public Modal? LegendModal { get; set; }

    /// <summary>
    /// Gets/set a refrence to the <see cref="DagreGraph"/> component
    /// </summary>
    public DagreGraph? DagreGraph;

    #region Selectors
    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="WorkflowDiagramState.WorkflowDefinition"/> changes
    /// </summary>
    public IObservable<WorkflowDefinition?> WorkflowDefinition => this.Select(state => state.WorkflowDefinition).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="WorkflowDiagramState.Orientation"/> changes
    /// </summary>
    public IObservable<WorkflowDiagramOrientation> Orientation => this.Select(state => state.Orientation).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="WorkflowDiagramState.WorkflowInstances"/> changes
    /// </summary>
    public IObservable<EquatableList<WorkflowInstance>> WorkflowInstances => this.Select(state => state.WorkflowInstances).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="DagreGraphOptions"/> changes
    /// </summary>
    public IObservable<DagreGraphOptions> Options => this.Orientation
        .Select(orientation => new DagreGraphOptions()
        {
            Direction = orientation == WorkflowDiagramOrientation.LeftToRight ? DagreGraphDirection.LeftToRight : DagreGraphDirection.TopToBottom
        })
        .DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="IGraphViewModel"/> changes
    /// </summary>
    public IObservable<IGraphViewModel> RawGraph => this.WorkflowDefinition
        .Where(workflowDefinition => workflowDefinition != null)
        .Select((workflowDefinition) => this.WorkflowGraphBuilder.Build(workflowDefinition!))
        .DistinctUntilChanged()
        .Publish()
        .RefCount()
        ;

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="IGraphViewModel"/>, with its activities, changes 
    /// </summary>
    public IObservable<IGraphViewModel?> Graph => Observable.CombineLatest(
        this.RawGraph,
        this.WorkflowInstances,
        (graph, instances) =>
        {
            var tasks = instances.SelectMany(instance => instance.Status?.Tasks ?? []);
            //var allNodes =  ((IReadOnlyDictionary<string, IWorkflowNodeViewModel>)graph.AllNodes).Concat((IReadOnlyDictionary<string, IWorkflowNodeViewModel>)graph.AllClusters);
            foreach (var kvp in graph.AllNodes)
            {
                int activeCount, faultedCount;
                if (kvp.Key == "start-node")
                {
                    activeCount = instances.Count(instance => instance.Status == null || instance.Status?.Phase == WorkflowInstanceStatusPhase.Pending || instance.Status?.Phase == WorkflowInstanceStatusPhase.Waiting);
                    faultedCount = instances.Count(instance => (instance.Status?.Tasks == null || instance.Status.Tasks.Count == 0) && (instance.Status?.Phase == WorkflowInstanceStatusPhase.Cancelled || instance.Status?.Phase == WorkflowInstanceStatusPhase.Faulted));
                }
                else if (kvp.Key == "end-node")
                {
                    activeCount = instances.Count(instance => instance.Status?.Phase == WorkflowInstanceStatusPhase.Completed);
                    faultedCount = instances.Count(instance => instance.Status?.Tasks?.Count > 0 && (instance.Status?.Phase == WorkflowInstanceStatusPhase.Cancelled || instance.Status?.Phase == WorkflowInstanceStatusPhase.Faulted));
                }
                else
                {
                    var nodeTasks = tasks.Where(task => Regex.Replace(task.Reference.ToString(), @"/for/\d*", "") == kvp.Key);
                    activeCount = nodeTasks.Count(task => task.Status == TaskInstanceStatus.Running);
                    faultedCount = nodeTasks.Count(task => task.Status == TaskInstanceStatus.Faulted || task.Status == TaskInstanceStatus.Cancelled);
                }
                ((IWorkflowNodeViewModel)kvp.Value).OperativeInstancesCount = activeCount;
                ((IWorkflowNodeViewModel)kvp.Value).FaultedInstancesCount = faultedCount;
            }
            foreach (var kvp in graph.AllClusters)
            {
                int activeCount, faultedCount;
                if (kvp.Key == "start-node")
                {
                    activeCount = instances.Count(instance => instance.Status?.Phase == WorkflowInstanceStatusPhase.Pending || instance.Status?.Phase == WorkflowInstanceStatusPhase.Waiting);
                    faultedCount = instances.Count(instance => (instance.Status?.Tasks == null || instance.Status.Tasks.Count == 0) && (instance.Status?.Phase == WorkflowInstanceStatusPhase.Cancelled || instance.Status?.Phase == WorkflowInstanceStatusPhase.Faulted));
                }
                else if (kvp.Key == "end-node")
                {
                    activeCount = instances.Count(instance => instance.Status?.Phase == WorkflowInstanceStatusPhase.Completed);
                    faultedCount = instances.Count(instance => instance.Status?.Tasks?.Count > 0 && (instance.Status?.Phase == WorkflowInstanceStatusPhase.Cancelled || instance.Status?.Phase == WorkflowInstanceStatusPhase.Faulted));
                }
                else
                {
                    var nodeTasks = tasks.Where(task => Regex.Replace(task.Reference.ToString(), @"/for/\d*", "") == kvp.Key);
                    activeCount = nodeTasks.Count(task => task.Status == TaskInstanceStatus.Running);
                    faultedCount = nodeTasks.Count(task => task.Status == TaskInstanceStatus.Faulted || task.Status == TaskInstanceStatus.Cancelled);
                }
                ((IWorkflowNodeViewModel)kvp.Value).OperativeInstancesCount = activeCount;
                ((IWorkflowNodeViewModel)kvp.Value).FaultedInstancesCount = faultedCount;
            }
            return graph;
        });
    #endregion

    #region Setters
    /// <summary>
    /// Sets the state <see cref="WorkflowDiagramState.WorkflowDefinition"/>'s value
    /// </summary>
    /// <param name="workflowDefinition">The new value</param>
    public void SetWorkflowDefinition(WorkflowDefinition? workflowDefinition)
    {
        this.Reduce(state => state with
        {
            WorkflowDefinition = workflowDefinition
        });
    }

    /// <summary>
    /// Sets the state <see cref="WorkflowDiagramState.Orientation"/>'s value
    /// </summary>
    /// <param name="orientation">The new value</param>
    public void SetOrientation(WorkflowDiagramOrientation orientation)
    {
        this.Reduce(state => state with
        {
            Orientation = orientation
        });
    }

    /// <summary>
    /// Sets the state <see cref="WorkflowDiagramState.WorkflowInstances"/>'s value
    /// </summary>
    /// <param name="workflowInstances">The new value</param>
    public void SetWorkflowInstances(EquatableList<WorkflowInstance> workflowInstances)
    {
        this.Reduce(state => state with
        {
            WorkflowInstances = workflowInstances
        });
    }
    #endregion

    #region Actions
    /// <summary>
    /// Shows the legend modal
    /// </summary>
    /// <returns></returns>
    public async Task ShowLegendAsync()
    {
        if (this.LegendModal != null)
        {
            await this.LegendModal.ShowAsync<WorkflowDiagramLegend>(title: "Legend");
        }
    }
    #endregion
}