using System.Collections.Immutable;
using UnionTypes;

namespace UnionTests
{
    [TestClass]
    public class OptionTests
    {
        [TestMethod]
        public void Test_DefaultIsNone()
        {
            Option<int> zed = default;
            Assert.IsTrue(zed.IsNone);
        }

        [TestMethod]
        public void Test_AssignableFromNone()
        {
            Option<int> zed = None.Singleton;
            Assert.AreEqual(Option<int>.Case.None, zed.Kind);
            Assert.IsTrue(zed.IsNone);
        }

        [TestMethod]
        public void Test_AssignableFromValue()
        {
            Option<int> zed = 10;
            Assert.AreEqual(Option<int>.Case.Some, zed.Kind);
            Assert.AreEqual(10, zed.Value);
        }

        [TestMethod]
        public void Test_ValueAccessFromGet()
        {
            Option<int> zed = 10;
            Assert.AreEqual(Option<int>.Case.Some, zed.Kind);
            Assert.IsTrue(zed.TryGet(out int value));
            Assert.AreEqual(10, value);
        }

        [TestMethod]
        public void Test_NoneAccessFromGet()
        {
            Option<int> zed = None.Singleton;
            Assert.AreEqual(Option<int>.Case.None, zed.Kind);
            Assert.IsTrue(zed.TryGet(out None value));
            Assert.AreSame(None.Singleton, value);
        }

        [TestMethod]
        public void Test_Select_Value()
        {
            Option<int> zed = 10;
            Assert.IsTrue(zed.Select(value => true, () => false));
        }

        [TestMethod]
        public void Test_Match_Value()
        {
            Option<int> zed = 10;
            zed.Match(value => { }, () => { Assert.Fail("Expected a value"); });
        }

        [TestMethod]
        public void Test_Select_None()
        {
            Option<int> zed = default;
            Assert.IsFalse(zed.Select(value => true, () => false));
        }

        [TestMethod]
        public void Test_Match_None()
        {
            Option<int> zed = default;
            zed.Match(value => { Assert.Fail("Expected None"); }, () => { });
        }
    }
}