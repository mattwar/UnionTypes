using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace UnionTypes;

/// <summary>
/// A decimal value in 64 bits. 
/// Because, why not.
/// </summary>
public readonly struct Decimal64 :
    ISignedNumber<Decimal64>,
    IComparable<Decimal64>,
    IEquatable<Decimal64>,
    IMinMaxValue<Decimal64>,
    IParsable<Decimal64>,
    ISpanParsable<Decimal64>,
    IFormattable
{
    private readonly long _bits;

    public static readonly long MaxMagnitude = 0x07FF_FFFF_FFFF_FFFF;
    public static readonly long MinMagnitude = ~MaxMagnitude;
    public static readonly byte MaxScale = 15;

    public static Decimal64 Zero => new Decimal64(0);
    public static Decimal64 One => new Decimal64(1, 0);
    public static Decimal64 NegativeOne => new Decimal64(-1, 0);
    public static Decimal64 MinValue => new Decimal64(MinMagnitude, 0);
    public static Decimal64 MaxValue => new Decimal64(MaxMagnitude, 0);
    public static Decimal64 AdditiveIdentity => Zero;
    public static Decimal64 MultiplicativeIdentity => One;

    /// <summary>
    /// Constructs a new <see cref="Decimal64"/> given a magnitude and scale.
    /// The magnitude is the number as an integer without any decimal places.
    /// The scale is the number of decimal places.
    /// </summary>
    public Decimal64(long magnitude, byte scale)
    {
        if (magnitude < MinMagnitude || magnitude > MaxMagnitude)
            throw new ArgumentOutOfRangeException(nameof(magnitude));
        if (scale < 0 || scale > 15)
            throw new ArgumentOutOfRangeException(nameof(scale));
        _bits = magnitude << 4 | scale;
    }

    /// <summary>
    /// Constructs a new <see cref="Decimal64"/> from bits.
    /// </summary>
    private Decimal64(long bits)
    {
        _bits = bits;
    }

    /// <summary>
    /// Constructs a new <see cref="Decimal64"/> from bits encoded in a <see cref="Int64"/>.
    /// </summary>
    public static Decimal64 FromBits(long bits) => new Decimal64(bits);

    /// <summary>
    /// Gets the encoded bits of the <see cref="Decimal64"/> as a <see cref="Int64"/>.
    /// </summary>
    public long GetBits() => _bits;

    /// <summary>
    /// The number of decimal places in the value, a value between 0 and 15.
    /// </summary>
    public byte Scale => (byte)(_bits & 0xF);

    /// <summary>
    /// The unscaled integer magnitude of the <see cref="SmallDecimal"/>.
    /// </summary>
    public long Magnitude => _bits >> 4;

    /// <summary>
    /// True if the <see cref="Decimal64"/> is an integer.
    /// </summary>
    public bool IsInteger => Scale == 0;

    /// <summary>
    /// Creates a <see cref="Decimal64"/> from a <see cref="decimal"/> value.
    /// Returns true if the <see cref="decimal"/> value can be represented correctly in a <see cref="Decimal64"/>.
    /// </summary>
    public static bool TryCreate(decimal value, out Decimal64 dec64)
    {
        var scale = value.Scale;
        if (scale <= 15)
        {
            // get unscaled magnitude (which may be larger than can fit in long)
            var magnitude = value * s_scaleFactor[scale];

            if (magnitude >= MinMagnitude && magnitude <= MaxMagnitude)
            {
                dec64 = new Decimal64((long)magnitude, scale);
                return true;
            }
        }

        dec64 = default;
        return false;
    }

    // because indexing is faster than Math.Pow()
    private static readonly long[] s_scaleFactor = new long[]
    {
        1,                  // 0
        10,                 // 1
        100,                // 2
        1000,               // 3
        10000,              // 4
        100000,             // 5
        1000000,            // 6
        10000000,           // 7
        100000000,          // 8
        1000000000,         // 9
        10000000000,        // 10
        100000000000,       // 11
        1000000000000,      // 12
        10000000000000,     // 13
        100000000000000,    // 14
        1000000000000000    // 15
    };

    /// <summary>
    /// Creates a <see cref="Decimal64"/> from a <see cref="Int64"/> value.
    /// Returns true if the <see cref="Int64"/> value can be represented correctly in a <see cref="Decimal64"/>.
    /// </summary>
    public static bool TryCreate(long value, out Decimal64 dec64)
    {
        if (value >= MinMagnitude && value <= MaxMagnitude)
        {
            dec64 = new Decimal64(value, 0);
            return true;
        }

        dec64 = default;
        return false;
    }

    /// <summary>
    /// Returns the equivalant <see cref="Decimal64"/> value given a <see cref="decimal"/> value.
    /// Throws an <see cref="OverflowException"/> if it is not possible.
    /// </summary>
    public static Decimal64 Create(decimal value) =>
        TryCreate(value, out var dec64) ? dec64 : throw CreateOverflowException();

    /// <summary>
    /// Returns the equivalant <see cref="Decimal64"/> value given a <see cref="long"/> value.
    /// Throws an <see cref="OverflowException"/> if it is not possible.
    /// </summary>
    public static Decimal64 Create(long value) =>
        TryCreate(value, out var dec64) ? dec64 : throw CreateOverflowException();

    private static Exception CreateOverflowException() =>
        new OverflowException($"The value cannot be represented in a {nameof(Decimal64)}.");

    /// <summary>
    /// Removes the decimal fraction.
    /// </summary>
    public readonly Decimal64 Truncate()
    {
        var tens = (int)Math.Pow(10, Math.Abs(Scale));
        var newMagnitude = Magnitude / tens;
        return new Decimal64(newMagnitude, 0);
    }

    /// <summary>
    /// Converts the <see cref="Decimal64"/> to a <see cref="Decimal"/> value.
    /// </summary>
    public readonly decimal ToDecimal()
    {
        var scale = (byte)(_bits & 0xF);
        var negative = _bits < 0;
        var magnitude = Math.Abs(_bits >> 4);
        var lo = unchecked((int)(magnitude & 0x0000_0000_FFFF_FFFF));
        var mid = unchecked((int)((magnitude & 0x0FFF_FFFF_0000_0000) >> 32));
        var d = new decimal(lo, mid, 0, negative, scale);
        return d;
    }


    #region Parsing and Formatting
    public static Decimal64 Parse(string s, IFormatProvider? provider) =>
        Create(decimal.Parse(s, provider));

    public static Decimal64 Parse(string s, NumberStyles style, IFormatProvider? provider) =>
        Create(decimal.Parse(s, style, provider));

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Decimal64 result)
    {
        if (decimal.TryParse(s, provider, out var value) && TryCreate(value, out result))
            return true;
        result = default;
        return false;
    }

    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Decimal64 result)
    {
        if (decimal.TryParse(s, style, provider, out var value) && TryCreate(value, out result))
            return true;
        result = default;
        return false;
    }

    public static Decimal64 Parse(string s) =>
        Create(decimal.Parse(s, null));

    public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out Decimal64 result) =>
        TryParse(s, null, out result);

    public static Decimal64 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) =>
        Create(decimal.Parse(s, provider));

    public static Decimal64 Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) =>
        Create(decimal.Parse(s, style, provider));

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Decimal64 result)
    {
        if (decimal.TryParse(s, provider, out var value) && TryCreate(value, out result))
            return true;
        result = default;
        return false;
    }

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Decimal64 result)
    {
        if (decimal.TryParse(s, style, provider, out var value) && TryCreate(value, out result))
            return true;
        result = default;
        return false;
    }

    public readonly override string ToString() =>
        ToDecimal().ToString();

    public readonly string ToString(string? format, IFormatProvider? formatProvider) =>
        ToDecimal().ToString(format, formatProvider);

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
        ToDecimal().TryFormat(destination, out charsWritten, format, provider);

    #endregion

    #region Equality and Comparison

    public readonly bool Equals(Decimal64 other) =>
        ToDecimal().Equals(other.ToDecimal());

    public readonly bool Equals(decimal other) =>
        ToDecimal().Equals(other);

    public readonly override bool Equals([NotNullWhen(true)] object? obj) =>
        obj is Decimal64 sd && Equals(sd)
        || obj is decimal d && ToDecimal().Equals(d);

    public readonly override int GetHashCode() =>
        ToDecimal().GetHashCode();


    public static bool operator ==(Decimal64 a, Decimal64 b) =>
        a.Equals(b);

    public static bool operator ==(Decimal64 a, decimal b) =>
        a.Equals(b);

    public static bool operator ==(decimal a, Decimal64 b) =>
        a.Equals(b.ToDecimal());

    public static bool operator !=(Decimal64 a, Decimal64 b) =>
        !a.Equals(b);

    public static bool operator !=(Decimal64 a, decimal b) =>
        !a.Equals(b);

    public static bool operator !=(decimal a, Decimal64 b) =>
        !a.Equals(b.ToDecimal());


    public readonly int CompareTo(Decimal64 other) =>
        ToDecimal().CompareTo(other.ToDecimal());

    public readonly int CompareTo(decimal other) =>
        ToDecimal().CompareTo(other);

    public static bool operator <(Decimal64 a, Decimal64 b) =>
        a.ToDecimal() < b.ToDecimal();


    public static bool operator >(Decimal64 a, Decimal64 b) =>
        a.ToDecimal() > b.ToDecimal();

    public static bool operator <(Decimal64 a, decimal b) =>
        a.ToDecimal() < b;

    public static bool operator >(Decimal64 a, decimal b) =>
        a.ToDecimal() > b;

    public static bool operator <(decimal a, Decimal64 b) =>
        a < b.ToDecimal();

    public static bool operator >(decimal a, Decimal64 b) =>
        a > b.ToDecimal();
    #endregion

    #region Conversion
    public static implicit operator decimal(Decimal64 dec64) =>
        dec64.ToDecimal();

    public static implicit operator Decimal64(int value) =>
        Create((decimal)value);

    public static explicit operator Decimal64(long value) =>
        Create((decimal)value);

    public static explicit operator Decimal64(decimal value) =>
        Create(value);

    public static explicit operator long(Decimal64 value) =>
        value.Truncate().Magnitude;
    #endregion

    #region Math operators
    public static Decimal64 operator +(Decimal64 a, Decimal64 b) =>
        Create(a.ToDecimal() + b.ToDecimal());

    public static decimal operator +(Decimal64 a, decimal b) =>
        a.ToDecimal() + b;

    public static decimal operator +(decimal a, Decimal64 b) =>
        a + b.ToDecimal();

    public static Decimal64 operator +(Decimal64 a, long b) =>
        Create(a.ToDecimal() + b);

    public static Decimal64 operator +(long a, Decimal64 b) =>
        Create(a + b.ToDecimal());

    public static Decimal64 operator -(Decimal64 a, Decimal64 b) =>
        Create(a.ToDecimal() - b.ToDecimal());

    public static decimal operator -(Decimal64 a, decimal b) =>
        a.ToDecimal() - b;

    public static decimal operator -(decimal a, Decimal64 b) =>
        a - b.ToDecimal();

    public static Decimal64 operator -(Decimal64 a, long b) =>
        Create(a.ToDecimal() - b);

    public static Decimal64 operator -(long a, Decimal64 b) =>
        Create(a - b.ToDecimal());

    public static Decimal64 operator *(Decimal64 a, Decimal64 b) =>
        Create(a.ToDecimal() * b.ToDecimal());

    public static decimal operator *(Decimal64 a, decimal b) =>
        a.ToDecimal() * b;

    public static decimal operator *(decimal a, Decimal64 b) =>
        a + b.ToDecimal();

    public static Decimal64 operator *(Decimal64 a, long b) =>
        Create(a.ToDecimal() * b);

    public static Decimal64 operator *(long a, Decimal64 b) =>
        Create(a * b.ToDecimal());


    public static Decimal64 operator /(Decimal64 a, Decimal64 b) =>
        Create(a.ToDecimal() / b.ToDecimal());

    public static decimal operator /(Decimal64 a, decimal b) =>
        a.ToDecimal() / b;

    public static decimal operator /(decimal a, Decimal64 b) =>
        a / b.ToDecimal();

    public static Decimal64 operator /(Decimal64 a, long b) =>
        Create(a.ToDecimal() / b);

    public static Decimal64 operator /(long a, Decimal64 b) =>
        Create(a / b.ToDecimal());

    public static Decimal64 operator ++(Decimal64 value) =>
        new Decimal64(value.Magnitude + 1, value.Scale);

    public static Decimal64 operator --(Decimal64 value) =>
        new Decimal64(value.Magnitude - 1, value.Scale);

    public static Decimal64 operator -(Decimal64 value) =>
        new Decimal64(-value.Magnitude, value.Scale);

    public static Decimal64 operator +(Decimal64 value) =>
        value; // why does this operator exist?
    #endregion

    #region ISignedNumber<Decimal64>
    static Decimal64 INumberBase<Decimal64>.Abs(Decimal64 value) =>
        new Decimal64(Math.Abs(value.Magnitude), value.Scale);

    static bool INumberBase<Decimal64>.IsCanonical(Decimal64 value) =>
        true;

    static bool INumberBase<Decimal64>.IsComplexNumber(Decimal64 value) =>
        false;

    static bool INumberBase<Decimal64>.IsEvenInteger(Decimal64 value) =>
        value.Scale == 0 && (value.Magnitude & 1) == 0;

    static bool INumberBase<Decimal64>.IsFinite(Decimal64 value) =>
        true;

    static bool INumberBase<Decimal64>.IsImaginaryNumber(Decimal64 value) =>
        false;

    static bool INumberBase<Decimal64>.IsInfinity(Decimal64 value) =>
        false;

    static bool INumberBase<Decimal64>.IsInteger(Decimal64 value) =>
        value.Scale == 0;

    static bool INumberBase<Decimal64>.IsNaN(Decimal64 value) =>
        false;

    static bool INumberBase<Decimal64>.IsNegative(Decimal64 value) =>
        value.Magnitude < 0;

    static bool INumberBase<Decimal64>.IsNegativeInfinity(Decimal64 value) =>
        false;

    static bool INumberBase<Decimal64>.IsNormal(Decimal64 value) =>
        true;

    static bool INumberBase<Decimal64>.IsOddInteger(Decimal64 value) =>
        value.Scale == 0 && (value.Magnitude & 1) == 1;

    static bool INumberBase<Decimal64>.IsPositive(Decimal64 value) =>
        value.Magnitude >= 0;

    static bool INumberBase<Decimal64>.IsPositiveInfinity(Decimal64 value) =>
        false;

    static bool INumberBase<Decimal64>.IsRealNumber(Decimal64 value) =>
        true;

    static bool INumberBase<Decimal64>.IsSubnormal(Decimal64 value) =>
        false;

    static bool INumberBase<Decimal64>.IsZero(Decimal64 value) =>
        value == Zero;

    static Decimal64 INumberBase<Decimal64>.MaxMagnitude(Decimal64 x, Decimal64 y) =>
        x > y ? x : y;

    static Decimal64 INumberBase<Decimal64>.MaxMagnitudeNumber(Decimal64 x, Decimal64 y) =>
        x > y ? x : y;

    static Decimal64 INumberBase<Decimal64>.MinMagnitude(Decimal64 x, Decimal64 y) =>
        x < y ? x : y;

    static Decimal64 INumberBase<Decimal64>.MinMagnitudeNumber(Decimal64 x, Decimal64 y) =>
        x < y ? x : y;

    static int INumberBase<Decimal64>.Radix => 10;

    static bool INumberBase<Decimal64>.TryConvertFromChecked<TOther>(TOther value, out Decimal64 result)
    {
        if (TOther.TryConvertToChecked<decimal>(value, out var dval))
        {
            result = TryCreate(dval, out result)
                ? result
                : throw CreateOverflowException();
            return true;
        }

        result = default;
        return false;
    }

    static bool INumberBase<Decimal64>.TryConvertFromTruncating<TOther>(TOther value, out Decimal64 result)
    {
        if (TOther.TryConvertToTruncating<decimal>(value, out var dval))
        {
            if (TryCreate(dval, out result))
                return true;
            result = (dval < 0.0m) ? MinValue : MaxValue;
            return true;
        }

        result = default;
        return false;
    }

    static bool INumberBase<Decimal64>.TryConvertFromSaturating<TOther>(TOther value, out Decimal64 result)
    {
        if (TOther.TryConvertToSaturating<decimal>(value, out var dval))
        {
            if (TryCreate(dval, out result))
                return true;
            result = (dval < 0.0m) ? MinValue : MaxValue;
            return false;
        }

        result = default;
        return false;
    }

    static bool INumberBase<Decimal64>.TryConvertToChecked<TOther>(Decimal64 value, out TOther other)
    {
        return TOther.TryConvertFromChecked(value.ToDecimal(), out other!);
    }

    static bool INumberBase<Decimal64>.TryConvertToTruncating<TOther>(Decimal64 value, out TOther other)
    {
        return TOther.TryConvertFromTruncating(value.ToDecimal(), out other!);
    }

    static bool INumberBase<Decimal64>.TryConvertToSaturating<TOther>(Decimal64 value, out TOther other)
    {
        return TOther.TryConvertFromSaturating(value.ToDecimal(), out other!);
    }
    #endregion
}
