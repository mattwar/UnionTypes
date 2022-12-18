using System;

namespace UnionTypes
{
    public interface IOneOf
    {
        bool Is<T>();
        T Get<T>();
        bool TryGet<T>(out T value);
        bool Equals<T>(T value);
    }
}