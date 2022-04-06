namespace Synapse
{

    /// <summary>
    /// Represents an exception thrown whenever an operation returned a non-success <see cref="IOperationResult"/>
    /// </summary>
    public class OperationResultException
        : Exception
    {

        /// <summary>
        /// Initializes a new <see cref="OperationResultException"/>
        /// </summary>
        /// <param name="result">The <see cref="IOperationResult"/> which is the cause of the <see cref="OperationResultException"/></param>
        public OperationResultException(IOperationResult result)
            : base($"The operation was executed with a non-success result code '{result.Code}'.{Environment.NewLine}Errors: {(result.Errors == null ? "" : string.Join(Environment.NewLine, result.Errors.Select(e => $"{e.Code}: {e.Message}")))}")
        {

        }

    }

}
