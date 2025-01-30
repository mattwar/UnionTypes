using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using UnionTypes.Toolkit;

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
                using UnionTypes.Toolkit;

                [TypeUnion]
                public partial struct MyUnion
                {
                    [Case]
                    public record struct A(int x);

                    [Case]
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
                using UnionTypes.Toolkit;

                [TypeUnion]
                public partial struct MyUnion
                {
                    [Case(Name="Aa")]
                    public record struct A(int x);

                    [Case(Name="Bb")]
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
                using UnionTypes.Toolkit;

                [TypeUnion]
                public partial struct MyUnion
                {
                    [Case(FactoryName="MakeA")]
                    public record struct A(int x);

                    [Case(FactoryName="MakeB")]
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
                using UnionTypes.Toolkit;

                [TypeUnion]
                public partial struct MyUnion
                {
                    [Case(TagValue=4)]
                    public record struct A(int x);

                    [Case(TagValue=3)]
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
                using UnionTypes.Toolkit;

                public record struct A(int x);
                public record struct B(string y, float z);

                [TypeUnion]
                public partial struct MyUnion
                {
                    public static partial MyUnion Create(A a);
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
                using UnionTypes.Toolkit;

                public record struct A(int x);
                public record struct B(string y, float z);

                [TypeUnion]
                public partial struct MyUnion
                {
                    [Case(Name="Aa")]
                    public static partial MyUnion Create(A a);

                    [Case(Name="Bb")]
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
                using UnionTypes.Toolkit;

                public record struct A(int x);
                public record struct B(string y, float z);

                [TypeUnion]
                public partial struct MyUnion
                {
                    [Case(TagValue=4)]
                    public static partial MyUnion Create(A a);

                    [Case(TagValue=3)]
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
        public void TestTypeUnion_CasesOnType()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                public record struct A(int x);
                public record struct B(string y, float z);

                [TypeUnion]
                [Case(Type=typeof(A))]
                [Case(Type=typeof(B))]
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
                using UnionTypes.Toolkit;

                public record struct A(int x);
                public record struct B(string y, float z);

                [TypeUnion]
                [Case(Name="Aa", Type=typeof(A))]
                [Case(Name="Bb", Type=typeof(B))]
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
                using UnionTypes.Toolkit;

                public record struct A(int x);
                public record struct B(string y, float z);

                [TypeUnion]
                [Case(Type=typeof(A), FactoryName="MakeA")]
                [Case(Type=typeof(B), FactoryName="MakeB")]
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
                using UnionTypes.Toolkit;

                public record struct A(int x);
                public record struct B(string y, float z);

                [TypeUnion]
                [Case(Type=typeof(A), TagValue=4)]
                [Case(Type=typeof(B), TagValue=3)]
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
        public void TestTypeUnion_CasesOnType_Singleton()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                public class A { private A(){} public static readonly A Value = new A(); }
                public record struct B(string y, float z);

                [TypeUnion]
                [Case(Type=typeof(A))]
                [Case(Type=typeof(B))]
                public partial struct MyUnion
                {
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.Create(A.Value);
                        MyUnion unionA2 = A.Value;
                        MyUnion unionB = MyUnion.Create(new B("x", 5.0f));
                        MyUnion unionB2 = new B("x", 5.0f);
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
        public void TestTypeUnion_CasesOnType_Singleton_FactoryIsProperty()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                public class A { private A() {} public static readonly A Singleton = new A(); }
                public record struct B(string y, float z);

                [TypeUnion]
                [Case(Type=typeof(A), FactoryIsProperty=true)]
                [Case(Type=typeof(B))]
                public partial struct MyUnion
                {
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.A;
                        MyUnion unionA2 = A.Singleton;
                        MyUnion unionB = MyUnion.Create(new B("x", 5.0f));
                        MyUnion unionB2 = new B("x", 5.0f);
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
        public void TestTagUnion()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TagUnion]
                public partial struct MyUnion
                {
                    public static partial MyUnion A(int x);
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
                using UnionTypes.Toolkit;

                [TagUnion]
                [Case(Name="A")]
                [Case(Name="B")]
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
                using UnionTypes.Toolkit;

                [TagUnion]
                [Case(Name="A", FactoryName="MakeA")]
                [Case(Name="B", FactoryName="MakeB")]
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
                using UnionTypes.Toolkit;

                [TagUnion]
                [Case(Name="A", FactoryName="A", FactoryIsProperty=false)]
                [Case(Name="B", FactoryName="B", FactoryIsProperty=false)]
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
        public void TestTagUnion_CaseNames()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TagUnion]
                public partial struct MyUnion
                {
                    [Case(Name="A")]
                    public static partial MyUnion MakeA(int x);

                    [Case(Name="B")]
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
        public void TestTagUnion_AccessorNames()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TagUnion]
                public partial struct MyUnion
                {
                    [Case(AccessorName="StuffForA")]
                    public static partial MyUnion A(int x);

                    [Case(AccessorName="StuffForB")]
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
        public void TestTagUnion_TagValues()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TagUnion]
                public partial struct MyUnion
                {
                    [Case(TagValue=4)]
                    public static partial MyUnion A(int x);

                    [Case(TagValue=3)]
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
                using UnionTypes.Toolkit;

                [TagUnion]
                public partial struct MyUnion
                {
                    [Case(TagValue=0)]
                    public static partial MyUnion Nobody();

                    public static partial MyUnion Student(string name, decimal grade);

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
        public void TestTagUnion_Sharing()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TagUnion]
                public partial struct MyUnion
                {
                    [Case(TagValue=0)]
                    public static partial MyUnion Nobody();

                    public static partial MyUnion Student(string name, decimal grade);

                    public static partial MyUnion Teacher(string name, int students);
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionN = MyUnion.Nobody();
                        MyUnion unionT = MyUnion.Teacher("Mr. Bob", 5);
                        MyUnion unionS = MyUnion.Student("Alice", 4.0m);
                        var n = unionN.IsNobody;
                        var t = unionT.TeacherValues;
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
                using UnionTypes.Toolkit;

                [TagUnion]
                [Case(Name="None", TagValue=0)]
                public partial struct Option<T>
                {
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
                using UnionTypes.Toolkit;

                [TagUnion]
                public partial struct Result<T>
                {
                    public static partial Result<T> Success(T value);

                    [Case(AccessorName="FailureMessage")]
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
                using UnionTypes.Toolkit;

                public class None { private None() {} public static readonly None Singleton = new None(); }

                [TypeUnion]
                [Case(Type=typeof(None), TagValue=0, FactoryName="None", FactoryIsProperty=true)]
                public partial struct Option<T>
                {
                    public static partial Option<T> Some(T value);
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        Option<string> optS = Option<string>.Some("Hello");
                        Option<string> optSA = "Hello";
                        Option<string> optN = Option<string>.None;
                        Option<string> optNA = None.Singleton;
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
                using UnionTypes.Toolkit;

                [TypeUnion]
                public partial struct Result<T>
                {
                    [Case]
                    public static partial Result<T> Success(T value);

                    [Case(Name="Failure", AccessorName="FailureMessage")]
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
                using UnionTypes.Toolkit;

                namespace MyNamespace
                {
                    [TypeUnion]
                    public partial struct MyUnion
                    {
                        public static partial MyUnion Create(int x);
                        public static partial MyUnion Create(string y);
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
                using UnionTypes.Toolkit;

                namespace MyNamespace
                {
                    [TypeUnion]
                    public partial struct MyUnion
                    {
                        public static partial MyUnion Create(int x);
                        public static partial MyUnion Create(string y);
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
                using UnionTypes.Toolkit;

                namespace OtherNamespace
                {
                    public record struct A(int x);
                    public record struct B(string y);
                }

                [TypeUnion]
                public partial struct MyUnion
                {
                    public static partial MyUnion Create(OtherNamespace.A a);
                    public static partial MyUnion Create(OtherNamespace.B b);
                }
                """);
        }

        [TestMethod]
        public void TestUnion_TypesInsideOtherType()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                public static class OtherType
                {
                    public record struct A(int x);
                    public record struct B(string y);
                }

                [TypeUnion]
                public partial struct MyUnion
                {
                    public static partial MyUnion Create(OtherType.A a);
                    public static partial MyUnion Create(OtherType.B b);
                }
                """);
        }

        [TestMethod]
        public void TestTypeUnion_Internal()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TypeUnion]
                internal partial struct MyUnion
                {
                    public static partial MyUnion Create(int x);
                    public static partial MyUnion Create(string y);
                }
                """,
                newText => newText.Contains("internal"));
        }

        [TestMethod]
        public void TestTypeUnion_Internal_FactoryIsInternal()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                internal record struct A(int x);
                internal record struct B(string y);
                                
                [TypeUnion]
                [Case(Type=typeof(A), FactoryIsInternal=true)]
                [Case(Type=typeof(B), FactoryIsInternal=true)]
                public partial struct MyUnion
                {
                }
                """,
                newText => newText.Contains("internal"));
        }

        [TestMethod]
        public void TestTypeUnion_InternalNestedTypes()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TypeUnion]
                public partial struct MyUnion
                {
                    [Case]
                    internal record struct A(int x);

                    [Case]
                    internal record struct B(string y);
                }
                """,
                newText => newText.Contains("internal"));
        }

        [TestMethod]
        public void TestTypeUnion_InternalFactories()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                record A(int x);
                record B(string y);

                [TypeUnion]
                public partial struct MyUnion
                {
                    internal static partial MyUnion A(A value);
                    internal static partial MyUnion B(B value);
                }
                """,
                newText => newText.Contains("internal"));
        }

        [TestMethod]
        public void TestTagUnion_Internal()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TagUnion]
                internal partial struct MyUnion
                {
                    public static partial MyUnion A(int x);
                    public static partial MyUnion B(string y);
                }
                """,
                newText => newText.Contains("internal"));
        }

        [TestMethod]
        public void TestTagUnion_InternalFactories()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TagUnion]
                public partial struct MyUnion
                {
                    internal static partial MyUnion A(int x);
                    internal static partial MyUnion B(string y);
                }
                """,
                newText => newText.Contains("internal"));
        }

        [TestMethod]
        public void TestTagUnion_Internal_FactoryIsInternal()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TagUnion]
                [Case(Name="A", FactoryIsInternal=true)]
                [Case(Name="B", FactoryIsInternal=true)]
                public partial struct MyUnion
                {
                }
                """,
                newText => newText.Contains("internal"));
        }

        [TestMethod]
        public void TestTagUnion_ReferenceConstrainedTypeParameter()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TagUnion]
                public partial struct Result<T> where T : class
                {
                    public static partial Result<T> Success(T value);
                    public static partial Result<T> Failure(string reason);
                }
                """);
        }

        [TestMethod]
        public void TestTypeUnion_ReferencedConstrainedTypeParameter()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TypeUnion]
                public partial struct Result<T> where T : class
                {
                    [Case]
                    public record struct Success(T value);

                    [Case]
                    public record struct Failure(string reason);
                }
                """);
        }

        [TestMethod]
        public void TestUnion_NoShareReferenceFields()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TagUnion(ShareReferenceFields=false)]
                public partial struct MyUnion
                {
                    public static partial MyUnion A(string x);
                    public static partial MyUnion B(object y);
                }
                """,
                newText =>
                {
                    if (newText.Contains("OverlappedData"))
                        return false;

                    var fields = GetDataFields(newText);
                    if (fields.Count != 2)
                        return false;

                    return true;
                });
        }

        [TestMethod]
        public void TestUnion_NoShareSameTypeFields()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TagUnion(OverlapStructs=false, ShareSameTypeFields=false)]
                public partial struct MyUnion
                {
                    public static partial MyUnion A(int x);
                    public static partial MyUnion B(int y);
                }
                """,
                newText =>
                {
                    if (newText.Contains("OverlappedData"))
                        return false;

                    var fields = GetDataFields(newText);
                    if (fields.Count != 2)
                        return false;

                    return true;
                });
        }

        [TestMethod]
        public void TestUnion_NoOverlapStructs()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TagUnion(OverlapStructs=false)]
                public partial struct MyUnion
                {
                    public static partial MyUnion A(int x, long y);
                    public static partial MyUnion B(long x, int y);
                }
                """,
                newText =>
                {
                    if (newText.Contains("OverlappedData"))
                        return false;

                    var fields = GetDataFields(newText);
                    if (fields.Count != 2)
                        return false;

                    return true;
                });
        }

        [TestMethod]
        public void TestUnion_NoDecomposeStructs()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                public record struct AData(int x, string y);
                public record struct BData(long x, string y);

                [TagUnion(DecomposeStructs=false)]
                public partial struct MyUnion
                {
                    public static partial MyUnion A(AData data);
                    public static partial MyUnion B(BData data);
                }
                """,
                newText => !newText.Contains("OverlappedData")
                        && GetDataFields(newText).Count == 2
                );
        }

        [TestMethod]
        public void TestUnion_TagTypeName()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TagUnion(TagTypeName="Tag")]
                public partial struct MyUnion
                {
                    public static partial MyUnion A(int x);
                    public static partial MyUnion B(string y);
                }
                """,
                newText => newText.Contains("enum Tag")
                    && newText.Contains("Tag Kind { get; }")
                );
        }

        [TestMethod]
        public void TestUnion_TagPropertyName()
        {
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TagUnion(TagPropertyName="Tag")]
                public partial struct MyUnion
                {
                    public static partial MyUnion A(int x);
                    public static partial MyUnion B(string y);
                }
                """,
                newText => newText.Contains("enum Case")
                    && newText.Contains("Case Tag { get; }")
                );
        }

        [TestMethod]
        public void TestTagUnion_GenerateEquality()
        {
            // default is enabled
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TagUnion]
                public partial struct MyUnion
                {
                    public static partial MyUnion A(int x);
                    public static partial MyUnion B(string y);
                }
                """,
                newText => newText.Contains("bool Equals(")
                );

            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TagUnion(GenerateEquality=false)]
                public partial struct MyUnion
                {
                    public static partial MyUnion A(int x);
                    public static partial MyUnion B(string y);
                }
                """,
                newText => !newText.Contains("bool Equals(")
                );

        }

        [TestMethod]
        public void TestTypeUnion_GenerateEquality()
        {
            // default is enabled
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TypeUnion]
                public partial struct MyUnion
                {
                    public static partial MyUnion Create(int x);
                    public static partial MyUnion Create(string y);
                }
                """,
                newText => newText.Contains("bool Equals(")
                );

            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TypeUnion(GenerateEquality=false)]
                public partial struct MyUnion
                {
                    public static partial MyUnion Create(int x);
                    public static partial MyUnion Create(string y);
                }
                """,
                newText => !newText.Contains("bool Equals(")
                );

        }

        [TestMethod]
        public void TestTagUnion_GenerateToString()
        {
            // default is enabled
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TagUnion]
                public partial struct MyUnion
                {
                    public static partial MyUnion A(int x);
                    public static partial MyUnion B(string y);
                }
                """,
                newText => newText.Contains("override string ToString(")
                );

            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TagUnion(GenerateToString=false)]
                public partial struct MyUnion
                {
                    public static partial MyUnion A(int x);
                    public static partial MyUnion B(string y);
                }
                """,
                newText => !newText.Contains("override string ToString(")
                );

        }

        [TestMethod]
        public void TestTypeUnion_GenerateToString()
        {
            // default is enabled
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TypeUnion]
                public partial struct MyUnion
                {
                    public static partial MyUnion Create(int value);
                    public static partial MyUnion Create(string value);
                }
                """,
                newText => newText.Contains("override string ToString(")
                );

            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TypeUnion(GenerateToString=false)]
                public partial struct MyUnion
                {
                    public static partial MyUnion Create(int value);
                    public static partial MyUnion Create(string value);
                }
                """,
                newText => !newText.Contains("override string ToString(")
                );
        }

        [TestMethod]
        public void TestTagUnion_GenerateMatch()
        {
            // default is enabled
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TagUnion]
                public partial struct MyUnion
                {
                    public static partial MyUnion A(int x);
                    public static partial MyUnion B(string y, float z);
                }
                """,
                newText => newText.Contains("void Match(")
                    && newText.Contains("TResult Select<TResult>(")
                );

            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TagUnion(GenerateMatch=false)]
                public partial struct MyUnion
                {
                    public static partial MyUnion A(int x);
                    public static partial MyUnion B(string y, float z);
                }
                """,
                newText => !newText.Contains("void Match(")
                    && !newText.Contains("TResult Select<TResult>(")
                );

        }

        [TestMethod]
        public void TestTypeUnion_GenerateMatch()
        {
            // default is enabled
            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TypeUnion]
                public partial struct MyUnion
                {
                    public static partial MyUnion Create(int value);
                    public static partial MyUnion Create(string value);
                }
                """,
                newText => newText.Contains("void Match(")
                    && newText.Contains("TResult Select<TResult>(")
                );

            TestUnion(
                """
                using UnionTypes.Toolkit;

                [TypeUnion(GenerateMatch=false)]
                public partial struct MyUnion
                {
                    public static partial MyUnion Create(int value);
                    public static partial MyUnion Create(string value);
                }
                """,
                newText => !newText.Contains("void Match(")
                    && !newText.Contains("TResult Select<TResult>(")
                );
        }

        [TestMethod]
        public void TestTypeUnion_NoToolkit()
        {
            TestUnion(
                """
                // @TypeUnion
                public partial struct MyUnion
                {
                    public static partial MyUnion Create(int x);
                    public static partial MyUnion Create(double x);
                }
                """,
                newText => newText.Contains("Int32 = 1")
                    && newText.Contains("Double = 2")
                    && newText.Contains("TryGet")
                );
        }

        [TestMethod]
        public void TestTagUnion_NoToolkit()
        {
            TestUnion(
                """
                // @TagUnion
                public partial struct MyUnion
                {
                    public static partial MyUnion CreateA(int x);
                    public static partial MyUnion CreateB(double x);
                }
                """,
                newText => newText.Contains("MyUnion") 
                    && newText.Contains("A = 1")
                    && newText.Contains("B = 2")
                    && !newText.Contains("TryGet")
                );

            TestUnion(
                """
                // @TagUnion
                public partial struct MyUnion
                {
                    public static partial MyUnion A(int x);
                    public static partial MyUnion B(double x);
                }
                """,
                newText => newText.Contains("MyUnion")
                    && newText.Contains("A = 1")
                    && newText.Contains("B = 2")
                    && !newText.Contains("TryGet")
                );
        }

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

        public static HashSet<string> GetDataFields(string generatedText)
        {
            return GetIdentifiers("_data", generatedText);
        }

        public static HashSet<string> GetIdentifiers(string namePrefix, string generatedText)
        {
            var names = new HashSet<string>();
            var index = 0;
            while (index >= 0)
            {
                var foundIndex = generatedText.IndexOf(namePrefix, index);
                if (foundIndex < index)
                    break;

                if (foundIndex > 0 && IsIdentifierChar(generatedText[foundIndex - 1]))
                {
                    // middle of another identifier? skip forward
                    index = foundIndex + 1;
                }
                else
                {
                    var name = GetIdentifierAt(foundIndex);
                    names.Add(name);
                    index = foundIndex + name.Length;
                }
            }

            return names;

            string GetIdentifierAt(int start)
            {
                var end = start;
                
                while (end < generatedText.Length 
                    && IsIdentifierChar(generatedText[end]))
                {
                    end++;
                }

                return generatedText.Substring(start, end - start);
            }

            bool IsIdentifierChar(char ch) =>
                char.IsLetterOrDigit(ch) || ch == '_';
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
            MetadataReference.CreateFromFile(typeof(TypeUnionAttribute).GetTypeInfo().Assembly.Location);
    }
}