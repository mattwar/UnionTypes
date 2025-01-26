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
    public partial struct Result<TValue, TError> : IClosedTypeUnion<Result<TValue, TError>>, IEquatable<Result<TValue, TError>>
    {
        public enum Case
        {
            Success = 1,
            Failure = 2,
        }

        public Case Kind { get; }
        private readonly TValue _data_success_value;
        private readonly TError _data_failure_error;

        private Result(Case kind, TValue success_value, TError failure_error)
        {
            this.Kind = kind;
            _data_success_value = success_value;
            _data_failure_error = failure_error;
        }

        public static Result<TValue, TError> Success(TValue value) => new Result<TValue, TError>(kind: Result<TValue, TError>.Case.Success, success_value: value, failure_error: default!);
        public static Result<TValue, TError> Failure(TError error) => new Result<TValue, TError>(kind: Result<TValue, TError>.Case.Failure, success_value: default!, failure_error: error);

        public static implicit operator Result<TValue, TError>(TValue value) => Result<TValue, TError>.Success(value);
        public static implicit operator Result<TValue, TError>(TError value) => Result<TValue, TError>.Failure(value);

        public static bool TryCreate<TCreate>(TCreate value, out Result<TValue, TError> union)
        {
            switch (value)
            {
                case TValue v: union = Result<TValue, TError>.Success(v); return true;
                case TError v: union = Result<TValue, TError>.Failure(v); return true;
            }
            return TypeUnion.TryCreateFromUnion(value, out union);
        }

        /// <summary>Accessible when <see cref="Kind"/> is <see cref="Case.Success"/>.</summary>
        public TValue Value => this.Kind == Result<TValue, TError>.Case.Success ? _data_success_value : default!;

        /// <summary>Accessible when <see cref="Kind"/> is <see cref="Case.Failure"/>.</summary>
        public TError Error => this.Kind == Result<TValue, TError>.Case.Failure ? _data_failure_error : default!;

        public bool TryGet<TGet>([NotNullWhen(true)] out TGet value)
        {
            switch (this.Kind)
            {
                case Result<TValue, TError>.Case.Success:
                    if (this.Value is TGet tvSuccess)
                    {
                        value = tvSuccess;
                        return true;
                    }
                    return TypeUnion.TryCreate(this.Value, out value);
                case Result<TValue, TError>.Case.Failure:
                    if (this.Error is TGet tvFailure)
                    {
                        value = tvFailure;
                        return true;
                    }
                    return TypeUnion.TryCreate(this.Error, out value);
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
                    case Result<TValue, TError>.Case.Success: return typeof(TValue);
                    case Result<TValue, TError>.Case.Failure: return typeof(TError);
                }
                return typeof(object);
            }
        }

        static IReadOnlyList<Type> IClosedTypeUnion<Result<TValue, TError>>.Types { get; } =
            new [] { typeof(TValue), typeof(TError) };

        public bool Equals(Result<TValue, TError> other)
        {
            if (this.Kind != other.Kind) return false;

            switch (this.Kind)
            {
                case Result<TValue, TError>.Case.Success:
                    return object.Equals(this.Value, other.Value);
                case Result<TValue, TError>.Case.Failure:
                    return object.Equals(this.Error, other.Error);
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
                case Result<TValue, TError>.Case.Success:
                    return this.Value?.GetHashCode() ?? 0;
                case Result<TValue, TError>.Case.Failure:
                    return this.Error?.GetHashCode() ?? 0;
                default:
                    return 0;
            }
        }

        public static bool operator == (Result<TValue, TError> left, Result<TValue, TError> right) => left.Equals(right);
        public static bool operator != (Result<TValue, TError> left, Result<TValue, TError> right) => !left.Equals(right);

        public override string ToString()
        {
            switch (this.Kind)
            {
                case Result<TValue, TError>.Case.Success:
                    return this.Value?.ToString() ?? "";
                case Result<TValue, TError>.Case.Failure:
                    return this.Error?.ToString() ?? "";
                default:
                    return "";
            }
        }

        public void Match(Action<TValue> whenSuccess, Action<TError> whenFailure, Action? undefined = null)
        {
            switch (Kind)
            {
                case Result<TValue, TError>.Case.Success : whenSuccess(this.Value); break;
                case Result<TValue, TError>.Case.Failure : whenFailure(this.Error); break;
                default: if (undefined != null) undefined(); else throw new InvalidOperationException("Undefined union state."); break;
            }
        }

        public TResult Select<TResult>(Func<TValue, TResult> whenSuccess, Func<TError, TResult> whenFailure, Func<TResult>? undefined = null)
        {
            switch (Kind)
            {
                case Result<TValue, TError>.Case.Success: return whenSuccess(this.Value);
                case Result<TValue, TError>.Case.Failure: return whenFailure(this.Error);
                default: return undefined != null ? undefined() : throw new InvalidOperationException("Undefined union state.");
            }
        }
    }
}

