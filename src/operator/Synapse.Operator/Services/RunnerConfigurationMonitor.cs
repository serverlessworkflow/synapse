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

namespace Synapse.Operator.Services;

/// <summary>
/// Represents the service used to monitor the operator's <see cref="RunnerConfiguration"/>.
/// </summary>
/// <param name="application">The service used to monitor the current operator resource</param>
internal sealed class RunnerConfigurationMonitor(OperatorApplication application)
    : IHostedService, IOptionsMonitor<RunnerConfiguration>
{

    ConcurrentHashSet<RunnerConfigurationChangeTokenSource> _changeTokenSources = [];
    IDisposable? _subscription;
    IResourceMonitor<Resources.Operator> _operator = null!;

    /// <inheritdoc/>
    public RunnerConfiguration CurrentValue => _operator.Resource.Spec.Runner;

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _operator = application.OperatorController.Operator;
        _operator.Where(e => e.Type == ResourceWatchEventType.Updated).Select(e => e.Resource.Spec.Runner).Distinct().Subscribe(OnConfigurationChanged);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _subscription?.Dispose();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public RunnerConfiguration Get(string? name) => CurrentValue;

    void OnConfigurationChanged(RunnerConfiguration configuration)
    {
        foreach (var changeTokenSource in _changeTokenSources) changeTokenSource.Invoke(configuration, null);
    }

    IDisposable? IOptionsMonitor<RunnerConfiguration>.OnChange(Action<RunnerConfiguration, string?> listener)
    {
        var changeTokenSource = new RunnerConfigurationChangeTokenSource(listener);
        changeTokenSource.OnDisposed += OnChangeTokenSourceDisposed;
        _changeTokenSources.Add(changeTokenSource);
        return changeTokenSource;
    }

    void OnChangeTokenSourceDisposed(object? sender, EventArgs e)
    {
        if (sender is not RunnerConfigurationChangeTokenSource changeTokenSource) return;
        changeTokenSource.OnDisposed -= OnChangeTokenSourceDisposed;
        _changeTokenSources.Remove(changeTokenSource);
    }

    class RunnerConfigurationChangeTokenSource(Action<RunnerConfiguration, string?> listener)
        : IDisposable
    {

        public event EventHandler? OnDisposed;

        public void Invoke(RunnerConfiguration configuration, string? name) => listener(configuration, name);

        public void Dispose()
        {
            OnDisposed?.Invoke(this, EventArgs.Empty);
            GC.SuppressFinalize(this);
        }

    }

}