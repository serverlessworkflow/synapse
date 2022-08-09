/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using ServerlessWorkflow.Sdk.Services.Validation;

namespace Synapse
{
    /// <summary>
    /// Defines extensions for <see cref="IWorkflowValidationResult"/>
    /// </summary>
    public static class IWorkflowValidationResultExtensions
    {

        /// <summary>
        /// Converts the <see cref="IWorkflowValidationResult"/> into a new <see cref="IEnumerable{T}"/> of <see cref="Neuroglia.Error"/>s
        /// </summary>
        /// <param name="validationResult">The <see cref="IWorkflowValidationResult"/> to convert</param>
        /// <returns>A new <see cref="IEnumerable{T}"/> of <see cref="Neuroglia.Error"/>s</returns>
        public static IEnumerable<Error> AsErrors(this IWorkflowValidationResult validationResult)
        {
            List<Error> errors = new(validationResult.SchemaValidationErrors.Count() + validationResult.DslValidationErrors.Count());
            errors.AddRange(validationResult.SchemaValidationErrors.Select(e => new Error(e.Path, e.Message)));
            errors.AddRange(validationResult.DslValidationErrors.Select(e => new Error(e.ErrorCode, e.ErrorMessage)));
            return errors;
        }

    }

}
