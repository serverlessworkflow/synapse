// Copyright © 2024-Present Neuroglia SRL. All rights reserved.
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

global using Docker.DotNet;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Neuroglia;
global using Neuroglia.Data;
global using Neuroglia.Data.Infrastructure.ResourceOriented;
global using Neuroglia.Data.Infrastructure.ResourceOriented.Configuration;
global using Neuroglia.Data.Infrastructure.ResourceOriented.Services;
global using Neuroglia.Security.Services;
global using ServerlessWorkflow.Sdk.Models;
global using Synapse;
global using Synapse.Core.Infrastructure.Containers;
global using Synapse.Core.Infrastructure.Services;
global using Synapse.Operator.Configuration;
global using Synapse.Operator.Services;
global using Synapse.Resources;
global using Synapse.Runtime.Services;
global using System.Collections.Concurrent;
global using System.Diagnostics;
