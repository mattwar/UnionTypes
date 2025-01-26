using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace UnionTypes.Toolkit;

public interface IOneOf : ITypeUnion
{
    /// <summary>
    /// The value held by the union.
    /// </summary>
    object Value { get; }
}