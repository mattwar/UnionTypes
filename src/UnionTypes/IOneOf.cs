using System;

namespace UnionTypes
{
    public interface IOneOf
    {
        bool IsType<T>();
        bool TryGetValue<T>(out T value);
        object GetValue();
        bool Equals<T>(T value);
    }
}