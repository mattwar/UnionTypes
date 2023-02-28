using System;

namespace UnionTypes
{
    /// <summary>
    /// A type union is a type that can represent a value from one of two or more distinct/unrelated types.
    /// This interface is a common interface that type union implementations share.
    /// </summary>
    public interface ITypeUnion
    {
        /// <summary>
        /// Returns true if the value represented is of the type specified in the type parameter.
        /// </summary>
        bool Is<T>();

        /// <summary>
        /// Gets the value if it is of the specified type or throws an exception.
        /// </summary>
        T Get<T>();

        /// <summary>
        /// Returns true if the value is of the specified type and assigns it into the out parameter.
        /// </summary>
        bool TryGet<T>(out T value);

        /// <summary>
        /// Returns true if the value represented is equivalent to the value specified, 
        /// or in case of the value being a union type itself, equivalent to that union's represented value.
        /// </summary>
        bool Equals<T>(T value);
    }
}