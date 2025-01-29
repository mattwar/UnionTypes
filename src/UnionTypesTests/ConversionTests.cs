using UnionTypes.Toolkit;

namespace UnionTests
{
    [TestClass]
    public class ConversionTests
    {
        [TestMethod]
        public void TestTryConvert_Variant_To_Value()
        {
            // convert a variant holding one type to a value of a different type
            Variant variant = 3;
            TypeUnion.TryConvert(variant, out long lval);
            Assert.AreEqual(3L, lval);
        }

        [TestMethod]
        public void TestTryConvert_OneOf_To_Value()
        {
            OneOf<int, double> union = 3;
            TypeUnion.TryConvert(union, out long lval);
            Assert.AreEqual(3L, lval);
        }

        [TestMethod]
        public void TestTryConvert_Value_To_OneOf()
        {
            TypeUnion.TryConvert("3", out OneOf<int, double> u);
            Assert.AreEqual(1, u.Kind);
            Assert.AreEqual(3, u.Type1Value);
            Assert.AreEqual(typeof(int), u.Type);
            Assert.AreEqual(3, u.Value);
        }

        [TestMethod]
        public void TestTryConvert_OneOf_To_OneOf()
        {
            OneOf<int, double> union = 3;
            TypeUnion.TryConvert(union, out OneOf<long, double> u);
            Assert.AreEqual(1, u.Kind);
            Assert.AreEqual(3L, u.Type1Value);
            Assert.AreEqual(typeof(long), u.Type);
            Assert.AreEqual(3L, u.Value);
        }

        [TestMethod]
        public void TestTryConvert_Value_To_Value()
        {
            TypeUnion.TryConvert("3", out double v);
            Assert.AreEqual(3.0, v);
        }
    }
}