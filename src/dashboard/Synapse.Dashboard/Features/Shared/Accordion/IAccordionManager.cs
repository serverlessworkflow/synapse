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
