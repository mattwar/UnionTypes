using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace UnionTypes.Toolkit
{
    /// <summary>
    /// A interface for types that can convert themselves to other types.
    /// </summary>
    public interface IConvertible
    {
        public bool TryConvertTo<TValue>(out TValue value)
        {
            if (this is TValue tval)
            {
                value = tval;
                return true;
            }
            value = default!;
            return false;
        }
    }

    /// <summary>
    /// An interface for types that can convert values of other types to their own type.
    /// </summary>
    public interface IConvertible<TSelf> : IConvertible
        where TSelf : IConvertible<TSelf>
    {
        /// <summary>
        /// Converts a value of a different type to this type's type.
        /// </summary>
        static abstract bool TryConvertFrom<TValue>(TValue value, [NotNullWhen(true)]out TSelf converted);
    }

    public static class Convertible
    {
        public static bool TryConvertTo<TSource, TTarget>(TSource source, [NotNullWhen(true)] out TTarget target)
        {
            var sourceConverter = TypedConverter<TSource>.GetConverter();
            if (sourceConverter.TryConvertTo(source, out target))
                return true;

            var targetConverter = TypedConverter<TTarget>.GetConverter();
            if (targetConverter.TryConvertFrom(source, out target))
                return true;

            target = default!;
            return false;
        }

        private static class TypedConverter<TType>
        {
            private static IConverter<TType> _converter = null!;

            public static IConverter<TType> GetConverter()
            {
                if (_converter == null)
                {
                    Interlocked.CompareExchange(ref _converter, CreateConverter(), null);
                }

                return _converter;
            }

            private static IConverter<TType> CreateConverter()
            {
                var type = typeof(TType);
                var ifaces = type.GetInterfaces();

                if (ifaces.Any(iface =>
                    iface.IsGenericType
                    && iface.GetGenericTypeDefinition() == typeof(IConvertible<>)
                    && iface.GetGenericArguments()[0] == typeof(TType)))
                {
                    return (IConverter<TType>)Activator.CreateInstance(typeof(ConvertibleTConverter<>).MakeGenericType(typeof(TType))!)!;
                }
                else if (ifaces.Any(iface =>
                    iface.IsGenericType
                    && iface.GetGenericTypeDefinition() == typeof(IConvertible)))
                {
                    return (IConverter<TType>)Activator.CreateInstance(typeof(ConvertibleConverter<>).MakeGenericType(typeof(TType))!)!;
                }
                else if (ifaces.Any(iface =>
                    iface.IsGenericType
                    && iface.GetGenericTypeDefinition() == typeof(ITypeUnion<>)
                    && iface.GetGenericArguments()[0] == typeof(TType)))
                {
                    return (IConverter<TType>)Activator.CreateInstance(typeof(TypeUnionTConverter<>).MakeGenericType(typeof(TType))!)!;
                }
                else if (ifaces.Any(iface =>
                    iface.IsGenericType
                    && iface.GetGenericTypeDefinition() == typeof(ITypeUnion)))
                {
                    return (IConverter<TType>)Activator.CreateInstance(typeof(TypeUnionConverter<>).MakeGenericType(typeof(TType))!)!;
                }
                else if (type.IsEnum)
                {
                    return (IConverter<TType>)Activator.CreateInstance(typeof(EnumConverter<>).MakeGenericType(typeof(TType))!)!;
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return (IConverter<TType>)Activator.CreateInstance(typeof(NullableConverter<>).MakeGenericType(type.GetGenericArguments()[0])!)!;
                }
                else if (type == typeof(bool))
                    return (IConverter<TType>)(object)BoolConverter.Instance;
                else if (type == typeof(byte))
                    return (IConverter<TType>)(object)ByteConverter.Instance;
                else if (type == typeof(sbyte))
                    return (IConverter<TType>)(object)SByteConverter.Instance;
                else if (type == typeof(short))
                    return (IConverter<TType>)(object)Int16Converter.Instance;
                else if (type == typeof(ushort))
                    return (IConverter<TType>)(object)UInt16Converter.Instance;
                else if (type == typeof(int))
                    return (IConverter<TType>)(object)Int32Converter.Instance;
                else if (type == typeof(uint))
                    return (IConverter<TType>)(object)UInt32Converter.Instance;
                else if (type == typeof(long))
                    return (IConverter<TType>)(object)Int64Converter.Instance;
                else if (type == typeof(ulong))
                    return (IConverter<TType>)(object)UInt64Converter.Instance;
                else if (type == typeof(float))
                    return (IConverter<TType>)(object)SingleConverter.Instance;
                else if (type == typeof(double))
                    return (IConverter<TType>)(object)DoubleConverter.Instance;
                else if (type == typeof(decimal))
                    return (IConverter<TType>)(object)DecimalConverter.Instance;
                else if (type == typeof(Decimal64))
                    return (IConverter<TType>)(object)Decimal64Converter.Instance;
                else if (type == typeof(String))
                    return (IConverter<TType>)(object)StringConverter.Instance;
                else if (type == typeof(DateTime))
                    return (IConverter<TType>)(object)DateTimeConverter.Instance;
                else if (type == typeof(DateOnly))
                    return (IConverter<TType>)(object)DateOnlyConverter.Instance;
                else if (type == typeof(TimeSpan))
                    return (IConverter<TType>)(object)TimeSpanConverter.Instance;
                else if (type == typeof(TimeOnly))
                    return (IConverter<TType>)(object)TimeOnlyConverter.Instance;
                else
                {
                    return (IConverter<TType>)Activator.CreateInstance(typeof(ReferenceConverter<>).MakeGenericType(typeof(TType))!)!;
                }
            }
        }

        private interface IConverter<TType>
        {
            bool TryConvertTo<TTarget>(TType source, [NotNullWhen(true)] out TTarget target);
            bool TryConvertFrom<TSource>(TSource source, [NotNullWhen(true)] out TType target);
        }

        private abstract class Converter<TType> : IConverter<TType>
        {
            public virtual bool TryConvertTo<TTarget>(TType source, [NotNullWhen(true)] out TTarget target)
            {
                if (source is TTarget tval)
                {
                    target = tval;
                    return true;
                }
                target = default!;
                return false;
            }

            public virtual bool TryConvertFrom<TSource>(TSource source, [NotNullWhen(true)] out TType target)
            {
                if (source is TType tval)
                {
                    target = tval;
                    return true;
                }
                target = default!;
                return false;
            }
        }

        private class ConvertibleTConverter<TConvertible> : Converter<TConvertible>
            where TConvertible : IConvertible<TConvertible>
        {
            public override bool TryConvertTo<TTarget>(TConvertible source, [NotNullWhen(true)] out TTarget target)
            {
                return source.TryConvertTo(out target);
            }

            public override bool TryConvertFrom<TValue>(TValue value, out TConvertible converted)
            {
                return TConvertible.TryConvertFrom(value, out converted);
            }
        }

        private class ConvertibleConverter<TConvertible> : Converter<TConvertible>
            where TConvertible : IConvertible
        {
            public override bool TryConvertTo<TTarget>(TConvertible source, [NotNullWhen(true)] out TTarget target)
            {
                return source.TryConvertTo(out target);
            }
        }

        private class ReferenceConverter<TReference> : Converter<TReference>
        {
        }

        private class EnumConverter<TEnum> : Converter<TEnum>
            where TEnum : struct, System.Enum
        {
            public override bool TryConvertTo<TTarget>(TEnum source, [NotNullWhen(true)] out TTarget target)
            {
                if (typeof(TTarget) == typeof(string))
                {
                    target = (TTarget)(object)source.ToString();
                    return true;
                }

                var underlyngType = typeof(TEnum).GetEnumUnderlyingType();
                var lval = Type.GetTypeCode(underlyngType) switch
                {
                    TypeCode.SByte => Unsafe.As<TEnum, sbyte>(ref source),
                    TypeCode.Int16 => Unsafe.As<TEnum, short>(ref source),
                    TypeCode.Int32 => Unsafe.As<TEnum, int>(ref source),
                    TypeCode.Int64 => Unsafe.As<TEnum, long>(ref source),
                    TypeCode.Byte => Unsafe.As<TEnum, byte>(ref source),
                    TypeCode.UInt16 => Unsafe.As<TEnum, ushort>(ref source),
                    TypeCode.UInt32 => Unsafe.As<TEnum, uint>(ref source),
                    TypeCode.UInt64 => unchecked((long)Unsafe.As<TEnum, ulong>(ref source)),
                    _ => 0L
                };

                return Convertible.TryConvertTo(lval, out target);
            }

            public override bool TryConvertFrom<TValue>(TValue source, [NotNullWhen(true)] out TEnum target)
            {
                if (source is long lval || Convertible.TryConvertTo(source, out lval))
                {
                    var underlyingType = typeof(TEnum).GetEnumUnderlyingType();
                    switch (Type.GetTypeCode(underlyingType))
                    {
                        case TypeCode.SByte when lval >= sbyte.MinValue && lval <= sbyte.MaxValue:
                            var i8val = (sbyte)lval;
                            target = Unsafe.As<sbyte, TEnum>(ref i8val)!;
                            return true;
                        case TypeCode.Int16 when lval >= short.MinValue && lval <= short.MaxValue:
                            var i16val = (short)lval;
                            target = Unsafe.As<short, TEnum>(ref i16val)!;
                            return true;
                        case TypeCode.Int32 when lval >= int.MinValue && lval <= short.MaxValue:
                            var i32val = (int)lval;
                            target = Unsafe.As<int, TEnum>(ref i32val)!;
                            return true;
                        case TypeCode.Int64:
                            target = Unsafe.As<long, TEnum>(ref lval)!;
                            return true;
                        case TypeCode.Byte when lval >= byte.MinValue && lval <= byte.MaxValue:
                            var ui8val = (byte)lval;
                            target = Unsafe.As<byte, TEnum>(ref ui8val)!;
                            return true;
                        case TypeCode.UInt16 when lval >= ushort.MinValue && lval <= ushort.MaxValue:
                            var ui16val = (ushort)lval;
                            target = Unsafe.As<ushort, TEnum>(ref ui16val)!;
                            return true;
                        case TypeCode.UInt32 when lval >= uint.MinValue && lval <= uint.MaxValue:
                            var ui32val = (uint)lval;
                            target = Unsafe.As<uint, TEnum>(ref ui32val)!;
                            return true;
                        case TypeCode.UInt64:
                            var ui64val = unchecked((ulong)lval);
                            target = Unsafe.As<ulong, TEnum>(ref ui64val)!;
                            return true;
                    }
                }
                else if ((source is string sval || Convertible.TryConvertTo(source, out sval))
                    && Enum.TryParse(sval, ignoreCase: true, out target))
                {
                    return true;
                }

                target = default!;
                return false;
            }
        }

        private class NullableConverter<TType> : Converter<Nullable<TType>>
            where TType : struct
        {
            public override bool TryConvertTo<TTarget>(TType? source, [NotNullWhen(true)] out TTarget target)
            {
                if (source.HasValue)
                {
                    return Convertible.TryConvertTo(source.Value, out target);
                }

                target = default!;
                return false;
            }

            public override bool TryConvertFrom<TSource>(TSource source, [NotNullWhen(true)] out TType? target)
            {
                if (Convertible.TryConvertTo(source, out TType tvalue))
                {
                    target = tvalue;
                    return true;
                }

                target = default;
                return false;
            }
        }

        private class TypeUnionConverter<TUnion> : Converter<TUnion>
            where TUnion : ITypeUnion
        {
            public override bool TryConvertTo<TTarget>(TUnion source, [NotNullWhen(true)] out TTarget target)
            {
                // first try directly getting the target value
                if (source.TryGet(out target))
                    return true;

                if (typeof(TTarget) != typeof(object))
                {
                    // try accessing the value by the type it claims it is,
                    // and then converting to target from that.
                    if (typeof(TTarget) != source.Type)
                    {
                        var caseConverter = GetCaseGetConverter<TTarget>(source.Type);
                        if (caseConverter.TryGetAndConvert(source, out target))
                            return true;
                    }

                    // last resort, try getting as object (possible boxing) and
                    // converting that to target
                    if (source.TryGet(out object objValue))
                    {
                        return Convertible.TryConvertTo(objValue, out target);
                    }
                }

                target = default!;
                return false;
            }

            private ImmutableDictionary<Type, CaseGetConverter> _caseGetConverters =
                ImmutableDictionary<Type, CaseGetConverter>.Empty;

            private CaseGetConverter<TTarget> GetCaseGetConverter<TTarget>(Type caseType)
            {
                if (!_caseGetConverters.TryGetValue(caseType, out var converter))
                {
                    var converterType = typeof(CaseGetConverter<,>).MakeGenericType(typeof(TUnion), caseType, typeof(TTarget));
                    var newConverter = (CaseGetConverter)Activator.CreateInstance(converterType)!;
                    converter = ImmutableInterlocked.GetOrAdd(ref _caseGetConverters, caseType, newConverter);
                }

                return (CaseGetConverter<TTarget>)converter!;
            }

            private abstract class CaseGetConverter
            {
            }

            private abstract class CaseGetConverter<TTarget> : CaseGetConverter
            {
                public abstract bool TryGetAndConvert(TUnion source, [NotNullWhen(true)] out TTarget target);
            }

            private class CaseGetConverter<TCase, TTarget> : CaseGetConverter<TTarget>
            {
                public override bool TryGetAndConvert(TUnion source, [NotNullWhen(true)] out TTarget target)
                {
                    if (source.TryGet(out TCase caseValue))
                        return Convertible.TryConvertTo(caseValue, out target);
                    target = default!;
                    return false;
                }
            }
        }

        private class TypeUnionTConverter<TUnion> : TypeUnionConverter<TUnion>
            where TUnion : ITypeUnion<TUnion>
        {
            public override bool TryConvertFrom<TSource>(TSource source, [NotNullWhen(true)] out TUnion target)
            {
                if (TUnion.TryCreate(source, out target))
                    return true;

                // try converting to known case types and creating from that.
                var types = TypeUnion.GetCaseTypes<TUnion>();
                foreach (var tcase in types)
                {
                    var creator = GetCaseConvertCreator<TSource>(tcase);
                    if (creator.TryCreate(source, out target))
                        return true;
                }

                return false;
            }

            private ImmutableDictionary<Type, CaseConvertCreator> _caseConvertCreators =
                ImmutableDictionary<Type, CaseConvertCreator>.Empty;

            private CaseConvertCreator<TSource> GetCaseConvertCreator<TSource>(Type caseType)
            {
                if (!_caseConvertCreators.TryGetValue(caseType, out var converter))
                {
                    var converterType = typeof(CaseConvertCreator<,>).MakeGenericType(typeof(TUnion), typeof(TSource), caseType);
                    var newConverter = (CaseConvertCreator)Activator.CreateInstance(converterType)!;
                    converter = ImmutableInterlocked.GetOrAdd(ref _caseConvertCreators, caseType, newConverter);
                }

                return (CaseConvertCreator<TSource>)converter!;
            }

            private abstract class CaseConvertCreator
            {
            }

            private abstract class CaseConvertCreator<TSource> : CaseConvertCreator
            {
                public abstract bool TryCreate(TSource source, [NotNullWhen(true)] out TUnion target);
            }

            private class CaseConvertCreator<TSource, TCase> : CaseConvertCreator<TSource>
            {
                public override bool TryCreate(TSource source, [NotNullWhen(true)] out TUnion target)
                {
                    if (Convertible.TryConvertTo(source, out TCase tcase))
                        return TUnion.TryCreate(tcase, out target);
                    target = default!;
                    return false;
                }
            }
        }

        private class Int64Converter : Converter<long>
        {
            public static readonly Int64Converter Instance = new Int64Converter();

            public override bool TryConvertFrom<TValue>(TValue source, [NotNullWhen(true)] out long target)
            {
                switch (source)
                {
                    case bool v:
                        target = v ? 1L : 0L;
                        return true;
                    case sbyte v:
                        target = v;
                        return true;
                    case byte v:
                        target = v;
                        return true;
                    case short v:
                        target = v;
                        return true;
                    case ushort v:
                        target = v;
                        return true;
                    case int v:
                        target = v;
                        return true;
                    case uint v:
                        target = v;
                        return true;
                    case long v:
                        target = v;
                        return true;
                    case ulong v:
                        if (v <= long.MaxValue)
                        {
                            target = (long)v;
                            return true;
                        }
                        break;
                    case char v:
                        target = v;
                        return true;
                    case float v:
                        if (v >= long.MinValue && v <= long.MaxValue)
                        {
                            target = (long)v;
                            return true;
                        }
                        break;
                    case double v:
                        if (v >= long.MinValue && v <= long.MaxValue)
                        {
                            target = (long)v;
                            return true;
                        }
                        break;
                    case decimal v:
                        if (v >= long.MinValue && v <= long.MaxValue)
                        {
                            target = (long)v;
                            return true;
                        }
                        break;
                    case string v:
                        return Int64.TryParse(v, out target);
                    case Decimal64 v:
                        return Convertible.TryConvertTo(v.ToDecimal(), out target);
                    case Rune v:
                        target = v.Value;
                        return true;
                }

                target = default;
                return false;
            }
        }

        private abstract class Int64ConvertibleConverter<T> : Converter<T>
        {
            public abstract long ConvertToLong(T source);
            public abstract T ConvertFromLong(long source);

            public override bool TryConvertTo<TTarget>(T source, [NotNullWhen(true)] out TTarget target)
            {
                if (source is TTarget tval)
                {
                    target = tval;
                    return true;
                }
                else
                {
                    var lval = ConvertToLong(source);
                    return Convertible.TryConvertTo(lval, out target);
                }
            }

            public override bool TryConvertFrom<TSource>(TSource source, [NotNullWhen(true)] out T target)
            {
                switch (source)
                {
                    case T v:
                        target = v;
                        return true;
                    default:
                        if (Convertible.TryConvertTo(source, out long lval))
                        {
                            target = ConvertFromLong(lval)!;
                            return true;
                        }
                        break;
                }
                target = default!;
                return false;
            }
        }

        private class BoolConverter : Int64ConvertibleConverter<bool>
        {
            public static readonly BoolConverter Instance = new BoolConverter();
            public override long ConvertToLong(bool source) => source ? 1 : 0;
            public override bool ConvertFromLong(long source) => source != 0;
        }

        private class ByteConverter : Int64ConvertibleConverter<byte>
        {
            public static readonly ByteConverter Instance = new ByteConverter();
            public override long ConvertToLong(byte source) => source;
            public override byte ConvertFromLong(long source) => (byte)source;
        }

        private class SByteConverter : Int64ConvertibleConverter<sbyte>
        {
            public static readonly SByteConverter Instance = new SByteConverter();
            public override long ConvertToLong(sbyte source) => source;
            public override sbyte ConvertFromLong(long source) => (sbyte)source;
        }

        private class Int16Converter : Int64ConvertibleConverter<short>
        {
            public static readonly Int16Converter Instance = new Int16Converter();
            public override long ConvertToLong(short source) => source;
            public override short ConvertFromLong(long source) => (short)source;
        }

        private class UInt16Converter : Int64ConvertibleConverter<ushort>
        {
            public static readonly UInt16Converter Instance = new UInt16Converter();
            public override long ConvertToLong(ushort source) => source;
            public override ushort ConvertFromLong(long source) => (ushort)source;
        }

        private class Int32Converter : Int64ConvertibleConverter<int>
        {
            public static readonly Int32Converter Instance = new Int32Converter();
            public override long ConvertToLong(int source) => source;
            public override int ConvertFromLong(long source) => (int)source;
        }

        private class UInt32Converter : Int64ConvertibleConverter<uint>
        {
            public static readonly UInt32Converter Instance = new UInt32Converter();
            public override long ConvertToLong(uint source) => source;
            public override uint ConvertFromLong(long source) => (uint)source;
        }

        private class DateTimeConverter : Int64ConvertibleConverter<DateTime>
        {
            public static readonly DateTimeConverter Instance = new DateTimeConverter();
            public override long ConvertToLong(DateTime source) => source.Ticks;
            public override DateTime ConvertFromLong(long source) => new DateTime(source);
        }

        private class DateOnlyConverter : Int64ConvertibleConverter<DateOnly>
        {
            public static readonly DateOnlyConverter Instance = new DateOnlyConverter();
            public override long ConvertToLong(DateOnly source) => source.ToDateTime(default).Ticks;
            public override DateOnly ConvertFromLong(long source) => DateOnly.FromDateTime(new DateTime(source));
        }

        private class TimeSpanConverter : Int64ConvertibleConverter<TimeSpan>
        {
            public static readonly TimeSpanConverter Instance = new TimeSpanConverter();
            public override long ConvertToLong(TimeSpan source) => source.Ticks;
            public override TimeSpan ConvertFromLong(long source) => new TimeSpan(source);
        }

        private class TimeOnlyConverter : Int64ConvertibleConverter<TimeOnly>
        {
            public static readonly TimeOnlyConverter Instance = new TimeOnlyConverter();
            public override long ConvertToLong(TimeOnly source) => source.ToTimeSpan().Ticks;
            public override TimeOnly ConvertFromLong(long source) => TimeOnly.FromTimeSpan(new TimeSpan(source));
        }

        private class UInt64Converter : Converter<ulong>
        {
            public static readonly UInt64Converter Instance = new UInt64Converter();
            public override bool TryConvertFrom<TSource>(TSource source, [NotNullWhen(true)] out ulong target)
            {
                switch (source)
                {
                    case ulong v:
                        target = v;
                        return true;
                    case decimal v:
                        if (v >= ulong.MinValue && v <= ulong.MaxValue)
                        {
                            target = (ulong)v;
                            return true;
                        }
                        break;
                    case Decimal64 v:
                        return Convertible.TryConvertTo(v.ToDecimal(), out target);
                    case float v:
                        if (v >= ulong.MinValue && v <= ulong.MaxValue)
                        {
                            target = (ulong)v;
                            return true;
                        }
                        break;
                    case double v:
                        if (v >= ulong.MinValue && v <= ulong.MaxValue)
                        {
                            target = (ulong)v;
                            return true;
                        }
                        break;
                    case string v:
                        if (ulong.TryParse(v, out target))
                        {
                            return true;
                        }
                        break;
                    default:
                        if (Convertible.TryConvertTo(source, out long lval))
                        {
                            target = (ulong)lval;
                            return true;
                        }
                        break;
                }

                target = default!;
                return false;
            }
        }

        private class SingleConverter : Converter<float>
        {
            public static readonly SingleConverter Instance = new SingleConverter();
            public override bool TryConvertFrom<TSource>(TSource source, [NotNullWhen(true)] out float target)
            {
                switch (source)
                {
                    case float v:
                        target = v;
                        return true;
                    case double v:
                        if (v >= float.MinValue && v <= float.MaxValue)
                        {
                            target = (float)v;
                            return true;
                        }
                        break;
                    case decimal v:
                        target = (float)v;
                        return true;
                    case string v:
                        return float.TryParse(v, out target);
                    case Decimal64 v:
                        target = (float)v.ToDecimal();
                        return true;
                    default:
                        if (Convertible.TryConvertTo(source, out long lval))
                        {
                            target = lval;
                            return true;
                        }
                        break;
                }

                target = default;
                return false;
            }
        }

        private class DoubleConverter : Converter<double>
        {
            public static readonly DoubleConverter Instance = new DoubleConverter();
            public override bool TryConvertFrom<TSource>(TSource source, [NotNullWhen(true)] out double target)
            {
                switch (source)
                {
                    case float v:
                        target = v;
                        return true;
                    case double v:
                        target = v;
                        return true;
                    case decimal v:
                        target = (float)v;
                        return true;
                    case string v:
                        return double.TryParse(v, out target);
                    case Decimal64 v:
                        target = (double)v.ToDecimal();
                        return true;
                    default:
                        if (Convertible.TryConvertTo(source, out long lval))
                        {
                            target = lval;
                            return true;
                        }
                        break;
                }

                target = default;
                return false;
            }
        }

        private class DecimalConverter : Converter<decimal>
        {
            public static readonly DecimalConverter Instance = new DecimalConverter();
            public override bool TryConvertFrom<TSource>(TSource source, [NotNullWhen(true)] out decimal target)
            {
                switch (source)
                {
                    case ulong v:
                        target = v;
                        return true;
                    case float v:
                        if (v >= (float)decimal.MinValue && v <= (float)decimal.MaxValue)
                        {
                            target = (decimal)v;
                            return true;
                        }
                        break;
                    case double v:
                        if (v >= (double)decimal.MinValue && v <= (double)decimal.MaxValue)
                        {
                            target = (decimal)v;
                            return true;
                        }
                        break;
                    case decimal v:
                        target = v;
                        return true;
                    case string s:
                        return decimal.TryParse(s, out target);
                    case Decimal64 v:
                        target = v.ToDecimal();
                        return true;
                    default:
                        if (Convertible.TryConvertTo(source, out long lval))
                        {
                            target = lval;
                            return true;
                        }
                        break;
                }

                target = default;
                return false;
            }
        }

        private class Decimal64Converter : Converter<Decimal64>
        {
            public static readonly Decimal64Converter Instance = new Decimal64Converter();

            public override bool TryConvertTo<TTarget>(Decimal64 source, [NotNullWhen(true)] out TTarget target)
            {
                return Convertible.TryConvertTo(source.ToDecimal(), out target);
            }

            public override bool TryConvertFrom<TSource>(TSource source, [NotNullWhen(true)] out Decimal64 target)
            {
                switch (source)
                {
                    case Decimal64 v:
                        target = v;
                        return true;
                    case decimal v:
                        return Decimal64.TryCreate(v, out target);
                    default:
                        if (Convertible.TryConvertTo(source, out decimal dval))
                        {
                            return Decimal64.TryCreate(dval, out target);
                        }
                        break;
                }
                target = default;
                return false;
            }
        }

        private class CharConverter : Converter<char>
        {
            public static readonly CharConverter Instance = new CharConverter();
            public override bool TryConvertFrom<TSource>(TSource source, [NotNullWhen(true)] out char target)
            {
                switch (source)
                {
                    case char v:
                        target = v;
                        return true;
                    case string s when s.Length == 1:
                        target = s[0];
                        return true;
                    case Rune v:
                        Span<char> chars = stackalloc char[2];
                        int written = v.EncodeToUtf16(chars);
                        if (written == 1)
                        {
                            target = chars[0];
                            return true;
                        }
                        break;
                    default:
                        if (Convertible.TryConvertTo(source, out long lval)
                            && lval >= char.MinValue && lval <= char.MaxValue)
                        {
                            target = (char)lval;
                            return true;
                        }
                        break;
                }
                target = default;
                return false;
            }
        }

        private class RuneConverter : Converter<Rune>
        {
            public static readonly RuneConverter Instance = new RuneConverter();
            public override bool TryConvertFrom<TSource>(TSource source, [NotNullWhen(true)] out Rune target)
            {
                switch (source)
                {
                    case Rune v:
                        target = v;
                        return true;
                    case char v:
                        target = new Rune(v);
                        return true;
                    case string s:
                        if (Rune.DecodeFromUtf16(s, out target, out var consumed) == System.Buffers.OperationStatus.Done
                            && consumed == s.Length)
                        {
                            return true;
                        }
                        break;
                    case long v when v >= char.MinValue && v <= char.MaxValue:
                        target = new Rune((int)v);
                        return true;
                    default:
                        if (Convertible.TryConvertTo(source, out string sval))
                        {
                            return Convertible.TryConvertTo(sval, out target);
                        }
                        break;
                }
                target = default;
                return false;
            }
        }

        private class StringConverter : Converter<string>
        {
            public static readonly StringConverter Instance = new StringConverter();
            public override bool TryConvertTo<TTarget>(string source, [NotNullWhen(true)] out TTarget target)
            {
                if (TypeParser<TTarget>.TryGetParser(out var parser))
                {
                    return parser.TryParse(source, out target);
                }

                return base.TryConvertTo(source, out target);
            }

            public override bool TryConvertFrom<TValue>(TValue source, [NotNullWhen(true)] out string target)
            {
                target = source?.ToString() ?? string.Empty;
                return true;
            }
        }

        private abstract class TypeParser<T>
        {
            private static readonly Type? _parserType;
            private static TypeParser<T>? _parser;

            static TypeParser()
            {
                var type = typeof(T);
                var ifaces = type.GetInterfaces();

                if (ifaces.Any(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IParsable<>)))
                {
                    _parserType = typeof(StringParsableParser<>).MakeGenericType(typeof(T));
                }
                else if (ifaces.Any(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(ISpanParsable<>)))
                {
                    _parserType = typeof(SpanParsableParser<>).MakeGenericType(typeof(T));
                }
            }

            public static bool TryGetParser([NotNullWhen(true)] out TypeParser<T>? parser)
            {
                if (_parser == null && _parserType != null)
                {
                    var newParser = (TypeParser<T>)Activator.CreateInstance(_parserType)!;
                    Interlocked.CompareExchange(ref _parser, newParser, null);
                }

                parser = _parser;
                return parser is not null;
            }

            public abstract bool TryParse(string text, out T value);
        }

        private sealed class StringParsableParser<T> : TypeParser<T>
            where T : IParsable<T>
        {
            public override bool TryParse(string text, out T value) =>
                T.TryParse(text, null, out value!);
        }

        private sealed class SpanParsableParser<T> : TypeParser<T>
            where T : ISpanParsable<T>
        {
            public override bool TryParse(string text, out T value) =>
                T.TryParse(text, null, out value!);
        }
    }

#if false
    /// <summary>
    /// A class to access values held in variants by converting to other types if possible.
    /// </summary>
    public static class VariantConvert
    {
        /// <summary>
        /// Returns the variant converted to the specified type, or throws <see cref="InvalidCastException"/> if it cannot be converted.
        /// </summary>
        public static T ConvertTo<T>(this in Variant variant) =>
            TryConvertTo<T>(variant, out var value)
                ? value
                : throw new InvalidCastException();

        /// <summary>
        /// Returns true if the variant can be converted to the specified type.
        /// </summary>
        public static bool TryConvertTo<T>(this in Variant variant, out T value)
        {
            var ttype = typeof(T);

            if (ttype.IsEnum)
            {
                return TryConvertToEnum(in variant, out value);
            }

            switch (Type.GetTypeCode(ttype))
            {
                case TypeCode.Boolean:
                    if (TryConvertToBool(in variant, out var boolValue)
                        && boolValue is T tboolValue)
                    {
                        value = tboolValue;
                        return true;
                    }
                    break;
                case TypeCode.SByte:
                    if (TryConvertToSByte(in variant, out var sbyteValue)
                        && sbyteValue is T tsbyteValue)
                    {
                        value = tsbyteValue;
                        return true;
                    }
                    break;
                case TypeCode.Byte:
                    if (TryConvertToByte(in variant, out var byteValue)
                        && byteValue is T tbyteValue)
                    {
                        value = tbyteValue;
                        return true;
                    }
                    break;
                case TypeCode.Int16:
                    if (TryConvertToInt16(in variant, out var shortValue)
                        && shortValue is T tshortValue)
                    {
                        value = tshortValue;
                        return true;
                    }
                    break;
                case TypeCode.UInt16:
                    if (TryConvertToUInt16(in variant, out var ushortValue)
                        && ushortValue is T tushortValue)
                    {
                        value = tushortValue;
                        return true;
                    }
                    break;
                case TypeCode.Int32:
                    if (TryConvertToInt32(in variant, out var intValue)
                        && intValue is T tintValue)
                    {
                        value = tintValue;
                        return true;
                    }
                    break;
                case TypeCode.UInt32:
                    if (TryConvertToUInt32(in variant, out var uintValue)
                        && uintValue is T tuintValue)
                    {
                        value = tuintValue;
                        return true;
                    }
                    break;
                case TypeCode.Int64:
                    if (TryConvertToInt64(in variant, out var longValue)
                        && longValue is T tlongValue)
                    {
                        value = tlongValue;
                        return true;
                    }
                    break;
                case TypeCode.UInt64:
                    if (TryConvertToUInt64(in variant, out var ulongValue)
                        && ulongValue is T tulongValue)
                    {
                        value = tulongValue;
                        return true;
                    }
                    break;
                case TypeCode.Single:
                    if (TryConvertToSingle(in variant, out var floatValue)
                        && floatValue is T tfloatValue)
                    {
                        value = tfloatValue;
                        return true;
                    }
                    break;
                case TypeCode.Double:
                    if (TryConvertToDouble(in variant, out var doubleValue)
                        && doubleValue is T tdoubleValue)
                    {
                        value = tdoubleValue;
                        return true;
                    }
                    break;
                case TypeCode.Decimal:
                    if (TryConvertToDecimal(in variant, out var decimalValue)
                        && decimalValue is T tdecimalValue)
                    {
                        value = tdecimalValue;
                        return true;
                    }
                    break;
                case TypeCode.Char:
                    if (TryConvertToChar(in variant, out var charValue)
                        && charValue is T tcharValue)
                    {
                        value = tcharValue;
                        return true;
                    }
                    break;
                case TypeCode.String:
                    if (variant.ToString() is T tstringValue)
                    {
                        value = tstringValue;
                        return true;
                    }
                    break;
                case TypeCode.Object:
                    if (ttype == typeof(Variant)
                        && variant is T tvariant)
                    {
                        value = tvariant;
                        return true;
                    }
                    else if (ttype == typeof(Decimal64)
                        && TryConvertToDecimal(in variant, out decimalValue)
                        && Decimal64.TryCreate(decimalValue, out var dec64Value)
                        && dec64Value is T tdec64Value)
                    {
                        value = tdec64Value;
                        return true;
                    }
                    else if (ttype == typeof(Rune)
                        && TryConvertToRune(in variant, out var runeValue)
                        && runeValue is T truneValue)
                    {
                        value = truneValue;
                        return true;
                    }
                    else if (ttype == typeof(DateTime)
                        && TryConvertToDateTime(in variant, out var dtValue)
                        && dtValue is T tdtValue)
                    {
                        value = tdtValue;
                        return true;
                    }
                    else if (ttype == typeof(TimeSpan)
                        && TryConvertToTimeSpan(in variant, out var tsValue)
                        && tsValue is T ttsValue)
                    {
                        value = ttsValue;
                        return true;
                    }
                    else if (ttype == typeof(DateOnly)
                        && TryConvertToDateOnly(in variant, out var doValue)
                        && doValue is T tdoValue)
                    {
                        value = tdoValue;
                        return true;
                    }
                    else if (ttype == typeof(TimeOnly)
                        && TryConvertToTimeOnly(in variant, out var toValue)
                        && toValue is T ttoValue)
                    {
                        value = ttoValue;
                        return true;
                    }
                    else if (variant.Type == typeof(string)
                        && TypeParser<T>.TryGetParser(out var parser))
                    {
                        return parser.TryParse(variant.ToString(), out value);
                    }
                    break;
            }

            return variant.TryGet(out value);
        }

        private static bool TryConvertToBool(in Variant variant, out bool value)
        {
            if (TryConvertToInt64(in variant, out var longValue))
            {
                value = longValue != 0;
                return true;
            }

            value = default;
            return false;
        }

        private static bool TryConvertToInt64(in Variant variant, out long value)
        {
            var vtype = variant.Type;

            if (vtype.IsEnum)
            {
                var reader = EnumConverter.GetConverter(vtype);
                value = reader.ConvertToLong(in variant);
                return true;
            }

            switch (Type.GetTypeCode(vtype))
            {
                case TypeCode.Boolean:
                    value = variant.Get<bool>() ? 1 : 0;
                    return true;
                case TypeCode.SByte:
                    value = variant.Get<sbyte>();
                    return true;
                case TypeCode.Byte:
                    value = variant.Get<byte>();
                    return true;
                case TypeCode.Int16:
                    value = variant.Get<short>();
                    return true;
                case TypeCode.UInt16:
                    value = variant.Get<ushort>();
                    return true;
                case TypeCode.Int32:
                    value = variant.Get<int>();
                    return true;
                case TypeCode.UInt32:
                    value = variant.Get<uint>();
                    return true;
                case TypeCode.Int64:
                    value = variant.Get<long>();
                    return true;
                case TypeCode.UInt64:
                    var ulongValue = variant.Get<ulong>();
                    if (ulongValue <= long.MaxValue)
                    {
                        value = (long)ulongValue;
                        return true;
                    }
                    break;
                case TypeCode.Char:
                    value = variant.Get<char>();
                    return true;
                case TypeCode.Single:
                    var floatValue = variant.Get<float>();
                    if (floatValue >= long.MinValue && floatValue <= long.MaxValue)
                    {
                        value = (long)floatValue;
                        return true;
                    }
                    break;
                case TypeCode.Double:
                    var doubleValue = variant.Get<double>();
                    if (doubleValue >= long.MinValue && doubleValue <= long.MaxValue)
                    {
                        value = (long)doubleValue;
                        return true;
                    }
                    break;
                case TypeCode.Decimal:
                    var decValue = variant.Get<decimal>();
                    if (decValue >= long.MinValue && decValue <= long.MaxValue)
                    {
                        value = (long)decValue;
                        return true;
                    }
                    break;
                case TypeCode.String:
                    var strValue = variant.Get<string>();
                    return Int64.TryParse(strValue, out value);
                case TypeCode.Object:
                    if (vtype == typeof(Decimal64))
                    {
                        decValue = variant.Get<Decimal64>().ToDecimal();
                        if (decValue >= long.MinValue && decValue <= long.MaxValue)
                        {
                            value = (long)decValue;
                            return true;
                        }
                    }
                    else if (vtype == typeof(Rune))
                    {
                        value = variant.Get<Rune>().Value;
                        return true;
                    }
                    break;
            }
            value = default;
            return false;
        }

        private static bool TryConvertToDecimal(in Variant variant, out decimal value)
        {
            var vtype = variant.Type;
            switch (Type.GetTypeCode(vtype))
            {
                case TypeCode.Boolean:
                    value = variant.Get<bool>() ? 1 : 0;
                    return true;
                case TypeCode.SByte:
                    value = variant.Get<sbyte>();
                    return true;
                case TypeCode.Byte:
                    value = variant.Get<byte>();
                    return true;
                case TypeCode.Int16:
                    value = variant.Get<short>();
                    return true;
                case TypeCode.UInt16:
                    value = variant.Get<ushort>();
                    return true;
                case TypeCode.Int32:
                    value = variant.Get<int>();
                    return true;
                case TypeCode.UInt32:
                    value = variant.Get<uint>();
                    return true;
                case TypeCode.Int64:
                    value = variant.Get<long>();
                    return true;
                case TypeCode.UInt64:
                    var ulongValue = variant.Get<ulong>();
                    if (ulongValue <= long.MaxValue)
                    {
                        value = (long)ulongValue;
                        return true;
                    }
                    break;
                case TypeCode.Single:
                    var floatValue = variant.Get<float>();
                    if (floatValue >= (float)decimal.MinValue && floatValue <= (float)decimal.MaxValue)
                    {
                        value = (decimal)floatValue;
                        return true;
                    }
                    break;
                case TypeCode.Double:
                    var doubleValue = variant.Get<double>();
                    if (doubleValue >= (double)decimal.MinValue && doubleValue <= (double)decimal.MaxValue)
                    {
                        value = (decimal)doubleValue;
                        return true;
                    }
                    break;
                case TypeCode.Decimal:
                    value = variant.Get<Decimal>();
                    return true;
                case TypeCode.String:
                    return decimal.TryParse(variant.ToString(), out value);
                case TypeCode.Object:
                    if (vtype == typeof(Decimal64))
                    {
                        value = variant.Get<Decimal64>().ToDecimal();
                        return true;
                    }
                    break;
                default:
                    if (TryConvertToInt64(in variant, out var longValue))
                    {
                        value = longValue;
                        return true;
                    }
                    break;
            }

            value = default;
            return false;
        }

        private static bool TryConvertToDouble(in Variant variant, out double value)
        {
            var vtype = variant.Type;
            switch (Type.GetTypeCode(vtype))
            {
                case TypeCode.Boolean:
                    value = variant.Get<bool>() ? 1 : 0;
                    return true;
                case TypeCode.SByte:
                    value = variant.Get<sbyte>();
                    return true;
                case TypeCode.Byte:
                    value = variant.Get<byte>();
                    return true;
                case TypeCode.Int16:
                    value = variant.Get<short>();
                    return true;
                case TypeCode.UInt16:
                    value = variant.Get<ushort>();
                    return true;
                case TypeCode.Int32:
                    value = variant.Get<int>();
                    return true;
                case TypeCode.UInt32:
                    value = variant.Get<uint>();
                    return true;
                case TypeCode.Int64:
                    value = variant.Get<long>();
                    return true;
                case TypeCode.UInt64:
                    var ulongValue = variant.Get<ulong>();
                    if (ulongValue <= long.MaxValue)
                    {
                        value = (long)ulongValue;
                        return true;
                    }
                    break;
                case TypeCode.Single:
                    value = variant.Get<float>();
                    return true;
                case TypeCode.Double:
                    value = variant.Get<double>();
                    return true;
                case TypeCode.String:
                    return double.TryParse(variant.ToString(), out value);
                default:
                    if (TryConvertToDecimal(in variant, out var decValue))
                    {
                        value = (double)decValue;
                        return true;
                    }
                    break;
            }

            value = default;
            return false;
        }

        private static bool TryConvertToSByte(in Variant variant, out sbyte value)
        {
            if (TryConvertToInt64(in variant, out var longValue)
                && longValue >= sbyte.MinValue && longValue <= sbyte.MaxValue)
            {
                value = (sbyte)longValue;
                return true;
            }

            value = default;
            return false;
        }

        private static bool TryConvertToByte(in Variant variant, out byte value)
        {
            if (TryConvertToInt64(in variant, out var longValue)
                && longValue >= byte.MinValue && longValue <= byte.MaxValue)
            {
                value = (byte)longValue;
                return true;
            }

            value = default;
            return false;
        }

        private static bool TryConvertToInt16(in Variant variant, out short value)
        {
            if (TryConvertToInt64(in variant, out var longValue)
                && longValue >= short.MinValue && longValue <= short.MaxValue)
            {
                value = (short)longValue;
                return true;
            }

            value = default;
            return false;
        }

        private static bool TryConvertToUInt16(in Variant variant, out ushort value)
        {
            if (TryConvertToInt64(in variant, out var longValue)
                && longValue >= ushort.MinValue && longValue <= ushort.MaxValue)
            {
                value = (ushort)longValue;
                return true;
            }

            value = default;
            return false;
        }

        private static bool TryConvertToInt32(in Variant variant, out int value)
        {
            if (TryConvertToInt64(in variant, out var longValue)
                && longValue >= int.MinValue && longValue <= int.MaxValue)
            {
                value = (int)longValue;
                return true;
            }

            value = default;
            return false;
        }

        private static bool TryConvertToUInt32(in Variant variant, out uint value)
        {
            if (TryConvertToInt64(in variant, out var longValue)
                && longValue >= uint.MinValue && longValue <= uint.MaxValue)
            {
                value = (uint)longValue;
                return true;
            }

            value = default;
            return false;
        }

        private static bool TryConvertToUInt64(in Variant variant, out ulong value)
        {
            var vtype = variant.Type;
            switch (Type.GetTypeCode(vtype))
            {
                case TypeCode.UInt64:
                    value = variant.Get<UInt64>();
                    return true;
                case TypeCode.String:
                    return ulong.TryParse(variant.ToString(), out value);
                default:
                    if (TryConvertToDecimal(in variant, out var decimalValue)
                        && decimalValue >= ulong.MinValue && decimalValue <= ulong.MaxValue)
                    {
                        value = (ulong)decimalValue;
                        return true;
                    }
                    break;
            }

            value = default;
            return false;
        }

        private static bool TryConvertToSingle(in Variant variant, out float value)
        {
            if (TryConvertToDouble(in variant, out var doubleValue)
                && doubleValue >= float.MinValue && doubleValue <= float.MaxValue)
            {
                value = (float)doubleValue;
                return true;
            }

            value = default;
            return false;
        }

        private static bool TryConvertToChar(in Variant variant, out char value)
        {
            if (TryConvertToInt64(in variant, out var longValue)
                && longValue >= char.MinValue && longValue <= char.MaxValue)
            {
                value = (char)longValue;
                return true;
            }
            else if (variant.Type == typeof(string))
            {
                var strValue = variant.ToString();
                if (strValue.Length == 1)
                {
                    value = strValue[0];
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static bool TryConvertToRune(in Variant variant, out Rune value)
        {
            if (TryConvertToInt32(in variant, out var intValue)
                && Rune.IsValid(intValue))
            {
                value = new Rune(intValue);
                return true;
            }
            else if (variant.Type == typeof(string))
            {
                var strValue = variant.ToString();
                if (Rune.DecodeFromUtf16(strValue, out value, out var count) == System.Buffers.OperationStatus.Done
                    && count == strValue.Length)
                {
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static bool TryConvertToDateTime(in Variant variant, out DateTime value)
        {
            if (variant.TryGet(out value))
            {
                return true;
            }
            else if (variant.TryGet<TimeSpan>(out var tsValue))
            {
                value = new DateTime(tsValue.Ticks);
                return true;
            }
            else if (variant.TryGet<DateOnly>(out var doValue))
            {
                value = doValue.ToDateTime(default);
                return true;
            }
            else if (variant.TryGet<TimeOnly>(out var toValue))
            {
                value = new DateTime(toValue.Ticks);
                return true;
            }
            else if (variant.TryGet<string>(out var strValue))
            {
                return DateTime.TryParse(strValue, out value);
            }

            value = default!;
            return false;
        }

        private static bool TryConvertToTimeSpan(in Variant variant, out TimeSpan value)
        {
            if (variant.TryGet(out value))
            {
                return true;
            }
            else if (variant.TryGet<DateTime>(out var dtValue))
            {
                value = new TimeSpan(dtValue.Ticks);
                return true;
            }
            else if (variant.TryGet<DateOnly>(out var doValue))
            {
                value = TimeSpan.MinValue;
                return true;
            }
            else if (variant.TryGet<TimeOnly>(out var toValue))
            {
                value = new TimeSpan(toValue.Ticks);
                return true;
            }
            else if (variant.TryGet<string>(out var strValue))
            {
                return TimeSpan.TryParse(strValue, out value);
            }

            value = default!;
            return false;
        }

        private static bool TryConvertToDateOnly(in Variant variant, out DateOnly value)
        {
            if (variant.TryGet(out value))
            {
                return true;
            }
            else if (variant.TryGet<DateTime>(out var dtValue))
            {
                value = DateOnly.FromDateTime(dtValue);
                return true;
            }
            else if (variant.TryGet<TimeSpan>(out var tsValue))
            {
                value = DateOnly.FromDayNumber(tsValue.Days);
                return true;
            }
            else if (variant.TryGet<TimeOnly>(out var toValue))
            {
                value = DateOnly.MinValue;
                return true;
            }
            else if (variant.TryGet<string>(out var strValue))
            {
                return DateOnly.TryParse(strValue, out value);
            }

            value = default!;
            return false;
        }

        private static bool TryConvertToTimeOnly(in Variant variant, out TimeOnly value)
        {
            if (variant.TryGet(out value))
            {
                return true;
            }
            else if (variant.TryGet<DateTime>(out var dtValue))
            {
                value = TimeOnly.FromDateTime(dtValue);
                return true;
            }
            else if (variant.TryGet<TimeSpan>(out var tsValue))
            {
                value = TimeOnly.FromTimeSpan(tsValue);
                return true;
            }
            else if (variant.TryGet<DateOnly>(out var toValue))
            {
                value = TimeOnly.MinValue;
                return true;
            }
            else if (variant.TryGet<string>(out var strValue))
            {
                return TimeOnly.TryParse(strValue, out value);
            }

            value = default!;
            return false;
        }

        private static bool TryConvertToEnum<TEnum>(in Variant variant, out TEnum value)
        {
            if (variant.Type == typeof(TEnum))
            {
                value = variant.Get<TEnum>();
                return true;
            }
            else if (variant.Type.IsEnum
                || variant.Type == typeof(string))
            {
                return EnumConverter.GetConverter<TEnum>().TryParse(variant.ToString(), out value);
            }
            else if (TryConvertToInt64(in variant, out var longValue))
            {
                return EnumConverter<TEnum>.TryConvertLongToEnum(longValue, out value);
            }
            else if (TryConvertToUInt64(in variant, out var ulongValue))
            {
                return EnumConverter<TEnum>.TryConvertLongToEnum(unchecked((long)ulongValue), out value);
            }

            value = default!;
            return false;
        }

        private abstract class EnumConverter
        {
            public abstract long ConvertToLong(in Variant variant);

            public static EnumConverter GetConverter(Type enumType)
            {
                if (!s_converterMap.TryGetValue(enumType, out var converter))
                {
                    var converterType = typeof(EnumConverterImpl<>).MakeGenericType(enumType);
                    var newConverter = (EnumConverter)Activator.CreateInstance(converterType)!;
                    converter = ImmutableInterlocked.GetOrAdd(ref s_converterMap, enumType, newConverter);
                }

                return converter;
            }

            public static EnumConverter<TEnum> GetConverter<TEnum>() =>
                (EnumConverter<TEnum>)GetConverter(typeof(TEnum));

            private static ImmutableDictionary<Type, EnumConverter> s_converterMap
                = ImmutableDictionary<Type, EnumConverter>.Empty;
        }

        private abstract class EnumConverter<TEnum> : EnumConverter
        {
            public override long ConvertToLong(in Variant variant) =>
                ConvertEnumToLong(variant.Get<TEnum>());

            public abstract bool TryParse(string text, out TEnum value);

            private static long ConvertEnumToLong(TEnum value)
            {
                var enumType = typeof(TEnum);
                var underlyngType = enumType.GetEnumUnderlyingType();
                return Type.GetTypeCode(underlyngType) switch
                {
                    TypeCode.SByte => Unsafe.As<TEnum, sbyte>(ref value),
                    TypeCode.Int16 => Unsafe.As<TEnum, short>(ref value),
                    TypeCode.Int32 => Unsafe.As<TEnum, int>(ref value),
                    TypeCode.Int64 => Unsafe.As<TEnum, long>(ref value),
                    TypeCode.Byte => Unsafe.As<TEnum, byte>(ref value),
                    TypeCode.UInt16 => Unsafe.As<TEnum, ushort>(ref value),
                    TypeCode.UInt32 => Unsafe.As<TEnum, uint>(ref value),
                    TypeCode.UInt64 => unchecked((long)Unsafe.As<TEnum, ulong>(ref value)),
                    _ => 0
                };
            }

            public static bool TryConvertLongToEnum(long value, [NotNullWhen(true)] out TEnum enumValue)
            {
                var enumType = typeof(TEnum);
                if (enumType.IsEnum)
                {
                    var underlyingType = enumType.GetEnumUnderlyingType();
                    switch (Type.GetTypeCode(underlyingType))
                    {
                        case TypeCode.SByte when value >= sbyte.MinValue && value <= sbyte.MaxValue:
                            var i8val = (sbyte)value;
                            enumValue = Unsafe.As<sbyte, TEnum>(ref i8val)!;
                            return true;
                        case TypeCode.Int16 when value >= short.MinValue && value <= short.MaxValue:
                            var i16val = (short)value;
                            enumValue = Unsafe.As<short, TEnum>(ref i16val)!;
                            return true;
                        case TypeCode.Int32 when value >= int.MinValue && value <= short.MaxValue:
                            var i32val = (int)value;
                            enumValue = Unsafe.As<int, TEnum>(ref i32val)!;
                            return true;
                        case TypeCode.Int64:
                            enumValue = Unsafe.As<long, TEnum>(ref value)!;
                            return true;
                        case TypeCode.Byte when value >= byte.MinValue && value <= byte.MaxValue:
                            var ui8val = (byte)value;
                            enumValue = Unsafe.As<byte, TEnum>(ref ui8val)!;
                            return true;
                        case TypeCode.UInt16 when value >= ushort.MinValue && value <= ushort.MaxValue:
                            var ui16val = (ushort)value;
                            enumValue = Unsafe.As<ushort, TEnum>(ref ui16val)!;
                            return true;
                        case TypeCode.UInt32 when value >= uint.MinValue && value <= uint.MaxValue:
                            var ui32val = (uint)value;
                            enumValue = Unsafe.As<uint, TEnum>(ref ui32val)!;
                            return true;
                        case TypeCode.UInt64:
                            var ui64val = unchecked((ulong)value);
                            enumValue = Unsafe.As<ulong, TEnum>(ref ui64val)!;
                            return true;
                    };
                }

                enumValue = default!;
                return false;
            }
        }

        private sealed class EnumConverterImpl<TEnum> : EnumConverter<TEnum>
            where TEnum : struct, System.Enum
        {
            public override bool TryParse(string text, out TEnum value) =>
                Enum.TryParse<TEnum>(text, ignoreCase: true, out value);
        }

        private abstract class TypeParser<T>
        {
            private static readonly Type? _parserType;
            private static TypeParser<T>? _instance;

            static TypeParser()
            {
                if (typeof(T).IsAssignableTo(typeof(IParsable<>).MakeGenericType(typeof(T))))
                {
                    _parserType = typeof(StringParsableParser<>).MakeGenericType(typeof(T));
                }
                else if (typeof(T).IsAssignableTo(typeof(ISpanParsable<>).MakeGenericType(typeof(T))))
                {
                    _parserType = typeof(SpanParsableParser<>).MakeGenericType(typeof(T));
                }
            }

            public static bool TryGetParser([NotNullWhen(true)] out TypeParser<T>? parser)
            {
                if (_instance == null && _parserType != null)
                {
                    var newParser = (TypeParser<T>)Activator.CreateInstance(_parserType)!;
                    Interlocked.CompareExchange(ref _instance, newParser, null);
                }

                parser = _instance;
                return parser is not null;
            }

            public abstract bool TryParse(string text, out T value);
        }

        private sealed class StringParsableParser<T> : TypeParser<T>
            where T : IParsable<T>
        {
            public override bool TryParse(string text, out T value) =>
                T.TryParse(text, null, out value!);
        }

        private sealed class SpanParsableParser<T> : TypeParser<T>
            where T : ISpanParsable<T>
        {
            public override bool TryParse(string text, out T value) =>
                T.TryParse(text, null, out value!);
        }
    }
#endif
}
