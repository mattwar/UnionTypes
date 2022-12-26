using System;

namespace UnionTypes
{
    /// <summary>
    /// A type union is a type that can hold a value from two or more distinct/unrelated types.
    /// </summary>
    public interface ITypeUnion
    {
        bool Is<T>();
        T Get<T>();
        bool TryGet<T>(out T value);
        bool Equals<T>(T value);
    }
}