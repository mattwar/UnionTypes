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
    public partial struct Option<TValue> : IEquatable<Option<TValue>>
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
        public static Option<TValue> None => new Option<TValue>(kind: Option<TValue>.Case.None, field0: default!);

        public TValue SomeValue => this.Kind == Option<TValue>.Case.Some ? _field0 : default!;
        public bool IsNone => this.Kind == Option<TValue>.Case.None;

        public bool Equals(Option<TValue> other)
        {
            if (this.Kind != other.Kind) return false;

            switch (this.Kind)
            {
                case Option<TValue>.Case.Some:
                    return object.Equals(this.SomeValue, other.SomeValue);
                case Option<TValue>.Case.None:
                    return true;
                default:
                    return false;
            }
        }

        public override bool Equals(object? other)
        {
            return other is Option<TValue> union && this.Equals(union);
        }

        public override int GetHashCode()
        {
            switch (this.Kind)
            {
                case Option<TValue>.Case.Some:
                    return this.SomeValue?.GetHashCode() ?? 0;
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
                    return $"Some({this.SomeValue})";
                case Option<TValue>.Case.None:
                    return "None";
                default:
                    return "";
            }
        }

        public void Match(Action<TValue> whenSome, Action whenNone, Action? invalid = null)
        {
            switch (Kind)
            {
                case Option<TValue>.Case.Some : whenSome(this.SomeValue); break;
                case Option<TValue>.Case.None : whenNone(); break;
                default: invalid?.Invoke(); break;
            }
        }

        public TResult Match<TResult>(Func<TValue, TResult> whenSome, Func<TResult> whenNone, Func<TResult>? invalid = null)
        {
            switch (Kind)
            {
                case Option<TValue>.Case.Some: return whenSome(SomeValue);
                case Option<TValue>.Case.None: return whenNone();
                default: return invalid != null ? invalid() : throw new InvalidOperationException("Unhandled union state.");
            }
        }
    }
}

