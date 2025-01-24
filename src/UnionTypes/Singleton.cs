using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnionTypes.Toolkit
{
    public interface ISingleton<TSelf>
        where TSelf : ISingleton<TSelf>
    {
        abstract static TSelf Singleton { get; }
    }

    /// <summary>
    /// A class use to access and cache singleton values for singleton types.
    /// A singleton type is one that is meant to only ever have one instance,
    /// accessed via a static property or field on the type.
    /// </summary>
    public static class Singleton
    {
        /// <summary>
        /// Attempts to retrieve the singleton instance of the specified type.
        /// </summary>
        public static bool TryGetSingleton<T>(out T singleton)
        {
            return TypedSingleton<T>.TryGetSingleton(out singleton);
        }

        /// <summary>
        /// Returns the singleton value for the type, or default is no singleton is defined.
        /// </summary>
        public static T GetSingleton<T>()
        {
            if (TryGetSingleton(out T singleton))
            {
                return singleton;
            }
            return default!;
        }

        private interface ISingletonAccessor<T>
        {
            bool TryGetSingleton(out T singleton);
        }

        private class SingletonAccessor<T> : ISingletonAccessor<T>
            where T : ISingleton<T>
        {
            public bool TryGetSingleton(out T singleton)
            {
                singleton = T.Singleton;
                return true;
            }
        }

        private class ValueAccessor<T> : ISingletonAccessor<T>
        {
            private readonly T _singleton;

            public ValueAccessor(T singleton)
            {
                _singleton = singleton;
            }

            public bool TryGetSingleton(out T singleton)
            {
                singleton = _singleton;
                return true;
            }
        }

        private class NotSingletonAccessor<T> : ISingletonAccessor<T>
        {
            public bool TryGetSingleton(out T singleton)
            {
                singleton = default!;
                return false;
            }
        }

        private class TypedSingleton<T>
        {
            private static ISingletonAccessor<T>? _accessor;

            public static bool TryGetSingleton(out T singleton)
            {
                if (_accessor == null)
                {
                    _accessor = CreateAccessor();
                }

                return _accessor.TryGetSingleton(out singleton);
            }

            private static readonly HashSet<string> _singletonNames =
                new HashSet<string>(["Singleton", "Instance"]);

            private static ISingletonAccessor<T> CreateAccessor()
            {
                // does it implement ISingleton<T>?
                var ifaceType = typeof(T).GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISingleton<>) && i.GetGenericArguments()[0] == typeof(T));
                if (ifaceType != null)
                {
                    return (ISingletonAccessor<T>)Activator.CreateInstance(typeof(SingletonAccessor<>).MakeGenericType(typeof(T)))!;
                }

                // look for static properties of type T
                var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Static)
                    .Where(p => p.PropertyType == typeof(T))
                    .ToList();
                var prop = (props.Count == 1) ? props[0] : props.FirstOrDefault(p => _singletonNames.Contains(p.Name));
                if (prop != null)
                {
                    var value = prop.GetValue(null);
                    return (ISingletonAccessor<T>)Activator.CreateInstance(typeof(ValueAccessor<>).MakeGenericType(typeof(T)), new object?[] { value })!;
                }

                // look for static fields of type T.
                var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(f => f.FieldType == typeof(T))
                    .ToList();
                var field = fields.Count == 1 
                    ? fields[0] 
                    : fields.FirstOrDefault(f => _singletonNames.Contains(f.Name));
                if (field != null)
                {
                    var value = field.GetValue(null);
                    return (ISingletonAccessor<T>)Activator.CreateInstance(typeof(ValueAccessor<>).MakeGenericType(typeof(T)), new object?[] { value })!;
                }

                // otherwise, not a singleton type
                return (ISingletonAccessor<T>)Activator.CreateInstance(typeof(NotSingletonAccessor<>).MakeGenericType(typeof(T)))!;
            }
        }
    }
}