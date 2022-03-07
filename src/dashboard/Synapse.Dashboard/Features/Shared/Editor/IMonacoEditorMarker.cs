namespace Synapse.Dashboard
{

    /// <summary>
    /// See https://microsoft.github.io/monaco-editor/api/interfaces/monaco.editor.IMarker.html
    /// </summary>
    public interface IMonacoEditorMarker
    {
        public string? Code { get; set; }
        public int EndColumn { get; set; }
        public int EndLineNumber { get; set; }
        public string Message { get; set; }
        public string Owner { get; set; }
        public IEnumerable<Object>? RelatedInformation { get; set; }
        public Object Resource { get; set; }
        public int Severity { get; set; }
        public int StartColumn  { get; set; }
        public int StartLineNumber { get; set; }
        public IEnumerable<Object>? Tags { get; set; }
    }
}
