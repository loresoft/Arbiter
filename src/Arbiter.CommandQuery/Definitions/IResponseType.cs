namespace Arbiter.CommandQuery.Definitions;

/// <summary>
/// Defines a contract for types that declare their response type.
/// </summary>
public interface IResponseType
{
    /// <summary>
    /// Gets the type of the response.
    /// </summary>
    abstract Type ResponseType { get; }
}
