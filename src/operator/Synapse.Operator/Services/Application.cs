namespace Synapse.Operator.Services;

internal class Application(IServiceProvider serviceProvider)
    : IHostedService, IDisposable
{

    readonly IServiceScope _scope = serviceProvider.CreateScope();
    IServiceProvider ServiceProvider => this._scope.ServiceProvider;

    OperatorController _operatorController = null!;
    WorkflowController _workflowController = null!;
    WorkflowInstanceController _workflowInstanceController = null!;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        this._operatorController = this.ServiceProvider.GetRequiredService<OperatorController>();
        this._workflowController = this.ServiceProvider.GetRequiredService<WorkflowController>();
        this._workflowInstanceController = this.ServiceProvider.GetRequiredService<WorkflowInstanceController>();
        await this._operatorController.StartAsync(cancellationToken).ConfigureAwait(false);
        await Task.WhenAll(
        [
            this._workflowController.StartAsync(cancellationToken),
            this._workflowInstanceController.StartAsync(cancellationToken)
        ]).ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken) 
    {
        await Task.WhenAll(
        [
            this._operatorController.StopAsync(cancellationToken),
            this._workflowController.StopAsync(cancellationToken),
            this._workflowInstanceController.StopAsync(cancellationToken)
        ]).ConfigureAwait(false);
    }

    void IDisposable.Dispose() => this._scope.Dispose();

}