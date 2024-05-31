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

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Synapse.UnitTests.Containers;

public static class MongoContainerBuilder
{

    public const string DefaultUserName = "test";
    public const string DefaultPassword = "test";
    public const int PublicPort = 27017;

    public static IContainer Build()
    {
        return new ContainerBuilder()
            .WithName($"mongo-{Guid.NewGuid():N}")
            .WithImage("mongo:latest")
            .WithEnvironment("MONGO_INITDB_ROOT_USERNAME", DefaultUserName)
            .WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", DefaultPassword)
            .WithPortBinding(PublicPort, true)
            .WithWaitStrategy(Wait
                .ForUnixContainer()
                .UntilPortIsAvailable(PublicPort))
            .Build();
    }

}