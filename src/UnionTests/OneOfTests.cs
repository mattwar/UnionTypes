using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using UnionTypes;

namespace UnionTests
{
    [TestClass]
    public class OneOfTests
    {
        [TestMethod]
        public void TestOneOf2()
        {
            OneOfTest<OneOf<int, string>>.Test(1, 1, 0);
            OneOfTest<OneOf<int, string>>.Test("one", "one", 1);
        }

        [TestMethod]
        public void TestOneOf3()
        {
            OneOfTest<OneOf<int, string, double>>.Test(1, 1, 0);
            OneOfTest<OneOf<int, string, double>>.Test("one", "one", 1);
            OneOfTest<OneOf<int, string, double>>.Test(1.0, 1.0, 2);
        }

        private class OneOfTest<TOneOf>
            where TOneOf : IOneOf, IClosedTypeUnion<TOneOf>
        {
            public static OneOfTest<TOneOf> Instance { get; } = new();

            public static void Test<TExpected>(TOneOf oneOf, TExpected expectedValue, int expectedIndex)
            {
                Assert.AreEqual(oneOf.BoxedValue, expectedValue);
                Assert.AreEqual(expectedIndex, oneOf.TypeIndex);
                Assert.AreSame(oneOf.Type, typeof(TExpected));
                Assert.IsTrue(oneOf.TryGet<TExpected>(out var actualValue));
                Assert.AreEqual(expectedValue, actualValue);
            }
        }
    }
}