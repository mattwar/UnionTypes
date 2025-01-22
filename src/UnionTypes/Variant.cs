using Reference = System.Object;
using Bits = System.UInt64;
using System.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using System;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace UnionTypes;

/// <summary>
/// A union of all reference and value types that does not box small structs.
/// </summary>
public readonly struct Variant 
    : ITypeUnion<Variant>, IEquatable<Variant>
{
    private readonly Reference? _reference;
    private readonly OverlappedBits _overlapped;

    private Variant(Reference? reference, OverlappedBits bits)
    {
        _reference = reference;
        _overlapped = bits;
    }

    /// <summary>
    /// The encoding of this variant
    /// </summary>
    private Encoding GetEncoding() =>
        Encoding.GetEncoding(in this);

    /// <summary>
    /// A variant containing the null value.
    /// </summary>
    public static readonly Variant Null = default;

    #region Non-generic API
    /// <summary>
    /// True if the value held in this variant is a boxed value type.
    /// </summary>
    public bool IsBoxed =>
        GetEncoding() is ReferenceEncoding
        && Type.IsValueType;

    /// <summary>
    /// True if the value is null.
    /// </summary>
    public bool IsNull =>
        GetEncoding().IsNull(in this);

    /// <summary>
    /// The type of the value held within this variant.
    /// </summary>
    public Type Type =>
        GetEncoding().GetType(in this);

    /// <summary>
    /// The kind of value contained.
    /// </summary>
    public VariantKind Kind =>
        GetEncoding().GetKind(in this);

    public static Variant Create(bool value) => Encoders.Bool.Encode(value);
    public static Variant Create(bool? value) => (value == null) ? Null : Create(value.Value);
    public static Variant Create(byte value) => Encoders.Byte.Encode(value);
    public static Variant Create(byte? value) => (value == null) ? Null : Create(value.Value);
    public static Variant Create(sbyte value) => Encoders.SByte.Encode(value);
    public static Variant Create(sbyte? value) => (value == null) ? Null : Create(value.Value);
    public static Variant Create(short value) => Encoders.Int16.Encode(value);
    public static Variant Create(short? value) => (value == null) ? Null : Create(value.Value);
    public static Variant Create(ushort value) => Encoders.UInt16.Encode(value);
    public static Variant Create(ushort? value) => (value == null) ? Null : Create(value.Value);
    public static Variant Create(int value) => Encoders.Int32.Encode(value);
    public static Variant Create(int? value) => (value == null) ? Null : Create(value.Value);
    public static Variant Create(uint value) => Encoders.UInt32.Encode(value);
    public static Variant Create(uint? value) => (value == null) ? Null : Create(value.Value);
    public static Variant Create(long value) => Encoders.Int64.Encode(value);
    public static Variant Create(long? value) => (value == null) ? Null : Create(value.Value);
    public static Variant Create(ulong value) => Encoders.UInt64.Encode(value);
    public static Variant Create(ulong? value) => (value == null) ? Null : Create(value.Value);
    public static Variant Create(float value) => Encoders.Single.Encode(value);
    public static Variant Create(float? value) => (value == null) ? Null : Create(value.Value);
    public static Variant Create(double value) => Encoders.Double.Encode(value);
    public static Variant Create(double? value) => (value == null) ? Null : Create(value.Value);
    public static Variant Create(Decimal64 value) => Encoders.Decimal64.Encode(value);
    public static Variant Create(Decimal64? value) => (value == null) ? Null : Create(value.Value);
    public static Variant Create(decimal value) => Encoders.Decimal.Encode(value);
    public static Variant Create(decimal? value) => (value == null) ? Null : Create(value.Value);
    public static Variant Create(char value) => Encoders.Char.Encode(value);
    public static Variant Create(char? value) => (value == null) ? Null : Create(value.Value);
    public static Variant Create(Rune value) => Encoders.Rune.Encode(value);
    public static Variant Create(Rune? value) => (value == null) ? Null : Create(value.Value);
    public static Variant Create(string? value) => (value == null) ? Null : new Variant(value, default);
    public static Variant Create(Guid value) => new Variant(value, default);
    public static Variant Create(Guid? value) => (value == null) ? Null : Create(value.Value);
    public static Variant Create(DateTime value) => Encoders.DateTime.Encode(value);
    public static Variant Create(DateTime? value) => (value == null) ? Null : Create(value.Value);
    public static Variant Create(DateOnly value) => Encoders.DateOnly.Encode(value);
    public static Variant Create(DateOnly? value) => (value == null) ? Null : Create(value.Value);
    public static Variant Create(TimeSpan value) => Encoders.TimeSpan.Encode(value);
    public static Variant Create(TimeSpan? value) => (value == null) ? Null : Create(value.Value);
    public static Variant Create(TimeOnly value) => Encoders.TimeOnly.Encode(value);
    public static Variant Create(TimeOnly? value) => (value == null) ? Null : Create(value.Value);

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.Boolean"/>.
    /// </summary>
    public bool BooleanValue => Encoders.Bool.TryDecode(in this, out var value) ? value : default;

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.SByte"/>.
    /// </summary>
    public sbyte SByteValue => Encoders.SByte.TryDecode(in this, out var value) ? value : default;

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.Int16"/>.
    /// </summary>
    public short Int16Value => Encoders.Int16.TryDecode(in this, out var value) ? value : default;

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.Int32"/>.
    /// </summary>
    public int Int32Value => Encoders.Int32.TryDecode(in this, out var value) ? value : default;

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.Int64"/>.
    /// </summary>
    public long Int64Value => Encoders.Int64.TryDecode(in this, out var value) ? value : default;

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.Byte"/>.
    /// </summary>
    public byte ByteValue => Encoders.Byte.TryDecode(in this, out var value) ? value : default;

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.UInt16"/>.
    /// </summary>
    public ushort UInt16Value => Encoders.UInt16.TryDecode(in this, out var value) ? value : default;

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.UInt32"/>.
    /// </summary>
    public uint UInt32Value => Encoders.UInt32.TryDecode(in this, out var value) ? value : default;

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.UInt64"/>.
    /// </summary>
    public ulong UInt64Value => Encoders.UInt64.TryDecode(in this, out var value) ? value : default;

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.Single"/>.
    /// </summary>
    public float SingleValue => Encoders.Single.TryDecode(in this, out var value) ? value : default;

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.Double"/>.
    /// </summary>
    public double DoubleValue => Encoders.Double.TryDecode(in this, out var value) ? value : default;

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.Decimal64"/>.
    /// </summary>
    public Decimal64 Decimal64Value => Encoders.Decimal64.TryDecode(in this, out var value) ? value : default;

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.Decimal"/>.
    /// </summary>
    public decimal DecimalValue => Encoders.Decimal.TryDecode(in this, out var value) ? value : default;

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.Char"/>.
    /// </summary>
    public char CharValue => Encoders.Char.TryDecode(in this, out var value) ? value : default;

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.Rune"/>.
    /// </summary>
    public Rune RuneValue => Encoders.Rune.TryDecode(in this, out var value) ? value : default;

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.String"/>.
    /// </summary>
    public string StringValue => _reference is string value ? value : default!;

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.DateOnly"/>.
    /// </summary>
    public DateOnly DateOnlyValue => Encoders.DateOnly.TryDecode(in this, out var value) ? value : default;

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.TimeOnly"/>.
    /// </summary>
    public TimeOnly TimeOnlyValue => Encoders.TimeOnly.TryDecode(in this, out var value) ? value : default;

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.DateTime"/>.
    /// </summary>
    public DateTime DateTimeValue => Encoders.DateTime.TryDecode(in this, out var value) ? value : default;

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.TimeSpan"/>.
    /// </summary>
    public TimeSpan TimeSpanValue => Encoders.TimeSpan.TryDecode(in this, out var value) ? value : default;

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.Guid"/>.
    /// </summary>
    public Guid GuidValue => _reference is Guid value ? value : default!;

    /// <summary>
    /// The value of the variant when <see cref="Kind"/> is <see cref="VariantKind.Other"/>.
    /// To get more type specific values without boxing use <see cref="TryGet{T}(out T)"/>.
    /// </summary>
    public object OtherValue => TryGet<object>(out var value) ? value : default!;

    #endregion

    #region generic API
    /// <summary>
    /// Create a <see cref="Variant"/> from a value.
    /// </summary>
    public static Variant Create<TValue>(TValue value)
    {
        if (value == null)
            return Null;

        return Encoder<TValue>.Instance.Encode(value);
    }

    /// <summary>
    /// Create a <see cref="Variant"/> from a value.
    /// </summary>
    public static bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out Variant variant)
    {
        variant = Create(value);
        return true;
    }

    /// <summary>
    /// True if the variant's value can be converted to the type.
    /// </summary>
    public bool CanGet<T>() =>
        GetEncoding().CanConvertTo(in this, typeof(T));

    /// <summary>
    /// Returns <see langword="true"/> and the value as the specified type if the value is of the specified type, otherwise returns <see langword="false"/>.
    /// </summary>
    public bool TryGet<T>([NotNullWhen(true)] out T value) =>
        GetEncoding().TryGet(in this, out value);

    /// <summary>
    /// Returns the value as the specified type if the value is of the specified type, otherwise throws <see cref="InvalidCastException"/>.
    /// </summary>
    public T Get<T>() =>
        TryGet<T>(out var value) ? value : throw GetInvalidCast<T>(this);

    private static Exception GetInvalidCast<T>(Variant variant) => 
        new InvalidCastException($"Cannot convert variant holding type '{variant.Type.Name}' to type '{typeof(T).Name}'");

    /// <summary>
    /// Returns the value as the specified type if the value is of the specified type or the default value of the type if not.
    /// </summary>
    public T GetOrDefault<T>() => TryGet<T>(out var value) ? value! : default!;

    /// <summary>
    /// Returns the value converted to a string.
    /// </summary>
    public override string ToString() =>
        GetEncoding().GetString(in this);

    /// <summary>
    /// Returns true if the value held by the variant is equal to the specified value.
    /// </summary>
    public bool Equals<T>(T value) =>
        GetEncoding().IsEqual(in this, value);

    /// <summary>
    /// Returns true if the value held by the variant is equal to the value held by the specified variant.
    /// </summary>
    public bool Equals(Variant variant) =>
        GetEncoding().IsEqual(in this, in variant);

    /// <summary>
    /// Returns true if the value held by the variant is equal to the specified value.
    /// </summary>
    public override bool Equals([NotNullWhen(true)] object? value) =>
        GetEncoding().IsEqual(in this, value);

    /// <summary>
    /// Returns the hash code of the value held by the variant.
    /// </summary>
    public override int GetHashCode() =>
        GetEncoding().GetHashCode(in this);


    #endregion

    #region Operators
    public static bool operator ==(Variant a, Variant b) => a.Equals(b);
    public static bool operator !=(Variant a, Variant b) => !a.Equals(b);

    public static implicit operator Variant(bool value) => Create(value);
    public static implicit operator Variant(sbyte value) => Create(value);
    public static implicit operator Variant(short value) => Create(value);
    public static implicit operator Variant(int value) => Create(value);
    public static implicit operator Variant(long value) => Create(value);
    public static implicit operator Variant(byte value) => Create(value);
    public static implicit operator Variant(ushort value) => Create(value);
    public static implicit operator Variant(uint value) => Create(value);
    public static implicit operator Variant(ulong value) => Create(value);
    public static implicit operator Variant(float value) => Create(value);
    public static implicit operator Variant(double value) => Create(value);
    public static implicit operator Variant(Decimal64 value) => Create(value);
    public static implicit operator Variant(decimal value) => Create(value);
    public static implicit operator Variant(char value) => Create(value);
    public static implicit operator Variant(Rune value) => Create(value);
    public static implicit operator Variant(string value) => Create(value);
    public static implicit operator Variant(DateOnly value) => Create(value);
    public static implicit operator Variant(TimeOnly value) => Create(value);
    public static implicit operator Variant(DateTime value) => Create(value);
    public static implicit operator Variant(DateTimeOffset value) => Create(value);
    public static implicit operator Variant(TimeSpan value) => Create(value);
    public static implicit operator Variant(Guid value) => Create(value);

    public static implicit operator Variant(bool? value) => Create(value);
    public static implicit operator Variant(sbyte? value) => Create(value);
    public static implicit operator Variant(short? value) => Create(value);
    public static implicit operator Variant(int? value) => Create(value);
    public static implicit operator Variant(long? value) => Create(value);
    public static implicit operator Variant(byte? value) => Create(value);
    public static implicit operator Variant(ushort? value) => Create(value);
    public static implicit operator Variant(uint? value) => Create(value);
    public static implicit operator Variant(ulong? value) => Create(value);
    public static implicit operator Variant(float? value) => Create(value);
    public static implicit operator Variant(double? value) => Create(value);
    public static implicit operator Variant(Decimal64? value) => Create(value);
    public static implicit operator Variant(decimal? value) => Create(value);
    public static implicit operator Variant(char? value) => Create(value);
    public static implicit operator Variant(Rune? value) => Create(value);
    public static implicit operator Variant(DateOnly? value) => Create(value);
    public static implicit operator Variant(TimeOnly? value) => Create(value);
    public static implicit operator Variant(DateTime? value) => Create(value);
    public static implicit operator Variant(DateTimeOffset? value) => Create(value);
    public static implicit operator Variant(TimeSpan? value) => Create(value);
    public static implicit operator Variant(Guid? value) => Create(value);

    public static explicit operator bool(Variant variant) => Encoders.Bool.TryDecode(in variant, out var value) ? value : throw GetInvalidCast<bool>(variant);
    public static explicit operator sbyte(Variant variant) => Encoders.SByte.TryDecode(in variant, out var value) ? value : throw GetInvalidCast<sbyte>(variant);
    public static explicit operator short(Variant variant) => Encoders.Int16.TryDecode(in variant, out var value) ? value : throw GetInvalidCast<short>(variant);
    public static explicit operator int(Variant variant) => Encoders.Int32.TryDecode(in variant, out var value) ? value : throw GetInvalidCast<int>(variant);
    public static explicit operator long(Variant variant) => Encoders.Int64.TryDecode(in variant, out var value) ? value : throw GetInvalidCast<long>(variant);
    public static explicit operator byte(Variant variant) => Encoders.Byte.TryDecode(in variant, out var value) ? value : throw GetInvalidCast<byte>(variant);
    public static explicit operator ushort(Variant variant) => Encoders.UInt16.TryDecode(in variant, out var value) ? value : throw GetInvalidCast<ushort>(variant);
    public static explicit operator uint(Variant variant) => Encoders.UInt32.TryDecode(in variant, out var value) ? value : throw GetInvalidCast<uint>(variant);
    public static explicit operator ulong(Variant variant) => Encoders.UInt64.TryDecode(in variant, out var value) ? value : throw GetInvalidCast<ulong>(variant);
    public static explicit operator float(Variant variant) => Encoders.Single.TryDecode(in variant, out var value) ? value : throw GetInvalidCast<float>(variant);
    public static explicit operator double(Variant variant) => Encoders.Double.TryDecode(in variant, out var value) ? value : throw GetInvalidCast<double>(variant);
    public static explicit operator Decimal64(Variant variant) => Encoders.Decimal64.TryDecode(in variant, out var value) ? value : throw GetInvalidCast<Decimal64>(variant);
    public static explicit operator decimal(Variant variant) => Encoders.Decimal.TryDecode(in variant, out var value) ? value : throw GetInvalidCast<decimal>(variant);
    public static explicit operator char(Variant variant) => Encoders.Char.TryDecode(in variant, out var value) ? value : throw GetInvalidCast<char>(variant);
    public static explicit operator Rune(Variant variant) => Encoders.Rune.TryDecode(in variant, out var value) ? value : throw GetInvalidCast<Rune>(variant);
    public static explicit operator String(Variant variant) => variant._reference is string value ? value : throw GetInvalidCast<string>(variant);
    public static explicit operator DateOnly(Variant variant) => Encoders.DateOnly.TryDecode(in variant, out var value) ? value : throw GetInvalidCast<DateOnly>(variant);
    public static explicit operator TimeOnly(Variant variant) => Encoders.TimeOnly.TryDecode(in variant, out var value) ? value : throw GetInvalidCast<TimeOnly>(variant);
    public static explicit operator DateTime(Variant variant) => Encoders.DateTime.TryDecode(in variant, out var value) ? value : throw GetInvalidCast<DateTime>(variant);
    public static explicit operator TimeSpan(Variant variant) => Encoders.TimeSpan.TryDecode(in variant, out var value) ? value : throw GetInvalidCast<TimeSpan>(variant);
    public static explicit operator Guid(Variant variant) => variant._reference is Guid value ? value : throw GetInvalidCast<Guid>(variant);

    public static explicit operator bool?(Variant value) => value.IsNull ? default : (bool)value;
    public static explicit operator sbyte?(Variant value) => value.IsNull ? default : (sbyte)value;
    public static explicit operator short?(Variant value) => value.IsNull ? default : (short)value;
    public static explicit operator int?(Variant value) => value.IsNull ? default : (int)value;
    public static explicit operator long?(Variant value) => value.IsNull ? default : (long)value;
    public static explicit operator byte?(Variant value) => value.IsNull ? default : (byte)value;
    public static explicit operator ushort?(Variant value) => value.IsNull ? default : (ushort)value;
    public static explicit operator uint?(Variant value) => value.IsNull ? default : (uint)value;
    public static explicit operator ulong?(Variant value) => value.IsNull ? default : (ulong)value;
    public static explicit operator float?(Variant value) => value.IsNull ? default : (float)value;
    public static explicit operator double?(Variant value) => value.IsNull ? default : (double)value;
    public static explicit operator Decimal64?(Variant value) => value.IsNull ? default : (Decimal64)value;
    public static explicit operator decimal?(Variant value) => value.IsNull ? default : (decimal)value;
    public static explicit operator char?(Variant value) => value.IsNull ? default : (char)value;
    public static explicit operator Rune?(Variant value) => value.IsNull ? default : (Rune)value;
    public static explicit operator DateOnly?(Variant value) => value.IsNull ? default : (DateOnly)value;
    public static explicit operator TimeOnly?(Variant value) => value.IsNull ? default : (TimeOnly)value;
    public static explicit operator DateTime?(Variant value) => value.IsNull ? default : (DateTime)value;
    public static explicit operator TimeSpan?(Variant value) => value.IsNull ? default : (TimeSpan)value;
    public static explicit operator Guid?(Variant value) => value.IsNull ? default : (Guid)value;
    #endregion

    #region Encoders

    #region primitive encoders

    private static class Encoders
    {
        private static Encoder<bool> _bool = default!;
        public static Encoder<bool> Bool => _bool ??= Encoder<bool>.Instance;

        private static Encoder<byte> _byte = default!;
        public static Encoder<byte> Byte => _byte ??= Encoder<byte>.Instance;

        private static Encoder<sbyte> _sbyte = default!;
        public static Encoder<sbyte> SByte => _sbyte ??= Encoder<sbyte>.Instance;

        private static Encoder<short> _int16 = default!;
        public static Encoder<short> Int16 => _int16 ??= Encoder<short>.Instance;

        private static Encoder<ushort> _uint16 = default!;
        public static Encoder<ushort> UInt16 => _uint16 ??= Encoder<ushort>.Instance;

        private static Encoder<int> _int32 = default!;
        public static Encoder<int> Int32 => _int32 ??= Encoder<int>.Instance;

        private static Encoder<uint> _uint32 = default!;
        public static Encoder<uint> UInt32 => _uint32 ??= Encoder<uint>.Instance;

        private static Encoder<long> _int64 = default!;
        public static Encoder<long> Int64 => _int64 ??= Encoder<long>.Instance;

        private static Encoder<ulong> _uint64 = default!;
        public static Encoder<ulong> UInt64 => _uint64 ??= Encoder<ulong>.Instance;

        private static Encoder<float> _single = default!;
        public static Encoder<float> Single => _single ??= Encoder<float>.Instance;

        private static Encoder<double> _double = default!;
        public static Encoder<double> Double => _double ??= Encoder<double>.Instance;

        private static Encoder<decimal> _decimal = default!;
        public static Encoder<decimal> Decimal => _decimal ??= Encoder<decimal>.Instance;
        private static Encoder<Decimal64> _decimal64 = default!;
        public static Encoder<Decimal64> Decimal64 => _decimal64 ??= Encoder<Decimal64>.Instance;

        private static Encoder<char> _char = default!;
        public static Encoder<char> Char => _char ??= Encoder<char>.Instance;

        private static Encoder<Rune> _rune = default!;
        public static Encoder<Rune> Rune => _rune ??= Encoder<Rune>.Instance;

        private static Encoder<DateTime> _dateTime = default!;
        public static Encoder<DateTime> DateTime => _dateTime ??= Encoder<DateTime>.Instance;

        private static Encoder<DateOnly> _dateOnly = default!;
        public static Encoder<DateOnly> DateOnly => _dateOnly ??= Encoder<DateOnly>.Instance;

        private static Encoder<TimeSpan> _timeSpan = default!;
        public static Encoder<TimeSpan> TimeSpan => _timeSpan ??= Encoder<TimeSpan>.Instance;

        private static Encoder<TimeOnly> _timeOnly = default!;
        public static Encoder<TimeOnly> TimeOnly => _timeOnly ??= Encoder<TimeOnly>.Instance;
    }

    #endregion

    public abstract class Encoder<TValue>
    {
        private static Encoder<TValue>? _instance;

        public static Encoder<TValue> Instance
        {
            get
            {
                if (_instance == null)
                {
                    var encoder = CreateEncoder();
                    Interlocked.CompareExchange(ref _instance, encoder, null);
                }

                return _instance;
            }
        }

        private static unsafe Encoder<TValue> CreateEncoder()
        {
            var ttype = typeof(TValue);
            if (ttype.IsValueType)
            {
                if (default(TValue) == null)
                {
                    // value is some Nullable<T>
                    var elementType = ttype.GetGenericArguments()[0];
                    var encoderType = typeof(NullableEncoder<>).MakeGenericType(elementType);
                    return (Encoder<TValue>)Activator.CreateInstance(encoderType)!;
                }
                else if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
                {
                    // references, but only room for one, must be wrapper struct
                    if (sizeof(TValue) == sizeof(Reference))
                    {
                        return new WrapperStructEncoder<TValue>();
                    }
                }
                else if (ttype == typeof(decimal))
                {
                    // use decimal encoder to conditionaly fit most common decimals in bits
                    return (Encoder<TValue>)(object)new DecimalEncoder();
                }
                else if (sizeof(TValue) <= sizeof(Bits))
                {
                    // no references and small enough to fit in bits
                    return new BitsEncoder<TValue>();
                }
                else if (ttype == typeof(Variant))
                {
                    // use variant encoder to pass the value right back
                    return (Encoder<TValue>)(object)new VariantVariantEncoder();
                }

                if (ttype.IsAssignableTo(typeof(TypeUnion)))
                {
                    // struct that is a type union, use ITypeUnion interface to access value as a variant.
                    var encoderType = typeof(TypeUnionEncoder<>).MakeGenericType(ttype);
                    return (Encoder<TValue>)Activator.CreateInstance(encoderType)!;
                }
            }

            // Nothing left but being a reference (boxed if a value type)
            return new ReferenceEncoder<TValue>();
        }

        /// <summary>
        /// Encodes a typed value into a <see cref="Variant"/>
        /// </summary>
        public abstract Variant Encode(TValue value);

        /// <summary>
        /// Decodes the variant back to a typed value.
        /// </summary>
        public abstract bool TryDecode(in Variant variant, [NotNullWhen(true)] out TValue value);

        /// <summary>
        /// Returns true if the variant is holding an instance of the typed value.
        /// </summary>
        public abstract bool IsType(in Variant variant);
    }

    /// <summary>
    /// An <see cref="VariantEncoder"/> for values that can be stored in the bits field.
    /// </summary>
    private sealed class BitsEncoder<TValue> : Encoder<TValue>
    {
        private BitsEncoding<TValue> _encoding = default!;
        private BitsEncoding<TValue> Encoding => _encoding ??= BitsEncoding<TValue>.Instance;

        public unsafe BitsEncoder()
        {
            Debug.Assert(typeof(TValue).IsValueType);
            Debug.Assert(default(TValue) != null);
            Debug.Assert(!RuntimeHelpers.IsReferenceOrContainsReferences<TValue>());
            Debug.Assert(sizeof(TValue) <= sizeof(Bits));
        }

        public override Variant Encode(TValue value)
        {
            var bits = Unsafe.As<TValue, Bits>(ref value);
            return new Variant(this.Encoding, new OverlappedBits { _bitsVal = bits });
        }

        public override bool IsType(in Variant variant)
        {
            return variant._reference == this.Encoding
                || variant._reference is TValue
                || variant.GetEncoding() is VariantEncoding<TValue>;
        }

        public override bool TryDecode(in Variant variant, [NotNullWhen(true)] out TValue value)
        {
            if (variant._reference == this.Encoding)
            {
                value = this.Encoding.Decode(variant)!;
                return true;
            }
            else if (variant._reference is TValue tvalue)
            {
                value = tvalue;
                return true;
            }
            else if (variant.GetEncoding() is VariantEncoding<TValue> encoding)
            {
                value = encoding.Decode(variant)!;
                return true;
            }
            else
            {
                value = default!;
                return false;
            }
        }
    }

    /// <summary>
    /// An <see cref="Encoder{TValue}"/> for types that are references or must be boxed.
    /// </summary>
    private sealed class ReferenceEncoder<TValue> : Encoder<TValue>
    {
        public override Variant Encode(TValue value)
        {
            return new Variant(value, default);
        }

        public override bool IsType(in Variant variant)
        {
            return variant._reference is TValue;
        }

        public override bool TryDecode(in Variant variant, [NotNullWhen(true)] out TValue value)
        {
            if (variant._reference is TValue tvalue)
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
    }

    /// <summary>
    /// An <see cref="Encoder{TValue}"/> for types that are struct wrappers around
    /// a single reference.
    /// </summary>
    private sealed class WrapperStructEncoder<TValue> : Encoder<TValue>
    {
        private readonly WrapperStructEncoding<TValue> _encoding =
            WrapperStructEncoding<TValue>.Instance;

        private readonly int _encodingId =
            WrapperStructEncoding<TValue>.Instance.GetId();

        public unsafe WrapperStructEncoder()
        {
            Debug.Assert(typeof(TValue).IsValueType);
            Debug.Assert(default(TValue) != null);
            Debug.Assert(RuntimeHelpers.IsReferenceOrContainsReferences<TValue>());
            Debug.Assert(sizeof(TValue) == sizeof(Reference));
        }

        public override Variant Encode(TValue value)
        {
            // interior pointer from value is stored in the reference field.
            // ID of encoding is stored in the bits field.
            var refValue = Unsafe.As<TValue, Reference>(ref value);
            return new Variant(refValue, new OverlappedBits { _int32Val = _encodingId });
        }

        public override bool IsType(in Variant variant)
        {
            return (variant._overlapped._int32Val == _encodingId
                && !(variant._reference is Encoding))
                || variant.GetEncoding() is VariantEncoding<TValue>;
        }

        public override bool TryDecode(in Variant variant, [NotNullWhen(true)] out TValue value)
        {
            if (variant._overlapped._int32Val == _encodingId
                && !(variant._reference is Encoding))
            {
                value = _encoding.Decode(variant)!;
                return true;
            }
            else if (variant._reference is TValue tvalue)
            {
                value = tvalue;
                return true;
            }
            else if (variant.GetEncoding() is VariantEncoding<TValue> encoding)
            {
                value = encoding.Decode(variant)!;
                return true;
            }
            else
            {
                value = default!;
                return false;
            }
        }
    }

    /// <summary>
    /// An <see cref="Encoder{TValue}"/> for Nullable&lt;T&gt; values.
    /// </summary>
    private sealed class NullableEncoder<TElement> : Encoder<Nullable<TElement>>
        where TElement : struct
    {
        private readonly Encoder<TElement> _encoder =
            Encoder<TElement>.Instance;

        public override Variant Encode(TElement? value)
        {
            if (value == null)
                return Variant.Null;
            return _encoder.Encode(value.GetValueOrDefault());
        }

        public override bool IsType(in Variant variant)
        {
            // does not match type when null
            return _encoder.IsType(variant);
        }

        public override bool TryDecode(in Variant variant, [NotNullWhen(true)] out TElement? value)
        {
            // cannot decode when null
            var success = _encoder.TryDecode(variant, out var nnValue);
            value = nnValue;
            return success;
        }
    }

    /// <summary>
    /// A <see cref="Encoder{TValue}"/> for decimal values
    /// </summary>
    private sealed class DecimalEncoder : Encoder<decimal>
    {
        public override Variant Encode(decimal value)
        {
            if (Decimal64.TryCreate(value, out var decVal))
            {
                return new Variant(DecimalAsDecimal64Encoding.Instance, new OverlappedBits { _decimal64Val = decVal });
            }
            else
            {
                // box it!
                return new Variant(value, default);
            }
        }

        public override bool IsType(in Variant variant)
        {
            return variant._reference == DecimalAsDecimal64Encoding.Instance
                || variant._reference is decimal
                || variant.GetEncoding() is VariantEncoding<decimal>;
        }

        public override bool TryDecode(in Variant variant, [NotNullWhen(true)] out decimal value)
        {
            if (variant._reference == DecimalAsDecimal64Encoding.Instance)
            {
                value = DecimalAsDecimal64Encoding.Instance.Decode(variant);
                return true;
            }
            else if (variant._reference is decimal dval)
            {
                value = dval;
                return true;
            }
            else if (variant.GetEncoding() is VariantEncoding<decimal> encoding)
            {
                value = encoding.Decode(variant)!;
                return true;
            }
            else
            {
                value = default!;
                return false;
            }
        }
    }

    /// <summary>
    /// An encoder for variant values that just returns the value.
    /// </summary>
    private sealed class VariantVariantEncoder : Encoder<Variant>
    {
        public override Variant Encode(Variant variant) =>
            variant;

        public override bool IsType(in Variant variant)
        {
            return true;
        }

        public override bool TryDecode(in Variant variant, out Variant value)
        {
            value = variant;
            return true;
        }
    }

    /// <summary>
    /// An encoder for type unions that access the unions value as a variant.
    /// </summary>
    private sealed class TypeUnionEncoder<TUnion> : Encoder<TUnion>
        where TUnion : ITypeUnion<TUnion>
    {
        public TypeUnionEncoder()
        {
            Debug.Assert(typeof(TUnion).IsAssignableTo(typeof(TypeUnion)));
        }

        public override Variant Encode(TUnion value)
        {
            if (value.TryGet<Variant>(out var variant))
            {
                return variant;
            }
            else if (value.TryGet<object>(out var obj))
            {
                return Variant.Create(obj);
            }
            else
            {
                return Variant.Null;
            }
        }

        public override bool IsType(in Variant variant)
        {
            return variant.CanGet<TUnion>();
        }

        public override bool TryDecode(in Variant variant, out TUnion value)
        {
            // cannot determine constructable union from ITypeUnion, so no fast path here
            return variant.TryGet(out value);
        }
    }

    #endregion

    #region Encodings
    /// <summary>
    /// A type that faciliates access to the value encoded inside a <see cref="Variant"/>
    /// </summary>
    private abstract class Encoding
    {
        public abstract Type GetType(in Variant variant);
        public abstract VariantKind GetKind(in Variant variant);
        public virtual bool IsNull(in Variant variant) => false;
        public abstract bool IsType<T>(in Variant variant);
        public abstract bool CanConvertTo(in Variant variant, Type type);
        public abstract bool TryGet<T>(in Variant variant, out T value);
        public abstract string GetString(in Variant variant);
        public abstract bool IsEqual<T>(in Variant variant, T value);
        public abstract bool IsEqual(in Variant variant, in Variant other);
        public abstract int GetHashCode(in Variant variant);
        public abstract object GetBoxed(in Variant variant);

        /// <summary>
        /// Return the <see cref="Encoding"/> used by the <see cref="Variant"/>,
        /// or null if the variant holds a value as a reference.
        /// </summary>
        public static Encoding GetEncoding(in Variant variant)
        {
            if (variant._reference is Encoding enc)
            {
                return enc;
            }
            else if (variant._overlapped._bitsVal > 0)
            {
                // _bits has the encoding's id
                var encodingId = (int)variant._overlapped._bitsVal;
                return s_encodingList[encodingId];
            }
            else
            {
                return ReferenceEncoding.Instance;
            }
        }

        protected static List<Encoding> s_encodingList =
            new List<Encoding>(capacity: 1024) { null! };
    }

    private static VariantKind Unknown = (VariantKind)(-1);

    private static VariantKind GetKind(Type type)
    {
        switch (TypeInfo.GetTypeCode(type))
        {
            case TypeCode.Boolean:
                return VariantKind.Boolean;
            case TypeCode.Byte:
                return VariantKind.Byte;
            case TypeCode.SByte:
                return VariantKind.SByte;
            case TypeCode.Int16:
                return VariantKind.Int16;
            case TypeCode.UInt16:
                return VariantKind.UInt16;
            case TypeCode.Int32:
                return VariantKind.Int32;
            case TypeCode.UInt32:
                return VariantKind.UInt32;
            case TypeCode.Int64:
                return VariantKind.Int64;
            case TypeCode.UInt64:
                return VariantKind.UInt64;
            case TypeCode.Single:
                return VariantKind.Single;
            case TypeCode.Double:
                return VariantKind.Double;
            case TypeCode.DateTime:
                return VariantKind.DateTime;
            case TypeCode.String:
                return VariantKind.String;
            case TypeCode.Char:
                return VariantKind.Char;
            default:
                if (type == typeof(Guid))
                    return VariantKind.Guid;
                else if (type == typeof(DateOnly))
                    return VariantKind.DateOnly;
                else if (type == typeof(TimeSpan))
                    return VariantKind.TimeSpan;
                else if (type == typeof(TimeOnly))
                    return VariantKind.TimeOnly;
                else if (type == typeof(Rune))
                    return VariantKind.Rune;
                return VariantKind.Other;
        }
    }

    /// <summary>
    /// Abstract base for a strongly typed variant encoding.
    /// </summary>
    private abstract class VariantEncoding<TValue> : Encoding
    {
        public abstract TValue Decode(in Variant variant);

        public override object GetBoxed(in Variant variant) =>
            Decode(in variant)!;

        public override Type GetType(in Variant variant) =>
            typeof(TValue);


        private VariantKind _kind = Unknown;

        public override VariantKind GetKind(in Variant variant)
        {
            if (_kind == Unknown)
            {
                _kind = Variant.GetKind(GetType(in variant));
            }

            return _kind;
        }

        public override bool IsType<TOther>(in Variant variant) =>
            TryGet<TOther>(in variant, out _);

        public override bool CanConvertTo(in Variant variant, Type type)
        {
            var decoded = Decode(in variant);
            return TypeUnion.CanCreate(decoded, type);
        }

        public override bool TryGet<TOther>(in Variant variant, out TOther value)
        {
            var decoded = Decode(in variant);
            if (decoded is TOther other)
            {
                value = other;
                return true;
            }
            else
            {
                return TypeUnion.TryCreate(variant, out value);
            }
        }

        public override string GetString(in Variant variant) =>
            Decode(in variant)?.ToString() ?? "";

        public override bool IsEqual<T>(in Variant variant, T value)
        {
            if (Decode(in variant) is T tvalue)
            {
                return EqualityComparer<T>.Default.Equals(tvalue, value);
            }
            else if (value is Variant v)
            {
                return IsEqual(in variant, in v);
            }

            return false;
        }

        public override bool IsEqual(in Variant variant, in Variant other) =>
            other.Equals(Decode(in variant));

        public override int GetHashCode(in Variant variant) =>
            Decode(in variant)?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Encoding for type that are encoded in the bits field.
    /// </summary>
    private sealed class BitsEncoding<TValue> : VariantEncoding<TValue>
    {
        public static BitsEncoding<TValue> Instance =
            new BitsEncoding<TValue>();

        private BitsEncoding() { }

        public override TValue Decode(in Variant variant)
        {
            Bits bits = variant._overlapped._bitsVal;
            return Unsafe.As<Bits, TValue>(ref bits);
        }
    }

    /// <summary>
    /// Encoding for a reference or boxed value.
    /// </summary>
    private sealed class ReferenceEncoding : Encoding
    {
        internal static ReferenceEncoding Instance =
            new ReferenceEncoding();

        private ReferenceEncoding() { }

        public override object GetBoxed(in Variant variant) =>
            variant._reference!;

        public override Type GetType(in Variant variant) =>
            variant._reference?.GetType() ?? typeof(object);

        private VariantKind _kind = Unknown;

        public override VariantKind GetKind(in Variant variant)
        {
            if (_kind == Unknown)
            {
                _kind = Variant.GetKind(GetType(in variant));
            }

            return _kind;
        }

        public override bool IsNull(in Variant variant) =>
            variant._reference is null;

        public override bool IsType<T>(in Variant variant) =>
            variant._reference is T;

        public override bool CanConvertTo(in Variant variant, Type type) =>
            TypeUnion.CanCreate(variant._reference, type);

        public override bool TryGet<T>(in Variant variant, out T value)
        {
            if (variant._reference is T other)
            {
                value = other;
                return true;
            }
            else
            {
                return TypeUnion.TryCreate(variant._reference, out value);
            }
        }

        public override string GetString(in Variant variant) =>
            variant._reference?.ToString() ?? "";

        public override bool IsEqual<T>(in Variant variant, T value)
        {
            if (value is Variant v)
            {
                return IsEqual(in variant, in v);
            }
            else
            {
                return object.Equals(variant._reference, value);
            }
        }

        public override bool IsEqual(in Variant variant, in Variant other) =>
            other.Equals(variant._reference);

        public override int GetHashCode(in Variant variant) =>
            variant._reference?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Encoding for a struct wrapper around a single reference.
    /// </summary>
    private sealed class WrapperStructEncoding<TValue> : VariantEncoding<TValue>
    {
        public static WrapperStructEncoding<TValue> Instance =
            new WrapperStructEncoding<TValue>();

        private WrapperStructEncoding() { }

        private int _id = 0;

        public int GetId()
        {
            if (_id == 0)
            {
                lock (s_encodingList)
                {
                    _id = s_encodingList.Count;
                    s_encodingList.Add(this);
                }
            }

            return _id;
        }

        public override TValue Decode(in Variant variant)
        {
            Reference? refValue = variant._reference;
            return Unsafe.As<Reference, TValue>(ref refValue!);
        }
    }

    /// <summary>
    /// An encoding for a decimal values stored as a Decimal64 value.
    /// </summary>
    private sealed class Decimal64Encoding : VariantEncoding<Decimal64>
    {
        public static readonly Decimal64Encoding Instance =
        new Decimal64Encoding();

        private Decimal64Encoding() { }

        public override Decimal64 Decode(in Variant variant)
        {
            return variant._overlapped._decimal64Val;
        }
    }

    /// <summary>
    /// An encoding for a decimal values stored as a Decimal64 value.
    /// </summary>
    private sealed class DecimalAsDecimal64Encoding : VariantEncoding<decimal>
    {
        public static readonly DecimalAsDecimal64Encoding Instance =
            new DecimalAsDecimal64Encoding();

        private DecimalAsDecimal64Encoding() { }

        public override decimal Decode(in Variant variant)
        {
            return variant._overlapped._decimal64Val.ToDecimal();
        }
    }

    #endregion

    #region BitsOverlay
    [StructLayout(LayoutKind.Explicit)]
    private struct OverlappedBits
    {
        [FieldOffset(0)]
        public bool _boolVal;

        [FieldOffset(0)]
        public byte _byteVal;

        [FieldOffset(0)]
        public sbyte _sbyteVal;

        [FieldOffset(0)]
        public short _int16Val;

        [FieldOffset(0)]
        public ushort _uint16Val;

        [FieldOffset(0)]
        public int _int32Val;

        [FieldOffset(0)]
        public uint _uint32Val;

        [FieldOffset(0)]
        public long _int64Val;

        [FieldOffset(0)]
        public ulong _uint64Val;

        [FieldOffset(0)]
        public float _singleVal;

        [FieldOffset(0)]
        public double _doubleVal;

        [FieldOffset(0)]
        public Decimal64 _decimal64Val;

        [FieldOffset(0)]
        public char _charVal;

        [FieldOffset(0)]
        public Rune _runeVal;

        [FieldOffset(0)]
        public DateTime _dateTimeVal;

        [FieldOffset(0)]
        public TimeSpan _timeSpanVal;

        [FieldOffset(0)]
        public DateOnly _dateOnlyVal;

        [FieldOffset(0)]
        public TimeOnly _timeOnlyVal;

        [FieldOffset(0)]
        public Bits _bitsVal;
    }
    #endregion
}

public enum VariantKind
{
    Null = 0,
    Boolean,
    Byte,
    SByte,
    Int16,
    UInt16,
    Int32,
    UInt32,
    Int64,
    UInt64,
    Single,
    Double,
    Decimal,
    Char,
    Rune,
    String,
    Guid,
    DateTime,
    DateOnly,
    TimeSpan,
    TimeOnly,
    Other
}