using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace UnionTypes.Toolkit
{
    public static class Option
    {
        public static Option<TValue> Some<TValue>(TValue value) => 
            Option<TValue>.Some(value);
    }

    public partial struct Option<TValue>
    {
        /// <summary>
        /// True when the option has a value.
        /// </summary>
        public bool IsSome => this.Kind == Case.Some;

        /// <summary>
        /// True when the option has no value.
        /// </summary>
        public bool IsNone => this.Kind == Case.None;

        /// <summary>
        /// Maps a value to a new value or returns the default value.
        /// </summary>
        public Option<TResult> Map<TResult>(Func<TValue, TResult> map) =>
            this.Kind switch
            {
                Case.Some => Option.Some(map(this.Value)),
                Case.None => default,
                _ => throw new InvalidOperationException("Unknown case."),
            };

        /// <summary>
        /// Maps a value to a new value or returns the default value.
        /// </summary>
        public Option<TResult> Map<TResult>(Func<TValue, Option<TResult>> map) =>
            this.Kind switch
            {
                Case.Some => map(this.Value),
                Case.None => default,
                _ => throw new InvalidOperationException("Unknown case."),
            };

    }
}