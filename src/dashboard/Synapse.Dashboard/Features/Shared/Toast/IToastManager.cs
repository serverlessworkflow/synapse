namespace Synapse.Dashboard
{

    /// <summary>
    /// Defines the fundamentals of a service used to manage toasts
    /// </summary>
    public interface IToastManager
        : IDisposable
    {

        /// <summary>
        /// Represents the event fired whenever a toast has been shown
        /// </summary>
        public event Action OnShowToast;
        /// <summary>
        /// Represents the event fired whenever a toast has been hidden
        /// </summary>
        public event Action OnHideToast;

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> containing 
        /// </summary>
        IEnumerable<Toast> Toasts { get; }

        void ShowToast(Action<IToastBuilder> setup);

    }

}
