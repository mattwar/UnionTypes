namespace UnionTypes.Toolkit;

/// <summary>
/// This type represents the no-value case for the type <see cref="Option{TValue}"/>.
/// You may use it in your own type union for the same purpose.
/// </summary>
public class None
{
    private None() {}
    public static readonly None Singleton = new None();
}

/// <summary>
/// This type can be used to represent the undefined case for any type union
/// without a default case.
/// </summary>
public class Undefined
{
    private Undefined() { }
    public static readonly Undefined Singleton = new Undefined();
}