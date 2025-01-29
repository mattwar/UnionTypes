using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace UnionTypes.Toolkit
{
    public static partial class TypeUnion
    {
        /// <summary>
        /// Converts from value in the source union to the target type,
        /// or converts the value to a value that fits in the target union type.
        /// </summary>
        public static bool TryConvert<TSource, TTarget>(TSource source, [NotNullWhen(true)] out TTarget target)
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
            private static Converter<TType> _converter = null!;

            public static Converter<TType> GetConverter()
            {
                if (_converter == null)
                {
                    Interlocked.CompareExchange(ref _converter, CreateConverter(), null);
                }

                return _converter;
            }

            private static Converter<TType> CreateConverter()
            {
                var type = typeof(TType);
                var ifaces = type.GetInterfaces();

                if (ifaces.Any(iface =>
                    iface.IsGenericType
                    && iface.GetGenericTypeDefinition() == typeof(ITypeUnion<>)))
                {
                    return (Converter<TType>)Activator.CreateInstance(typeof(TypeUnionTConverter<>).MakeGenericType(typeof(TType))!)!;
                }
                else if (ifaces.Any(iface =>
                    iface.IsGenericType
                    && iface.GetGenericTypeDefinition() == typeof(ITypeUnion)))
                {
                    return (Converter<TType>)Activator.CreateInstance(typeof(TypeUnionConverter<>).MakeGenericType(typeof(TType))!)!;
                }
                else if (type.IsEnum)
                {
                    return (Converter<TType>)Activator.CreateInstance(typeof(EnumConverter<>).MakeGenericType(typeof(TType))!)!;
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return (Converter<TType>)Activator.CreateInstance(typeof(NullableConverter<>).MakeGenericType(type.GetGenericArguments()[0])!)!;
                }
                else if (type == typeof(bool))
                    return (Converter<TType>)(object)BoolConverter.Instance;
                else if (type == typeof(byte))
                    return (Converter<TType>)(object)ByteConverter.Instance;
                else if (type == typeof(sbyte))
                    return (Converter<TType>)(object)SByteConverter.Instance;
                else if (type == typeof(short))
                    return (Converter<TType>)(object)Int16Converter.Instance;
                else if (type == typeof(ushort))
                    return (Converter<TType>)(object)UInt16Converter.Instance;
                else if (type == typeof(int))
                    return (Converter<TType>)(object)Int32Converter.Instance;
                else if (type == typeof(uint))
                    return (Converter<TType>)(object)UInt32Converter.Instance;
                else if (type == typeof(long))
                    return (Converter<TType>)(object)Int64Converter.Instance;
                else if (type == typeof(ulong))
                    return (Converter<TType>)(object)UInt64Converter.Instance;
                else if (type == typeof(float))
                    return (Converter<TType>)(object)SingleConverter.Instance;
                else if (type == typeof(double))
                    return (Converter<TType>)(object)DoubleConverter.Instance;
                else if (type == typeof(decimal))
                    return (Converter<TType>)(object)DecimalConverter.Instance;
                else if (type == typeof(Decimal64))
                    return (Converter<TType>)(object)Decimal64Converter.Instance;
                else if (type == typeof(String))
                    return (Converter<TType>)(object)StringConverter.Instance;
                else if (type == typeof(DateTime))
                    return (Converter<TType>)(object)DateTimeConverter.Instance;
                else if (type == typeof(DateOnly))
                    return (Converter<TType>)(object)DateOnlyConverter.Instance;
                else if (type == typeof(TimeSpan))
                    return (Converter<TType>)(object)TimeSpanConverter.Instance;
                else if (type == typeof(TimeOnly))
                    return (Converter<TType>)(object)TimeOnlyConverter.Instance;
                else
                {
                    return (Converter<TType>)Activator.CreateInstance(typeof(ReferenceConverter<>).MakeGenericType(typeof(TType))!)!;
                }
            }
        }

        private abstract class Converter<TType>
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
                        var caseConverter = TargetConverters<TTarget>.GetCaseGetConverter(source.Type);
                        if (caseConverter.TryGetAndConvert(source, out target))
                            return true;
                    }

                    // last resort, try getting as object (possible boxing) and
                    // converting that to target
                    if (source.TryGet(out object objValue))
                    {
                        return TypeUnion.TryConvert(objValue, out target);
                    }
                }

                target = default!;
                return false;
            }

            private static class TargetConverters<TTarget>
            {
                private static ImmutableDictionary<Type, CaseGetConverter> _caseGetConverters =
                    ImmutableDictionary<Type, CaseGetConverter>.Empty;

                public static CaseGetConverter<TTarget> GetCaseGetConverter(Type caseType)
                {
                    if (!_caseGetConverters.TryGetValue(caseType, out var converter))
                    {
                        var converterType = typeof(CaseGetConverter<,>).MakeGenericType(typeof(TUnion), caseType, typeof(TTarget));
                        var newConverter = (CaseGetConverter)Activator.CreateInstance(converterType)!;
                        converter = ImmutableInterlocked.GetOrAdd(ref _caseGetConverters, caseType, newConverter);
                    }

                    return (CaseGetConverter<TTarget>)converter!;
                }
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
                        return TypeUnion.TryConvert(caseValue, out target);
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
                    var creator = SourceConverters<TSource>.GetCaseConvertCreator(tcase);
                    if (creator.TryCreate(source, out target))
                        return true;
                }

                return false;
            }

            private static class SourceConverters<TSource>
            {
                private static ImmutableDictionary<Type, CaseConvertCreator> _caseConvertCreators =
                    ImmutableDictionary<Type, CaseConvertCreator>.Empty;

                public static CaseConvertCreator<TSource> GetCaseConvertCreator(Type caseType)
                {
                    if (!_caseConvertCreators.TryGetValue(caseType, out var converter))
                    {
                        var converterType = typeof(CaseConvertCreator<,>).MakeGenericType(typeof(TUnion), typeof(TSource), caseType);
                        var newConverter = (CaseConvertCreator)Activator.CreateInstance(converterType)!;
                        converter = ImmutableInterlocked.GetOrAdd(ref _caseConvertCreators, caseType, newConverter);
                    }

                    return (CaseConvertCreator<TSource>)converter!;
                }
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
                    if (TypeUnion.TryConvert(source, out TCase tcase))
                        return TUnion.TryCreate(tcase, out target);
                    target = default!;
                    return false;
                }
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

                return TypeUnion.TryConvert(lval, out target);
            }

            public override bool TryConvertFrom<TValue>(TValue source, [NotNullWhen(true)] out TEnum target)
            {
                if (source is long lval || TypeUnion.TryConvert(source, out lval))
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
                else if ((source is string sval || TypeUnion.TryConvert(source, out sval))
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
                    return TypeUnion.TryConvert(source.Value, out target);
                }

                target = default!;
                return false;
            }

            public override bool TryConvertFrom<TSource>(TSource source, [NotNullWhen(true)] out TType? target)
            {
                if (TypeUnion.TryConvert(source, out TType tvalue))
                {
                    target = tvalue;
                    return true;
                }

                target = default;
                return false;
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
                        return TypeUnion.TryConvert(v.ToDecimal(), out target);
                    case Rune v:
                        target = v.Value;
                        return true;
                }

                target = default;
                return false;
            }
        }

        /// <summary>
        /// A converter base class for types that can easily convert themselves to/from <see cref="Int64"/>.
        /// </summary>
        private abstract class Int64SubConverter<T> : Converter<T>
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
                    return TypeUnion.TryConvert(lval, out target);
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
                        if (TypeUnion.TryConvert(source, out long lval))
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

        private class BoolConverter : Int64SubConverter<bool>
        {
            public static readonly BoolConverter Instance = new BoolConverter();
            public override long ConvertToLong(bool source) => source ? 1 : 0;
            public override bool ConvertFromLong(long source) => source != 0;
        }

        private class ByteConverter : Int64SubConverter<byte>
        {
            public static readonly ByteConverter Instance = new ByteConverter();
            public override long ConvertToLong(byte source) => source;
            public override byte ConvertFromLong(long source) => (byte)source;
        }

        private class SByteConverter : Int64SubConverter<sbyte>
        {
            public static readonly SByteConverter Instance = new SByteConverter();
            public override long ConvertToLong(sbyte source) => source;
            public override sbyte ConvertFromLong(long source) => (sbyte)source;
        }

        private class Int16Converter : Int64SubConverter<short>
        {
            public static readonly Int16Converter Instance = new Int16Converter();
            public override long ConvertToLong(short source) => source;
            public override short ConvertFromLong(long source) => (short)source;
        }

        private class UInt16Converter : Int64SubConverter<ushort>
        {
            public static readonly UInt16Converter Instance = new UInt16Converter();
            public override long ConvertToLong(ushort source) => source;
            public override ushort ConvertFromLong(long source) => (ushort)source;
        }

        private class Int32Converter : Int64SubConverter<int>
        {
            public static readonly Int32Converter Instance = new Int32Converter();
            public override long ConvertToLong(int source) => source;
            public override int ConvertFromLong(long source) => (int)source;
        }

        private class UInt32Converter : Int64SubConverter<uint>
        {
            public static readonly UInt32Converter Instance = new UInt32Converter();
            public override long ConvertToLong(uint source) => source;
            public override uint ConvertFromLong(long source) => (uint)source;
        }

        private class DateTimeConverter : Int64SubConverter<DateTime>
        {
            public static readonly DateTimeConverter Instance = new DateTimeConverter();
            public override long ConvertToLong(DateTime source) => source.Ticks;
            public override DateTime ConvertFromLong(long source) => new DateTime(source);
        }

        private class DateOnlyConverter : Int64SubConverter<DateOnly>
        {
            public static readonly DateOnlyConverter Instance = new DateOnlyConverter();
            public override long ConvertToLong(DateOnly source) => source.ToDateTime(default).Ticks;
            public override DateOnly ConvertFromLong(long source) => DateOnly.FromDateTime(new DateTime(source));
        }

        private class TimeSpanConverter : Int64SubConverter<TimeSpan>
        {
            public static readonly TimeSpanConverter Instance = new TimeSpanConverter();
            public override long ConvertToLong(TimeSpan source) => source.Ticks;
            public override TimeSpan ConvertFromLong(long source) => new TimeSpan(source);
        }

        private class TimeOnlyConverter : Int64SubConverter<TimeOnly>
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
                        return TypeUnion.TryConvert(v.ToDecimal(), out target);
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
                        if (TypeUnion.TryConvert(source, out long lval))
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
                        if (TypeUnion.TryConvert(source, out long lval))
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
                        if (TypeUnion.TryConvert(source, out long lval))
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
                        if (TypeUnion.TryConvert(source, out long lval))
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
                return TypeUnion.TryConvert(source.ToDecimal(), out target);
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
                        if (TypeUnion.TryConvert(source, out decimal dval))
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
                        if (TypeUnion.TryConvert(source, out long lval)
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
                        if (TypeUnion.TryConvert(source, out string sval))
                        {
                            return TypeUnion.TryConvert(sval, out target);
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
}
