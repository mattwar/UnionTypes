using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using UnionTypes;

namespace UnionTests
{
    [TestClass]
    public class UnionSourceGeneratorTests
    {
        [TestMethod]
        public void TestTypeUnion_NestedRecords()
        {
            TestUnion(
                """
                using UnionTypes;

                [TypeUnion]
                public partial struct MyUnion
                {
                    [UnionCase]
                    public record struct A(int x);

                    [UnionCase]
                    public record struct B(string y, float z);
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.Create(new MyUnion.A(10));
                        MyUnion unionB = MyUnion.Create(new MyUnion.B("x", 5.0f));
                        MyUnion.A a = unionA.AValue;
                        MyUnion.B b = unionB.BValue;
                        var correctTagA = unionA.Kind == MyUnion.Case.A;
                        var correctTagB = unionB.Kind == MyUnion.Case.B;
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("A = 1")
                        && newText.Contains("B = 2");
                });
        }

        [TestMethod]
        public void TestTypeUnion_NestedRecords_CaseNames()
        {
            TestUnion(
                """
                using UnionTypes;

                [TypeUnion]
                public partial struct MyUnion
                {
                    [UnionCase(Name="Aa")]
                    public record struct A(int x);

                    [UnionCase(Name="Bb")]
                    public record struct B(string y, float z);
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.Create(new MyUnion.A(10));
                        MyUnion unionB = MyUnion.Create(new MyUnion.B("x", 5.0f));
                        MyUnion.A a = unionA.AaValue;
                        MyUnion.B b = unionB.BbValue;
                        var correctTagA = unionA.Kind == MyUnion.Case.Aa;
                        var correctTagB = unionB.Kind == MyUnion.Case.Bb; 
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("Aa = 1")
                        && newText.Contains("Bb = 2");
                });
        }

        [TestMethod]
        public void TestTypeUnion_NestedRecords_FactoryNames()
        {
            TestUnion(
                """
                using UnionTypes;

                [TypeUnion]
                public partial struct MyUnion
                {
                    [UnionCase(FactoryName="MakeA")]
                    public record struct A(int x);

                    [UnionCase(FactoryName="MakeB")]
                    public record struct B(string y, float z);
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.MakeA(new MyUnion.A(10));
                        MyUnion unionB = MyUnion.MakeB(new MyUnion.B("x", 5.0f));
                        MyUnion.A a = unionA.AValue;
                        MyUnion.B b = unionB.BValue;
                        var correctTagA = unionA.Kind == MyUnion.Case.A;
                        var correctTagB = unionB.Kind == MyUnion.Case.B; 
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("A = 1")
                        && newText.Contains("B = 2");
                });
        }

        [TestMethod]
        public void TestTypeUnion_NestedRecords_TagValues()
        {
            TestUnion(
                """
                using UnionTypes;

                [TypeUnion]
                public partial struct MyUnion
                {
                    [UnionCase(Value=4)]
                    public record struct A(int x);

                    [UnionCase(Value=3)]
                    public record struct B(string y, float z);
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.Create(new MyUnion.A(10));
                        MyUnion unionB = MyUnion.Create(new MyUnion.B("x", 5.0f));
                        MyUnion.A a = unionA.AValue;
                        MyUnion.B b = unionB.BValue;
                        var correctTagA = unionA.Kind == MyUnion.Case.A;
                        var correctTagB = unionB.Kind == MyUnion.Case.B;
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("A = 4")
                        && newText.Contains("B = 3");
                });
        }

        [TestMethod]
        public void TestTypeUnion_FactoryMethods()
        {
            TestUnion(
                """
                using UnionTypes;

                public record struct A(int x);
                public record struct B(string y, float z);

                [TypeUnion]
                public partial struct MyUnion
                {
                    [UnionCase]
                    public static partial MyUnion Create(A a);

                    [UnionCase]
                    public static partial MyUnion Create(B b);
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.Create(new A(10));
                        MyUnion unionB = MyUnion.Create(new B("x", 5.0f));
                        A a = unionA.AValue;
                        B b = unionB.BValue;
                        var correctTagA = unionA.Kind == MyUnion.Case.A;
                        var correctTagB = unionB.Kind == MyUnion.Case.B;
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("A = 1")
                        && newText.Contains("B = 2");
                });
        }

        [TestMethod]
        public void TestTypeUnion_FactoryMethods_CaseNames()
        {
            TestUnion(
                """
                using UnionTypes;

                public record struct A(int x);
                public record struct B(string y, float z);

                [TypeUnion]
                public partial struct MyUnion
                {
                    [UnionCase(Name="Aa")]
                    public static partial MyUnion Create(A a);

                    [UnionCase(Name="Bb")]
                    public static partial MyUnion Create(B b);
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.Create(new A(10));
                        MyUnion unionB = MyUnion.Create(new B("x", 5.0f));
                        A a = unionA.AaValue;
                        B b = unionB.BbValue;
                        var correctTagA = unionA.Kind == MyUnion.Case.Aa;
                        var correctTagB = unionB.Kind == MyUnion.Case.Bb;
                    }
                }
                """,
                newText =>
                {
                    // tag values
                    return newText.Contains("Aa = 1")
                        && newText.Contains("Bb = 2");
                });
        }

        [TestMethod]
        public void TestTypeUnion_FactoryMethods_TagValues()
        {
            TestUnion(
                """
                using UnionTypes;

                public record struct A(int x);
                public record struct B(string y, float z);

                [TypeUnion]
                public partial struct MyUnion
                {
                    [UnionCase(Value=4)]
                    public static partial MyUnion Create(A a);

                    [UnionCase(Value=3)]
                    public static partial MyUnion Create(B b);
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.Create(new A(10));
                        MyUnion unionB = MyUnion.Create(new B("x", 5.0f));
                        A a = unionA.AValue;
                        B b = unionB.BValue;
                        var correctTagA = unionA.Kind == MyUnion.Case.A;
                        var correctTagB = unionB.Kind == MyUnion.Case.B;
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("A = 4")
                        && newText.Contains("B = 3");
                });
        }

        [TestMethod]
        public void TestTypeUnion_CasesOnType_Types()
        {
            TestUnion(
                """
                using UnionTypes;

                public record struct A(int x);
                public record struct B(string y, float z);

                [TypeUnion]
                [UnionCase(Type=typeof(A))]
                [UnionCase(Type=typeof(B))]
                public partial struct MyUnion
                {
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.Create(new A(10));
                        MyUnion unionB = MyUnion.Create(new B("x", 5.0f));
                        A a = unionA.AValue;
                        B b = unionB.BValue;
                        var correctTagA = unionA.Kind == MyUnion.Case.A;
                        var correctTagB = unionB.Kind == MyUnion.Case.B;                
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("A = 1")
                        && newText.Contains("B = 2");
                });
        }

        [TestMethod]
        public void TestTypeUnion_CasesOnType_CaseNames()
        {
            TestUnion(
                """
                using UnionTypes;

                public record struct A(int x);
                public record struct B(string y, float z);

                [TypeUnion]
                [UnionCase(Name="Aa", Type=typeof(A))]
                [UnionCase(Name="Bb", Type=typeof(B))]
                public partial struct MyUnion
                {
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.Create(new A(10));
                        MyUnion unionB = MyUnion.Create(new B("x", 5.0f));
                        A a = unionA.AaValue;
                        B b = unionB.BbValue;
                        var correctTagA = unionA.Kind == MyUnion.Case.Aa;
                        var correctTagB = unionB.Kind == MyUnion.Case.Bb;                
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("Aa = 1")
                        && newText.Contains("Bb = 2");
                });
        }

        [TestMethod]
        public void TestTypeUnion_CasesOnType_FactoryNames()
        {
            TestUnion(
                """
                using UnionTypes;

                public record struct A(int x);
                public record struct B(string y, float z);

                [TypeUnion]
                [UnionCase(Type=typeof(A), FactoryName="MakeA")]
                [UnionCase(Type=typeof(B), FactoryName="MakeB")]
                public partial struct MyUnion
                {
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.MakeA(new A(10));
                        MyUnion unionB = MyUnion.MakeB(new B("x", 5.0f));
                        A a = unionA.AValue;
                        B b = unionB.BValue;
                        var correctTagA = unionA.Kind == MyUnion.Case.A;
                        var correctTagB = unionB.Kind == MyUnion.Case.B;                
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("A = 1")
                        && newText.Contains("B = 2");
                });
        }

        [TestMethod]
        public void TestTypeUnion_CasesOnType_TagValues()
        {
            TestUnion(
                """
                using UnionTypes;

                public record struct A(int x);
                public record struct B(string y, float z);

                [TypeUnion]
                [UnionCase(Type=typeof(A), Value=4)]
                [UnionCase(Type=typeof(B), Value=3)]
                public partial struct MyUnion
                {
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.Create(new A(10));
                        MyUnion unionB = MyUnion.Create(new B("x", 5.0f));
                        A a = unionA.AValue;
                        B b = unionB.BValue;
                        var correctTagA = unionA.Kind == MyUnion.Case.A;
                        var correctTagB = unionB.Kind == MyUnion.Case.B;                
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("A = 4")
                        && newText.Contains("B = 3");
                });
        }

        [TestMethod]
        public void TestTagUnion()
        {
            TestUnion(
                """
                using UnionTypes;

                [TagUnion]
                public partial struct MyUnion
                {
                    [UnionCase]
                    public static partial MyUnion A(int x);

                    [UnionCase]
                    public static partial MyUnion B(string y, float z);
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.A(10);
                        MyUnion unionB = MyUnion.B("x", 5.0f);
                        var a = unionA.AValue;
                        var b = unionB.BValues;
                        var correctTagA = unionA.Kind == MyUnion.Case.A;
                        var correctTagB = unionB.Kind == MyUnion.Case.B;
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("A = 1")
                        && newText.Contains("B = 2");
                });
        }

        [TestMethod]
        public void TestTagUnion_CasesOnType()
        {
            TestUnion(
                """
                using UnionTypes;

                [TagUnion]
                [UnionCase(Name="A")]
                [UnionCase(Name="B")]
                public partial struct MyUnion
                {
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.A;
                        MyUnion unionB = MyUnion.B;
                        var a = unionA.IsA;
                        var b = unionB.IsB;
                        var correctTagA = unionA.Kind == MyUnion.Case.A;
                        var correctTagB = unionB.Kind == MyUnion.Case.B;
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("A = 1")
                        && newText.Contains("B = 2");
                });
        }

        [TestMethod]
        public void TestTagUnion_CasesOnType_FactoryName()
        {
            TestUnion(
                """
                using UnionTypes;

                [TagUnion]
                [UnionCase(Name="A", FactoryName="MakeA")]
                [UnionCase(Name="B", FactoryName="MakeB")]
                public partial struct MyUnion
                {
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.MakeA;
                        MyUnion unionB = MyUnion.MakeB;
                        var a = unionA.IsA;
                        var b = unionB.IsB;
                        var correctTagA = unionA.Kind == MyUnion.Case.A;
                        var correctTagB = unionB.Kind == MyUnion.Case.B;
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("A = 1")
                        && newText.Contains("B = 2");
                });
        }

        [TestMethod]
        public void TestTagUnion_CasesOnType_FactoryIsProperty()
        {
            TestUnion(
                """
                using UnionTypes;

                [TagUnion]
                [UnionCase(Name="A", FactoryName="A", FactoryIsProperty=false)]
                [UnionCase(Name="B", FactoryName="B", FactoryIsProperty=false)]
                public partial struct MyUnion
                {
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.A();
                        MyUnion unionB = MyUnion.B();
                        var a = unionA.IsA;
                        var b = unionB.IsB;
                        var correctTagA = unionA.Kind == MyUnion.Case.A;
                        var correctTagB = unionB.Kind == MyUnion.Case.B;
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("A = 1")
                        && newText.Contains("B = 2");
                });
        }

        [TestMethod]
        public void TestTagUnion_OverrideCaseNames()
        {
            TestUnion(
                """
                using UnionTypes;

                [TagUnion]
                public partial struct MyUnion
                {
                    [UnionCase(Name="A")]
                    public static partial MyUnion MakeA(int x);

                    [UnionCase(Name="B")]
                    public static partial MyUnion MakeB(string y, float z);
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.MakeA(10);
                        MyUnion unionB = MyUnion.MakeB("x", 5.0f);
                        var a = unionA.AValue;
                        var b = unionB.BValues;
                        var correctTagA = unionA.Kind == MyUnion.Case.A;
                        var correctTagB = unionB.Kind == MyUnion.Case.B;
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("A = 1")
                        && newText.Contains("B = 2");
                });
        }

        [TestMethod]
        public void TestTagUnion_OverrideAccessorNames()
        {
            TestUnion(
                """
                using UnionTypes;

                [TagUnion]
                public partial struct MyUnion
                {
                    [UnionCase(AccessorName="StuffForA")]
                    public static partial MyUnion A(int x);

                    [UnionCase(AccessorName="StuffForB")]
                    public static partial MyUnion B(string y, float z);
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.A(10);
                        MyUnion unionB = MyUnion.B("x", 5.0f);
                        var a = unionA.StuffForA;
                        var b = unionB.StuffForB;
                        var correctTagA = unionA.Kind == MyUnion.Case.A;
                        var correctTagB = unionB.Kind == MyUnion.Case.B;
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("A = 1")
                        && newText.Contains("B = 2");
                });
        }

        [TestMethod]
        public void TestTagUnion_OverTagValues()
        {
            TestUnion(
                """
                using UnionTypes;

                [TagUnion]
                public partial struct MyUnion
                {
                    [UnionCase(Value=4)]
                    public static partial MyUnion A(int x);

                    [UnionCase(Value=3)]
                    public static partial MyUnion B(string y, float z);
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.A(10);
                        MyUnion unionB = MyUnion.B("x", 5.0f);
                        var a = unionA.AValue;
                        var b = unionB.BValues;
                        var correctTagA = unionA.Kind == MyUnion.Case.A;
                        var correctTagB = unionB.Kind == MyUnion.Case.B;
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("A = 4")
                        && newText.Contains("B = 3");
                });
        }

        [TestMethod]
        public void TestTagUnion_DefaultState()
        {
            TestUnion(
                """
                using UnionTypes;

                [TagUnion]
                public partial struct MyUnion
                {
                    [UnionCase(Value=0)]
                    public static partial MyUnion Nobody();

                    [UnionCase]
                    public static partial MyUnion Student(string name, decimal grade);

                    [UnionCase]
                    public static partial MyUnion Teacher(string name);
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionN = MyUnion.Nobody();
                        MyUnion unionT = MyUnion.Teacher("Mr. Bob");
                        MyUnion unionS = MyUnion.Student("Alice", 4.0m);
                        var n = unionN.IsNobody;
                        var t = unionT.TeacherValue;
                        var s = unionS.StudentValues;
                        var correctTagN = unionN.Kind == MyUnion.Case.Nobody;
                        var correctTagT = unionT.Kind == MyUnion.Case.Teacher;
                        var correctTagS = unionS.Kind == MyUnion.Case.Student;
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("Nobody = 0")
                        && newText.Contains("Student = 1")
                        && newText.Contains("Teacher = 2");
                });
        }

        [TestMethod]
        public void TestTagUnion_Option()
        {
            TestUnion(
                """
                using UnionTypes;

                [TagUnion]
                [UnionCase(Name="None", Value=0)]
                public partial struct Option<T>
                {
                    [UnionCase]
                    public static partial Option<T> Some(T value);
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        Option<string> optS = Option<string>.Some("Hello");
                        Option<string> optN = Option<string>.None;
                        var valS = optS.SomeValue;
                        var valN = optN.IsNone;
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("None = 0")
                        && newText.Contains("Some = 1");
                });
        }

        [TestMethod]
        public void TestTagUnion_Result()
        {
            TestUnion(
                """
                using UnionTypes;

                [TagUnion]
                public partial struct Result<T>
                {
                    [UnionCase]
                    public static partial Result<T> Success(T value);

                    [UnionCase(AccessorName="FailureMessage")]
                    public static partial Result<T> Failure(string message);
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        Result<int> success = Result<int>.Success(10);
                        Result<int> failure = Result<int>.Failure("Oops");
                        var value = success.SuccessValue;
                        var message = failure.FailureMessage;
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("Success = 1")
                        && newText.Contains("Failure = 2");
                });
        }

        [TestMethod]
        public void TestTypeUnion_Option()
        {
            TestUnion(
                """
                using UnionTypes;

                public class None : ISingleton<None> { public static None Singleton { get; } = new None(); }

                [TypeUnion]
                [UnionCase(Type=typeof(None), IsSingleton=true, Value=0)]
                public partial struct Option<T>
                {
                    [UnionCase]
                    public static partial Option<T> Some(T value);
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        Option<string> optS = Option<string>.Some("Hello");
                        Option<string> optN = None.Singleton;
                        var valS = optS.SomeValue;
                        var valN = optN.NoneValue;
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("None = 0")
                        && newText.Contains("Some = 1");
                });
        }

        [TestMethod]
        public void TestTypeUnion_Result()
        {
            TestUnion(
                """
                using UnionTypes;

                [TypeUnion]
                public partial struct Result<T>
                {
                    [UnionCase]
                    public static partial Result<T> Success(T value);

                    [UnionCase(Name="Failure", AccessorName="FailureMessage")]
                    public static partial Result<T> Failure(string message);
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        Result<int> success = Result<int>.Success(10);
                        Result<int> failure = Result<int>.Failure("Oops");
                        var valS = success.SuccessValue;
                        var valF = failure.FailureMessage;
                    }
                }
                """,
                newText =>
                {
                    return newText.Contains("Success = 1")
                        && newText.Contains("Failure = 2");
                });
        }


        [TestMethod]
        public void TestUnion_InNamespace()
        {
            TestUnion(
                """
                using UnionTypes;

                namespace MyNamespace
                {
                    [TypeUnion]
                    public partial struct MyUnion
                    {
                        [UnionCase]
                        public record struct A(int x);

                        [UnionCase]
                        public record struct B(string y);
                    }
                }
                """,
                newText => newText.Contains("namespace MyNamespace"));
        }

        [TestMethod]
        public void TestUnion_GlobalUsings()
        {
            TestUnion(
                """
                global using System;
                using UnionTypes;

                namespace MyNamespace
                {
                    [TypeUnion]
                    public partial struct MyUnion
                    {
                        [UnionCase]
                        public record struct A(int x);

                        [UnionCase]
                        public record struct B(string y);
                    }
                }
                """,
                newText => newText.Contains("UnionTypes")
                        && !newText.Contains("global"));
        }

        [TestMethod]
        public void TestUnion_TypesInOtherNamespace()
        {
            TestUnion(
                """
                using UnionTypes;

                namespace OtherNamespace
                {
                    public record struct A(int x);
                    public record struct B(string y);
                }

                [TypeUnion]
                [UnionCase(Type=typeof(OtherNamespace.A))]
                [UnionCase(Type=typeof(OtherNamespace.B))]
                public partial struct MyUnion
                {
                }
                """);
        }

        [TestMethod]
        public void TestUnion_TypesInsideOtherType()
        {
            TestUnion(
                """
                using UnionTypes;

                public static class OtherType
                {
                    public record struct A(int x);
                    public record struct B(string y);
                }

                [TypeUnion]
                [UnionCase(Type=typeof(OtherType.A))]
                [UnionCase(Type=typeof(OtherType.B))]
                public partial struct MyUnion
                {
                }
                """);
        }

        [TestMethod]
        public void TestTagUnion_ReferenceConstrainedTypeParameter()
        {
            TestUnion(
                """
                using UnionTypes;

                [TagUnion]
                public partial struct Result<T> where T : class
                {
                    [UnionCase]
                    public static partial Result<T> Success(T value);

                    [UnionCase]
                    public static partial Result<T> Failure(string reason);
                }
                """);
        }

        [TestMethod]
        public void TestTypeUnion_ReferencedConstrainedTypeParameter()
        {
            TestUnion(
                """
                using UnionTypes;

                [TypeUnion]
                public partial struct Result<T> where T : class
                {
                    [UnionCase]
                    public record struct Success(T value);

                    [UnionCase]
                    public record struct Failure(string reason);
                }
                """);
        }


#if false

        [TestMethod]
        public void TestTypeUnionWithInternalAccessibility()
        {
            TestUnion(
                """
                using UnionTypes;

                namespace MyNamespace
                {
                    [Union]
                    internal partial struct MyUnion
                    {
                        public record struct A(int x);
                        public record struct B(string y);
                    }
                }
                """,
                newText => newText.Contains("internal partial struct MyUnion"));
        }

        [TestMethod]
        public void TestTypeUnionFromTypesWithInternalAccessibility()
        {
            TestUnion(
                """
                using UnionTypes;

                namespace MyNamespace
                {
                    [Union]
                    internal partial struct MyUnion
                    {
                        internal record struct A(int x);
                        internal record struct B(string y);
                    }
                }
                """,
                newText => newText.Contains(" internal static MyUnion Create"));
        }
#endif

        private void TestUnion(string sourceText, Func<string, bool>? generatedTextAssertion = null)
        {
            var compilation = CreateCompilation(sourceText);
            var diagnostics = compilation.GetDiagnostics();
            var trees = compilation.SyntaxTrees.ToArray();

            var newCompilation = RunGenerators(compilation, out var genDiagnostics, new UnionTypes.Generators.UnionSourceGenerator());

            var newTrees = newCompilation.SyntaxTrees.ToArray();
            Assert.IsTrue(newTrees.Length > trees.Length, "no new files were generated");
            var newTree = newTrees[trees.Length]; // assumes new tree is added to list
            var newText = newTree.GetText().ToString();

            var newDiagnostics = newCompilation.GetDiagnostics().Where(
                d => d.Severity == DiagnosticSeverity.Error
                    || d.Severity == DiagnosticSeverity.Warning).ToImmutableArray();
            AssertNoDiagnostics(newDiagnostics, newText);

            if (generatedTextAssertion != null)
            {
                Assert.IsTrue(generatedTextAssertion(newText), "generated text assertion failed");
            }
        }

        private static void AssertNoDiagnostics(ImmutableArray<Diagnostic> diagnostics, string newText)
        {
            if (diagnostics.Length > 0)
            {
                Assert.Fail($"Unexpected diagnostic: {diagnostics[0]}");
            }
        }

        private static Compilation RunGenerators(Compilation compilation, out ImmutableArray<Diagnostic> diagnostics, params ISourceGenerator[] generators)
        {
            CreateDriver(compilation, generators).RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out diagnostics);
            return updatedCompilation;
        }

        private static GeneratorDriver CreateDriver(Compilation compilation, params ISourceGenerator[] generators) =>
            CSharpGeneratorDriver.Create(
                generators: ImmutableArray.Create(generators),
                additionalTexts: ImmutableArray<AdditionalText>.Empty,
                parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First().Options,
                optionsProvider: null
            );

        private static Compilation CreateCompilation(string source)
        {
            return CSharpCompilation.Create(
                assemblyName: "compilation",
                syntaxTrees: new[] {
                    CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp11))
                    },
                references: new[] {
                    Core,
                    Netstandard,
                    SystemRuntime,
                    UnionTypes
                    },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );
        }

        private static string RuntimeDirectory = 
            Path.GetDirectoryName(typeof(Enumerable).GetTypeInfo().Assembly.Location)!;
        
        private static readonly MetadataReference Netstandard =
            MetadataReference.CreateFromFile(Path.Combine(RuntimeDirectory, "netstandard.dll"));

        private static readonly MetadataReference SystemRuntime =
            MetadataReference.CreateFromFile(Path.Combine(RuntimeDirectory, "System.Runtime.dll"));

        private static readonly MetadataReference Core =
            MetadataReference.CreateFromFile(typeof(int).GetTypeInfo().Assembly.Location);

        private static readonly MetadataReference UnionTypes =
            MetadataReference.CreateFromFile(typeof(UnionAttribute).GetTypeInfo().Assembly.Location);
    }
}