namespace Synapse;

/// <summary>
/// Represents an object used to lock asynchronous processes
/// </summary>
/// <remarks>Code based on <see href="https://medium.com/swlh/async-lock-mechanism-on-asynchronous-programing-d43f15ad0b3"/></remarks>
public class AsyncLock
{

    readonly SemaphoreSlim _semaphore = new(1, 2);
    readonly Task<IDisposable> _releaser;

    /// <summary>
    /// Initializes a new <see cref="AsyncLock"/>
    /// </summary>
    public AsyncLock()
    {
        this._releaser = Task.FromResult((IDisposable)new Releaser(this));
    }

    /// <summary>
    /// Locks asynchronously
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new object which releases the lock upon disposal</returns>
    public Task<IDisposable> LockAsync(CancellationToken cancellationToken = default)
    {
        var waitTask = this._semaphore.WaitAsync(cancellationToken);
        return waitTask.IsCompleted
            ? this._releaser
            : waitTask.ContinueWith((_, state) => (IDisposable)state!, this._releaser.Result, cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }

    class Releaser(AsyncLock toRelease)
        : IDisposable
    {

        public void Dispose() => toRelease._semaphore.Release();

    }

}
