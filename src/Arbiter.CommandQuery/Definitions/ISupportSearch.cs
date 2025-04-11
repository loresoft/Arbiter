namespace Arbiter.CommandQuery.Definitions;

/// <summary>
/// An <see langword="interface"/> indicating the implemented type supports search
/// </summary>
public interface ISupportSearch
{
    /// <summary>
    /// Gets a list of search the fields.
    /// </summary>
    /// <returns>A list of search fields</returns>
    static abstract IEnumerable<string> SearchFields();

    /// <summary>
    /// Gets the default sort the field.
    /// </summary>
    /// <returns>The default sort field</returns>
    static abstract string SortField();
}
