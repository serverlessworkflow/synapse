namespace Synapse;

/// <summary>
/// Enumerates all default operator runtime modes
/// </summary>
public static class OperatorRuntimeMode
{

    /// <summary>
    /// Gets the native operator runtime mode
    /// </summary>
    public const string Native = "native";
    /// <summary>
    /// Gets the containerized operator runtime mode
    /// </summary>
    public const string Containerized = "containerized";

    /// <summary>
    /// Gets a new <see cref="IEnumerable{T}"/> containing all default operator runtime modes
    /// </summary>
    /// <returns>A new <see cref="IEnumerable{T}"/> containing all default operator runtime modes</returns>
    public static IEnumerable<string> AsEnumerable()
    {
        yield return Native;
        yield return Containerized;
    }

}