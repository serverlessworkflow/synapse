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

global using BlazorBootstrap;
global using BlazorMonaco.Editor;
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Web;
global using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
global using Microsoft.JSInterop;
global using Neuroglia;
global using Neuroglia.Data.Infrastructure.ResourceOriented;
global using Neuroglia.Reactive;
global using Neuroglia.Serialization;
global using Synapse.Api.Client;
global using Synapse.Dashboard;
global using Synapse.Dashboard.Components;
global using Synapse.Dashboard.Components.ResourceManagement;
global using Synapse.Dashboard.Services;
global using Synapse.Dashboard.StateManagement;
global using System.Reactive.Linq;
