using UnionTypes;

namespace UnionTests
{
    [TestClass]
    public class ConvertibleTests
    {
        [TestMethod]
        public void TestVariant_ConvertFrom_Indirect()
        {
            // convert a variant holding one type to a value of a different type
            Variant variant = 3;
            Convertible.TryConvertTo(variant, out long lval);
            Assert.AreEqual(3L, lval);
        }

        [TestMethod]
        public void TestOneOf_ConvertTo_Indirect()
        {
            OneOf<int, double> union = 3;
            Convertible.TryConvertTo(union, out long lval);
            Assert.AreEqual(3L, lval);
        }

        [TestMethod]
        public void TestOneOf_ConvertFrom_Indirect()
        {
            Convertible.TryConvertTo("3", out OneOf<int, double> u);
            Assert.AreEqual(1, u.Kind);
            Assert.AreEqual(3, u.Type1Value);
            Assert.AreEqual(typeof(int), u.Type);
            Assert.AreEqual(3, u.Value);
        }
    }
}