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

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Synapse.UnitTests.Services;

public class TestHostEnvironment
    : IHostEnvironment
{

    public string ApplicationName { get; set; } = "unit-tests";

    public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(AppContext.BaseDirectory);

    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

    public string EnvironmentName { get; set; } = "Debug";

}