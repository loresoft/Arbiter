namespace Arbiter;

/// <summary>
/// Represents a void type, since <see cref="System.Void"/> is not a valid return type in C#.
/// </summary>
public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>, IComparable
{
    private static readonly Unit _value = new();

    /// <summary>
    /// Default and only value of the <see cref="Unit"/> type.
    /// </summary>
    public static ref readonly Unit Value => ref _value;

    /// <summary>
    /// Task from a <see cref="Unit"/> type.
    /// </summary>
    public static ValueTask<Unit> Task { get; } = ValueTask.FromResult(_value);

    /// <inheritdoc />
    public int CompareTo(Unit other) => 0;

    /// <inheritdoc />
    int IComparable.CompareTo(object? obj) => 0;

    /// <inheritdoc />
    public override int GetHashCode() => 0;

    /// <inheritdoc />
    public bool Equals(Unit other) => true;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Unit;

    /// <inheritdoc />
    public static bool operator ==(Unit first, Unit second) => true;

    /// <inheritdoc />
    public static bool operator !=(Unit first, Unit second) => false;

    /// <inheritdoc />
    public static bool operator <(Unit left, Unit right) => left.CompareTo(right) < 0;

    /// <inheritdoc />
    public static bool operator <=(Unit left, Unit right) => left.CompareTo(right) <= 0;

    /// <inheritdoc />
    public static bool operator >(Unit left, Unit right) => left.CompareTo(right) > 0;

    /// <inheritdoc />
    public static bool operator >=(Unit left, Unit right) => left.CompareTo(right) >= 0;

    /// <inheritdoc />
    public override string ToString() => "()";
}
