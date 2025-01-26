// 
// 
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using UnionTypes.Toolkit;
#nullable enable

namespace UnionTypes.Toolkit
{
    public partial struct Option<TValue> : IClosedTypeUnion<Option<TValue>>, IEquatable<Option<TValue>>
    {
        public enum Case
        {
            Some = 1,
            None = 0,
        }

        public Case Kind { get; }
        private readonly TValue _data_some_value;

        private Option(Case kind, TValue some_value)
        {
            this.Kind = kind;
            _data_some_value = some_value;
        }

        public static Option<TValue> Some(TValue value) => new Option<TValue>(kind: Option<TValue>.Case.Some, some_value: value);
        public static Option<TValue> None() => new Option<TValue>(kind: Option<TValue>.Case.None, some_value: default!);

        public static implicit operator Option<TValue>(TValue value) => Option<TValue>.Some(value);
        public static implicit operator Option<TValue>(UnionTypes.Toolkit.None value) => Option<TValue>.None();

        public static bool TryCreate<TCreate>(TCreate value, out Option<TValue> union)
        {
            switch (value)
            {
                case TValue v: union = Option<TValue>.Some(v); return true;
                case UnionTypes.Toolkit.None v: union = Option<TValue>.None(); return true;
            }
            return TypeUnion.TryCreateFromUnion(value, out union);
        }

        /// <summary>Accessible when <see cref="Kind"/> is <see cref="Case.Some"/>.</summary>
        public TValue Value => this.Kind == Option<TValue>.Case.Some ? _data_some_value : default!;

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
                    if (global::UnionTypes.Toolkit.None.Singleton is TGet tvNone)
                    {
                        value = tvNone;
                        return true;
                    }
                    return TypeUnion.TryCreate(global::UnionTypes.Toolkit.None.Singleton, out value);
            }
            value = default!;
            return false;
        }

        public Type Type
        {
            get
            {
                switch (this.Kind)
                {
                    case Option<TValue>.Case.Some: return typeof(TValue);
                    case Option<TValue>.Case.None: return typeof(UnionTypes.Toolkit.None);
                }
                return typeof(object);
            }
        }

        static IReadOnlyList<Type> IClosedTypeUnion<Option<TValue>>.Types { get; } =
            new [] { typeof(TValue), typeof(UnionTypes.Toolkit.None) };

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
                    return global::UnionTypes.Toolkit.None.Singleton?.ToString() ?? "";
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

