using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace UnionTypes.Toolkit
{
    /// <summary>
    /// Helper functions for interacting with types that implement <see cref="ITypeUnion{TSelf}"/>.
    /// </summary>
    public static partial class TypeUnion
    {
        /// <summary>
        /// Returns true if an instance of the specfieid type can be successfully created from the value of type <typeparamref name="TValue"/>.
        /// </summary>
        public static bool CanCreate<TValue>(TValue value, Type unionType)
        {
            return GetHelper(unionType).CanCreate(value);
        }

        /// <summary>
        /// Returns true if type <typeparamref name="TUnion"/> can be successfully created from the value of type <typeparamref name="TValue"/>.
        /// </summary>
        public static bool TryCreate<TValue, TUnion>(TValue value, [NotNullWhen(true)] out TUnion union)
        {
            // try creating union from value
            var unionHelper = GetHelper<TUnion>();
            if (unionHelper.TryCreate(value, out union))
                return true;

            if (value is ITypeUnion)
            {
                return TryCreateFromUnion(value, out union);
            }

            return false;
        }

        /// <summary>
        /// Gets the value of the union as the type <typeparamref name="TValue"/> if possible
        /// or by creating the value from the union's value if the <typeparamref name="TValue"/> is a type union.
        /// </summary>
        public static bool TryGet<TUnion, TValue>(TUnion union, [NotNullWhen(true)] out TValue value)
        {
            // try getting the value from the union as the requested type
            var unionHelper = GetHelper<TUnion>();
            if (unionHelper.TryGet(union, out value))
                return true;

            // if the value is also a union, try creating it from the union's value.
            return TryCreateFromUnion(union, out value);
        }

        /// <summary>
        /// Creates the target union <typeparamref name="TTargetUnion"/> from the source <typeparamref name="TSourceUnion"/>.
        /// </summary>
        public static bool TryCreateFromUnion<TSourceUnion, TTargetUnion>(TSourceUnion source, [NotNullWhen(true)] out TTargetUnion target)
        {
            var sourceHelper = GetHelper<TTargetUnion>();
            return sourceHelper.TryCreateFrom(source, out target);
        }

        /// <summary>
        /// Gets the known case types from the union type <typeparamref name="TUnion"/>.
        /// </summary>
        public static IReadOnlyList<Type> GetCaseTypes<TUnion>()
        {
            return GetHelper<TUnion>().GetCaseTypes();
        }

        /// <summary>
        /// Current set of known type union helpers.
        /// </summary>
        private static ImmutableDictionary<Type, Helper> _helpers =
            ImmutableDictionary<Type, Helper>.Empty;

        /// <summary>
        /// Gets the current converter for the target type.
        /// </summary>
        private static Helper GetHelper(Type unionType)
        {
            if (!_helpers.TryGetValue(unionType, out var converter))
            {
                converter = ImmutableInterlocked.GetOrAdd(ref _helpers, unionType, tt => CreateHelper(tt));
            }

            return converter;
        }

        private static Helper<TUnion> GetHelper<TUnion>()
        {
            return (Helper<TUnion>)GetHelper(typeof(TUnion));
        }

        /// <summary>
        /// Creates the helper for the type
        /// </summary>
        private static Helper CreateHelper(Type unionType)
        {
            if (unionType.GetInterfaces().Any(iface =>
                iface.IsGenericType
                && iface.GetGenericTypeDefinition() == typeof(IClosedTypeUnion<>)
                && iface.GenericTypeArguments[0] == unionType))
            {
                var utc = typeof(ClosedTypeUnionTHelper<>).MakeGenericType(unionType);
                return (Helper?)Activator.CreateInstance(utc)!;
            }
            else if (unionType.GetInterfaces().Any(iface => 
                iface.IsGenericType 
                && iface.GetGenericTypeDefinition() == typeof(ITypeUnion<>)
                && iface.GenericTypeArguments[0] == unionType))
            {
                var utc = typeof(TypeUnionTHelper<>).MakeGenericType(unionType);
                return (Helper?)Activator.CreateInstance(utc)!;
            }
            else if (unionType.IsAssignableTo(typeof(ITypeUnion)))
            {
                var utc = typeof(TypeUnionHelper<>).MakeGenericType(unionType);
                return (Helper?)Activator.CreateInstance(utc)!;
            }
            else
            {
                return (Helper?)Activator.CreateInstance(typeof(DefaultHelper<>).MakeGenericType(unionType))!;
            }
        }

        private abstract class Helper
        {
            /// <summary>
            /// Returns true if the value can be converted into the the type converter's target type.
            /// </summary>
            public abstract bool CanCreate<TValue>(TValue value);
        }

        private abstract class Helper<TUnion> : Helper
        {
            public override bool CanCreate<TValue>(TValue value)
            {
                return value is TUnion;
            }

            public virtual bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out TUnion union)
            {
                // allow reference conversions only
                if (value is TUnion tunion)
                {
                    union = tunion;
                    return true;
                }
                else
                {
                    union = default!;
                    return false;
                }
            }

            public virtual IReadOnlyList<Type> GetCaseTypes()
            {
                return Array.Empty<Type>();
            }

            public virtual bool TryGet<TValue>(TUnion union, [NotNullWhen(true)] out TValue value)
            {
                if (union is TValue tvalue)
                {
                    value = tvalue;
                    return true;
                }
                else
                {
                    value = default!;
                    return false;
                }
            }

            public virtual bool TryCreateFrom<TOther>(TOther other, [NotNullWhen(true)] out TUnion union)
            {
                union = default!;
                return false;
            }
        }

        private class TypeUnionHelper<TUnion> : Helper<TUnion>
            where TUnion : ITypeUnion
        {
            public override bool TryGet<TValue>(TUnion union, [NotNullWhen(true)] out TValue value)
            {
                return union.TryGet(out value);
            }

            public override bool TryCreateFrom<TOther>(TOther other, [NotNullWhen(true)] out TUnion union)
            {
                // not creatable...
                union = default!;
                return false;
            }
        }

        private class TypeUnionTHelper<TUnion> : TypeUnionHelper<TUnion>
            where TUnion : ITypeUnion<TUnion>
        {
            public override bool CanCreate<TValue>(TValue value)
            {
                return TUnion.TryCreate(value, out _);
            }

            public override bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out TUnion union)
            {
                return TUnion.TryCreate(value, out union);
            }

            public override bool TryCreateFrom<TOther>(TOther other, [NotNullWhen(true)] out TUnion union)
            {
                if (other is ITypeUnion u)
                {
                    var helper = GetCreateFromHelper<TOther>(u.Type);
                    return helper.TryCreateFrom(other, out union);
                }
                union = default!;
                return false;
            }

            private static ImmutableDictionary<Type, CreateFromUnionHelper> _createFromHelpers =
                ImmutableDictionary<Type, CreateFromUnionHelper>.Empty;

            private static CreateFromUnionHelper<TOther> GetCreateFromHelper<TOther>(Type valueType)
            {
                if (!_createFromHelpers.TryGetValue(typeof(TOther), out var helper))
                {
                    var tmp = CreateGetUnionHelper<TOther>(valueType);
                    helper = ImmutableInterlocked.GetOrAdd(ref _createFromHelpers, typeof(TOther), tmp);
                }

                return (CreateFromUnionHelper<TOther>)helper;
            }

            private static CreateFromUnionHelper CreateGetUnionHelper<TOther>(Type valueType)
            {
                return (CreateFromUnionHelper)Activator.CreateInstance(typeof(CreateFromUnionHelper<,>).MakeGenericType(typeof(TUnion), typeof(TOther), valueType))!;
            }

            private protected abstract class CreateFromUnionHelper { }
            private protected abstract class CreateFromUnionHelper<TOther> : CreateFromUnionHelper
            {
                public abstract bool TryCreateFrom(TOther other, [NotNullWhen(true)] out TUnion union);
            }

            private class CreateFromUnionHelper<TOther, TValue> : CreateFromUnionHelper<TOther>
                where TOther : ITypeUnion
            {
                public override bool TryCreateFrom(TOther other, [NotNullWhen(true)] out TUnion union)
                {
                    if (other.TryGet(out TValue value) && TUnion.TryCreate(value, out union))
                        return true;
                    union = default!;
                    return false;
                }
            }
        }

        private class ClosedTypeUnionTHelper<TUnion> : TypeUnionTHelper<TUnion>
            where TUnion : IClosedTypeUnion<TUnion>
        {
            public override bool TryGet<TValue>(TUnion union, [NotNullWhen(true)] out TValue value)
            {
                if (union.TryGet(out value))
                    return true;

                return base.TryGet(union, out value);
            }

            public override IReadOnlyList<Type> GetCaseTypes()
            {
                return TUnion.Types;
            }
        }

        /// <summary>
        /// A default type factory that only supports reference conversions.
        /// </summary>
        private class DefaultHelper<TUnion> : Helper<TUnion>
        {
        }
    }
}