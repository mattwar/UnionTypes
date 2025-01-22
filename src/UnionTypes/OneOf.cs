using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

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
    static abstract TSelf Construct(OneOfCore<TSelf> core);
}

internal struct OneOfCore<TOneOf>
    where TOneOf: IOneOf<TOneOf>
{
    private readonly int _kind;
    private readonly object _value;

    public OneOfCore(int kind, object value)
    {
        _kind = kind;
        _value = value;
    }

    public int Kind => _kind;
    public object Value => _value;

    /// <summary>
    /// The type from the union's Types collection that the current value corresponds to.
    /// </summary>
    public Type Type
    {
        get
        {
            var index = this.Kind - 1;
            if (index >= 0 && index < TOneOf.Types.Count)
                return TOneOf.Types[index];
            return typeof(object);
        }
    }

    public bool CanGet<T>()
    {
        return _value is T
            || TypeUnion.CanCreate(_value, typeof(T));
    }

    public bool TryGet<T>([NotNullWhen(true)] out T value)
    {
        if (_value is T t)
        {
            value = t;
            return true;
        }
        else
        {
            return TypeUnion.TryCreate(_value, out value);
        }
    }

    public T Get<T>()
    {
        if (TryGet<T>(out var t))
        {
            return t;
        }

        throw new InvalidCastException();
    }

    public T GetOrDefault<T>() =>
        TryGet<T>(out var value) ? value : default!;

    public static bool CanCreateFrom<TValue>(TValue value)
    {
        var index = TypeUnion.GetTypeIndex<TOneOf, TValue>(value);
        return index >= 0 && index < TOneOf.Types.Count;
    }

    public static bool TryCreateFrom<TValue>(TValue value, [NotNullWhen(true)] out TOneOf union)
    {
        // if value is a union, get its underlying value.
        if (!(value is ITypeUnion u && u.TryGet<object>(out var underlyingValue)))
        {
            underlyingValue = value;
        }

        var typeIndex = TypeUnion.GetTypeIndex<TOneOf, object>(underlyingValue!);
        if (typeIndex > 0)
        {
            union = TOneOf.Construct(new OneOfCore<TOneOf>(typeIndex, underlyingValue!));
            return true;
        }

        union = default!;
        return false;
    }

    public static TOneOf CreateFrom<TValue>(TValue value)
    {
        if (TryCreateFrom(value, out var union))
            return union;
        throw new InvalidCastException();
    }

    public override string ToString() => _value?.ToString() ?? string.Empty;
}