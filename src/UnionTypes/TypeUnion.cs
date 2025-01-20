using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace UnionTypes
{
    /// <summary>
    /// A type union holds a single value of one or more different types
    /// that need not be from the same inheritance hierarchy.
    /// </summary>
    public interface ITypeUnion
    {
        /// <summary>
        /// The current type of the value held by the union.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Retrieves the union's value as the specified type.
        /// Returns true if the value is successfully retrieved.
        /// </summary>
        bool TryGet<T>([NotNullWhen(true)] out T value);
    }

    public interface ITypeUnion<TSelf> : ITypeUnion
        where TSelf : ITypeUnion<TSelf>
    {
        abstract static bool TryCreateFrom<TValue>(TValue value, [NotNullWhen(true)] out TSelf union);

        virtual static bool CanCreateFrom<TValue>(TValue value) =>
            TSelf.TryCreateFrom(value, out _);
    }

    public interface IClosedTypeUnion : ITypeUnion
    {
    }

    public interface IClosedTypeUnion<TSelf> : IClosedTypeUnion, ITypeUnion<TSelf>
        where TSelf : IClosedTypeUnion<TSelf>
    {
        /// <summary>
        /// The closed set of possible types the union's value may take.
        /// </summary>
        static abstract IReadOnlyList<Type> Types { get; }

        /// <summary>
        /// The index into the <see cref="Types"/> collection of the type that the current value corresponds to.
        /// </summary>
        int TypeIndex { get; }
    }

    /// <summary>
    /// A helper class for implementions of <see cref="ITypeUnion"/>
    /// </summary>
    public static class TypeUnion
    {
        /// <summary>
        /// Returns true if the source value can be converted into the target type.
        /// </summary>
        public static bool CanCreateFrom<TValue>(TValue source, Type unionType)
        {
            return GetFactory(unionType).CanCreateFrom(source);
        }

        /// <summary>
        /// Returns true if the source value is successfully converted to the target type.
        /// </summary>
        public static bool TryCreateFrom<TValue, TUnion>(TValue source, [NotNullWhen(true)] out TUnion target)
        {
            var converter = TypedFactory<TUnion>.GetTypedFactory();
            return converter.TryCreateFrom(source, out target);
        }

        /// <summary>
        /// Current set of known converters.
        /// </summary>
        private static ImmutableDictionary<Type, ITypeUnionFactory> _factories =
            ImmutableDictionary<Type, ITypeUnionFactory>.Empty;

        /// <summary>
        /// Gets the current converter for the target type.
        /// </summary>
        private static ITypeUnionFactory GetFactory(Type targetType)
        {
            if (!_factories.TryGetValue(targetType, out var converter))
            {
                converter = ImmutableInterlocked.GetOrAdd(ref _factories, targetType, tt => CreateFactory(tt));
            }

            return converter;
        }

        /// <summary>
        /// Creates the converter for the target type.
        /// </summary>
        private static ITypeUnionFactory CreateFactory(Type targetType)
        {
            if (targetType.IsGenericType
                && targetType.GetInterfaces().Any(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(ITypeUnion<>)))
            {
                var utc = typeof(TypeUnionFactory<>).MakeGenericType(targetType);
                return (ITypeUnionFactory?)Activator.CreateInstance(utc)!;
            }
            else
            {
                return (ITypeUnionFactory?)Activator.CreateInstance(typeof(DefaultFactory<>).MakeGenericType(targetType))!;
            }
        }

        private class TypedFactory<TType>
        {
            private static ITypeUnionFactory<TType>? _converter;

            internal static ITypeUnionFactory<TType> GetTypedFactory()
            {
                if (_converter == null)
                {
                    _converter = (ITypeUnionFactory<TType>)GetFactory(typeof(TType));
                }

                return _converter;
            }
        }

        private interface ITypeUnionFactory
        {
            /// <summary>
            /// Returns true if the value can be converted into the the type converter's target type.
            /// </summary>
            abstract bool CanCreateFrom<TValue>(TValue value);
        }

        private interface ITypeUnionFactory<TType> : ITypeUnionFactory
        {
            bool TryCreateFrom<TValue>(TValue value, [NotNullWhen(true)] out TType instance);
        }

        private class TypeUnionFactory<TType> : ITypeUnionFactory<TType>
            where TType : ITypeUnion<TType>
        {
            public bool CanCreateFrom<TValue>(TValue value)
            {
                return TType.CanCreateFrom(value);
            }

            public bool TryCreateFrom<TValue>(TValue value, [NotNullWhen(true)] out TType instance)
            {
                return TType.TryCreateFrom(value, out instance);
            }
        }

        /// <summary>
        /// A default type factory that only supports reference conversions.
        /// </summary>
        private class DefaultFactory<TType> : ITypeUnionFactory<TType>
        {
            public bool CanCreateFrom<TValue>(TValue value)
            {
                return value is TType;
            }

            public bool TryCreateFrom<TValue>(TValue value, [NotNullWhen(true)] out TType instance)
            {
                if (value is TType tvalue)
                {
                    instance = tvalue;
                    return true;
                }
                else
                {
                    instance = default!;
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the 1-based type index for the value based on the union type's Types property.
        /// </summary>
        public static int GetTypeIndex<TUnion, TValue>(TValue value)
            where TUnion : IClosedTypeUnion<TUnion>
        {
            return ClosedTypeUnion<TUnion>.GetTypeIndex(value);
        }

        private static class ClosedTypeUnion<TUnion>
            where TUnion : IClosedTypeUnion<TUnion>
        {
            private static ImmutableDictionary<Type, int>? _indexMap;
            private static ImmutableDictionary<Type, int> GetIndexMap()
            {
                if (_indexMap == null)
                {
                    var types = TUnion.Types;
                    ImmutableDictionary<Type, int>.Builder builder = ImmutableDictionary.CreateBuilder<Type, int>();
                    for (int i = 0; i < types.Count; i++)
                    {
                        builder.Add(types[i], i + 1);
                    }
                    Interlocked.CompareExchange(ref _indexMap, builder.ToImmutable(), null);
                }

                return _indexMap!;
            }

            private static void SetTypeIndex(Type type, int index)
            {
                ImmutableInterlocked.Update(ref _indexMap, (map, value) => map!.SetItem(type, index), index);
            }

            /// <summary>
            /// Return the index into the TUnion.Types array for the value's corresponding type.
            /// </summary>
            public static int GetTypeIndex<TValue>(TValue value)
            {
                // if value is a union, get its underlying value.
                if (!(value is ITypeUnion u && u.TryGet<object>(out var underlyingValue)))
                {
                    underlyingValue = (object?)value;
                }

                if (underlyingValue != null)
                {
                    var valType = underlyingValue.GetType();

                    // fast path: look for know type index.
                    var map = GetIndexMap();
                    if (map.TryGetValue(valType, out var index))
                        return index;

                    // slow path: find first matching type index for value
                    var types = TUnion.Types;
                    for (int i = 0; i < types.Count; i++)
                    {
                        if (valType.IsAssignableTo(types[i])
                            || TypeUnion.CanCreateFrom(underlyingValue, types[i]))
                        {
                            // set fast path for next time
                            SetTypeIndex(valType, i);
                            return i;
                        }
                    }
                }

                return -1;
            }
        }
    }
}