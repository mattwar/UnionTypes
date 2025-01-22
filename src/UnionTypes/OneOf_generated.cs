// 
// 
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#nullable enable

namespace UnionTypes
{
    public struct OneOf<T1, T2>
        : IOneOf<OneOf<T1, T2>>
    {
        private readonly OneOfCore<OneOf<T1, T2>> _core;
        private OneOf(OneOfCore<OneOf<T1, T2>> core) => _core = core;
        static OneOf<T1, T2> IOneOf<OneOf<T1, T2>>.Construct(OneOfCore<OneOf<T1, T2>> core) => new OneOf<T1, T2>(core);
        public static OneOf<T1, T2> Create(T1 value) => new OneOf<T1, T2>(new OneOfCore<OneOf<T1, T2>>(1, value!));
        public static OneOf<T1, T2> Create(T2 value) => new OneOf<T1, T2>(new OneOfCore<OneOf<T1, T2>>(2, value!));
        public static bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2> union) => OneOfCore<OneOf<T1, T2>>.TryCreateFrom(value, out union);
        public static OneOf<T1, T2> Create<TValue>(TValue value) => TryCreate(value, out var union) ? union : throw new InvalidCastException("Invalid type for union.");
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Type1Value => _core.GetOrDefault<T1>();
        /// <summary>The union's value as type <typeparamref name="T2"/>.</summary>
        public T2 Type2Value => _core.GetOrDefault<T2>();
        /// <summary>The type case for the union's value; 1 == T1, 2 == T2, etc.</summary>
        public int Kind => _core.Kind;
        public Type Type => _core.Type;
        public object Value => _core.Value;
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2)];
        static IReadOnlyList<Type> IClosedTypeUnion<OneOf<T1, T2>>.Types => _types;
        public bool TryGet<T>([NotNullWhen(true)] out T value) => _core.TryGet(out value);
        /// <summary>Returns the ToString() result of the value held by the union.</summary>
        public override string ToString() => _core.ToString();
        public static implicit operator OneOf<T1, T2>(T1 value) => Create(value);
        public static implicit operator OneOf<T1, T2>(T2 value) => Create(value);
        public TResult Match<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2)
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
        : IOneOf<OneOf<T1, T2, T3>>
    {
        private readonly OneOfCore<OneOf<T1, T2, T3>> _core;
        private OneOf(OneOfCore<OneOf<T1, T2, T3>> core) => _core = core;
        static OneOf<T1, T2, T3> IOneOf<OneOf<T1, T2, T3>>.Construct(OneOfCore<OneOf<T1, T2, T3>> core) => new OneOf<T1, T2, T3>(core);
        public static OneOf<T1, T2, T3> Create(T1 value) => new OneOf<T1, T2, T3>(new OneOfCore<OneOf<T1, T2, T3>>(1, value!));
        public static OneOf<T1, T2, T3> Create(T2 value) => new OneOf<T1, T2, T3>(new OneOfCore<OneOf<T1, T2, T3>>(2, value!));
        public static OneOf<T1, T2, T3> Create(T3 value) => new OneOf<T1, T2, T3>(new OneOfCore<OneOf<T1, T2, T3>>(3, value!));
        public static bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2, T3> union) => OneOfCore<OneOf<T1, T2, T3>>.TryCreateFrom(value, out union);
        public static OneOf<T1, T2, T3> Create<TValue>(TValue value) => TryCreate(value, out var union) ? union : throw new InvalidCastException("Invalid type for union.");
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Type1Value => _core.GetOrDefault<T1>();
        /// <summary>The union's value as type <typeparamref name="T2"/>.</summary>
        public T2 Type2Value => _core.GetOrDefault<T2>();
        /// <summary>The union's value as type <typeparamref name="T3"/>.</summary>
        public T3 Type3Value => _core.GetOrDefault<T3>();
        /// <summary>The type case for the union's value; 1 == T1, 2 == T2, etc.</summary>
        public int Kind => _core.Kind;
        public Type Type => _core.Type;
        public object Value => _core.Value;
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2), typeof(T3)];
        static IReadOnlyList<Type> IClosedTypeUnion<OneOf<T1, T2, T3>>.Types => _types;
        public bool TryGet<T>([NotNullWhen(true)] out T value) => _core.TryGet(out value);
        /// <summary>Returns the ToString() result of the value held by the union.</summary>
        public override string ToString() => _core.ToString();
        public static implicit operator OneOf<T1, T2, T3>(T1 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3>(T2 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3>(T3 value) => Create(value);
        public TResult Match<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2, Func<T3, TResult> match3)
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
        : IOneOf<OneOf<T1, T2, T3, T4>>
    {
        private readonly OneOfCore<OneOf<T1, T2, T3, T4>> _core;
        private OneOf(OneOfCore<OneOf<T1, T2, T3, T4>> core) => _core = core;
        static OneOf<T1, T2, T3, T4> IOneOf<OneOf<T1, T2, T3, T4>>.Construct(OneOfCore<OneOf<T1, T2, T3, T4>> core) => new OneOf<T1, T2, T3, T4>(core);
        public static OneOf<T1, T2, T3, T4> Create(T1 value) => new OneOf<T1, T2, T3, T4>(new OneOfCore<OneOf<T1, T2, T3, T4>>(1, value!));
        public static OneOf<T1, T2, T3, T4> Create(T2 value) => new OneOf<T1, T2, T3, T4>(new OneOfCore<OneOf<T1, T2, T3, T4>>(2, value!));
        public static OneOf<T1, T2, T3, T4> Create(T3 value) => new OneOf<T1, T2, T3, T4>(new OneOfCore<OneOf<T1, T2, T3, T4>>(3, value!));
        public static OneOf<T1, T2, T3, T4> Create(T4 value) => new OneOf<T1, T2, T3, T4>(new OneOfCore<OneOf<T1, T2, T3, T4>>(4, value!));
        public static bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2, T3, T4> union) => OneOfCore<OneOf<T1, T2, T3, T4>>.TryCreateFrom(value, out union);
        public static OneOf<T1, T2, T3, T4> Create<TValue>(TValue value) => TryCreate(value, out var union) ? union : throw new InvalidCastException("Invalid type for union.");
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Type1Value => _core.GetOrDefault<T1>();
        /// <summary>The union's value as type <typeparamref name="T2"/>.</summary>
        public T2 Type2Value => _core.GetOrDefault<T2>();
        /// <summary>The union's value as type <typeparamref name="T3"/>.</summary>
        public T3 Type3Value => _core.GetOrDefault<T3>();
        /// <summary>The union's value as type <typeparamref name="T4"/>.</summary>
        public T4 Type4Value => _core.GetOrDefault<T4>();
        /// <summary>The type case for the union's value; 1 == T1, 2 == T2, etc.</summary>
        public int Kind => _core.Kind;
        public Type Type => _core.Type;
        public object Value => _core.Value;
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2), typeof(T3), typeof(T4)];
        static IReadOnlyList<Type> IClosedTypeUnion<OneOf<T1, T2, T3, T4>>.Types => _types;
        public bool TryGet<T>([NotNullWhen(true)] out T value) => _core.TryGet(out value);
        /// <summary>Returns the ToString() result of the value held by the union.</summary>
        public override string ToString() => _core.ToString();
        public static implicit operator OneOf<T1, T2, T3, T4>(T1 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4>(T2 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4>(T3 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4>(T4 value) => Create(value);
        public TResult Match<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2, Func<T3, TResult> match3, Func<T4, TResult> match4)
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
        : IOneOf<OneOf<T1, T2, T3, T4, T5>>
    {
        private readonly OneOfCore<OneOf<T1, T2, T3, T4, T5>> _core;
        private OneOf(OneOfCore<OneOf<T1, T2, T3, T4, T5>> core) => _core = core;
        static OneOf<T1, T2, T3, T4, T5> IOneOf<OneOf<T1, T2, T3, T4, T5>>.Construct(OneOfCore<OneOf<T1, T2, T3, T4, T5>> core) => new OneOf<T1, T2, T3, T4, T5>(core);
        public static OneOf<T1, T2, T3, T4, T5> Create(T1 value) => new OneOf<T1, T2, T3, T4, T5>(new OneOfCore<OneOf<T1, T2, T3, T4, T5>>(1, value!));
        public static OneOf<T1, T2, T3, T4, T5> Create(T2 value) => new OneOf<T1, T2, T3, T4, T5>(new OneOfCore<OneOf<T1, T2, T3, T4, T5>>(2, value!));
        public static OneOf<T1, T2, T3, T4, T5> Create(T3 value) => new OneOf<T1, T2, T3, T4, T5>(new OneOfCore<OneOf<T1, T2, T3, T4, T5>>(3, value!));
        public static OneOf<T1, T2, T3, T4, T5> Create(T4 value) => new OneOf<T1, T2, T3, T4, T5>(new OneOfCore<OneOf<T1, T2, T3, T4, T5>>(4, value!));
        public static OneOf<T1, T2, T3, T4, T5> Create(T5 value) => new OneOf<T1, T2, T3, T4, T5>(new OneOfCore<OneOf<T1, T2, T3, T4, T5>>(5, value!));
        public static bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2, T3, T4, T5> union) => OneOfCore<OneOf<T1, T2, T3, T4, T5>>.TryCreateFrom(value, out union);
        public static OneOf<T1, T2, T3, T4, T5> Create<TValue>(TValue value) => TryCreate(value, out var union) ? union : throw new InvalidCastException("Invalid type for union.");
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Type1Value => _core.GetOrDefault<T1>();
        /// <summary>The union's value as type <typeparamref name="T2"/>.</summary>
        public T2 Type2Value => _core.GetOrDefault<T2>();
        /// <summary>The union's value as type <typeparamref name="T3"/>.</summary>
        public T3 Type3Value => _core.GetOrDefault<T3>();
        /// <summary>The union's value as type <typeparamref name="T4"/>.</summary>
        public T4 Type4Value => _core.GetOrDefault<T4>();
        /// <summary>The union's value as type <typeparamref name="T5"/>.</summary>
        public T5 Type5Value => _core.GetOrDefault<T5>();
        /// <summary>The type case for the union's value; 1 == T1, 2 == T2, etc.</summary>
        public int Kind => _core.Kind;
        public Type Type => _core.Type;
        public object Value => _core.Value;
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5)];
        static IReadOnlyList<Type> IClosedTypeUnion<OneOf<T1, T2, T3, T4, T5>>.Types => _types;
        public bool TryGet<T>([NotNullWhen(true)] out T value) => _core.TryGet(out value);
        /// <summary>Returns the ToString() result of the value held by the union.</summary>
        public override string ToString() => _core.ToString();
        public static implicit operator OneOf<T1, T2, T3, T4, T5>(T1 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5>(T2 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5>(T3 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5>(T4 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5>(T5 value) => Create(value);
        public TResult Match<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2, Func<T3, TResult> match3, Func<T4, TResult> match4, Func<T5, TResult> match5)
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
        : IOneOf<OneOf<T1, T2, T3, T4, T5, T6>>
    {
        private readonly OneOfCore<OneOf<T1, T2, T3, T4, T5, T6>> _core;
        private OneOf(OneOfCore<OneOf<T1, T2, T3, T4, T5, T6>> core) => _core = core;
        static OneOf<T1, T2, T3, T4, T5, T6> IOneOf<OneOf<T1, T2, T3, T4, T5, T6>>.Construct(OneOfCore<OneOf<T1, T2, T3, T4, T5, T6>> core) => new OneOf<T1, T2, T3, T4, T5, T6>(core);
        public static OneOf<T1, T2, T3, T4, T5, T6> Create(T1 value) => new OneOf<T1, T2, T3, T4, T5, T6>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6>>(1, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6> Create(T2 value) => new OneOf<T1, T2, T3, T4, T5, T6>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6>>(2, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6> Create(T3 value) => new OneOf<T1, T2, T3, T4, T5, T6>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6>>(3, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6> Create(T4 value) => new OneOf<T1, T2, T3, T4, T5, T6>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6>>(4, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6> Create(T5 value) => new OneOf<T1, T2, T3, T4, T5, T6>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6>>(5, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6> Create(T6 value) => new OneOf<T1, T2, T3, T4, T5, T6>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6>>(6, value!));
        public static bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2, T3, T4, T5, T6> union) => OneOfCore<OneOf<T1, T2, T3, T4, T5, T6>>.TryCreateFrom(value, out union);
        public static OneOf<T1, T2, T3, T4, T5, T6> Create<TValue>(TValue value) => TryCreate(value, out var union) ? union : throw new InvalidCastException("Invalid type for union.");
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Type1Value => _core.GetOrDefault<T1>();
        /// <summary>The union's value as type <typeparamref name="T2"/>.</summary>
        public T2 Type2Value => _core.GetOrDefault<T2>();
        /// <summary>The union's value as type <typeparamref name="T3"/>.</summary>
        public T3 Type3Value => _core.GetOrDefault<T3>();
        /// <summary>The union's value as type <typeparamref name="T4"/>.</summary>
        public T4 Type4Value => _core.GetOrDefault<T4>();
        /// <summary>The union's value as type <typeparamref name="T5"/>.</summary>
        public T5 Type5Value => _core.GetOrDefault<T5>();
        /// <summary>The union's value as type <typeparamref name="T6"/>.</summary>
        public T6 Type6Value => _core.GetOrDefault<T6>();
        /// <summary>The type case for the union's value; 1 == T1, 2 == T2, etc.</summary>
        public int Kind => _core.Kind;
        public Type Type => _core.Type;
        public object Value => _core.Value;
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6)];
        static IReadOnlyList<Type> IClosedTypeUnion<OneOf<T1, T2, T3, T4, T5, T6>>.Types => _types;
        public bool TryGet<T>([NotNullWhen(true)] out T value) => _core.TryGet(out value);
        /// <summary>Returns the ToString() result of the value held by the union.</summary>
        public override string ToString() => _core.ToString();
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T1 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T2 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T3 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T4 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T5 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T6 value) => Create(value);
        public TResult Match<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2, Func<T3, TResult> match3, Func<T4, TResult> match4, Func<T5, TResult> match5, Func<T6, TResult> match6)
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
        : IOneOf<OneOf<T1, T2, T3, T4, T5, T6, T7>>
    {
        private readonly OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7>> _core;
        private OneOf(OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7>> core) => _core = core;
        static OneOf<T1, T2, T3, T4, T5, T6, T7> IOneOf<OneOf<T1, T2, T3, T4, T5, T6, T7>>.Construct(OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7>> core) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(core);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create(T1 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7>>(1, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create(T2 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7>>(2, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create(T3 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7>>(3, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create(T4 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7>>(4, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create(T5 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7>>(5, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create(T6 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7>>(6, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create(T7 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7>>(7, value!));
        public static bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2, T3, T4, T5, T6, T7> union) => OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7>>.TryCreateFrom(value, out union);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create<TValue>(TValue value) => TryCreate(value, out var union) ? union : throw new InvalidCastException("Invalid type for union.");
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Type1Value => _core.GetOrDefault<T1>();
        /// <summary>The union's value as type <typeparamref name="T2"/>.</summary>
        public T2 Type2Value => _core.GetOrDefault<T2>();
        /// <summary>The union's value as type <typeparamref name="T3"/>.</summary>
        public T3 Type3Value => _core.GetOrDefault<T3>();
        /// <summary>The union's value as type <typeparamref name="T4"/>.</summary>
        public T4 Type4Value => _core.GetOrDefault<T4>();
        /// <summary>The union's value as type <typeparamref name="T5"/>.</summary>
        public T5 Type5Value => _core.GetOrDefault<T5>();
        /// <summary>The union's value as type <typeparamref name="T6"/>.</summary>
        public T6 Type6Value => _core.GetOrDefault<T6>();
        /// <summary>The union's value as type <typeparamref name="T7"/>.</summary>
        public T7 Type7Value => _core.GetOrDefault<T7>();
        /// <summary>The type case for the union's value; 1 == T1, 2 == T2, etc.</summary>
        public int Kind => _core.Kind;
        public Type Type => _core.Type;
        public object Value => _core.Value;
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7)];
        static IReadOnlyList<Type> IClosedTypeUnion<OneOf<T1, T2, T3, T4, T5, T6, T7>>.Types => _types;
        public bool TryGet<T>([NotNullWhen(true)] out T value) => _core.TryGet(out value);
        /// <summary>Returns the ToString() result of the value held by the union.</summary>
        public override string ToString() => _core.ToString();
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T1 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T2 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T3 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T4 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T5 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T6 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T7 value) => Create(value);
        public TResult Match<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2, Func<T3, TResult> match3, Func<T4, TResult> match4, Func<T5, TResult> match5, Func<T6, TResult> match6, Func<T7, TResult> match7)
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
        : IOneOf<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>
    {
        private readonly OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>> _core;
        private OneOf(OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>> core) => _core = core;
        static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> IOneOf<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>.Construct(OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>> core) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(core);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T1 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>(1, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T2 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>(2, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T3 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>(3, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T4 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>(4, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T5 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>(5, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T6 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>(6, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T7 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>(7, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create(T8 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>(8, value!));
        public static bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2, T3, T4, T5, T6, T7, T8> union) => OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>.TryCreateFrom(value, out union);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create<TValue>(TValue value) => TryCreate(value, out var union) ? union : throw new InvalidCastException("Invalid type for union.");
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Type1Value => _core.GetOrDefault<T1>();
        /// <summary>The union's value as type <typeparamref name="T2"/>.</summary>
        public T2 Type2Value => _core.GetOrDefault<T2>();
        /// <summary>The union's value as type <typeparamref name="T3"/>.</summary>
        public T3 Type3Value => _core.GetOrDefault<T3>();
        /// <summary>The union's value as type <typeparamref name="T4"/>.</summary>
        public T4 Type4Value => _core.GetOrDefault<T4>();
        /// <summary>The union's value as type <typeparamref name="T5"/>.</summary>
        public T5 Type5Value => _core.GetOrDefault<T5>();
        /// <summary>The union's value as type <typeparamref name="T6"/>.</summary>
        public T6 Type6Value => _core.GetOrDefault<T6>();
        /// <summary>The union's value as type <typeparamref name="T7"/>.</summary>
        public T7 Type7Value => _core.GetOrDefault<T7>();
        /// <summary>The union's value as type <typeparamref name="T8"/>.</summary>
        public T8 Type8Value => _core.GetOrDefault<T8>();
        /// <summary>The type case for the union's value; 1 == T1, 2 == T2, etc.</summary>
        public int Kind => _core.Kind;
        public Type Type => _core.Type;
        public object Value => _core.Value;
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8)];
        static IReadOnlyList<Type> IClosedTypeUnion<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>.Types => _types;
        public bool TryGet<T>([NotNullWhen(true)] out T value) => _core.TryGet(out value);
        /// <summary>Returns the ToString() result of the value held by the union.</summary>
        public override string ToString() => _core.ToString();
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T2 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T3 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T4 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T5 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T6 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T7 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T8 value) => Create(value);
        public TResult Match<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2, Func<T3, TResult> match3, Func<T4, TResult> match4, Func<T5, TResult> match5, Func<T6, TResult> match6, Func<T7, TResult> match7, Func<T8, TResult> match8)
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
        : IOneOf<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>
    {
        private readonly OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>> _core;
        private OneOf(OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>> core) => _core = core;
        static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> IOneOf<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Construct(OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>> core) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(core);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T1 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(1, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T2 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(2, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T3 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(3, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T4 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(4, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T5 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(5, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T6 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(6, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T7 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(7, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T8 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(8, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T9 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(9, value!));
        public static bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> union) => OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.TryCreateFrom(value, out union);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create<TValue>(TValue value) => TryCreate(value, out var union) ? union : throw new InvalidCastException("Invalid type for union.");
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Type1Value => _core.GetOrDefault<T1>();
        /// <summary>The union's value as type <typeparamref name="T2"/>.</summary>
        public T2 Type2Value => _core.GetOrDefault<T2>();
        /// <summary>The union's value as type <typeparamref name="T3"/>.</summary>
        public T3 Type3Value => _core.GetOrDefault<T3>();
        /// <summary>The union's value as type <typeparamref name="T4"/>.</summary>
        public T4 Type4Value => _core.GetOrDefault<T4>();
        /// <summary>The union's value as type <typeparamref name="T5"/>.</summary>
        public T5 Type5Value => _core.GetOrDefault<T5>();
        /// <summary>The union's value as type <typeparamref name="T6"/>.</summary>
        public T6 Type6Value => _core.GetOrDefault<T6>();
        /// <summary>The union's value as type <typeparamref name="T7"/>.</summary>
        public T7 Type7Value => _core.GetOrDefault<T7>();
        /// <summary>The union's value as type <typeparamref name="T8"/>.</summary>
        public T8 Type8Value => _core.GetOrDefault<T8>();
        /// <summary>The union's value as type <typeparamref name="T9"/>.</summary>
        public T9 Type9Value => _core.GetOrDefault<T9>();
        /// <summary>The type case for the union's value; 1 == T1, 2 == T2, etc.</summary>
        public int Kind => _core.Kind;
        public Type Type => _core.Type;
        public object Value => _core.Value;
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9)];
        static IReadOnlyList<Type> IClosedTypeUnion<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Types => _types;
        public bool TryGet<T>([NotNullWhen(true)] out T value) => _core.TryGet(out value);
        /// <summary>Returns the ToString() result of the value held by the union.</summary>
        public override string ToString() => _core.ToString();
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T2 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T3 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T4 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T5 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T6 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T7 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T8 value) => Create(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T9 value) => Create(value);
        public TResult Match<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2, Func<T3, TResult> match3, Func<T4, TResult> match4, Func<T5, TResult> match5, Func<T6, TResult> match6, Func<T7, TResult> match7, Func<T8, TResult> match8, Func<T9, TResult> match9)
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


