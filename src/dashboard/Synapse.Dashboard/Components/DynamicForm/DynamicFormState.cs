// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Json.Schema;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard.Components.DynamicFormStateManagement;

/// <summary>
/// Represents the <see cref="DynamicForm"/> component's state
/// </summary>
public record DynamicFormState
{

    /// <summary>
    /// Gets/sets the definition of the schema that defines the structure of the form's data to be rendered
    /// </summary>
    public SchemaDefinition? SchemaDefinition { get; set; }

    /// <summary>
    /// Gets/sets the <see cref="JsonSchema"/> that defines the structure of the form's data to be rendered
    /// </summary>
    public JsonSchema? Schema { get; set; }

    /// <summary>
    /// Gets/sets the form's value
    /// </summary>
    public object? Value { get; set; }

}
