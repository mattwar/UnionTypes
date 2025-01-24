using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace UnionTypes
{
    public partial struct Option<TValue>
    {
        /// <summary>
        /// True when <see cref="Kind"/> is <see cref="Case.None"/>
        /// </summary>
        public bool IsNone => this.Kind == Case.None;
    }
}