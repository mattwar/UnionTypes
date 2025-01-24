using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace UnionTypes;

public interface IOneOf : IClosedTypeUnion
{
    /// <summary>
    /// The value held by the union.
    /// </summary>
    object Value { get; }
}

internal interface IOneOf<TSelf> : IOneOf, IClosedTypeUnion<TSelf>
    where TSelf : IOneOf<TSelf>
{
    static abstract TSelf Construct(int kind, object value);
}

internal static class OneOf
{
    public static bool TryCreate<TOneOf, TValue>(TValue value, [NotNullWhen(true)] out TOneOf union)
        where TOneOf : IOneOf<TOneOf>
    {
        // if value is a union, get its underlying value.
        if (!(value is ITypeUnion u && u.TryGet<object>(out var underlyingValue)))
        {
            underlyingValue = value;
        }

        // determine which type argument the underlying type would associate with.
        var typeIndex = TypeUnion.GetTypeIndex<TOneOf, object>(underlyingValue!);
        if (typeIndex >= 0)
        {
            union = TOneOf.Construct(typeIndex + 1, underlyingValue!);
            return true;
        }

        union = default!;
        return false;
    }
}