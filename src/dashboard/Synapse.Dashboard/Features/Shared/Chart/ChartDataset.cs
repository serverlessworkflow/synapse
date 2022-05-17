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

namespace Synapse.Dashboard
{

    /// <summary>
    /// Represents a <see cref="Chart"/>'s dataset
    /// </summary>
    public class ChartDataset
    {

        /// <summary>
        /// Gets/sets the <see cref="ChartDataset"/>'s <see cref="ChartType"/>, in case of mixed charts
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
        public virtual ChartType? Type { get; set; }

        /// <summary>
        /// Gets/sets the <see cref="ChartDataset"/>'s label
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public virtual string? Label { get; set; }

        /// <summary>
        /// Gets/sets a <see cref="List{T}"/> containing the data the <see cref="ChartDataset"/> is made out of
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public virtual List<object> Data { get; set; } = new();

        /// <summary>
        /// Gets/sets a <see cref="List{T}"/> containing the background colors to use when rendering the <see cref="ChartDataset"/>
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public virtual List<string> BackgroundColor { get; set; } = new();

        /// <summary>
        /// Gets/sets the border color to use when rendering the <see cref="ChartDataset"/>
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public virtual string? BorderColor { get; set; }

        /// <summary>
        /// Gets/sets the border width to use when rendering the <see cref="ChartDataset"/>
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public virtual double? BorderWidth { get; set; }

    }

}
