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
    }
}