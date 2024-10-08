﻿// Copyright © 2024-Present The Synapse Authors
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

namespace Synapse.Dashboard.Components;

/// <summary>
/// Represents a composite task node view model
/// </summary>
public class WaitTaskNodeViewModel
    : WorkflowNodeViewModel
{
    /// <summary>
    /// Initializes a new <see cref="WaitTaskNodeViewModel"/>
    /// </summary>
    /// <param name="taskReference">The node task reference</param>
    /// <param name="name">The node name</param>
    /// <param name="content">The node content</param>
    public WaitTaskNodeViewModel(string taskReference, string name, string content)
        : base(taskReference, new() { Label = name, CssClass = "wait-task-node" })
    {
        Content = content;
        Symbol = "wait-symbol";
        Type = "WAIT";
    }
}
