using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace UnionTypes.Toolkit
{
    /// <summary>
    /// A type union holds a single value of one or more possible types
    /// that need not be from the same inheritance hierarchy.
    /// </summary>
    public interface ITypeUnion
    {
        /// <summary>
        /// The current type of the value held by the union.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets the union's value as the specified type.
        /// Returns true if the value is successfully retrieved.
        /// </summary>
        bool TryGet<T>([NotNullWhen(true)] out T value);
    }

    /// <summary>
    /// A type union that can create itself from a value.
    /// </summary>
    public interface ITypeUnion<TSelf> : ITypeUnion
    {
        /// <summary>
        /// Creates the union from the specified value, if possible.
        /// Returns true if the union is successfully created from the value.
        /// </summary>
        abstract static bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out TSelf union);
    }

    /// <summary>
    /// A type union that has a closed set of possible types.
    /// </summary>
    public interface IClosedTypeUnion<TSelf> : ITypeUnion<TSelf>
    {
        /// <summary>
        /// The closed set of possible types the union's value may take.
        /// </summary>
        static abstract IReadOnlyList<Type> Types { get; }
    }
}