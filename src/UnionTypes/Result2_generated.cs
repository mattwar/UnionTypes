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
    public partial struct Result<TValue, TError> : IEquatable<Result<TValue, TError>>
    {
        public enum Case
        {
            Success = 1,
            Failure = 2,
        }

        public Case Kind { get; }
        private readonly TValue _field0;
        private readonly TError _field1;

        private Result(Case kind, TValue field0, TError field1)
        {
            this.Kind = kind;
            _field0 = field0;
            _field1 = field1;
        }

        public static Result<TValue, TError> Success(TValue value) => new Result<TValue, TError>(kind: Result<TValue, TError>.Case.Success, field0: value, field1: default!);
        public static Result<TValue, TError> Failure(TError error) => new Result<TValue, TError>(kind: Result<TValue, TError>.Case.Failure, field0: default!, field1: error);

        public TValue SuccessValue => this.Kind == Result<TValue, TError>.Case.Success ? _field0 : default!;
        public TError FailureValue => this.Kind == Result<TValue, TError>.Case.Failure ? _field1 : default!;

        public bool Equals(Result<TValue, TError> other)
        {
            if (this.Kind != other.Kind) return false;

            switch (this.Kind)
            {
                case Result<TValue, TError>.Case.Success:
                    return object.Equals(this.SuccessValue, other.SuccessValue);
                case Result<TValue, TError>.Case.Failure:
                    return object.Equals(this.FailureValue, other.FailureValue);
                default:
                    return false;
            }
        }

        public override bool Equals(object? other)
        {
            return other is Result<TValue, TError> union && this.Equals(union);
        }

        public override int GetHashCode()
        {
            switch (this.Kind)
            {
                case Result<TValue, TError>.Case.Success:
                    return this.SuccessValue?.GetHashCode() ?? 0;
                case Result<TValue, TError>.Case.Failure:
                    return this.FailureValue?.GetHashCode() ?? 0;
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
                    return $"Success({this.SuccessValue})";
                case Result<TValue, TError>.Case.Failure:
                    return $"Failure({this.FailureValue})";
                default:
                    return "";
            }
        }

        public void Match(Action<TValue> whenSuccess, Action<TError> whenFailure, Action? invalid = null)
        {
            switch (Kind)
            {
                case Result<TValue, TError>.Case.Success : whenSuccess(this.SuccessValue); break;
                case Result<TValue, TError>.Case.Failure : whenFailure(this.FailureValue); break;
                default: invalid?.Invoke(); break;
            }
        }

        public TResult Match<TResult>(Func<TValue, TResult> whenSuccess, Func<TError, TResult> whenFailure, Func<TResult>? invalid = null)
        {
            switch (Kind)
            {
                case Result<TValue, TError>.Case.Success: return whenSuccess(SuccessValue);
                case Result<TValue, TError>.Case.Failure: return whenFailure(FailureValue);
                default: return invalid != null ? invalid() : throw new InvalidOperationException("Unhandled union state.");
            }
        }
    }
}

