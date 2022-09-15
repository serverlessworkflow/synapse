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
using Synapse.Apis.Management;
using Synapse.Integration.Models;

namespace Synapse.Dashboard
{

    [Feature]
    public class V1CorrelationCollectionState
        : List<V1Correlation>
    {

        public V1CorrelationCollectionState()
        {

        }

        public V1CorrelationCollectionState(IEnumerable<V1Correlation> correlations)
            : base(correlations)
        {

        }

    }

    [Reducer]
    public static class V1CorrelationCollectionReducers
    {

        public static V1CorrelationCollectionState OnSetV1CorrelationCollection(V1CorrelationCollectionState state, SetV1CorrelationCollection action)
        {
            state = new(action.Correlations);
            return state;
        }

        public static V1CorrelationCollectionState OnAddV1Correlation(V1CorrelationCollectionState state, AddV1Correlation action)
        {
            state.Add(action.Correlation);
            return state;
        }

        public static V1CorrelationCollectionState OnAddContextToV1Correlation(V1CorrelationCollectionState state, AddContextToV1Correlation action)
        {
            var correl = state.FirstOrDefault(c => c.Id == action.CorrelationId);
            if (correl == null)
                return state;
            if (correl.Contexts == null)
                correl.Contexts = new List<V1CorrelationContext>();
            correl.Contexts.Add(action.Context);
            return state;
        }

        public static V1CorrelationCollectionState OnRemoveContextFromV1Correlation(V1CorrelationCollectionState state, RemoveContextFromV1Correlation action)
        {
            var correl = state.FirstOrDefault(c => c.Id == action.CorrelationId);
            if (correl == null)
                return state;
            if (correl.Contexts == null)
                correl.Contexts = new List<V1CorrelationContext>();
            var context = correl.Contexts.FirstOrDefault(c => c.Id == action.ContextId);
            if (context != null)
                correl.Contexts.Remove(context);
            return state;
        }

        public static V1CorrelationCollectionState OnCorrelateV1Event(V1CorrelationCollectionState state, CorrelateV1Event action)
        {
            var correl = state.FirstOrDefault(c => c.Id == action.CorrelationId);
            if (correl == null)
                return state;
            if (correl.Contexts == null)
                correl.Contexts = new List<V1CorrelationContext>();
            var context = correl.Contexts.FirstOrDefault(c => c.Id == action.ContextId);
            if (context == null)
                return state;
            if (context.PendingEvents == null)
                context.PendingEvents = new List<V1Event>();
            context.PendingEvents.Add(action.Event);
            return state;
        }

    }

    [Effect]
    public static class V1CorrelationCollectionEffects
    {

        public static async Task OnListV1Correlations(ListV1Correlations action, IEffectContext context)
        {
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            var correlations = await api.GetCorrelationsAsync();
            context.Dispatcher.Dispatch(new SetV1CorrelationCollection(correlations));
        }

        public static async Task OnSearchV1Correlations(SearchV1Correlations action, IEffectContext context)
        {
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            var correlations = await api.GetCorrelationsAsync($"$search={action.Term}");
            context.Dispatcher.Dispatch(new SetV1CorrelationCollection(correlations));
        }

        public static async Task OnGetV1WorkflowById(GetV1CorrelationById action, IEffectContext context)
        {
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            var correlation = await api.GetCorrelationByIdAsync(action.CorrelationId);
            context.Dispatcher.Dispatch(new AddV1Correlation(correlation));
        }

    }

    public class SetV1CorrelationCollection
    {

        public SetV1CorrelationCollection(IEnumerable<V1Correlation> correlations)
        {
            this.Correlations = correlations;
        }

        public IEnumerable<V1Correlation> Correlations { get; }

    }

    public class ListV1Correlations
    {



    }

    public class SearchV1Correlations
    {

        public SearchV1Correlations(string term)
        {
            this.Term = term;
        }

        public string Term { get; }

    }

    public class GetV1CorrelationById
    {

        public GetV1CorrelationById(string correlationId)
        {
            this.CorrelationId = correlationId;
        }

        public string CorrelationId { get; }

    }

    public class AddV1Correlation
    {

        public AddV1Correlation(V1Correlation correlation)
        {
            this.Correlation = correlation;
        }

        public V1Correlation Correlation { get; }

    }

    public class AddContextToV1Correlation
    {

        public AddContextToV1Correlation(string correlationId, V1CorrelationContext context)
        {
            this.CorrelationId = correlationId;
            this.Context = context;
        }

        public string CorrelationId { get; }

        public V1CorrelationContext Context { get; }

    }

    public class RemoveContextFromV1Correlation
    {

        public RemoveContextFromV1Correlation(string correlationId, string contextId)
        {
            this.CorrelationId = correlationId;
            this.ContextId = contextId;
        }

        public string CorrelationId { get; }

        public string ContextId { get; }

    }

    public class CorrelateV1Event
    {

        public CorrelateV1Event(string correlationId, string contextId, V1Event e)
        {
            this.CorrelationId = correlationId;
            this.ContextId = contextId;
            this.Event = e;
        }

        public string CorrelationId { get; }

        public string ContextId { get; }

        public V1Event Event { get; }

    }

}
