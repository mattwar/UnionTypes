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
    public partial struct Result<TValue> : IEquatable<Result<TValue>>
    {
        public enum Case
        {
            Success = 1,
            Failure = 2,
        }

        public Case Kind { get; }
        private readonly TValue _field0;
        private readonly string _field1;

        private Result(Case kind, TValue field0, string field1)
        {
            this.Kind = kind;
            _field0 = field0;
            _field1 = field1;
        }

        public static Result<TValue> Success(TValue value) => new Result<TValue>(kind: Result<TValue>.Case.Success, field0: value, field1: default!);
        public static Result<TValue> Failure(string message) => new Result<TValue>(kind: Result<TValue>.Case.Failure, field0: default!, field1: message);

        public TValue SuccessValue => this.Kind == Result<TValue>.Case.Success ? _field0 : default!;
        public string FailureMessage => this.Kind == Result<TValue>.Case.Failure ? _field1 : default!;

        public bool Equals(Result<TValue> other)
        {
            if (this.Kind != other.Kind) return false;

            switch (this.Kind)
            {
                case Result<TValue>.Case.Success:
                    return object.Equals(this.SuccessValue, other.SuccessValue);
                case Result<TValue>.Case.Failure:
                    return object.Equals(this.FailureMessage, other.FailureMessage);
                default:
                    return false;
            }
        }

        public override bool Equals(object? other)
        {
            return other is Result<TValue> union && this.Equals(union);
        }

        public override int GetHashCode()
        {
            switch (this.Kind)
            {
                case Result<TValue>.Case.Success:
                    return this.SuccessValue?.GetHashCode() ?? 0;
                case Result<TValue>.Case.Failure:
                    return this.FailureMessage?.GetHashCode() ?? 0;
                default:
                    return 0;
            }
        }

        public static bool operator == (Result<TValue> left, Result<TValue> right) => left.Equals(right);
        public static bool operator != (Result<TValue> left, Result<TValue> right) => !left.Equals(right);

        public override string ToString()
        {
            switch (this.Kind)
            {
                case Result<TValue>.Case.Success:
                    return $"Success({this.SuccessValue})";
                case Result<TValue>.Case.Failure:
                    return $"Failure({this.FailureMessage})";
                default:
                    return "";
            }
        }

        public void Match(Action<TValue> whenSuccess, Action<string> whenFailure, Action? invalid = null)
        {
            switch (Kind)
            {
                case Result<TValue>.Case.Success : whenSuccess(this.SuccessValue); break;
                case Result<TValue>.Case.Failure : whenFailure(this.FailureMessage); break;
                default: invalid?.Invoke(); break;
            }
        }

        public TResult Match<TResult>(Func<TValue, TResult> whenSuccess, Func<string, TResult> whenFailure, Func<TResult>? invalid = null)
        {
            switch (Kind)
            {
                case Result<TValue>.Case.Success: return whenSuccess(SuccessValue);
                case Result<TValue>.Case.Failure: return whenFailure(FailureMessage);
                default: return invalid != null ? invalid() : throw new InvalidOperationException("Unhandled union state.");
            }
        }
    }
}

