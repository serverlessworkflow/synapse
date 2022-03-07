namespace Synapse.Dashboard
{
    /// <summary>
    /// Manage the state of accordions components
    /// </summary>
    public interface IAccordionManager
    {
        /// <summary>
        /// Register an accordion to be interacted with
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Task Register(IAccordionModel model);
        /// <summary>
        /// Deregister an accordion for observed accordions
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Task Deregister(IAccordionModel model);
        /// <summary>
        /// Opens the targeted accordion
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Task Open(IAccordionModel model);
        /// <summary>
        /// Closes the targeted accordion
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Task Close(IAccordionModel model);
        /// <summary>
        /// Toggles the targeted accordion
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Task Toggle(IAccordionModel model);
    }
}
