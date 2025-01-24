// 
// 
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#nullable enable

namespace UnionTypes.Toolkit
{
    public struct OneOf<T1, T2>
        : IOneOf<OneOf<T1, T2>>, IEquatable<OneOf<T1, T2>>
    {
        /// <summary>The type case for the union's value; 1 == T1, 2 == T2, etc.</summary>
        public int Kind { get; }
        /// <summary>The underlying value of the union.</summary>
        public object Value { get; }
        private OneOf(int kind, object value) { this.Kind = kind; this.Value = value; }
        static OneOf<T1, T2> IOneOf<OneOf<T1, T2>>.Construct(int kind, object value) => new OneOf<T1, T2>(kind, value);
        public static OneOf<T1, T2> Create(T1 value) => new OneOf<T1, T2>(1, value!);
        public static OneOf<T1, T2> Create(T2 value) => new OneOf<T1, T2>(2, value!);
        public static bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2> union) => OneOf.TryCreate<OneOf<T1, T2>, TValue>(value, out union);
        public static OneOf<T1, T2> Create<TValue>(TValue value) => TryCreate(value, out var union) ? union : throw new InvalidCastException("Invalid type for union.");
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Type1Value => (this.Value is T1 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T2"/>.</summary>
        public T2 Type2Value => (this.Value is T2 value || this.TryGet(out value)) ? value : default!;
        public Type Type => this.Value?.GetType() ?? typeof(object);
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2)];
        static IReadOnlyList<Type> IClosedTypeUnion<OneOf<T1, T2>>.Types => _types;
        public bool TryGet<TValue>([NotNullWhen(true)] out TValue value)
        {
            if (this.Value is TValue tval) { value = tval; return true; }
            return TypeUnion.TryCreate(this.Value, out value);
        }
        /// <summary>Returns the ToString() result of the value held by the union.</summary>
        public override string ToString() => this.Value?.ToString() ?? "";
        public static implicit operator OneOf<T1, T2>(T1 value) => Create(value);
        public static implicit operator OneOf<T1, T2>(T2 value) => Create(value);
        public static explicit operator T1(OneOf<T1, T2> union) => union.Kind == 1 ? union.Type1Value : throw new InvalidCastException();
        public static explicit operator T2(OneOf<T1, T2> union) => union.Kind == 2 ? union.Type2Value : throw new InvalidCastException();
        public bool Equals(OneOf<T1, T2> other) => object.Equals(this.Value, other.Value);
        public bool Equals<TValue>(TValue value) => (value is OneOf<T1, T2> other || TryCreate(value, out other)) && Equals(other);
        public override bool Equals(object? obj) => Equals<object?>(obj);
        public override int GetHashCode() => this.Value?.GetHashCode() ?? 0;
        public static bool operator ==(OneOf<T1, T2> a, OneOf<T1, T2> b) => a.Equals(b);
        public static bool operator !=(OneOf<T1, T2> a, OneOf<T1, T2> b) => !a.Equals(b);
        public TResult Select<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2)
        {
            switch (this.Kind)
            {
                case 1: return match1(Type1Value);
                case 2: return match2(Type2Value);
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
        public void Match<TResult>(Action<T1> match1, Action<T2> match2)
        {
            switch (this.Kind)
            {
                case 1: match1(Type1Value); break;
                case 2: match2(Type2Value); break;
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
    }

    public struct OneOf<T1, T2, T3>
        : IOneOf<OneOf<T1, T2, T3>>, IEquatable<OneOf<T1, T2, T3>>
    {
        /// <summary>The type case for the union's value; 1 == T1, 2 == T2, etc.</summary>
        public int Kind { get; }
        /// <summary>The underlying value of the union.</summary>
        public object Value { get; }
        private OneOf(int kind, object value) { this.Kind = kind; this.Value = value; }
        static OneOf<T1, T2, T3> IOneOf<OneOf<T1, T2, T3>>.Construct(int kind, object value) => new OneOf<T1, T2, T3>(kind, value);
        public static OneOf<T1, T2, T3> Create(T1 value) => new OneOf<T1, T2, T3>(1, value!);
        public static OneOf<T1, T2, T3> Create(T2 value) => new OneOf<T1, T2, T3>(2, value!);
        public static OneOf<T1, T2, T3> Create(T3 value) => new OneOf<T1, T2, T3>(3, value!);
        public static bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2, T3> union) => OneOf.TryCreate<OneOf<T1, T2, T3>, TValue>(value, out union);
        public static OneOf<T1, T2, T3> Create<TValue>(TValue value) => TryCreate(value, out var union) ? union : throw new InvalidCastException("Invalid type for union.");
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Type1Value => (this.Value is T1 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T2"/>.</summary>
        public T2 Type2Value => (this.Value is T2 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T3"/>.</summary>
        public T3 Type3Value => (this.Value is T3 value || this.TryGet(out value)) ? value : default!;
        public Type Type => this.Value?.GetType() ?? typeof(object);
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2), typeof(T3)];
        static IReadOnlyList<Type> IClosedTypeUnion<OneOf<T1, T2, T3>>.Types => _types;
        public bool TryGet<TValue>([NotNullWhen(true)] out TValue value)
        {
            if (this.Value is TValue tval) { value = tval; return true; }
            return TypeUnion.TryCreate(this.Value, out value);
        }
        /// <summary>Returns the ToString() result of the value held by the union.</summary>
        public override string ToString() => this.Value?.ToString() ?? "";
        public static implicit operator OneOf<T1, T2, T3>(T1 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3>(T2 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3>(T3 value) => Create(value);
        public static explicit operator T1(OneOf<T1, T2, T3> union) => union.Kind == 1 ? union.Type1Value : throw new InvalidCastException();
        public static explicit operator T2(OneOf<T1, T2, T3> union) => union.Kind == 2 ? union.Type2Value : throw new InvalidCastException();
        public static explicit operator T3(OneOf<T1, T2, T3> union) => union.Kind == 3 ? union.Type3Value : throw new InvalidCastException();
        public bool Equals(OneOf<T1, T2, T3> other) => object.Equals(this.Value, other.Value);
        public bool Equals<TValue>(TValue value) => (value is OneOf<T1, T2, T3> other || TryCreate(value, out other)) && Equals(other);
        public override bool Equals(object? obj) => Equals<object?>(obj);
        public override int GetHashCode() => this.Value?.GetHashCode() ?? 0;
        public static bool operator ==(OneOf<T1, T2, T3> a, OneOf<T1, T2, T3> b) => a.Equals(b);
        public static bool operator !=(OneOf<T1, T2, T3> a, OneOf<T1, T2, T3> b) => !a.Equals(b);
        public TResult Select<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2, Func<T3, TResult> match3)
        {
            switch (this.Kind)
            {
                case 1: return match1(Type1Value);
                case 2: return match2(Type2Value);
                case 3: return match3(Type3Value);
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
        public void Match<TResult>(Action<T1> match1, Action<T2> match2, Action<T3> match3)
        {
            switch (this.Kind)
            {
                case 1: match1(Type1Value); break;
                case 2: match2(Type2Value); break;
                case 3: match3(Type3Value); break;
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
    }

    public struct OneOf<T1, T2, T3, T4>
        : IOneOf<OneOf<T1, T2, T3, T4>>, IEquatable<OneOf<T1, T2, T3, T4>>
    {
        /// <summary>The type case for the union's value; 1 == T1, 2 == T2, etc.</summary>
        public int Kind { get; }
        /// <summary>The underlying value of the union.</summary>
        public object Value { get; }
        private OneOf(int kind, object value) { this.Kind = kind; this.Value = value; }
        static OneOf<T1, T2, T3, T4> IOneOf<OneOf<T1, T2, T3, T4>>.Construct(int kind, object value) => new OneOf<T1, T2, T3, T4>(kind, value);
        public static OneOf<T1, T2, T3, T4> Create(T1 value) => new OneOf<T1, T2, T3, T4>(1, value!);
        public static OneOf<T1, T2, T3, T4> Create(T2 value) => new OneOf<T1, T2, T3, T4>(2, value!);
        public static OneOf<T1, T2, T3, T4> Create(T3 value) => new OneOf<T1, T2, T3, T4>(3, value!);
        public static OneOf<T1, T2, T3, T4> Create(T4 value) => new OneOf<T1, T2, T3, T4>(4, value!);
        public static bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2, T3, T4> union) => OneOf.TryCreate<OneOf<T1, T2, T3, T4>, TValue>(value, out union);
        public static OneOf<T1, T2, T3, T4> Create<TValue>(TValue value) => TryCreate(value, out var union) ? union : throw new InvalidCastException("Invalid type for union.");
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Type1Value => (this.Value is T1 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T2"/>.</summary>
        public T2 Type2Value => (this.Value is T2 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T3"/>.</summary>
        public T3 Type3Value => (this.Value is T3 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T4"/>.</summary>
        public T4 Type4Value => (this.Value is T4 value || this.TryGet(out value)) ? value : default!;
        public Type Type => this.Value?.GetType() ?? typeof(object);
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2), typeof(T3), typeof(T4)];
        static IReadOnlyList<Type> IClosedTypeUnion<OneOf<T1, T2, T3, T4>>.Types => _types;
        public bool TryGet<TValue>([NotNullWhen(true)] out TValue value)
        {
            if (this.Value is TValue tval) { value = tval; return true; }
            return TypeUnion.TryCreate(this.Value, out value);
        }
        /// <summary>Returns the ToString() result of the value held by the union.</summary>
        public override string ToString() => this.Value?.ToString() ?? "";
        public static implicit operator OneOf<T1, T2, T3, T4>(T1 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4>(T2 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4>(T3 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4>(T4 value) => Create(value);
        public static explicit operator T1(OneOf<T1, T2, T3, T4> union) => union.Kind == 1 ? union.Type1Value : throw new InvalidCastException();
        public static explicit operator T2(OneOf<T1, T2, T3, T4> union) => union.Kind == 2 ? union.Type2Value : throw new InvalidCastException();
        public static explicit operator T3(OneOf<T1, T2, T3, T4> union) => union.Kind == 3 ? union.Type3Value : throw new InvalidCastException();
        public static explicit operator T4(OneOf<T1, T2, T3, T4> union) => union.Kind == 4 ? union.Type4Value : throw new InvalidCastException();
        public bool Equals(OneOf<T1, T2, T3, T4> other) => object.Equals(this.Value, other.Value);
        public bool Equals<TValue>(TValue value) => (value is OneOf<T1, T2, T3, T4> other || TryCreate(value, out other)) && Equals(other);
        public override bool Equals(object? obj) => Equals<object?>(obj);
        public override int GetHashCode() => this.Value?.GetHashCode() ?? 0;
        public static bool operator ==(OneOf<T1, T2, T3, T4> a, OneOf<T1, T2, T3, T4> b) => a.Equals(b);
        public static bool operator !=(OneOf<T1, T2, T3, T4> a, OneOf<T1, T2, T3, T4> b) => !a.Equals(b);
        public TResult Select<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2, Func<T3, TResult> match3, Func<T4, TResult> match4)
        {
            switch (this.Kind)
            {
                case 1: return match1(Type1Value);
                case 2: return match2(Type2Value);
                case 3: return match3(Type3Value);
                case 4: return match4(Type4Value);
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
        public void Match<TResult>(Action<T1> match1, Action<T2> match2, Action<T3> match3, Action<T4> match4)
        {
            switch (this.Kind)
            {
                case 1: match1(Type1Value); break;
                case 2: match2(Type2Value); break;
                case 3: match3(Type3Value); break;
                case 4: match4(Type4Value); break;
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
    }

    public struct OneOf<T1, T2, T3, T4, T5>
        : IOneOf<OneOf<T1, T2, T3, T4, T5>>, IEquatable<OneOf<T1, T2, T3, T4, T5>>
    {
        /// <summary>The type case for the union's value; 1 == T1, 2 == T2, etc.</summary>
        public int Kind { get; }
        /// <summary>The underlying value of the union.</summary>
        public object Value { get; }
        private OneOf(int kind, object value) { this.Kind = kind; this.Value = value; }
        static OneOf<T1, T2, T3, T4, T5> IOneOf<OneOf<T1, T2, T3, T4, T5>>.Construct(int kind, object value) => new OneOf<T1, T2, T3, T4, T5>(kind, value);
        public static OneOf<T1, T2, T3, T4, T5> Create(T1 value) => new OneOf<T1, T2, T3, T4, T5>(1, value!);
        public static OneOf<T1, T2, T3, T4, T5> Create(T2 value) => new OneOf<T1, T2, T3, T4, T5>(2, value!);
        public static OneOf<T1, T2, T3, T4, T5> Create(T3 value) => new OneOf<T1, T2, T3, T4, T5>(3, value!);
        public static OneOf<T1, T2, T3, T4, T5> Create(T4 value) => new OneOf<T1, T2, T3, T4, T5>(4, value!);
        public static OneOf<T1, T2, T3, T4, T5> Create(T5 value) => new OneOf<T1, T2, T3, T4, T5>(5, value!);
        public static bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2, T3, T4, T5> union) => OneOf.TryCreate<OneOf<T1, T2, T3, T4, T5>, TValue>(value, out union);
        public static OneOf<T1, T2, T3, T4, T5> Create<TValue>(TValue value) => TryCreate(value, out var union) ? union : throw new InvalidCastException("Invalid type for union.");
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Type1Value => (this.Value is T1 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T2"/>.</summary>
        public T2 Type2Value => (this.Value is T2 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T3"/>.</summary>
        public T3 Type3Value => (this.Value is T3 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T4"/>.</summary>
        public T4 Type4Value => (this.Value is T4 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T5"/>.</summary>
        public T5 Type5Value => (this.Value is T5 value || this.TryGet(out value)) ? value : default!;
        public Type Type => this.Value?.GetType() ?? typeof(object);
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5)];
        static IReadOnlyList<Type> IClosedTypeUnion<OneOf<T1, T2, T3, T4, T5>>.Types => _types;
        public bool TryGet<TValue>([NotNullWhen(true)] out TValue value)
        {
            if (this.Value is TValue tval) { value = tval; return true; }
            return TypeUnion.TryCreate(this.Value, out value);
        }
        /// <summary>Returns the ToString() result of the value held by the union.</summary>
        public override string ToString() => this.Value?.ToString() ?? "";
        public static implicit operator OneOf<T1, T2, T3, T4, T5>(T1 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5>(T2 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5>(T3 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5>(T4 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5>(T5 value) => Create(value);
        public static explicit operator T1(OneOf<T1, T2, T3, T4, T5> union) => union.Kind == 1 ? union.Type1Value : throw new InvalidCastException();
        public static explicit operator T2(OneOf<T1, T2, T3, T4, T5> union) => union.Kind == 2 ? union.Type2Value : throw new InvalidCastException();
        public static explicit operator T3(OneOf<T1, T2, T3, T4, T5> union) => union.Kind == 3 ? union.Type3Value : throw new InvalidCastException();
        public static explicit operator T4(OneOf<T1, T2, T3, T4, T5> union) => union.Kind == 4 ? union.Type4Value : throw new InvalidCastException();
        public static explicit operator T5(OneOf<T1, T2, T3, T4, T5> union) => union.Kind == 5 ? union.Type5Value : throw new InvalidCastException();
        public bool Equals(OneOf<T1, T2, T3, T4, T5> other) => object.Equals(this.Value, other.Value);
        public bool Equals<TValue>(TValue value) => (value is OneOf<T1, T2, T3, T4, T5> other || TryCreate(value, out other)) && Equals(other);
        public override bool Equals(object? obj) => Equals<object?>(obj);
        public override int GetHashCode() => this.Value?.GetHashCode() ?? 0;
        public static bool operator ==(OneOf<T1, T2, T3, T4, T5> a, OneOf<T1, T2, T3, T4, T5> b) => a.Equals(b);
        public static bool operator !=(OneOf<T1, T2, T3, T4, T5> a, OneOf<T1, T2, T3, T4, T5> b) => !a.Equals(b);
        public TResult Select<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2, Func<T3, TResult> match3, Func<T4, TResult> match4, Func<T5, TResult> match5)
        {
            switch (this.Kind)
            {
                case 1: return match1(Type1Value);
                case 2: return match2(Type2Value);
                case 3: return match3(Type3Value);
                case 4: return match4(Type4Value);
                case 5: return match5(Type5Value);
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
        public void Match<TResult>(Action<T1> match1, Action<T2> match2, Action<T3> match3, Action<T4> match4, Action<T5> match5)
        {
            switch (this.Kind)
            {
                case 1: match1(Type1Value); break;
                case 2: match2(Type2Value); break;
                case 3: match3(Type3Value); break;
                case 4: match4(Type4Value); break;
                case 5: match5(Type5Value); break;
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
    }

    public struct OneOf<T1, T2, T3, T4, T5, T6>
        : IOneOf<OneOf<T1, T2, T3, T4, T5, T6>>, IEquatable<OneOf<T1, T2, T3, T4, T5, T6>>
    {
        /// <summary>The type case for the union's value; 1 == T1, 2 == T2, etc.</summary>
        public int Kind { get; }
        /// <summary>The underlying value of the union.</summary>
        public object Value { get; }
        private OneOf(int kind, object value) { this.Kind = kind; this.Value = value; }
        static OneOf<T1, T2, T3, T4, T5, T6> IOneOf<OneOf<T1, T2, T3, T4, T5, T6>>.Construct(int kind, object value) => new OneOf<T1, T2, T3, T4, T5, T6>(kind, value);
        public static OneOf<T1, T2, T3, T4, T5, T6> Create(T1 value) => new OneOf<T1, T2, T3, T4, T5, T6>(1, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6> Create(T2 value) => new OneOf<T1, T2, T3, T4, T5, T6>(2, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6> Create(T3 value) => new OneOf<T1, T2, T3, T4, T5, T6>(3, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6> Create(T4 value) => new OneOf<T1, T2, T3, T4, T5, T6>(4, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6> Create(T5 value) => new OneOf<T1, T2, T3, T4, T5, T6>(5, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6> Create(T6 value) => new OneOf<T1, T2, T3, T4, T5, T6>(6, value!);
        public static bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2, T3, T4, T5, T6> union) => OneOf.TryCreate<OneOf<T1, T2, T3, T4, T5, T6>, TValue>(value, out union);
        public static OneOf<T1, T2, T3, T4, T5, T6> Create<TValue>(TValue value) => TryCreate(value, out var union) ? union : throw new InvalidCastException("Invalid type for union.");
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Type1Value => (this.Value is T1 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T2"/>.</summary>
        public T2 Type2Value => (this.Value is T2 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T3"/>.</summary>
        public T3 Type3Value => (this.Value is T3 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T4"/>.</summary>
        public T4 Type4Value => (this.Value is T4 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T5"/>.</summary>
        public T5 Type5Value => (this.Value is T5 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T6"/>.</summary>
        public T6 Type6Value => (this.Value is T6 value || this.TryGet(out value)) ? value : default!;
        public Type Type => this.Value?.GetType() ?? typeof(object);
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6)];
        static IReadOnlyList<Type> IClosedTypeUnion<OneOf<T1, T2, T3, T4, T5, T6>>.Types => _types;
        public bool TryGet<TValue>([NotNullWhen(true)] out TValue value)
        {
            if (this.Value is TValue tval) { value = tval; return true; }
            return TypeUnion.TryCreate(this.Value, out value);
        }
        /// <summary>Returns the ToString() result of the value held by the union.</summary>
        public override string ToString() => this.Value?.ToString() ?? "";
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T1 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T2 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T3 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T4 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T5 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T6 value) => Create(value);
        public static explicit operator T1(OneOf<T1, T2, T3, T4, T5, T6> union) => union.Kind == 1 ? union.Type1Value : throw new InvalidCastException();
        public static explicit operator T2(OneOf<T1, T2, T3, T4, T5, T6> union) => union.Kind == 2 ? union.Type2Value : throw new InvalidCastException();
        public static explicit operator T3(OneOf<T1, T2, T3, T4, T5, T6> union) => union.Kind == 3 ? union.Type3Value : throw new InvalidCastException();
        public static explicit operator T4(OneOf<T1, T2, T3, T4, T5, T6> union) => union.Kind == 4 ? union.Type4Value : throw new InvalidCastException();
        public static explicit operator T5(OneOf<T1, T2, T3, T4, T5, T6> union) => union.Kind == 5 ? union.Type5Value : throw new InvalidCastException();
        public static explicit operator T6(OneOf<T1, T2, T3, T4, T5, T6> union) => union.Kind == 6 ? union.Type6Value : throw new InvalidCastException();
        public bool Equals(OneOf<T1, T2, T3, T4, T5, T6> other) => object.Equals(this.Value, other.Value);
        public bool Equals<TValue>(TValue value) => (value is OneOf<T1, T2, T3, T4, T5, T6> other || TryCreate(value, out other)) && Equals(other);
        public override bool Equals(object? obj) => Equals<object?>(obj);
        public override int GetHashCode() => this.Value?.GetHashCode() ?? 0;
        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6> a, OneOf<T1, T2, T3, T4, T5, T6> b) => a.Equals(b);
        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6> a, OneOf<T1, T2, T3, T4, T5, T6> b) => !a.Equals(b);
        public TResult Select<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2, Func<T3, TResult> match3, Func<T4, TResult> match4, Func<T5, TResult> match5, Func<T6, TResult> match6)
        {
            switch (this.Kind)
            {
                case 1: return match1(Type1Value);
                case 2: return match2(Type2Value);
                case 3: return match3(Type3Value);
                case 4: return match4(Type4Value);
                case 5: return match5(Type5Value);
                case 6: return match6(Type6Value);
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
        public void Match<TResult>(Action<T1> match1, Action<T2> match2, Action<T3> match3, Action<T4> match4, Action<T5> match5, Action<T6> match6)
        {
            switch (this.Kind)
            {
                case 1: match1(Type1Value); break;
                case 2: match2(Type2Value); break;
                case 3: match3(Type3Value); break;
                case 4: match4(Type4Value); break;
                case 5: match5(Type5Value); break;
                case 6: match6(Type6Value); break;
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
    }

    public struct OneOf<T1, T2, T3, T4, T5, T6, T7>
        : IOneOf<OneOf<T1, T2, T3, T4, T5, T6, T7>>, IEquatable<OneOf<T1, T2, T3, T4, T5, T6, T7>>
    {
        /// <summary>The type case for the union's value; 1 == T1, 2 == T2, etc.</summary>
        public int Kind { get; }
        /// <summary>The underlying value of the union.</summary>
        public object Value { get; }
        private OneOf(int kind, object value) { this.Kind = kind; this.Value = value; }
        static OneOf<T1, T2, T3, T4, T5, T6, T7> IOneOf<OneOf<T1, T2, T3, T4, T5, T6, T7>>.Construct(int kind, object value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(kind, value);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create(T1 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(1, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create(T2 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(2, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create(T3 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(3, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create(T4 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(4, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create(T5 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(5, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create(T6 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(6, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create(T7 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(7, value!);
        public static bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2, T3, T4, T5, T6, T7> union) => OneOf.TryCreate<OneOf<T1, T2, T3, T4, T5, T6, T7>, TValue>(value, out union);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create<TValue>(TValue value) => TryCreate(value, out var union) ? union : throw new InvalidCastException("Invalid type for union.");
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Type1Value => (this.Value is T1 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T2"/>.</summary>
        public T2 Type2Value => (this.Value is T2 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T3"/>.</summary>
        public T3 Type3Value => (this.Value is T3 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T4"/>.</summary>
        public T4 Type4Value => (this.Value is T4 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T5"/>.</summary>
        public T5 Type5Value => (this.Value is T5 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T6"/>.</summary>
        public T6 Type6Value => (this.Value is T6 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T7"/>.</summary>
        public T7 Type7Value => (this.Value is T7 value || this.TryGet(out value)) ? value : default!;
        public Type Type => this.Value?.GetType() ?? typeof(object);
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7)];
        static IReadOnlyList<Type> IClosedTypeUnion<OneOf<T1, T2, T3, T4, T5, T6, T7>>.Types => _types;
        public bool TryGet<TValue>([NotNullWhen(true)] out TValue value)
        {
            if (this.Value is TValue tval) { value = tval; return true; }
            return TypeUnion.TryCreate(this.Value, out value);
        }
        /// <summary>Returns the ToString() result of the value held by the union.</summary>
        public override string ToString() => this.Value?.ToString() ?? "";
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T1 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T2 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T3 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T4 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T5 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T6 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T7 value) => Create(value);
        public static explicit operator T1(OneOf<T1, T2, T3, T4, T5, T6, T7> union) => union.Kind == 1 ? union.Type1Value : throw new InvalidCastException();
        public static explicit operator T2(OneOf<T1, T2, T3, T4, T5, T6, T7> union) => union.Kind == 2 ? union.Type2Value : throw new InvalidCastException();
        public static explicit operator T3(OneOf<T1, T2, T3, T4, T5, T6, T7> union) => union.Kind == 3 ? union.Type3Value : throw new InvalidCastException();
        public static explicit operator T4(OneOf<T1, T2, T3, T4, T5, T6, T7> union) => union.Kind == 4 ? union.Type4Value : throw new InvalidCastException();
        public static explicit operator T5(OneOf<T1, T2, T3, T4, T5, T6, T7> union) => union.Kind == 5 ? union.Type5Value : throw new InvalidCastException();
        public static explicit operator T6(OneOf<T1, T2, T3, T4, T5, T6, T7> union) => union.Kind == 6 ? union.Type6Value : throw new InvalidCastException();
        public static explicit operator T7(OneOf<T1, T2, T3, T4, T5, T6, T7> union) => union.Kind == 7 ? union.Type7Value : throw new InvalidCastException();
        public bool Equals(OneOf<T1, T2, T3, T4, T5, T6, T7> other) => object.Equals(this.Value, other.Value);
        public bool Equals<TValue>(TValue value) => (value is OneOf<T1, T2, T3, T4, T5, T6, T7> other || TryCreate(value, out other)) && Equals(other);
        public override bool Equals(object? obj) => Equals<object?>(obj);
        public override int GetHashCode() => this.Value?.GetHashCode() ?? 0;
        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7> a, OneOf<T1, T2, T3, T4, T5, T6, T7> b) => a.Equals(b);
        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7> a, OneOf<T1, T2, T3, T4, T5, T6, T7> b) => !a.Equals(b);
        public TResult Select<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2, Func<T3, TResult> match3, Func<T4, TResult> match4, Func<T5, TResult> match5, Func<T6, TResult> match6, Func<T7, TResult> match7)
        {
            switch (this.Kind)
            {
                case 1: return match1(Type1Value);
                case 2: return match2(Type2Value);
                case 3: return match3(Type3Value);
                case 4: return match4(Type4Value);
                case 5: return match5(Type5Value);
                case 6: return match6(Type6Value);
                case 7: return match7(Type7Value);
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
        public void Match<TResult>(Action<T1> match1, Action<T2> match2, Action<T3> match3, Action<T4> match4, Action<T5> match5, Action<T6> match6, Action<T7> match7)
        {
            switch (this.Kind)
            {
                case 1: match1(Type1Value); break;
                case 2: match2(Type2Value); break;
                case 3: match3(Type3Value); break;
                case 4: match4(Type4Value); break;
                case 5: match5(Type5Value); break;
                case 6: match6(Type6Value); break;
                case 7: match7(Type7Value); break;
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
    }

    public struct OneOf<T1, T2, T3, T4, T5, T6, T7, T8>
        : IOneOf<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>, IEquatable<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>
    {
        /// <summary>The type case for the union's value; 1 == T1, 2 == T2, etc.</summary>
        public int Kind { get; }
        /// <summary>The underlying value of the union.</summary>
        public object Value { get; }
        private OneOf(int kind, object value) { this.Kind = kind; this.Value = value; }
        static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> IOneOf<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>.Construct(int kind, object value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(kind, value);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T1 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(1, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T2 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(2, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T3 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(3, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T4 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(4, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T5 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(5, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T6 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(6, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T7 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(7, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T8 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(8, value!);
        public static bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2, T3, T4, T5, T6, T7, T8> union) => OneOf.TryCreate<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>, TValue>(value, out union);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create<TValue>(TValue value) => TryCreate(value, out var union) ? union : throw new InvalidCastException("Invalid type for union.");
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Type1Value => (this.Value is T1 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T2"/>.</summary>
        public T2 Type2Value => (this.Value is T2 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T3"/>.</summary>
        public T3 Type3Value => (this.Value is T3 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T4"/>.</summary>
        public T4 Type4Value => (this.Value is T4 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T5"/>.</summary>
        public T5 Type5Value => (this.Value is T5 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T6"/>.</summary>
        public T6 Type6Value => (this.Value is T6 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T7"/>.</summary>
        public T7 Type7Value => (this.Value is T7 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T8"/>.</summary>
        public T8 Type8Value => (this.Value is T8 value || this.TryGet(out value)) ? value : default!;
        public Type Type => this.Value?.GetType() ?? typeof(object);
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8)];
        static IReadOnlyList<Type> IClosedTypeUnion<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>.Types => _types;
        public bool TryGet<TValue>([NotNullWhen(true)] out TValue value)
        {
            if (this.Value is TValue tval) { value = tval; return true; }
            return TypeUnion.TryCreate(this.Value, out value);
        }
        /// <summary>Returns the ToString() result of the value held by the union.</summary>
        public override string ToString() => this.Value?.ToString() ?? "";
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T2 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T3 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T4 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T5 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T6 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T7 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T8 value) => Create(value);
        public static explicit operator T1(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> union) => union.Kind == 1 ? union.Type1Value : throw new InvalidCastException();
        public static explicit operator T2(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> union) => union.Kind == 2 ? union.Type2Value : throw new InvalidCastException();
        public static explicit operator T3(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> union) => union.Kind == 3 ? union.Type3Value : throw new InvalidCastException();
        public static explicit operator T4(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> union) => union.Kind == 4 ? union.Type4Value : throw new InvalidCastException();
        public static explicit operator T5(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> union) => union.Kind == 5 ? union.Type5Value : throw new InvalidCastException();
        public static explicit operator T6(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> union) => union.Kind == 6 ? union.Type6Value : throw new InvalidCastException();
        public static explicit operator T7(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> union) => union.Kind == 7 ? union.Type7Value : throw new InvalidCastException();
        public static explicit operator T8(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> union) => union.Kind == 8 ? union.Type8Value : throw new InvalidCastException();
        public bool Equals(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> other) => object.Equals(this.Value, other.Value);
        public bool Equals<TValue>(TValue value) => (value is OneOf<T1, T2, T3, T4, T5, T6, T7, T8> other || TryCreate(value, out other)) && Equals(other);
        public override bool Equals(object? obj) => Equals<object?>(obj);
        public override int GetHashCode() => this.Value?.GetHashCode() ?? 0;
        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> a, OneOf<T1, T2, T3, T4, T5, T6, T7, T8> b) => a.Equals(b);
        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7, T8> a, OneOf<T1, T2, T3, T4, T5, T6, T7, T8> b) => !a.Equals(b);
        public TResult Select<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2, Func<T3, TResult> match3, Func<T4, TResult> match4, Func<T5, TResult> match5, Func<T6, TResult> match6, Func<T7, TResult> match7, Func<T8, TResult> match8)
        {
            switch (this.Kind)
            {
                case 1: return match1(Type1Value);
                case 2: return match2(Type2Value);
                case 3: return match3(Type3Value);
                case 4: return match4(Type4Value);
                case 5: return match5(Type5Value);
                case 6: return match6(Type6Value);
                case 7: return match7(Type7Value);
                case 8: return match8(Type8Value);
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
        public void Match<TResult>(Action<T1> match1, Action<T2> match2, Action<T3> match3, Action<T4> match4, Action<T5> match5, Action<T6> match6, Action<T7> match7, Action<T8> match8)
        {
            switch (this.Kind)
            {
                case 1: match1(Type1Value); break;
                case 2: match2(Type2Value); break;
                case 3: match3(Type3Value); break;
                case 4: match4(Type4Value); break;
                case 5: match5(Type5Value); break;
                case 6: match6(Type6Value); break;
                case 7: match7(Type7Value); break;
                case 8: match8(Type8Value); break;
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
    }

    public struct OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>
        : IOneOf<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>, IEquatable<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>
    {
        /// <summary>The type case for the union's value; 1 == T1, 2 == T2, etc.</summary>
        public int Kind { get; }
        /// <summary>The underlying value of the union.</summary>
        public object Value { get; }
        private OneOf(int kind, object value) { this.Kind = kind; this.Value = value; }
        static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> IOneOf<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Construct(int kind, object value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(kind, value);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T1 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(1, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T2 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(2, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T3 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(3, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T4 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(4, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T5 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(5, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T6 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(6, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T7 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(7, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T8 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(8, value!);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T9 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(9, value!);
        public static bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> union) => OneOf.TryCreate<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TValue>(value, out union);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create<TValue>(TValue value) => TryCreate(value, out var union) ? union : throw new InvalidCastException("Invalid type for union.");
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Type1Value => (this.Value is T1 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T2"/>.</summary>
        public T2 Type2Value => (this.Value is T2 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T3"/>.</summary>
        public T3 Type3Value => (this.Value is T3 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T4"/>.</summary>
        public T4 Type4Value => (this.Value is T4 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T5"/>.</summary>
        public T5 Type5Value => (this.Value is T5 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T6"/>.</summary>
        public T6 Type6Value => (this.Value is T6 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T7"/>.</summary>
        public T7 Type7Value => (this.Value is T7 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T8"/>.</summary>
        public T8 Type8Value => (this.Value is T8 value || this.TryGet(out value)) ? value : default!;
        /// <summary>The union's value as type <typeparamref name="T9"/>.</summary>
        public T9 Type9Value => (this.Value is T9 value || this.TryGet(out value)) ? value : default!;
        public Type Type => this.Value?.GetType() ?? typeof(object);
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9)];
        static IReadOnlyList<Type> IClosedTypeUnion<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Types => _types;
        public bool TryGet<TValue>([NotNullWhen(true)] out TValue value)
        {
            if (this.Value is TValue tval) { value = tval; return true; }
            return TypeUnion.TryCreate(this.Value, out value);
        }
        /// <summary>Returns the ToString() result of the value held by the union.</summary>
        public override string ToString() => this.Value?.ToString() ?? "";
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T2 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T3 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T4 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T5 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T6 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T7 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T8 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T9 value) => Create(value);
        public static explicit operator T1(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> union) => union.Kind == 1 ? union.Type1Value : throw new InvalidCastException();
        public static explicit operator T2(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> union) => union.Kind == 2 ? union.Type2Value : throw new InvalidCastException();
        public static explicit operator T3(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> union) => union.Kind == 3 ? union.Type3Value : throw new InvalidCastException();
        public static explicit operator T4(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> union) => union.Kind == 4 ? union.Type4Value : throw new InvalidCastException();
        public static explicit operator T5(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> union) => union.Kind == 5 ? union.Type5Value : throw new InvalidCastException();
        public static explicit operator T6(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> union) => union.Kind == 6 ? union.Type6Value : throw new InvalidCastException();
        public static explicit operator T7(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> union) => union.Kind == 7 ? union.Type7Value : throw new InvalidCastException();
        public static explicit operator T8(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> union) => union.Kind == 8 ? union.Type8Value : throw new InvalidCastException();
        public static explicit operator T9(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> union) => union.Kind == 9 ? union.Type9Value : throw new InvalidCastException();
        public bool Equals(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> other) => object.Equals(this.Value, other.Value);
        public bool Equals<TValue>(TValue value) => (value is OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> other || TryCreate(value, out other)) && Equals(other);
        public override bool Equals(object? obj) => Equals<object?>(obj);
        public override int GetHashCode() => this.Value?.GetHashCode() ?? 0;
        public static bool operator ==(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> a, OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> b) => a.Equals(b);
        public static bool operator !=(OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> a, OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> b) => !a.Equals(b);
        public TResult Select<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2, Func<T3, TResult> match3, Func<T4, TResult> match4, Func<T5, TResult> match5, Func<T6, TResult> match6, Func<T7, TResult> match7, Func<T8, TResult> match8, Func<T9, TResult> match9)
        {
            switch (this.Kind)
            {
                case 1: return match1(Type1Value);
                case 2: return match2(Type2Value);
                case 3: return match3(Type3Value);
                case 4: return match4(Type4Value);
                case 5: return match5(Type5Value);
                case 6: return match6(Type6Value);
                case 7: return match7(Type7Value);
                case 8: return match8(Type8Value);
                case 9: return match9(Type9Value);
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
        public void Match<TResult>(Action<T1> match1, Action<T2> match2, Action<T3> match3, Action<T4> match4, Action<T5> match5, Action<T6> match6, Action<T7> match7, Action<T8> match8, Action<T9> match9)
        {
            switch (this.Kind)
            {
                case 1: match1(Type1Value); break;
                case 2: match2(Type2Value); break;
                case 3: match3(Type3Value); break;
                case 4: match4(Type4Value); break;
                case 5: match5(Type5Value); break;
                case 6: match6(Type6Value); break;
                case 7: match7(Type7Value); break;
                case 8: match8(Type8Value); break;
                case 9: match9(Type9Value); break;
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
    }
}


