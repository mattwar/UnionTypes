// 
// 
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using UnionTypes;
#nullable enable

namespace UnionTypes
{
    public partial struct Option<TValue> : IClosedTypeUnion<Option<TValue>>, IEquatable<Option<TValue>>
    {
        public enum Case
        {
            Some = 1,
            None = 0,
        }

        public Case Kind { get; }
        private readonly TValue _field0;

        private Option(Case kind, TValue field0)
        {
            this.Kind = kind;
            _field0 = field0;
        }

        public static Option<TValue> Some(TValue value) => new Option<TValue>(kind: Option<TValue>.Case.Some, field0: value);
        public static Option<TValue> None() => new Option<TValue>(kind: Option<TValue>.Case.None, field0: default!);

        public static implicit operator Option<TValue>(TValue value) => Option<TValue>.Some(value);
        public static implicit operator Option<TValue>(UnionTypes.None value) => Option<TValue>.None();

        /// <summary>Accessible when <see cref="Kind"> is <see cref="Case.Some">.</summary>
        public TValue Value => this.Kind == Option<TValue>.Case.Some ? _field0 : default!;

        #region ITypeUnion, ITypeUnion<TUnion>, ICloseTypeUnion, ICloseTypeUnion<TUnion> implementation.
        public static bool TryCreate<TCreate>(TCreate value, out Option<TValue> union)
        {
            switch (value)
            {
                case TValue v: union = Option<TValue>.Some(v); return true;
                case UnionTypes.None v: union = Option<TValue>.None(); return true;
            }

            if (value is ITypeUnion u && u.TryGet<object>(out var uvalue))
            {
                return TryCreate(uvalue, out union);
            }

            var index = TypeUnion.GetTypeIndex<Option<TValue>, TCreate>(value);
            switch (index)
            {
                case 0 when TypeUnion.TryCreate<TCreate, TValue>(value, out var vSome): union = Option<TValue>.Some(vSome); return true;
                case 1 when TypeUnion.TryCreate<TCreate, UnionTypes.None>(value, out var vNone): union = Option<TValue>.None(); return true;
            }

            union = default!; return false;
        }

        private static IReadOnlyList<Type> _types = new [] {typeof(TValue), typeof(UnionTypes.None)};
        static IReadOnlyList<Type> IClosedTypeUnion<Option<TValue>>.Types => _types;
        private int GetTypeIndex()
        {
            switch (Kind)
            {
                case Option<TValue>.Case.Some: return 0;
                case Option<TValue>.Case.None: return 1;
                default: return -1;
            }
        }
        Type ITypeUnion.Type { get { var index = this.GetTypeIndex(); return index >= 0 && index < _types.Count ? _types[index] : typeof(object); } }

        public bool TryGet<TGet>([NotNullWhen(true)] out TGet value)
        {
            switch (this.Kind)
            {
                case Option<TValue>.Case.Some:
                    if (this.Value is TGet tvSome)
                    {
                        value = tvSome;
                        return true;
                    }
                    return TypeUnion.TryCreate(this.Value, out value);
                case Option<TValue>.Case.None:
                    if (Singleton.GetSingleton<UnionTypes.None>() is TGet tvNone)
                    {
                        value = tvNone;
                        return true;
                    }
                    return TypeUnion.TryCreate(Singleton.GetSingleton<UnionTypes.None>(), out value);
            }
            value = default!; return false;
        }
        #endregion

        public bool Equals(Option<TValue> other)
        {
            if (this.Kind != other.Kind) return false;

            switch (this.Kind)
            {
                case Option<TValue>.Case.Some:
                    return object.Equals(this.Value, other.Value);
                case Option<TValue>.Case.None:
                    return true;
                default:
                    return false;
            }
        }

        public override bool Equals(object? other)
        {
            return TryCreate(other, out var union) && this.Equals(union);
        }

        public override int GetHashCode()
        {
            switch (this.Kind)
            {
                case Option<TValue>.Case.Some:
                    return this.Value?.GetHashCode() ?? 0;
                case Option<TValue>.Case.None:
                    return (int)this.Kind;
                default:
                    return 0;
            }
        }

        public static bool operator == (Option<TValue> left, Option<TValue> right) => left.Equals(right);
        public static bool operator != (Option<TValue> left, Option<TValue> right) => !left.Equals(right);

        public override string ToString()
        {
            switch (this.Kind)
            {
                case Option<TValue>.Case.Some:
                    return this.Value?.ToString() ?? "";
                case Option<TValue>.Case.None:
                    return Singleton.GetSingleton<UnionTypes.None>()?.ToString() ?? "";
                default:
                    return "";
            }
        }

        public void Match(Action<TValue> whenSome, Action whenNone)
        {
            switch (Kind)
            {
                case Option<TValue>.Case.Some : whenSome(this.Value); break;
                case Option<TValue>.Case.None : whenNone(); break;
                default: throw new InvalidOperationException("Invalid union state");
            }
        }

        public TResult Select<TResult>(Func<TValue, TResult> whenSome, Func<TResult> whenNone)
        {
            switch (Kind)
            {
                case Option<TValue>.Case.Some: return whenSome(this.Value);
                case Option<TValue>.Case.None: return whenNone();
                default: throw new InvalidOperationException("Invalid union state");
            }
        }
    }
}

