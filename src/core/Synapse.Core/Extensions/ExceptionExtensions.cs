using System.Net;

namespace Synapse;

/// <summary>
/// Defines extensions for <see cref="Exception"/>s
/// </summary>
public static class ExceptionExtensions
{

    /// <summary>
    /// Converts the <see cref="Exception"/> into a new <see cref="Error"/>
    /// </summary>
    /// <param name="ex">The <see cref="Exception"/> to convert</param>
    /// <param name="instance">The <see cref="Uri"/>, if any, that references the instance to which the <see cref="Error"/> to create applies</param>
    /// <returns>A new <see cref="Error"/> based on the specified <see cref="Exception"/></returns>
    public static Error ToError(this Exception ex, Uri? instance = null) 
    {
        return ex switch
        {
            HttpRequestException httpEx => new()
            {
                Status = (ushort)(httpEx.StatusCode ?? HttpStatusCode.InternalServerError),
                Type = ErrorType.Communication,
                Title = ErrorTitle.Communication,
                Instance = instance,
                Detail = httpEx.Message
            },
            _ => new()
            {
                Status = ErrorStatus.Runtime,
                Type = ErrorType.Runtime,
                Title = ErrorTitle.Runtime,
                Instance = instance,
                Detail = ex.Message
            }
        };
    }

}
