namespace Neuroglia.BlazorDagre.Models
{
    public class BaseViewModel
        : IIdentifiable, ILabeled, IMetadata
    {
        public virtual Guid Id { get; set; } = new Guid();
        public virtual string? Label { get; set; }
        public virtual object? Metadata { get; set; }

        protected BaseViewModel() { }

    }
}
