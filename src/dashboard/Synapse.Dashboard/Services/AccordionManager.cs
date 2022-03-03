using Synapse.Dashboard.Models;

namespace Synapse.Dashboard.Services
{
    /// <summary>
    /// Manage the state of accordions components
    /// </summary>
    public class AccordionManager
        : IAccordionManager
    {
        protected virtual List<IAccordionModel> Items { get; set; } = new List<IAccordionModel>();

        /// <summary>
        /// Register an accordion to be interacted with
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task Register(IAccordionModel model)
        {
            this.Items.Add(model);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Deregister an accordion for observed accordions
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task Deregister(IAccordionModel model)
        {
            if (this.Items.Contains(model))
            {
                this.Items.Remove(model);
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Opens the targeted accordion
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task Open(IAccordionModel model)
        {
            foreach(var accordion in this.Items.Where(accordion => !accordion.AllowsMultiple))
            {
                await this.Close(accordion);
            }
            model.IsExpanded = true;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Closes the targeted accordion
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task Close(IAccordionModel model)
        {
            model.IsExpanded = false;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Toggles the targeted accordion
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task Toggle(IAccordionModel model)
        {
            Console.WriteLine("Toggling ...");
            if (model.IsExpanded) {
                await this.Close(model);
            }
            else
            {
                await this.Open(model);
            }
            await Task.CompletedTask;
        }
    }
}
