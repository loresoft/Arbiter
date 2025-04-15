namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// A set of operators for filtering entities.
/// </summary>
public static class EntityFilterOperators
{
    /// <summary>
    /// The operator for filtering entities that start with a specific value.
    /// </summary>
    public const string StartsWith = "StartsWith";
    /// <summary>
    /// The operator for filtering entities that end with a specific value.
    /// </summary>
    public const string EndsWith = "EndsWith";
    /// <summary>
    /// The operator for filtering entities that contain a specific value.
    /// </summary>
    public const string Contains = "Contains";
    /// <summary>
    /// The operator for filtering entities that equal specific value.
    /// </summary>
    public const string Equal = "eq";
    /// <summary>
    /// The operator for filtering entities that do not equal specific value.
    /// </summary>
    public const string NotEqual = "neq";
    /// <summary>
    /// The operator for filtering entities that are less than a specific value.
    /// </summary>
    public const string LessThan = "lt";
    /// <summary>
    /// The operator for filtering entities that are less than or equal to a specific value.
    /// </summary>
    public const string LessThanOrEqual = "lte";
    /// <summary>
    /// The operator for filtering entities that are greater than a specific value.
    /// </summary>
    public const string GreaterThan = "gt";
    /// <summary>
    /// The operator for filtering entities that are greater than or equal to a specific value.
    /// </summary>
    public const string GreaterThanOrEqual = "gte";
    /// <summary>
    /// The operator for filtering entities that are in a specific set of values.
    /// </summary>
    public const string In = "in";
    /// <summary>
    /// The operator for filtering entities that are null.
    /// </summary>
    public const string IsNull = "IsNull";
    /// <summary>
    /// The operator for filtering entities that are not null.
    /// </summary>
    public const string IsNotNull = "IsNotNull";
    /// <summary>
    /// The operator for filtering entities that match the raw expression.
    /// </summary>
    public const string Expression = "Expression";
}
