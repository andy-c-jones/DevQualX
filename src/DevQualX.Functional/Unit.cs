namespace DevQualX.Functional;

/// <summary>
/// Represents a type with only one value, used as a return type for operations that have no meaningful result.
/// This is the functional programming equivalent of void.
/// </summary>
public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>
{
    /// <summary>
    /// The default and only value of the Unit type.
    /// </summary>
    public static readonly Unit Default = new();

    /// <summary>
    /// Determines whether the specified Unit is equal to the current Unit.
    /// Since all Unit instances are equal, this always returns true.
    /// </summary>
    public bool Equals(Unit other) => true;

    /// <summary>
    /// Determines whether the specified object is equal to the current Unit.
    /// </summary>
    public override bool Equals(object? obj) => obj is Unit;

    /// <summary>
    /// Returns the hash code for this Unit.
    /// </summary>
    public override int GetHashCode() => 0;

    /// <summary>
    /// Compares the current Unit with another Unit.
    /// Since all Unit instances are equal, this always returns 0.
    /// </summary>
    public int CompareTo(Unit other) => 0;

    /// <summary>
    /// Returns a string representation of the Unit.
    /// </summary>
    public override string ToString() => "()";

    /// <summary>
    /// Determines whether two Unit instances are equal.
    /// </summary>
    public static bool operator ==(Unit left, Unit right) => true;

    /// <summary>
    /// Determines whether two Unit instances are not equal.
    /// </summary>
    public static bool operator !=(Unit left, Unit right) => false;
}
