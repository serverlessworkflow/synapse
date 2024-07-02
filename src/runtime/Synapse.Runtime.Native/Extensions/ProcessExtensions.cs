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

namespace Synapse.Runtime;

/// <summary>
/// Defines extensions for <see cref="Process"/>es
/// </summary>
public static class ProcessExtensions
{

    /// <summary>
    /// Gets a new <see cref="IObservable{T}"/> to observe the <see cref="Process"/>'s Standard Output
    /// </summary>
    /// <param name="process">The <see cref="Process"/> to observe the logs of</param>
    /// <returns>A new <see cref="IObservable{T}"/> used to observe the process's logs</returns>
    public static IObservable<string> GetStandardOutputAsObservable(this Process process) => Observable
        .FromEventPattern<DataReceivedEventHandler, DataReceivedEventArgs>(handler => process.OutputDataReceived += handler, handler => process.OutputDataReceived -= handler)
        .Where(l => !string.IsNullOrWhiteSpace(l?.EventArgs?.Data))
        .Select(l => l.EventArgs.Data!);

    /// <summary>
    /// Gets a new <see cref="IObservable{T}"/> to observe the <see cref="Process"/>'s Standard Error
    /// </summary>
    /// <param name="process">The <see cref="Process"/> to observe the logs of</param>
    /// <returns>A new <see cref="IObservable{T}"/> used to observe the process's logs</returns>
    public static IObservable<string> GetStandardErrorAsObservable(this Process process) => Observable
        .FromEventPattern<DataReceivedEventHandler, DataReceivedEventArgs>(handler => process.ErrorDataReceived += handler, handler => process.ErrorDataReceived -= handler)
        .Where(l => !string.IsNullOrWhiteSpace(l?.EventArgs?.Data))
        .Select(l => l.EventArgs.Data!);

}
