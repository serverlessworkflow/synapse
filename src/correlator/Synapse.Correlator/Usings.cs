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

global using Json.Patch;
global using Microsoft.AspNetCore.Diagnostics;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.Controllers;
global using Microsoft.Extensions.Options;
global using Microsoft.OpenApi.Models;
global using Neuroglia;
global using Neuroglia.Data;
global using Neuroglia.Data.Expressions;
global using Neuroglia.Data.Expressions.JavaScript;
global using Neuroglia.Data.Expressions.JQ;
global using Neuroglia.Data.Expressions.Services;
global using Neuroglia.Data.Infrastructure.ResourceOriented;
global using Neuroglia.Data.Infrastructure.ResourceOriented.Configuration;
global using Neuroglia.Data.Infrastructure.ResourceOriented.Services;
global using Neuroglia.Eventing.CloudEvents;
global using Neuroglia.Eventing.CloudEvents.AspNetCore;
global using Neuroglia.Eventing.CloudEvents.Infrastructure;
global using Neuroglia.Eventing.CloudEvents.Infrastructure.Services;
global using Neuroglia.Mediation;
global using Neuroglia.Mediation.AspNetCore;
global using Neuroglia.Reactive;
global using Neuroglia.Security.Services;
global using Neuroglia.Serialization;
global using Neuroglia.Serialization.Json;
global using ServerlessWorkflow.Sdk;
global using ServerlessWorkflow.Sdk.Models;
global using Swashbuckle.AspNetCore.SwaggerUI;
global using Synapse;
global using Synapse.Core.Infrastructure.Services;
global using Synapse.Correlator.Commands.CloudEvents;
global using Synapse.Correlator.Configuration;
global using Synapse.Correlator.Services;
global using Synapse.Resources;
global using System.Diagnostics;
global using System.Net;
global using System.Net.Mime;
global using System.Reactive.Linq;
global using System.Runtime.Serialization;
global using System.Text.RegularExpressions;
