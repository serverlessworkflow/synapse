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

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Neuroglia.Data.Expressions.JavaScript;
global using Neuroglia.Data.Expressions.JQ;
global using Neuroglia.Data.Expressions.Services;
global using Neuroglia.Eventing.CloudEvents;
global using Neuroglia.Eventing.CloudEvents.Infrastructure.Services;
global using Neuroglia.Reactive;
global using Neuroglia.Scripting;
global using Neuroglia.Scripting.Services;
global using Neuroglia.Serialization;
global using Neuroglia.Serialization.Json;
global using ServerlessWorkflow.Sdk;
global using ServerlessWorkflow.Sdk.Models;
global using ServerlessWorkflow.Sdk.Models.Calls;
global using ServerlessWorkflow.Sdk.Models.Processes;
global using ServerlessWorkflow.Sdk.Models.Tasks;
global using Synapse.Api.Client;
global using Synapse.Api.Client.Services;
global using Synapse.Core.Infrastructure.Services;
global using Synapse.Resources;
global using Synapse.Runner.Configuration;
global using Synapse.Runner.Services;
global using System.Diagnostics;
global using System.Net;
global using System.Reactive.Linq;
global using System.Reactive.Subjects;
global using System.Text.RegularExpressions;
