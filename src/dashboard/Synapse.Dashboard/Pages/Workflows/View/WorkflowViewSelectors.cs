/*
 * Copyright © 2022-Present The Synapse Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Neuroglia.Data.Flux;
using Synapse.Dashboard.Pages.Workflows.View.State;
using Synapse.Integration.Models;
using System.Reactive.Linq;

namespace Synapse.Dashboard.Pages.Workflows.View
{
    /// <summary>
    /// Holds the workflow view selectors
    /// </summary>
    public static class WorkflowViewSelectors
    {
        /// <summary>
        /// Selects the workflow
        /// </summary>
        /// <param name="store">The global <see cref="IStore"/></param>
        /// <returns></returns>
        public static IObservable<V1Workflow?> SelectWorkflow(IStore store)
        {
            return store.GetFeature<WorkflowViewState>()
                .Select(featureState => featureState.Workflow)
                .DistinctUntilChanged();
        }

        /// <summary>
        /// Selects the workflow instances
        /// </summary>
        /// <param name="store">The global <see cref="IStore"/></param>
        /// <returns></returns>
        public static IObservable<Dictionary<string, V1WorkflowInstance>?> SelectWorkflowInstances(IStore store)
        {
            return store.GetFeature<WorkflowViewState>()
                .Select(featureState => featureState.Instances)
                .DistinctUntilChanged();
        }

        /// <summary>
        /// Selects the displayed <see cref="V1WorkflowInstance"/>
        /// </summary>
        /// <param name="store">The global <see cref="IStore"/></param>
        /// <returns></returns>
        public static IObservable<V1WorkflowInstance?> SelectActiveInstance(IStore store)
        {
            return store.GetFeature<WorkflowViewState>()
                .Select(featureState => featureState.ActiveInstance)
                .DistinctUntilChanged();
        }

        /// <summary>
        /// Selects the activities to be displayed
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public static IObservable<ICollection<V1WorkflowActivity>> SelectActivities(IStore store)
        {
            return store.GetFeature<WorkflowViewState>()
                .Select(featureState => 
                    featureState.ActiveInstance == null ? 
                        featureState.Activities?.Values : 
                        featureState.Activities?.Values.Where(activity => activity.WorkflowInstanceId == featureState.ActiveInstance.Id)
                )
                .Select(activities => activities?.OrderBy(activity => activity.CreatedAt).ToList() ?? new List<V1WorkflowActivity>())
                .DistinctUntilChanged();
        }
    }
}
