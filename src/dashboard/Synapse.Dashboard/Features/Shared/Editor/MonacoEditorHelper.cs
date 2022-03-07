using BlazorMonaco;

namespace Synapse.Dashboard
{
    public class MonacoEditorHelper
        : IMonacoEditorHelper
    {
        public Func<MonacoEditor, StandaloneEditorConstructionOptions> GetStandaloneEditorConstructionOptions(string value = "", string language = "json", bool readOnly = false) {
            return (MonacoEditor editor) => new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Minimap = new EditorMinimapOptions { Enabled = false },
                Language = language,
                ReadOnly = readOnly,
                Value = value
            };
        }
    }
}
