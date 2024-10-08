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

namespace Synapse.Runner;

/// <summary>
/// Exposes all default types of events that can be emitted during a task lifecycle
/// </summary>
public static class TaskLifeCycleEventType
{
    /// <summary>
    /// Indicates that the task has been initialized
    /// </summary>
    public const string Initialized = "initialized";
    /// <summary>
    /// Indicates that the task is running
    /// </summary>
    public const string Running = "running";
    /// <summary>
    /// Indicates that the task has been suspended
    /// </summary>
    public const string Suspended = "suspended";
    /// <summary>
    /// Indicates that the task has been cancelled
    /// </summary>
    public const string Cancelled = "cancelled";
    /// <summary>
    /// Indicates that the task has faulted
    /// </summary>
    public const string Faulted = "faulted";
    /// <summary>
    /// Indicates that the task ran to completion
    /// </summary>
    public const string Completed = "completed";
    /// <summary>
    /// Indicates that the task has been skipped
    /// </summary>
    public const string Skipped = "skipped";

}
