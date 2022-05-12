namespace Synapse.Dashboard.Services
{
    public interface IClonerService
    {
        T Clone<T>(T obj);
        Task<T> CloneAsync<T>(T obj);
    }
}
