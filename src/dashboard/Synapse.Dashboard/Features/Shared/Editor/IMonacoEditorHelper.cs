using BlazorMonaco;

namespace Synapse.Dashboard
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMonacoEditorHelper
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="language"></param>
        /// <param name="readOnly"></param>
        /// <returns></returns>
        public Func<MonacoEditor, StandaloneEditorConstructionOptions> GetStandaloneEditorConstructionOptions(string value = "", string language = "json", bool readOnly = false);
    }
}
