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
                    [TypeCase]
                    public record struct A(int x);

                    [TypeCase]
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
                    [TypeCase(Name="Aa")]
                    public record struct A(int x);

                    [TypeCase(Name="Bb")]
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
                    [TypeCase(FactoryName="MakeA")]
                    public record struct A(int x);

                    [TypeCase(FactoryName="MakeB")]
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
                    [TypeCase(TagValue=4)]
                    public record struct A(int x);

                    [TypeCase(TagValue=3)]
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
                    [TypeCase]
                    public static partial MyUnion Create(A a);

                    [TypeCase]
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
                    [TypeCase(Name="Aa")]
                    public static partial MyUnion Create(A a);

                    [TypeCase(Name="Bb")]
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
                    [TypeCase(TagValue=4)]
                    public static partial MyUnion Create(A a);

                    [TypeCase(TagValue=3)]
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
                [TypeCase(Type=typeof(A))]
                [TypeCase(Type=typeof(B))]
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
                [TypeCase(Name="Aa", Type=typeof(A))]
                [TypeCase(Name="Bb", Type=typeof(B))]
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
                [TypeCase(Type=typeof(A), FactoryName="MakeA")]
                [TypeCase(Type=typeof(B), FactoryName="MakeB")]
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
                [TypeCase(Type=typeof(A), TagValue=4)]
                [TypeCase(Type=typeof(B), TagValue=3)]
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
                    [TagCase]
                    public static partial MyUnion A(int x);

                    [TagCase]
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
                [TagCase(Name="A")]
                [TagCase(Name="B")]
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
                [TagCase(Name="A", FactoryName="MakeA")]
                [TagCase(Name="B", FactoryName="MakeB")]
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
                [TagCase(Name="A", FactoryName="A", FactoryIsProperty=false)]
                [TagCase(Name="B", FactoryName="B", FactoryIsProperty=false)]
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
                using UnionTypes;

                [TagUnion]
                public partial struct MyUnion
                {
                    [TagCase(Name="A")]
                    public static partial MyUnion MakeA(int x);

                    [TagCase(Name="B")]
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
                using UnionTypes;

                [TagUnion]
                public partial struct MyUnion
                {
                    [TagCase(AccessorName="StuffForA")]
                    public static partial MyUnion A(int x);

                    [TagCase(AccessorName="StuffForB")]
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
                using UnionTypes;

                [TagUnion]
                public partial struct MyUnion
                {
                    [TagCase(TagValue=4)]
                    public static partial MyUnion A(int x);

                    [TagCase(TagValue=3)]
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
                    [TagCase(TagValue=0)]
                    public static partial MyUnion Nobody();

                    [TagCase]
                    public static partial MyUnion Student(string name, decimal grade);

                    [TagCase]
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
                [TagCase(Name="None", TagValue=0)]
                public partial struct Option<T>
                {
                    [TagCase]
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
                    [TagCase]
                    public static partial Result<T> Success(T value);

                    [TagCase(AccessorName="FailureMessage")]
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
                [TypeCase(Type=typeof(None), IsSingleton=true, TagValue=0)]
                public partial struct Option<T>
                {
                    [TypeCase]
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
                    [TypeCase]
                    public static partial Result<T> Success(T value);

                    [TypeCase(Name="Failure", AccessorName="FailureMessage")]
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
                        [TypeCase]
                        public record struct A(int x);

                        [TypeCase]
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
                        [TypeCase]
                        public record struct A(int x);

                        [TypeCase]
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
                [TypeCase(Type=typeof(OtherNamespace.A))]
                [TypeCase(Type=typeof(OtherNamespace.B))]
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
                [TypeCase(Type=typeof(OtherType.A))]
                [TypeCase(Type=typeof(OtherType.B))]
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
                    [TagCase]
                    public static partial Result<T> Success(T value);

                    [TagCase]
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
                    [TypeCase]
                    public record struct Success(T value);

                    [TypeCase]
                    public record struct Failure(string reason);
                }
                """);
        }

        [TestMethod]
        public void TestUnion_NoShareReferenceFields()
        {
            TestUnion(
                """
                using UnionTypes;

                [TagUnion(ShareReferenceFields=false)]
                public partial struct MyUnion
                {
                    [TagCase]
                    public static partial MyUnion A(string x);

                    [TagCase]
                    public static partial MyUnion B(object y);
                }
                """,
                newText =>
                {
                    if (newText.Contains("OverlappedData"))
                        return false;

                    var fields = GetFields(newText);
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
                using UnionTypes;

                [TagUnion(OverlapStructs=false, ShareSameTypeFields=false)]
                public partial struct MyUnion
                {
                    [TagCase]
                    public static partial MyUnion A(int x);

                    [TagCase]
                    public static partial MyUnion B(int y);
                }
                """,
                newText =>
                {
                    if (newText.Contains("OverlappedData"))
                        return false;

                    var fields = GetFields(newText);
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
                using UnionTypes;

                [TagUnion(OverlapStructs=false)]
                public partial struct MyUnion
                {
                    [TagCase]
                    public static partial MyUnion A(int x, int y);

                    [TagCase]
                    public static partial MyUnion B(long x, long y);
                }
                """,
                newText =>
                {
                    if (newText.Contains("OverlappedData"))
                        return false;

                    var fields = GetFields(newText);
                    if (fields.Count != 4)
                        return false;

                    return true;
                });
        }

        [TestMethod]
        public void TestUnion_NoDecomposeStructs()
        {
            TestUnion(
                """
                using UnionTypes;

                public record struct AData(int x, string y);
                public record struct BData(long x, string y);

                [TagUnion(DecomposeStructs=false)]
                public partial struct MyUnion
                {
                    [TagCase]
                    public static partial MyUnion A(AData data);

                    [TagCase]
                    public static partial MyUnion B(BData data);
                }
                """,
                newText => !newText.Contains("OverlappedData")
                        && GetFields(newText).Count == 2
                );
        }

        [TestMethod]
        public void TestUnion_TagTypeName()
        {
            TestUnion(
                """
                using UnionTypes;

                [TagUnion(TagTypeName="Tag")]
                public partial struct MyUnion
                {
                    [TagCase]
                    public static partial MyUnion A(int x);

                    [TagCase]
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
                using UnionTypes;

                [TagUnion(TagPropertyName="Tag")]
                public partial struct MyUnion
                {
                    [TagCase]
                    public static partial MyUnion A(int x);

                    [TagCase]
                    public static partial MyUnion B(string y);
                }
                """,
                newText => newText.Contains("enum Case")
                    && newText.Contains("Case Tag { get; }")
                );
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

        public static HashSet<string> GetFields(string generatedText)
        {
            return GetIdentifiers("_field", generatedText);
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