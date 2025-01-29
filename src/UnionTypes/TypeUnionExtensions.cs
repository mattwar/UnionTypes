using System;

namespace UnionTypes.Toolkit;

public static class TypeUnionExtensions
{
    /// <summary>
    /// Converts this union's value to the target type or union.
    /// </summary>
    public static bool TryConvertTo<TUnion, TValue>(this TUnion union, out TValue value)
        where TUnion : ITypeUnion
    {
        return TypeUnion.TryConvert(union, out value);
    }
}