using System.Collections.Immutable;
using UnionTypes.Toolkit;

namespace UnionTests
{
    [TestClass]
    public class OneOfTests
    {
        [TestMethod]
        public void TestOneOf2()
        {
            TestOneOf2<int, string>(1, 1, 1, typeof(int));
            TestOneOf2<int, string>("one", "one", 2, typeof(string));
            TestOneOf2<int, string>(default(OneOf<int, string>), null, 0, typeof(object));
        }

        [TestMethod]
        public void TestOneOf3()
        {
            TestOneOf3<int, string, double>(1, 1, 1, typeof(int));
            TestOneOf3<int, string, double>("one", "one", 2, typeof(string));
            TestOneOf3<int, string, double>(3.0, 3.0, 3, typeof(double));
        }

        [TestMethod]
        public void TestEquality()
        {
            OneOf<int, string> value = 1;
            if (value == 2 || 1 == value)
            {
            }
        }

        private void TestOneOf2<T1, T2>(
            OneOf<T1, T2> oneOf, object? expectedValue, int expectedKind, Type expectedType)
        {
            Assert.AreEqual(oneOf.Value, expectedValue);
            Assert.AreEqual(expectedKind, oneOf.Kind);
            Assert.AreSame(oneOf.Type, expectedType);
            TestOneOf(oneOf, expectedValue, expectedType);
        }

        private void TestOneOf3<T1, T2, T3>(
            OneOf<T1, T2, T3> oneOf, object? expectedValue, int expectedKind, Type expectedType)
        {
            Assert.AreEqual(oneOf.Value, expectedValue);
            Assert.AreEqual(expectedKind, oneOf.Kind);
            Assert.AreSame(oneOf.Type, expectedType);
            TestOneOf(oneOf, expectedValue, expectedType);
        }

        private void TestOneOf<TOneOf>(TOneOf oneOf, object? expectedValue, Type expectedType)
            where TOneOf : IOneOf, IClosedTypeUnion<TOneOf>
        {
            Assert.AreEqual(oneOf.Value, expectedValue);
            Assert.AreEqual(oneOf.Type, expectedType);
        }

        private class OneOfTest<TOneOf>
            where TOneOf : IOneOf, IClosedTypeUnion<TOneOf>
        {
            public static OneOfTest<TOneOf> Instance { get; } = new();

            public static void Test<TExpected>(TOneOf oneOf, TExpected expectedValue, int expectedKind)
            {
            }
        }
    }
}