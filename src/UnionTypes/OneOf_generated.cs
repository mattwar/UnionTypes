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
        public static bool TryCreateFrom<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2> union) => OneOfCore<OneOf<T1, T2>>.TryCreateFrom(value, out union);
        public static OneOf<T1, T2> Create1(T1 value) => new OneOf<T1, T2>(new OneOfCore<OneOf<T1, T2>>(1, value!));
        public static OneOf<T1, T2> Create2(T2 value) => new OneOf<T1, T2>(new OneOfCore<OneOf<T1, T2>>(2, value!));
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Value1 => _core.Get<T1>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T2 Value2 => _core.Get<T2>();
        public object BoxedValue => _core.Value;
        public Type Type => _core.GetIndexType();
        public int TypeIndex => _core.GetTypeIndex();
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2)];
        public static IReadOnlyList<Type> Types => _types;
        public bool TryGet<T>([NotNullWhen(true)] out T value) => _core.TryGet(out value);
        public override string ToString() => _core.ToString();
        public static implicit operator OneOf<T1, T2>(T1 value) => Create1(value);
        public static implicit operator OneOf<T1, T2>(T2 value) => Create2(value);
        public TResult Match<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2)
        {
            switch (TypeIndex)
            {
                case 1: return match1(Value1);
                case 2: return match2(Value2);
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
        public void Match<TResult>(Action<T1> match1, Action<T2> match2)
        {
            switch (TypeIndex)
            {
                case 1: match1(Value1); break;
                case 2: match2(Value2); break;
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
        public static bool TryCreateFrom<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2, T3> union) => OneOfCore<OneOf<T1, T2, T3>>.TryCreateFrom(value, out union);
        public static OneOf<T1, T2, T3> Create1(T1 value) => new OneOf<T1, T2, T3>(new OneOfCore<OneOf<T1, T2, T3>>(1, value!));
        public static OneOf<T1, T2, T3> Create2(T2 value) => new OneOf<T1, T2, T3>(new OneOfCore<OneOf<T1, T2, T3>>(2, value!));
        public static OneOf<T1, T2, T3> Create3(T3 value) => new OneOf<T1, T2, T3>(new OneOfCore<OneOf<T1, T2, T3>>(3, value!));
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Value1 => _core.Get<T1>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T2 Value2 => _core.Get<T2>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T3 Value3 => _core.Get<T3>();
        public object BoxedValue => _core.Value;
        public Type Type => _core.GetIndexType();
        public int TypeIndex => _core.GetTypeIndex();
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2), typeof(T3)];
        public static IReadOnlyList<Type> Types => _types;
        public bool TryGet<T>([NotNullWhen(true)] out T value) => _core.TryGet(out value);
        public override string ToString() => _core.ToString();
        public static implicit operator OneOf<T1, T2, T3>(T1 value) => Create1(value);
        public static implicit operator OneOf<T1, T2, T3>(T2 value) => Create2(value);
        public static implicit operator OneOf<T1, T2, T3>(T3 value) => Create3(value);
        public TResult Match<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2, Func<T3, TResult> match3)
        {
            switch (TypeIndex)
            {
                case 1: return match1(Value1);
                case 2: return match2(Value2);
                case 3: return match3(Value3);
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
        public void Match<TResult>(Action<T1> match1, Action<T2> match2, Action<T3> match3)
        {
            switch (TypeIndex)
            {
                case 1: match1(Value1); break;
                case 2: match2(Value2); break;
                case 3: match3(Value3); break;
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
        public static bool TryCreateFrom<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2, T3, T4> union) => OneOfCore<OneOf<T1, T2, T3, T4>>.TryCreateFrom(value, out union);
        public static OneOf<T1, T2, T3, T4> Create1(T1 value) => new OneOf<T1, T2, T3, T4>(new OneOfCore<OneOf<T1, T2, T3, T4>>(1, value!));
        public static OneOf<T1, T2, T3, T4> Create2(T2 value) => new OneOf<T1, T2, T3, T4>(new OneOfCore<OneOf<T1, T2, T3, T4>>(2, value!));
        public static OneOf<T1, T2, T3, T4> Create3(T3 value) => new OneOf<T1, T2, T3, T4>(new OneOfCore<OneOf<T1, T2, T3, T4>>(3, value!));
        public static OneOf<T1, T2, T3, T4> Create4(T4 value) => new OneOf<T1, T2, T3, T4>(new OneOfCore<OneOf<T1, T2, T3, T4>>(4, value!));
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Value1 => _core.Get<T1>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T2 Value2 => _core.Get<T2>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T3 Value3 => _core.Get<T3>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T4 Value4 => _core.Get<T4>();
        public object BoxedValue => _core.Value;
        public Type Type => _core.GetIndexType();
        public int TypeIndex => _core.GetTypeIndex();
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2), typeof(T3), typeof(T4)];
        public static IReadOnlyList<Type> Types => _types;
        public bool TryGet<T>([NotNullWhen(true)] out T value) => _core.TryGet(out value);
        public override string ToString() => _core.ToString();
        public static implicit operator OneOf<T1, T2, T3, T4>(T1 value) => Create1(value);
        public static implicit operator OneOf<T1, T2, T3, T4>(T2 value) => Create2(value);
        public static implicit operator OneOf<T1, T2, T3, T4>(T3 value) => Create3(value);
        public static implicit operator OneOf<T1, T2, T3, T4>(T4 value) => Create4(value);
        public TResult Match<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2, Func<T3, TResult> match3, Func<T4, TResult> match4)
        {
            switch (TypeIndex)
            {
                case 1: return match1(Value1);
                case 2: return match2(Value2);
                case 3: return match3(Value3);
                case 4: return match4(Value4);
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
        public void Match<TResult>(Action<T1> match1, Action<T2> match2, Action<T3> match3, Action<T4> match4)
        {
            switch (TypeIndex)
            {
                case 1: match1(Value1); break;
                case 2: match2(Value2); break;
                case 3: match3(Value3); break;
                case 4: match4(Value4); break;
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
        public static bool TryCreateFrom<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2, T3, T4, T5> union) => OneOfCore<OneOf<T1, T2, T3, T4, T5>>.TryCreateFrom(value, out union);
        public static OneOf<T1, T2, T3, T4, T5> Create1(T1 value) => new OneOf<T1, T2, T3, T4, T5>(new OneOfCore<OneOf<T1, T2, T3, T4, T5>>(1, value!));
        public static OneOf<T1, T2, T3, T4, T5> Create2(T2 value) => new OneOf<T1, T2, T3, T4, T5>(new OneOfCore<OneOf<T1, T2, T3, T4, T5>>(2, value!));
        public static OneOf<T1, T2, T3, T4, T5> Create3(T3 value) => new OneOf<T1, T2, T3, T4, T5>(new OneOfCore<OneOf<T1, T2, T3, T4, T5>>(3, value!));
        public static OneOf<T1, T2, T3, T4, T5> Create4(T4 value) => new OneOf<T1, T2, T3, T4, T5>(new OneOfCore<OneOf<T1, T2, T3, T4, T5>>(4, value!));
        public static OneOf<T1, T2, T3, T4, T5> Create5(T5 value) => new OneOf<T1, T2, T3, T4, T5>(new OneOfCore<OneOf<T1, T2, T3, T4, T5>>(5, value!));
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Value1 => _core.Get<T1>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T2 Value2 => _core.Get<T2>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T3 Value3 => _core.Get<T3>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T4 Value4 => _core.Get<T4>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T5 Value5 => _core.Get<T5>();
        public object BoxedValue => _core.Value;
        public Type Type => _core.GetIndexType();
        public int TypeIndex => _core.GetTypeIndex();
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5)];
        public static IReadOnlyList<Type> Types => _types;
        public bool TryGet<T>([NotNullWhen(true)] out T value) => _core.TryGet(out value);
        public override string ToString() => _core.ToString();
        public static implicit operator OneOf<T1, T2, T3, T4, T5>(T1 value) => Create1(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5>(T2 value) => Create2(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5>(T3 value) => Create3(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5>(T4 value) => Create4(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5>(T5 value) => Create5(value);
        public TResult Match<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2, Func<T3, TResult> match3, Func<T4, TResult> match4, Func<T5, TResult> match5)
        {
            switch (TypeIndex)
            {
                case 1: return match1(Value1);
                case 2: return match2(Value2);
                case 3: return match3(Value3);
                case 4: return match4(Value4);
                case 5: return match5(Value5);
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
        public void Match<TResult>(Action<T1> match1, Action<T2> match2, Action<T3> match3, Action<T4> match4, Action<T5> match5)
        {
            switch (TypeIndex)
            {
                case 1: match1(Value1); break;
                case 2: match2(Value2); break;
                case 3: match3(Value3); break;
                case 4: match4(Value4); break;
                case 5: match5(Value5); break;
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
        public static bool TryCreateFrom<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2, T3, T4, T5, T6> union) => OneOfCore<OneOf<T1, T2, T3, T4, T5, T6>>.TryCreateFrom(value, out union);
        public static OneOf<T1, T2, T3, T4, T5, T6> Create1(T1 value) => new OneOf<T1, T2, T3, T4, T5, T6>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6>>(1, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6> Create2(T2 value) => new OneOf<T1, T2, T3, T4, T5, T6>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6>>(2, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6> Create3(T3 value) => new OneOf<T1, T2, T3, T4, T5, T6>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6>>(3, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6> Create4(T4 value) => new OneOf<T1, T2, T3, T4, T5, T6>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6>>(4, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6> Create5(T5 value) => new OneOf<T1, T2, T3, T4, T5, T6>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6>>(5, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6> Create6(T6 value) => new OneOf<T1, T2, T3, T4, T5, T6>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6>>(6, value!));
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Value1 => _core.Get<T1>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T2 Value2 => _core.Get<T2>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T3 Value3 => _core.Get<T3>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T4 Value4 => _core.Get<T4>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T5 Value5 => _core.Get<T5>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T6 Value6 => _core.Get<T6>();
        public object BoxedValue => _core.Value;
        public Type Type => _core.GetIndexType();
        public int TypeIndex => _core.GetTypeIndex();
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6)];
        public static IReadOnlyList<Type> Types => _types;
        public bool TryGet<T>([NotNullWhen(true)] out T value) => _core.TryGet(out value);
        public override string ToString() => _core.ToString();
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T1 value) => Create1(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T2 value) => Create2(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T3 value) => Create3(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T4 value) => Create4(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T5 value) => Create5(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T6 value) => Create6(value);
        public TResult Match<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2, Func<T3, TResult> match3, Func<T4, TResult> match4, Func<T5, TResult> match5, Func<T6, TResult> match6)
        {
            switch (TypeIndex)
            {
                case 1: return match1(Value1);
                case 2: return match2(Value2);
                case 3: return match3(Value3);
                case 4: return match4(Value4);
                case 5: return match5(Value5);
                case 6: return match6(Value6);
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
        public void Match<TResult>(Action<T1> match1, Action<T2> match2, Action<T3> match3, Action<T4> match4, Action<T5> match5, Action<T6> match6)
        {
            switch (TypeIndex)
            {
                case 1: match1(Value1); break;
                case 2: match2(Value2); break;
                case 3: match3(Value3); break;
                case 4: match4(Value4); break;
                case 5: match5(Value5); break;
                case 6: match6(Value6); break;
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
        public static bool TryCreateFrom<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2, T3, T4, T5, T6, T7> union) => OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7>>.TryCreateFrom(value, out union);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create1(T1 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7>>(1, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create2(T2 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7>>(2, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create3(T3 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7>>(3, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create4(T4 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7>>(4, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create5(T5 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7>>(5, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create6(T6 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7>>(6, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7> Create7(T7 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7>>(7, value!));
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Value1 => _core.Get<T1>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T2 Value2 => _core.Get<T2>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T3 Value3 => _core.Get<T3>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T4 Value4 => _core.Get<T4>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T5 Value5 => _core.Get<T5>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T6 Value6 => _core.Get<T6>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T7 Value7 => _core.Get<T7>();
        public object BoxedValue => _core.Value;
        public Type Type => _core.GetIndexType();
        public int TypeIndex => _core.GetTypeIndex();
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7)];
        public static IReadOnlyList<Type> Types => _types;
        public bool TryGet<T>([NotNullWhen(true)] out T value) => _core.TryGet(out value);
        public override string ToString() => _core.ToString();
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T1 value) => Create1(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T2 value) => Create2(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T3 value) => Create3(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T4 value) => Create4(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T5 value) => Create5(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T6 value) => Create6(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T7 value) => Create7(value);
        public TResult Match<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2, Func<T3, TResult> match3, Func<T4, TResult> match4, Func<T5, TResult> match5, Func<T6, TResult> match6, Func<T7, TResult> match7)
        {
            switch (TypeIndex)
            {
                case 1: return match1(Value1);
                case 2: return match2(Value2);
                case 3: return match3(Value3);
                case 4: return match4(Value4);
                case 5: return match5(Value5);
                case 6: return match6(Value6);
                case 7: return match7(Value7);
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
        public void Match<TResult>(Action<T1> match1, Action<T2> match2, Action<T3> match3, Action<T4> match4, Action<T5> match5, Action<T6> match6, Action<T7> match7)
        {
            switch (TypeIndex)
            {
                case 1: match1(Value1); break;
                case 2: match2(Value2); break;
                case 3: match3(Value3); break;
                case 4: match4(Value4); break;
                case 5: match5(Value5); break;
                case 6: match6(Value6); break;
                case 7: match7(Value7); break;
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
        public static bool TryCreateFrom<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2, T3, T4, T5, T6, T7, T8> union) => OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>.TryCreateFrom(value, out union);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create1(T1 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>(1, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create2(T2 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>(2, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create3(T3 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>(3, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create4(T4 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>(4, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create5(T5 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>(5, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create6(T6 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>(6, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create7(T7 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>(7, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Create8(T8 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8>>(8, value!));
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Value1 => _core.Get<T1>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T2 Value2 => _core.Get<T2>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T3 Value3 => _core.Get<T3>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T4 Value4 => _core.Get<T4>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T5 Value5 => _core.Get<T5>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T6 Value6 => _core.Get<T6>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T7 Value7 => _core.Get<T7>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T8 Value8 => _core.Get<T8>();
        public object BoxedValue => _core.Value;
        public Type Type => _core.GetIndexType();
        public int TypeIndex => _core.GetTypeIndex();
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8)];
        public static IReadOnlyList<Type> Types => _types;
        public bool TryGet<T>([NotNullWhen(true)] out T value) => _core.TryGet(out value);
        public override string ToString() => _core.ToString();
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value) => Create1(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T2 value) => Create2(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T3 value) => Create3(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T4 value) => Create4(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T5 value) => Create5(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T6 value) => Create6(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T7 value) => Create7(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T8 value) => Create8(value);
        public TResult Match<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2, Func<T3, TResult> match3, Func<T4, TResult> match4, Func<T5, TResult> match5, Func<T6, TResult> match6, Func<T7, TResult> match7, Func<T8, TResult> match8)
        {
            switch (TypeIndex)
            {
                case 1: return match1(Value1);
                case 2: return match2(Value2);
                case 3: return match3(Value3);
                case 4: return match4(Value4);
                case 5: return match5(Value5);
                case 6: return match6(Value6);
                case 7: return match7(Value7);
                case 8: return match8(Value8);
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
        public void Match<TResult>(Action<T1> match1, Action<T2> match2, Action<T3> match3, Action<T4> match4, Action<T5> match5, Action<T6> match6, Action<T7> match7, Action<T8> match8)
        {
            switch (TypeIndex)
            {
                case 1: match1(Value1); break;
                case 2: match2(Value2); break;
                case 3: match3(Value3); break;
                case 4: match4(Value4); break;
                case 5: match5(Value5); break;
                case 6: match6(Value6); break;
                case 7: match7(Value7); break;
                case 8: match8(Value8); break;
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
        public static bool TryCreateFrom<TValue>(TValue value, [NotNullWhen(true)] out OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> union) => OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.TryCreateFrom(value, out union);
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create1(T1 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(1, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create2(T2 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(2, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create3(T3 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(3, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create4(T4 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(4, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create5(T5 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(5, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create6(T6 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(6, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create7(T7 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(7, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create8(T8 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(8, value!));
        public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create9(T9 value) => new OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(new OneOfCore<OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(9, value!));
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T1 Value1 => _core.Get<T1>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T2 Value2 => _core.Get<T2>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T3 Value3 => _core.Get<T3>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T4 Value4 => _core.Get<T4>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T5 Value5 => _core.Get<T5>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T6 Value6 => _core.Get<T6>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T7 Value7 => _core.Get<T7>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T8 Value8 => _core.Get<T8>();
        /// <summary>The union's value as type <typeparamref name="T1"/>.</summary>
        public T9 Value9 => _core.Get<T9>();
        public object BoxedValue => _core.Value;
        public Type Type => _core.GetIndexType();
        public int TypeIndex => _core.GetTypeIndex();
        private static IReadOnlyList<Type> _types = [typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9)];
        public static IReadOnlyList<Type> Types => _types;
        public bool TryGet<T>([NotNullWhen(true)] out T value) => _core.TryGet(out value);
        public override string ToString() => _core.ToString();
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 value) => Create1(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T2 value) => Create2(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T3 value) => Create3(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T4 value) => Create4(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T5 value) => Create5(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T6 value) => Create6(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T7 value) => Create7(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T8 value) => Create8(value);
        public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T9 value) => Create9(value);
        public TResult Match<TResult>(Func<T1, TResult> match1, Func<T2, TResult> match2, Func<T3, TResult> match3, Func<T4, TResult> match4, Func<T5, TResult> match5, Func<T6, TResult> match6, Func<T7, TResult> match7, Func<T8, TResult> match8, Func<T9, TResult> match9)
        {
            switch (TypeIndex)
            {
                case 1: return match1(Value1);
                case 2: return match2(Value2);
                case 3: return match3(Value3);
                case 4: return match4(Value4);
                case 5: return match5(Value5);
                case 6: return match6(Value6);
                case 7: return match7(Value7);
                case 8: return match8(Value8);
                case 9: return match9(Value9);
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
        public void Match<TResult>(Action<T1> match1, Action<T2> match2, Action<T3> match3, Action<T4> match4, Action<T5> match5, Action<T6> match6, Action<T7> match7, Action<T8> match8, Action<T9> match9)
        {
            switch (TypeIndex)
            {
                case 1: match1(Value1); break;
                case 2: match2(Value2); break;
                case 3: match3(Value3); break;
                case 4: match4(Value4); break;
                case 5: match5(Value5); break;
                case 6: match6(Value6); break;
                case 7: match7(Value7); break;
                case 8: match8(Value8); break;
                case 9: match9(Value9); break;
                default: throw new InvalidOperationException("Invalid union state.");
            }
        }
    }
}


