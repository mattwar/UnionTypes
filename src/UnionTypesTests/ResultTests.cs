using System.Collections.Immutable;
using UnionTypes.Toolkit;

namespace UnionTests
{
    [TestClass]
    public class ResultTests
    {
        [TestMethod]
        public void Test_AssignableFromValue()
        {
            Result<int, Exception> result = 10;
            Assert.AreEqual(Result<int, Exception>.Case.Success, result.Kind);
            Assert.AreEqual(10, result.Value);
        }

        [TestMethod]
        public void Test_AssignableFromError()
        {
            var whoops = new Exception("Whoops");
            Result<int, Exception> result = whoops;
            Assert.AreEqual(Result<int, Exception>.Case.Failure, result.Kind);
            Assert.AreSame(whoops, result.Error);
        }

        [TestMethod]
        public void Test_DefaultIsInvalid()
        {
            Result<int, Exception> result = default;
            Assert.IsTrue(result.IsInvalid);
        }

        [TestMethod]
        public void Test_TryGet_Value()
        {
            Result<int, Exception> result = 10;
            Assert.AreEqual(Result<int, Exception>.Case.Success, result.Kind);
            Assert.IsTrue(result.TryGet(out int value));
            Assert.AreEqual(10, value);
        }

        [TestMethod]
        public void Test_TryGet_Error()
        {
            var whoops = new Exception("Whoops");
            Result<int, Exception> result = whoops;
            Assert.AreEqual(Result<int, Exception>.Case.Failure, result.Kind);
            Assert.IsTrue(result.TryGet(out Exception error));
            Assert.AreEqual(whoops, error);
        }

        [TestMethod]
        public void Test_Ambiguous_Value()
        {
            Result<string, string> result = Result<string, string>.Success("Success");
            Assert.AreEqual(Result<string, string>.Case.Success, result.Kind);
            Assert.AreEqual("Success", result.Value);
        }

        [TestMethod]
        public void Test_Ambiguous_Error()
        {
            Result<string, string> result = Result<string, string>.Failure("Whoops");
            Assert.AreEqual(Result<string, string>.Case.Failure, result.Kind);
            Assert.AreEqual("Whoops", result.Error);
        }

        [TestMethod]
        public void Test_Select_Value()
        {
            Result<int, Exception> result = 10;
            Assert.IsTrue(result.Select(value => true, error => false));
        }

        [TestMethod]
        public void Test_Select_Error()
        {
            Result<int, Exception> result = new Exception("Whoops");
            Assert.IsTrue(result.Select(value => false, error => true));
        }

        [TestMethod]
        public void Test_Select_Invalid()
        {
            Result<int, Exception> result = default;
            Assert.IsTrue(result.Select(value => false, error => false, () => true));
        }

        [TestMethod]
        public void Test_Match_Value()
        {
            Result<int, Exception> result = 10;
            result.Match(value => { Assert.AreEqual(10, value); }, error => { Assert.Fail("Not value"); }, () => { Assert.Fail("Not value"); });
        }

        [TestMethod]
        public void Test_Match_Error()
        {
            var whoops = new Exception("Whoops");
            Result<int, Exception> result = whoops;
            result.Match(value => { Assert.Fail("Not error"); }, error => { Assert.AreEqual(whoops, error); }, () => { Assert.Fail("Not error"); });
        }

        [TestMethod]
        public void Test_Match_Invalid()
        {
            Result<int, Exception> result = default;
            result.Match(value => { Assert.Fail("Not invalid"); }, error => { Assert.Fail("Not invalid"); }, () => { });
        }

        [TestMethod]
        public void Test_Map_ValueToValue()
        {
            Result<int, Exception> result = 10;
            var mapped = result.Map(value => value * 2);
            Assert.AreEqual(Result<int, Exception>.Case.Success, mapped.Kind);
            Assert.AreEqual(20, mapped.Value);
        }

        [TestMethod]
        public void Test_Map_Error()
        {
            var whoops = new Exception("Whoops");
            Result<int, Exception> result = whoops;
            var mapped = result.Map(value => value * 2);
            Assert.AreEqual(Result<int, Exception>.Case.Failure, mapped.Kind);
            Assert.AreEqual(whoops, mapped.Error);
        }

        [TestMethod]
        public void Test_Map_ValueToError()
        {
            var whoops = new Exception("Whoops");
            Result<int, Exception> result = 10;
            var mapped = result.Map(value => Result<int, Exception>.Failure(whoops));
            Assert.AreEqual(Result<int, Exception>.Case.Failure, mapped.Kind);
            Assert.AreEqual(whoops, mapped.Error);
        }
    }
}