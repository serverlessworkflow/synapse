using System;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Runner.Application
{

    /// <summary>
    /// Represents an object used to lock aynschronous processes
    /// </summary>
    /// <remarks>Code based on <see href="https://medium.com/swlh/async-lock-mechanism-on-asynchronous-programing-d43f15ad0b3"/></remarks>
    public class AsyncLock
    {

        private readonly SemaphoreSlim _Semaphore = new SemaphoreSlim(1, 1);
        private readonly Task<IDisposable> _Releaser;

        /// <summary>
        /// Initializes a new <see cref="AsyncLock"/>
        /// </summary>
        public AsyncLock()
        {
            _Releaser = Task.FromResult((IDisposable)new Releaser(this));
        }

        /// <summary>
        /// Locks asynchronously
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new object which releases the lock upon disposal</returns>
        public Task<IDisposable> LockAsync(CancellationToken cancellationToken = default)
        {
            Task waitTask = _Semaphore.WaitAsync(cancellationToken);
            return waitTask.IsCompleted ?
                        _Releaser :
                        waitTask.ContinueWith((_, state) => (IDisposable)state,
                            _Releaser.Result, cancellationToken,
            TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private sealed class Releaser 
            : IDisposable
        {

            private readonly AsyncLock _ToRelease;

            internal Releaser(AsyncLock toRelease) 
            { 
                _ToRelease = toRelease; 
            }

            public void Dispose() 
            { 
                _ToRelease._Semaphore.Release(); 
            }

        }

    }

}
