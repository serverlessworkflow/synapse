using BlazorMonaco;

namespace Synapse.Dashboard
{
    public class MonacoEditorHelper
        : IMonacoEditorHelper
    {
        public Func<MonacoEditor, StandaloneEditorConstructionOptions> GetStandaloneEditorConstructionOptions(string value = "", bool readOnly = false, string language = "json") {
            return (MonacoEditor editor) => new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Minimap = new EditorMinimapOptions { Enabled = false },
                Language = language,
                ReadOnly = readOnly,
                Value = value
            };
        }


        public Func<MonacoDiffEditor, DiffEditorConstructionOptions> GetDiffEditorConstructionOptions(bool readOnly = true)
        {
            return (MonacoDiffEditor editor) => new DiffEditorConstructionOptions
            {
                AutomaticLayout = true,
                Minimap = new EditorMinimapOptions { Enabled = false },
                ReadOnly = readOnly
            };
        }
    }
}
