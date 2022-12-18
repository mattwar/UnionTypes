// 
// 
using System;
using System.Collections.Generic;
#nullable enable

namespace UnionTypes
{
    public class OneOf<T1, T2>
        : IOneOf, IEquatable<OneOf<T1, T2>>
    {
        private readonly object _value;

        private OneOf(object value) { _value = value; }

        public static OneOf<T1, T2> Create(T1 value)
        {
            return new OneOf<T1, T2>(value!);
        }

        public static OneOf<T1, T2> Create(T2 value)
        {
            return new OneOf<T1, T2>(value!);
        }

        public static OneOf<T1, T2> Create<TValue>(TValue value)
        {
            switch (value)
            {
                case T1 value1: return Create(value1);
                case T2 value2: return Create(value2);
                case IOneOf otherOneOf: return Create(otherOneOf.Get<object>());
                default: throw new InvalidCastException();
            }
        }

        public static OneOf<T1, T2> Convert<TOneOf>(TOneOf oneOf) where TOneOf : IOneOf
        {
            if (oneOf is OneOf<T1, T2> me) return me;
            if (oneOf.TryGet(out T1 value1)) return Create(value1);
            if (oneOf.TryGet(out T2 value2)) return Create(value2);
            throw new InvalidCastException();
        }

        public static implicit operator OneOf<T1, T2>(T1 value)
        {
            return Create<T1>(value);
        }

        public static explicit operator T1(OneOf<T1, T2> oneOf)
        {
            return oneOf.Get<T1>();
        }

        public static implicit operator OneOf<T1, T2>(T2 value)
        {
            return Create<T2>(value);
        }

        public static explicit operator T2(OneOf<T1, T2> oneOf)
        {
            return oneOf.Get<T2>();
        }

        public bool Is<T>() => _value is T;
        public T Get<T>() => _value is T t ? t : throw new InvalidCastException();
        public bool TryGet<T>(out T value) { if (_value is T t) { value = t; return true; } else { value = default!; return false; } }
        public bool Equals<TValue>(TValue value)
        {
            if (value is OneOf<T1, T2> thisOneOf)
                return object.Equals(this.Get<object>(), thisOneOf.Get<object>());
            else if (value is IOneOf otherOneOf)
                return object.Equals(this.Get<object>(), otherOneOf.Get<object>());
            else
                return object.Equals(this.Get<object>(), value);
        }

        public override bool Equals(object? obj)
        {
            return obj is object other && Equals<object>(other);
        }

        public override int GetHashCode()
        {
            return _value?.GetHashCode() ?? 0;
        }

        public bool Equals(OneOf<T1, T2>? other)
        {
            return object.Equals(_value, other?._value);
        }

        public static bool operator ==(OneOf<T1, T2> oneOf, IOneOf? other)
        {
            return object.Equals(oneOf._value, other?.Get<object>());
        }

        public static bool operator !=(OneOf<T1, T2> oneOf, IOneOf? other)
        {
            return !object.Equals(oneOf._value, other?.Get<object>());
        }

        public static bool operator ==(OneOf<T1, T2> oneOf, T1 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2> oneOf, T1 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2> oneOf, T2 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2> oneOf, T2 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public override string ToString()
        {
            return _value?.ToString() ?? "";
        }
    }

    public class OneOf<T1, T2, T3>
        : IOneOf, IEquatable<OneOf<T1, T2, T3>>
    {
        private readonly object _value;

        private OneOf(object value) { _value = value; }

        public static OneOf<T1, T2, T3> Create(T1 value)
        {
            return new OneOf<T1, T2, T3>(value!);
        }

        public static OneOf<T1, T2, T3> Create(T2 value)
        {
            return new OneOf<T1, T2, T3>(value!);
        }

        public static OneOf<T1, T2, T3> Create(T3 value)
        {
            return new OneOf<T1, T2, T3>(value!);
        }

        public static OneOf<T1, T2, T3> Create<TValue>(TValue value)
        {
            switch (value)
            {
                case T1 value1: return Create(value1);
                case T2 value2: return Create(value2);
                case T3 value3: return Create(value3);
                case IOneOf otherOneOf: return Create(otherOneOf.Get<object>());
                default: throw new InvalidCastException();
            }
        }

        public static OneOf<T1, T2, T3> Convert<TOneOf>(TOneOf oneOf) where TOneOf : IOneOf
        {
            if (oneOf is OneOf<T1, T2, T3> me) return me;
            if (oneOf.TryGet(out T1 value1)) return Create(value1);
            if (oneOf.TryGet(out T2 value2)) return Create(value2);
            if (oneOf.TryGet(out T3 value3)) return Create(value3);
            throw new InvalidCastException();
        }

        public static implicit operator OneOf<T1, T2, T3>(T1 value)
        {
            return Create<T1>(value);
        }

        public static explicit operator T1(OneOf<T1, T2, T3> oneOf)
        {
            return oneOf.Get<T1>();
        }

        public static implicit operator OneOf<T1, T2, T3>(T2 value)
        {
            return Create<T2>(value);
        }

        public static explicit operator T2(OneOf<T1, T2, T3> oneOf)
        {
            return oneOf.Get<T2>();
        }

        public static implicit operator OneOf<T1, T2, T3>(T3 value)
        {
            return Create<T3>(value);
        }

        public static explicit operator T3(OneOf<T1, T2, T3> oneOf)
        {
            return oneOf.Get<T3>();
        }

        public bool Is<T>() => _value is T;
        public T Get<T>() => _value is T t ? t : throw new InvalidCastException();
        public bool TryGet<T>(out T value) { if (_value is T t) { value = t; return true; } else { value = default!; return false; } }
        public bool Equals<TValue>(TValue value)
        {
            if (value is OneOf<T1, T2, T3> thisOneOf)
                return object.Equals(this.Get<object>(), thisOneOf.Get<object>());
            else if (value is IOneOf otherOneOf)
                return object.Equals(this.Get<object>(), otherOneOf.Get<object>());
            else
                return object.Equals(this.Get<object>(), value);
        }

        public override bool Equals(object? obj)
        {
            return obj is object other && Equals<object>(other);
        }

        public override int GetHashCode()
        {
            return _value?.GetHashCode() ?? 0;
        }

        public bool Equals(OneOf<T1, T2, T3>? other)
        {
            return object.Equals(_value, other?._value);
        }

        public static bool operator ==(OneOf<T1, T2, T3> oneOf, IOneOf? other)
        {
            return object.Equals(oneOf._value, other?.Get<object>());
        }

        public static bool operator !=(OneOf<T1, T2, T3> oneOf, IOneOf? other)
        {
            return !object.Equals(oneOf._value, other?.Get<object>());
        }

        public static bool operator ==(OneOf<T1, T2, T3> oneOf, T1 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3> oneOf, T1 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3> oneOf, T2 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3> oneOf, T2 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3> oneOf, T3 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3> oneOf, T3 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public override string ToString()
        {
            return _value?.ToString() ?? "";
        }
    }

    public class OneOf<T1, T2, T3, T4>
        : IOneOf, IEquatable<OneOf<T1, T2, T3, T4>>
    {
        private readonly object _value;

        private OneOf(object value) { _value = value; }

        public static OneOf<T1, T2, T3, T4> Create(T1 value)
        {
            return new OneOf<T1, T2, T3, T4>(value!);
        }

        public static OneOf<T1, T2, T3, T4> Create(T2 value)
        {
            return new OneOf<T1, T2, T3, T4>(value!);
        }

        public static OneOf<T1, T2, T3, T4> Create(T3 value)
        {
            return new OneOf<T1, T2, T3, T4>(value!);
        }

        public static OneOf<T1, T2, T3, T4> Create(T4 value)
        {
            return new OneOf<T1, T2, T3, T4>(value!);
        }

        public static OneOf<T1, T2, T3, T4> Create<TValue>(TValue value)
        {
            switch (value)
            {
                case T1 value1: return Create(value1);
                case T2 value2: return Create(value2);
                case T3 value3: return Create(value3);
                case T4 value4: return Create(value4);
                case IOneOf otherOneOf: return Create(otherOneOf.Get<object>());
                default: throw new InvalidCastException();
            }
        }

        public static OneOf<T1, T2, T3, T4> Convert<TOneOf>(TOneOf oneOf) where TOneOf : IOneOf
        {
            if (oneOf is OneOf<T1, T2, T3, T4> me) return me;
            if (oneOf.TryGet(out T1 value1)) return Create(value1);
            if (oneOf.TryGet(out T2 value2)) return Create(value2);
            if (oneOf.TryGet(out T3 value3)) return Create(value3);
            if (oneOf.TryGet(out T4 value4)) return Create(value4);
            throw new InvalidCastException();
        }

        public static implicit operator OneOf<T1, T2, T3, T4>(T1 value)
        {
            return Create<T1>(value);
        }

        public static explicit operator T1(OneOf<T1, T2, T3, T4> oneOf)
        {
            return oneOf.Get<T1>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4>(T2 value)
        {
            return Create<T2>(value);
        }

        public static explicit operator T2(OneOf<T1, T2, T3, T4> oneOf)
        {
            return oneOf.Get<T2>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4>(T3 value)
        {
            return Create<T3>(value);
        }

        public static explicit operator T3(OneOf<T1, T2, T3, T4> oneOf)
        {
            return oneOf.Get<T3>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4>(T4 value)
        {
            return Create<T4>(value);
        }

        public static explicit operator T4(OneOf<T1, T2, T3, T4> oneOf)
        {
            return oneOf.Get<T4>();
        }

        public bool Is<T>() => _value is T;
        public T Get<T>() => _value is T t ? t : throw new InvalidCastException();
        public bool TryGet<T>(out T value) { if (_value is T t) { value = t; return true; } else { value = default!; return false; } }
        public bool Equals<TValue>(TValue value)
        {
            if (value is OneOf<T1, T2, T3, T4> thisOneOf)
                return object.Equals(this.Get<object>(), thisOneOf.Get<object>());
            else if (value is IOneOf otherOneOf)
                return object.Equals(this.Get<object>(), otherOneOf.Get<object>());
            else
                return object.Equals(this.Get<object>(), value);
        }

        public override bool Equals(object? obj)
        {
            return obj is object other && Equals<object>(other);
        }

        public override int GetHashCode()
        {
            return _value?.GetHashCode() ?? 0;
        }

        public bool Equals(OneOf<T1, T2, T3, T4>? other)
        {
            return object.Equals(_value, other?._value);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4> oneOf, IOneOf? other)
        {
            return object.Equals(oneOf._value, other?.Get<object>());
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4> oneOf, IOneOf? other)
        {
            return !object.Equals(oneOf._value, other?.Get<object>());
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4> oneOf, T1 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4> oneOf, T1 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4> oneOf, T2 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4> oneOf, T2 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4> oneOf, T3 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4> oneOf, T3 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4> oneOf, T4 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4> oneOf, T4 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public override string ToString()
        {
            return _value?.ToString() ?? "";
        }
    }

    public class OneOf<T1, T2, T3, T4, T5>
        : IOneOf, IEquatable<OneOf<T1, T2, T3, T4, T5>>
    {
        private readonly object _value;

        private OneOf(object value) { _value = value; }

        public static OneOf<T1, T2, T3, T4, T5> Create(T1 value)
        {
            return new OneOf<T1, T2, T3, T4, T5>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5> Create(T2 value)
        {
            return new OneOf<T1, T2, T3, T4, T5>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5> Create(T3 value)
        {
            return new OneOf<T1, T2, T3, T4, T5>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5> Create(T4 value)
        {
            return new OneOf<T1, T2, T3, T4, T5>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5> Create(T5 value)
        {
            return new OneOf<T1, T2, T3, T4, T5>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5> Create<TValue>(TValue value)
        {
            switch (value)
            {
                case T1 value1: return Create(value1);
                case T2 value2: return Create(value2);
                case T3 value3: return Create(value3);
                case T4 value4: return Create(value4);
                case T5 value5: return Create(value5);
                case IOneOf otherOneOf: return Create(otherOneOf.Get<object>());
                default: throw new InvalidCastException();
            }
        }

        public static OneOf<T1, T2, T3, T4, T5> Convert<TOneOf>(TOneOf oneOf) where TOneOf : IOneOf
        {
            if (oneOf is OneOf<T1, T2, T3, T4, T5> me) return me;
            if (oneOf.TryGet(out T1 value1)) return Create(value1);
            if (oneOf.TryGet(out T2 value2)) return Create(value2);
            if (oneOf.TryGet(out T3 value3)) return Create(value3);
            if (oneOf.TryGet(out T4 value4)) return Create(value4);
            if (oneOf.TryGet(out T5 value5)) return Create(value5);
            throw new InvalidCastException();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5>(T1 value)
        {
            return Create<T1>(value);
        }

        public static explicit operator T1(OneOf<T1, T2, T3, T4, T5> oneOf)
        {
            return oneOf.Get<T1>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5>(T2 value)
        {
            return Create<T2>(value);
        }

        public static explicit operator T2(OneOf<T1, T2, T3, T4, T5> oneOf)
        {
            return oneOf.Get<T2>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5>(T3 value)
        {
            return Create<T3>(value);
        }

        public static explicit operator T3(OneOf<T1, T2, T3, T4, T5> oneOf)
        {
            return oneOf.Get<T3>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5>(T4 value)
        {
            return Create<T4>(value);
        }

        public static explicit operator T4(OneOf<T1, T2, T3, T4, T5> oneOf)
        {
            return oneOf.Get<T4>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5>(T5 value)
        {
            return Create<T5>(value);
        }

        public static explicit operator T5(OneOf<T1, T2, T3, T4, T5> oneOf)
        {
            return oneOf.Get<T5>();
        }

        public bool Is<T>() => _value is T;
        public T Get<T>() => _value is T t ? t : throw new InvalidCastException();
        public bool TryGet<T>(out T value) { if (_value is T t) { value = t; return true; } else { value = default!; return false; } }
        public bool Equals<TValue>(TValue value)
        {
            if (value is OneOf<T1, T2, T3, T4, T5> thisOneOf)
                return object.Equals(this.Get<object>(), thisOneOf.Get<object>());
            else if (value is IOneOf otherOneOf)
                return object.Equals(this.Get<object>(), otherOneOf.Get<object>());
            else
                return object.Equals(this.Get<object>(), value);
        }

        public override bool Equals(object? obj)
        {
            return obj is object other && Equals<object>(other);
        }

        public override int GetHashCode()
        {
            return _value?.GetHashCode() ?? 0;
        }

        public bool Equals(OneOf<T1, T2, T3, T4, T5>? other)
        {
            return object.Equals(_value, other?._value);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5> oneOf, IOneOf? other)
        {
            return object.Equals(oneOf._value, other?.Get<object>());
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5> oneOf, IOneOf? other)
        {
            return !object.Equals(oneOf._value, other?.Get<object>());
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5> oneOf, T1 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5> oneOf, T1 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5> oneOf, T2 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5> oneOf, T2 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5> oneOf, T3 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5> oneOf, T3 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5> oneOf, T4 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5> oneOf, T4 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5> oneOf, T5 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5> oneOf, T5 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public override string ToString()
        {
            return _value?.ToString() ?? "";
        }
    }

    public class OneOf<T1, T2, T3, T4, T5, T6>
        : IOneOf, IEquatable<OneOf<T1, T2, T3, T4, T5, T6>>
    {
        private readonly object _value;

        private OneOf(object value) { _value = value; }

        public static OneOf<T1, T2, T3, T4, T5, T6> Create(T1 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6> Create(T2 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6> Create(T3 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6> Create(T4 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6> Create(T5 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6> Create(T6 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6> Create<TValue>(TValue value)
        {
            switch (value)
            {
                case T1 value1: return Create(value1);
                case T2 value2: return Create(value2);
                case T3 value3: return Create(value3);
                case T4 value4: return Create(value4);
                case T5 value5: return Create(value5);
                case T6 value6: return Create(value6);
                case IOneOf otherOneOf: return Create(otherOneOf.Get<object>());
                default: throw new InvalidCastException();
            }
        }

        public static OneOf<T1, T2, T3, T4, T5, T6> Convert<TOneOf>(TOneOf oneOf) where TOneOf : IOneOf
        {
            if (oneOf is OneOf<T1, T2, T3, T4, T5, T6> me) return me;
            if (oneOf.TryGet(out T1 value1)) return Create(value1);
            if (oneOf.TryGet(out T2 value2)) return Create(value2);
            if (oneOf.TryGet(out T3 value3)) return Create(value3);
            if (oneOf.TryGet(out T4 value4)) return Create(value4);
            if (oneOf.TryGet(out T5 value5)) return Create(value5);
            if (oneOf.TryGet(out T6 value6)) return Create(value6);
            throw new InvalidCastException();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T1 value)
        {
            return Create<T1>(value);
        }

        public static explicit operator T1(OneOf<T1, T2, T3, T4, T5, T6> oneOf)
        {
            return oneOf.Get<T1>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T2 value)
        {
            return Create<T2>(value);
        }

        public static explicit operator T2(OneOf<T1, T2, T3, T4, T5, T6> oneOf)
        {
            return oneOf.Get<T2>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T3 value)
        {
            return Create<T3>(value);
        }

        public static explicit operator T3(OneOf<T1, T2, T3, T4, T5, T6> oneOf)
        {
            return oneOf.Get<T3>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T4 value)
        {
            return Create<T4>(value);
        }

        public static explicit operator T4(OneOf<T1, T2, T3, T4, T5, T6> oneOf)
        {
            return oneOf.Get<T4>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T5 value)
        {
            return Create<T5>(value);
        }

        public static explicit operator T5(OneOf<T1, T2, T3, T4, T5, T6> oneOf)
        {
            return oneOf.Get<T5>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T6 value)
        {
            return Create<T6>(value);
        }

        public static explicit operator T6(OneOf<T1, T2, T3, T4, T5, T6> oneOf)
        {
            return oneOf.Get<T6>();
        }

        public bool Is<T>() => _value is T;
        public T Get<T>() => _value is T t ? t : throw new InvalidCastException();
        public bool TryGet<T>(out T value) { if (_value is T t) { value = t; return true; } else { value = default!; return false; } }
        public bool Equals<TValue>(TValue value)
        {
            if (value is OneOf<T1, T2, T3, T4, T5, T6> thisOneOf)
                return object.Equals(this.Get<object>(), thisOneOf.Get<object>());
            else if (value is IOneOf otherOneOf)
                return object.Equals(this.Get<object>(), otherOneOf.Get<object>());
            else
                return object.Equals(this.Get<object>(), value);
        }

        public override bool Equals(object? obj)
        {
            return obj is object other && Equals<object>(other);
        }

        public override int GetHashCode()
        {
            return _value?.GetHashCode() ?? 0;
        }

        public bool Equals(OneOf<T1, T2, T3, T4, T5, T6>? other)
        {
            return object.Equals(_value, other?._value);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6> oneOf, IOneOf? other)
        {
            return object.Equals(oneOf._value, other?.Get<object>());
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6> oneOf, IOneOf? other)
        {
            return !object.Equals(oneOf._value, other?.Get<object>());
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6> oneOf, T1 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6> oneOf, T1 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6> oneOf, T2 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6> oneOf, T2 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6> oneOf, T3 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6> oneOf, T3 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6> oneOf, T4 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6> oneOf, T4 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6> oneOf, T5 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6> oneOf, T5 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6> oneOf, T6 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6> oneOf, T6 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public override string ToString()
        {
            return _value?.ToString() ?? "";
        }
    }

    public class OneOf<T1, T2, T3, T4, T5, T6, T7>
        : IOneOf, IEquatable<OneOf<T1, T2, T3, T4, T5, T6, T7>>
    {
        private readonly object _value;

        private OneOf(object value) { _value = value; }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create(T1 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create(T2 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create(T3 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create(T4 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create(T5 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create(T6 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create(T7 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create<TValue>(TValue value)
        {
            switch (value)
            {
                case T1 value1: return Create(value1);
                case T2 value2: return Create(value2);
                case T3 value3: return Create(value3);
                case T4 value4: return Create(value4);
                case T5 value5: return Create(value5);
                case T6 value6: return Create(value6);
                case T7 value7: return Create(value7);
                case IOneOf otherOneOf: return Create(otherOneOf.Get<object>());
                default: throw new InvalidCastException();
            }
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Convert<TOneOf>(TOneOf oneOf) where TOneOf : IOneOf
        {
            if (oneOf is OneOf<T1, T2, T3, T4, T5, T6, T7> me) return me;
            if (oneOf.TryGet(out T1 value1)) return Create(value1);
            if (oneOf.TryGet(out T2 value2)) return Create(value2);
            if (oneOf.TryGet(out T3 value3)) return Create(value3);
            if (oneOf.TryGet(out T4 value4)) return Create(value4);
            if (oneOf.TryGet(out T5 value5)) return Create(value5);
            if (oneOf.TryGet(out T6 value6)) return Create(value6);
            if (oneOf.TryGet(out T7 value7)) return Create(value7);
            throw new InvalidCastException();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T1 value)
        {
            return Create<T1>(value);
        }

        public static explicit operator T1(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf)
        {
            return oneOf.Get<T1>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T2 value)
        {
            return Create<T2>(value);
        }

        public static explicit operator T2(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf)
        {
            return oneOf.Get<T2>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T3 value)
        {
            return Create<T3>(value);
        }

        public static explicit operator T3(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf)
        {
            return oneOf.Get<T3>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T4 value)
        {
            return Create<T4>(value);
        }

        public static explicit operator T4(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf)
        {
            return oneOf.Get<T4>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T5 value)
        {
            return Create<T5>(value);
        }

        public static explicit operator T5(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf)
        {
            return oneOf.Get<T5>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T6 value)
        {
            return Create<T6>(value);
        }

        public static explicit operator T6(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf)
        {
            return oneOf.Get<T6>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T7 value)
        {
            return Create<T7>(value);
        }

        public static explicit operator T7(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf)
        {
            return oneOf.Get<T7>();
        }

        public bool Is<T>() => _value is T;
        public T Get<T>() => _value is T t ? t : throw new InvalidCastException();
        public bool TryGet<T>(out T value) { if (_value is T t) { value = t; return true; } else { value = default!; return false; } }
        public bool Equals<TValue>(TValue value)
        {
            if (value is OneOf<T1, T2, T3, T4, T5, T6, T7> thisOneOf)
                return object.Equals(this.Get<object>(), thisOneOf.Get<object>());
            else if (value is IOneOf otherOneOf)
                return object.Equals(this.Get<object>(), otherOneOf.Get<object>());
            else
                return object.Equals(this.Get<object>(), value);
        }

        public override bool Equals(object? obj)
        {
            return obj is object other && Equals<object>(other);
        }

        public override int GetHashCode()
        {
            return _value?.GetHashCode() ?? 0;
        }

        public bool Equals(OneOf<T1, T2, T3, T4, T5, T6, T7>? other)
        {
            return object.Equals(_value, other?._value);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf, IOneOf? other)
        {
            return object.Equals(oneOf._value, other?.Get<object>());
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf, IOneOf? other)
        {
            return !object.Equals(oneOf._value, other?.Get<object>());
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf, T1 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf, T1 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf, T2 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf, T2 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf, T3 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf, T3 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf, T4 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf, T4 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf, T5 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf, T5 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf, T6 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf, T6 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf, T7 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7> oneOf, T7 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public override string ToString()
        {
            return _value?.ToString() ?? "";
        }
    }

    public class OneOf<T1, T2, T3, T4, T5, T6, T7, T8>
        : IOneOf, IEquatable<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>
    {
        private readonly object _value;

        private OneOf(object value) { _value = value; }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T1 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T2 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T3 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T4 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T5 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T6 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T7 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T8 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create<TValue>(TValue value)
        {
            switch (value)
            {
                case T1 value1: return Create(value1);
                case T2 value2: return Create(value2);
                case T3 value3: return Create(value3);
                case T4 value4: return Create(value4);
                case T5 value5: return Create(value5);
                case T6 value6: return Create(value6);
                case T7 value7: return Create(value7);
                case T8 value8: return Create(value8);
                case IOneOf otherOneOf: return Create(otherOneOf.Get<object>());
                default: throw new InvalidCastException();
            }
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Convert<TOneOf>(TOneOf oneOf) where TOneOf : IOneOf
        {
            if (oneOf is OneOf<T1, T2, T3, T4, T5, T6, T7, T8> me) return me;
            if (oneOf.TryGet(out T1 value1)) return Create(value1);
            if (oneOf.TryGet(out T2 value2)) return Create(value2);
            if (oneOf.TryGet(out T3 value3)) return Create(value3);
            if (oneOf.TryGet(out T4 value4)) return Create(value4);
            if (oneOf.TryGet(out T5 value5)) return Create(value5);
            if (oneOf.TryGet(out T6 value6)) return Create(value6);
            if (oneOf.TryGet(out T7 value7)) return Create(value7);
            if (oneOf.TryGet(out T8 value8)) return Create(value8);
            throw new InvalidCastException();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value)
        {
            return Create<T1>(value);
        }

        public static explicit operator T1(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf)
        {
            return oneOf.Get<T1>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T2 value)
        {
            return Create<T2>(value);
        }

        public static explicit operator T2(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf)
        {
            return oneOf.Get<T2>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T3 value)
        {
            return Create<T3>(value);
        }

        public static explicit operator T3(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf)
        {
            return oneOf.Get<T3>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T4 value)
        {
            return Create<T4>(value);
        }

        public static explicit operator T4(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf)
        {
            return oneOf.Get<T4>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T5 value)
        {
            return Create<T5>(value);
        }

        public static explicit operator T5(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf)
        {
            return oneOf.Get<T5>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T6 value)
        {
            return Create<T6>(value);
        }

        public static explicit operator T6(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf)
        {
            return oneOf.Get<T6>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T7 value)
        {
            return Create<T7>(value);
        }

        public static explicit operator T7(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf)
        {
            return oneOf.Get<T7>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T8 value)
        {
            return Create<T8>(value);
        }

        public static explicit operator T8(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf)
        {
            return oneOf.Get<T8>();
        }

        public bool Is<T>() => _value is T;
        public T Get<T>() => _value is T t ? t : throw new InvalidCastException();
        public bool TryGet<T>(out T value) { if (_value is T t) { value = t; return true; } else { value = default!; return false; } }
        public bool Equals<TValue>(TValue value)
        {
            if (value is OneOf<T1, T2, T3, T4, T5, T6, T7, T8> thisOneOf)
                return object.Equals(this.Get<object>(), thisOneOf.Get<object>());
            else if (value is IOneOf otherOneOf)
                return object.Equals(this.Get<object>(), otherOneOf.Get<object>());
            else
                return object.Equals(this.Get<object>(), value);
        }

        public override bool Equals(object? obj)
        {
            return obj is object other && Equals<object>(other);
        }

        public override int GetHashCode()
        {
            return _value?.GetHashCode() ?? 0;
        }

        public bool Equals(OneOf<T1, T2, T3, T4, T5, T6, T7, T8>? other)
        {
            return object.Equals(_value, other?._value);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf, IOneOf? other)
        {
            return object.Equals(oneOf._value, other?.Get<object>());
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf, IOneOf? other)
        {
            return !object.Equals(oneOf._value, other?.Get<object>());
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf, T1 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf, T1 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf, T2 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf, T2 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf, T3 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf, T3 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf, T4 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf, T4 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf, T5 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf, T5 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf, T6 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf, T6 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf, T7 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf, T7 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf, T8 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> oneOf, T8 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public override string ToString()
        {
            return _value?.ToString() ?? "";
        }
    }

    public class OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>
        : IOneOf, IEquatable<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>
    {
        private readonly object _value;

        private OneOf(object value) { _value = value; }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T1 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T2 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T3 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T4 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T5 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T6 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T7 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T8 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T9 value)
        {
            return new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(value!);
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create<TValue>(TValue value)
        {
            switch (value)
            {
                case T1 value1: return Create(value1);
                case T2 value2: return Create(value2);
                case T3 value3: return Create(value3);
                case T4 value4: return Create(value4);
                case T5 value5: return Create(value5);
                case T6 value6: return Create(value6);
                case T7 value7: return Create(value7);
                case T8 value8: return Create(value8);
                case T9 value9: return Create(value9);
                case IOneOf otherOneOf: return Create(otherOneOf.Get<object>());
                default: throw new InvalidCastException();
            }
        }

        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Convert<TOneOf>(TOneOf oneOf) where TOneOf : IOneOf
        {
            if (oneOf is OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> me) return me;
            if (oneOf.TryGet(out T1 value1)) return Create(value1);
            if (oneOf.TryGet(out T2 value2)) return Create(value2);
            if (oneOf.TryGet(out T3 value3)) return Create(value3);
            if (oneOf.TryGet(out T4 value4)) return Create(value4);
            if (oneOf.TryGet(out T5 value5)) return Create(value5);
            if (oneOf.TryGet(out T6 value6)) return Create(value6);
            if (oneOf.TryGet(out T7 value7)) return Create(value7);
            if (oneOf.TryGet(out T8 value8)) return Create(value8);
            if (oneOf.TryGet(out T9 value9)) return Create(value9);
            throw new InvalidCastException();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 value)
        {
            return Create<T1>(value);
        }

        public static explicit operator T1(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf)
        {
            return oneOf.Get<T1>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T2 value)
        {
            return Create<T2>(value);
        }

        public static explicit operator T2(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf)
        {
            return oneOf.Get<T2>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T3 value)
        {
            return Create<T3>(value);
        }

        public static explicit operator T3(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf)
        {
            return oneOf.Get<T3>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T4 value)
        {
            return Create<T4>(value);
        }

        public static explicit operator T4(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf)
        {
            return oneOf.Get<T4>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T5 value)
        {
            return Create<T5>(value);
        }

        public static explicit operator T5(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf)
        {
            return oneOf.Get<T5>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T6 value)
        {
            return Create<T6>(value);
        }

        public static explicit operator T6(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf)
        {
            return oneOf.Get<T6>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T7 value)
        {
            return Create<T7>(value);
        }

        public static explicit operator T7(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf)
        {
            return oneOf.Get<T7>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T8 value)
        {
            return Create<T8>(value);
        }

        public static explicit operator T8(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf)
        {
            return oneOf.Get<T8>();
        }

        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T9 value)
        {
            return Create<T9>(value);
        }

        public static explicit operator T9(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf)
        {
            return oneOf.Get<T9>();
        }

        public bool Is<T>() => _value is T;
        public T Get<T>() => _value is T t ? t : throw new InvalidCastException();
        public bool TryGet<T>(out T value) { if (_value is T t) { value = t; return true; } else { value = default!; return false; } }
        public bool Equals<TValue>(TValue value)
        {
            if (value is OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> thisOneOf)
                return object.Equals(this.Get<object>(), thisOneOf.Get<object>());
            else if (value is IOneOf otherOneOf)
                return object.Equals(this.Get<object>(), otherOneOf.Get<object>());
            else
                return object.Equals(this.Get<object>(), value);
        }

        public override bool Equals(object? obj)
        {
            return obj is object other && Equals<object>(other);
        }

        public override int GetHashCode()
        {
            return _value?.GetHashCode() ?? 0;
        }

        public bool Equals(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>? other)
        {
            return object.Equals(_value, other?._value);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf, IOneOf? other)
        {
            return object.Equals(oneOf._value, other?.Get<object>());
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf, IOneOf? other)
        {
            return !object.Equals(oneOf._value, other?.Get<object>());
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf, T1 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf, T1 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf, T2 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf, T2 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf, T3 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf, T3 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf, T4 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf, T4 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf, T5 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf, T5 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf, T6 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf, T6 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf, T7 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf, T7 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf, T8 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf, T8 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf, T9 other)
        {
            return object.Equals(oneOf._value, other);
        }

        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> oneOf, T9 other)
        {
            return !object.Equals(oneOf._value, other);
        }

        public override string ToString()
        {
            return _value?.ToString() ?? "";
        }
    }
}


