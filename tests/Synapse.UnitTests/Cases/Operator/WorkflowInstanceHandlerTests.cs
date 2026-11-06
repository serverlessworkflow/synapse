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

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Neuroglia.Data.Infrastructure.ResourceOriented;
using Neuroglia.Data.Infrastructure.ResourceOriented.Services;
using Neuroglia.Data.Infrastructure.Services;
using Synapse.Operator.Configuration;
using Synapse.Operator.Services;
using Synapse.Runtime.Services;

namespace Synapse.UnitTests.Cases.Operator;

public class WorkflowInstanceHandlerTests
{

    [Fact]
    public async Task Persist_Log_Batch_After_Dispose_Should_Not_Throw()
    {
        //arrange
        var handler = new TestWorkflowInstanceHandler(Mock.Of<ITextDocumentRepository>());
        handler.EnqueueLog("log");
        await handler.DisposeAsync();

        //act
        var persist = () => handler.PersistLogBatchAsync();

        //assert
        await persist.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Persist_Log_Batch_When_Append_Fails_Should_Not_Throw_And_Should_Retry_Next_Batch()
    {
        //arrange
        var logs = new Mock<ITextDocumentRepository>();
        logs.SetupSequence(l => l.AppendAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException())
            .Returns(Task.CompletedTask);
        var handler = new TestWorkflowInstanceHandler(logs.Object);
        handler.EnqueueLog("first");

        //act
        var persist = () => handler.PersistLogBatchAsync();
        await persist.Should().NotThrowAsync();
        handler.EnqueueLog("second");
        await handler.PersistLogBatchAsync();

        //assert
        logs.Verify(l => l.AppendAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    class TestWorkflowInstanceHandler(ITextDocumentRepository logs)
        : WorkflowInstanceHandler(NullLogger<WorkflowInstanceHandler>.Instance, Microsoft.Extensions.Options.Options.Create(new OperatorOptions()), Mock.Of<IResourceRepository>(), logs, Mock.Of<IWorkflowRuntime>(), Mock.Of<IJsonSerializer>(), CreateWorkflowInstanceMonitor())
    {

        public void EnqueueLog(string log) => this.LogBatchQueue.Enqueue(log);

        public Task PersistLogBatchAsync() => this.OnPersistLogBatchAsync();

        static IResourceMonitor<WorkflowInstance> CreateWorkflowInstanceMonitor()
        {
            var monitor = new Mock<IResourceMonitor<WorkflowInstance>>();
            monitor.Setup(m => m.Resource).Returns(new WorkflowInstance(new ResourceMetadata("fake-instance", "default"), new WorkflowInstanceSpec()));
            return monitor.Object;
        }

    }

}
