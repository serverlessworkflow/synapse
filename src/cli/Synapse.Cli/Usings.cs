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

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Neuroglia.Serialization;
global using ServerlessWorkflow.Sdk.IO;
global using Spectre.Console;
global using Synapse.Api.Client;
global using Synapse.Api.Client.Services;
global using Synapse.Cli;
global using Synapse.Resources;
global using System.CommandLine;
global using System.CommandLine.Builder;
global using System.CommandLine.NamingConventionBinder;
global using System.CommandLine.Parsing;
global using System.ComponentModel.DataAnnotations;
global using System.Runtime.Serialization;
global using System.Text.Json.Serialization;
global using YamlDotNet.Serialization;