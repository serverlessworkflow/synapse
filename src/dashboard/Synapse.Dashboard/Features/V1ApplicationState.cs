using Neuroglia.Data.Flux;
using Synapse.Apis.Management;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Features
{

    [Feature]
    public class V1ApplicationState
    {

        public V1ApplicationState()
        {

        }

        public V1ApplicationState(V1ApplicationInfo info)
        {
            this.Info = info;
        }

        public V1ApplicationInfo? Info { get; }

    }

    [Reducer]
    public static class V1ApplicationInfoReducers
    {

        public static V1ApplicationState OnSetV1ApplicationInfo(V1ApplicationState state, SetV1ApplicationInfo action)
        {
            return new(action.Info);
        }

    }

    [Effect]
    public static class V1ApplicationInfoEffects
    {

        public static async Task OnGetApplicationInfo(GetV1ApplicationInfo action, IEffectContext context)
        {
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            var info = await api.GetApplicationInfoAsync();
            context.Dispatcher.Dispatch(new SetV1ApplicationInfo(info));
        }

    }

    public class GetV1ApplicationInfo
    {

        

    }

    public class SetV1ApplicationInfo
    {

        public SetV1ApplicationInfo(V1ApplicationInfo info)
        {
            this.Info = info;
        }

        public V1ApplicationInfo Info { get; }

    }

}
