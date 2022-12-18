using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using UnionTypes;
using UnionTypes.Generators;

namespace UnionTests
{
    [TestClass]
    public class UnionSourceGeneratorTests
    {
        [TestMethod]
        public void TestUnionCasesFromNestedRecords()
        {
            TestUnion(
                """
                using UnionTypes;

                [Union]
                public partial struct MyUnion
                {
                    public record struct A(int x);
                    public record struct B(string y);
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = new MyUnion.A(10);
                        MyUnion unionA2 = MyUnion.CreateA(new MyUnion.A(10));
                        MyUnion unionA3 = MyUnion.CreateA(10);
                        
                        MyUnion unionB = new MyUnion.B("ten");
                        MyUnion unionB2 = MyUnion.CreateB(new MyUnion.B("ten"));
                        MyUnion unionB3 = MyUnion.CreateB("ten");
                        
                        bool isA = unionA.IsA;
                        bool isA2 = unionA.Is<MyUnion.A>();

                        bool tryA = unionA.TryGetA(out MyUnion.A maybeA);
                        bool tryA2 = unionA.TryGetAValues(out int x);

                        MyUnion.A a = unionA.GetA();
                        MyUnion.A a2 = unionA.Get<MyUnion.A>();
                        object boxedA = unionA.Get<object>();

                        var areEqual = unionA == unionB;
                        var areEqual2 = unionA == new MyUnion.A(20);
                    }
                }
                """,
                newText => newText.Contains("partial"));
        }

        [TestMethod]
        public void TestUnionCasesFromTypesAttribute()
        {
            TestUnion(
                """
                using UnionTypes;

                public record struct A(int x);
                public record struct B(string y);

                [Union]
                [UnionTypes(typeof(A), typeof(B))]
                public partial struct MyUnion
                {
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = new A(10);
                        MyUnion unionA2 = MyUnion.CreateA(new A(10));
                        MyUnion unionA3 = MyUnion.CreateA(10);
                        
                        MyUnion unionB = new B("ten");
                        MyUnion unionB2 = MyUnion.CreateB(new B("ten"));
                        MyUnion unionB3 = MyUnion.CreateB("ten");
                        
                        bool isA = unionA.IsA;
                        bool isA2 = unionA.Is<A>();
                
                        bool tryA = unionA.TryGetA(out A maybeA);
                        bool tryA2 = unionA.TryGetAValues(out int x);
                
                        A a = unionA.GetA();
                        A a2 = unionA.Get<A>();
                        object boxedA = unionA.Get<object>();
                
                        var areEqual = unionA == unionB;
                        var areEqual2 = unionA == new A(20);
                    }
                }
                """);
        }

        [TestMethod]
        public void TestUnionCasesFromTagsAttribute()
        {
            TestUnion(
                """
                using UnionTypes;

                [Union]
                [UnionTags("A", "B")]
                public partial struct MyUnion
                {
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.CreateA();
                        MyUnion unionA2 = MyUnion.A;
                        MyUnion unionB = MyUnion.CreateB();
                        MyUnion unionB2 = MyUnion.B;

                        var isA = unionA.IsA;
                        var isB = unionB.IsB;

                        var areEqual = unionA == unionB;
                    }
                }
                """);
        }

        [TestMethod]
        public void TestUnionCasesFromPartialFactories()
        {
            TestUnion(
                """
                using UnionTypes;

                public record struct C(double Z);

                [Union]
                public partial struct MyUnion
                {
                    public static partial MyUnion CreateA(int X);
                    public static partial MyUnion CreateB();
                    public static partial MyUnion CreateC(C value);
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.CreateA(10);
                        MyUnion unionB = MyUnion.CreateB();
                        MyUnion unionB2 = MyUnion.B;
                        MyUnion unionC = MyUnion.CreateC(new C(5.0));

                        bool isA = unionA.IsA;
                        bool tryA = unionA.TryGetA(out int x);                      
                
                        bool isB = unionB.IsB;

                        bool isC = unionC.IsC;
                        bool isC2 = unionC.Is<C>();
                        bool tryC = unionC.TryGetC(out C c);
                        bool tryC2 = unionC.TryGetCValues(out double z);
                        C getC = unionC.GetC();
                        C getC2 = unionC.Get<C>();

                        var areEqual = unionA == unionB;
                    }
                }
                """);
        }

        [TestMethod]
        public void TestUnionInNamespace()
        {
            TestUnion(
                """
                using UnionTypes;

                namespace MyNamespace
                {
                    [Union]
                    public partial struct MyUnion
                    {
                        public record struct A(int x);
                        public record struct B(string y);
                    }
                }
                """,
                newText => newText.Contains("namespace MyNamespace"));
        }

        [TestMethod]
        public void TestUnionCasesFromTypesInsideOtherNamespace()
        {
            TestUnion(
                """
                using UnionTypes;

                namespace OtherNamespace
                {
                    public record struct A(int x);
                    public record struct B(string y);
                }

                [Union]
                [UnionTypes(typeof(OtherNamespace.A), typeof(OtherNamespace.B))]
                public partial struct MyUnion
                {
                }
                """);
        }

        [TestMethod]
        public void TestUnionCasesFromTypesInsideOtherType()
        {
            TestUnion(
                """
                using UnionTypes;

                public static class OtherType
                {
                    public record struct A(int x);
                    public record struct B(string y);
                }

                [Union]
                [UnionTypes(typeof(OtherType.A), typeof(OtherType.B))]
                public partial struct MyUnion
                {
                }
                """);
        }

        [TestMethod]
        public void TestUnionWithInternalAccessibility()
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
        public void TestUnionCasesFromNestedRecordsWithInternalAccessibility()
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
                newText => newText.Contains("internal A GetA"));
        }

        [TestMethod]
        public void TestUnionWithTypeParameters()
        {
            TestUnion(
                """
                using UnionTypes;

                [Union]
                public partial struct Result<T>
                {
                    public record struct Success(T value);
                    public record struct Failure(string reason);
                }
                """);
        }

        private void TestUnion(string sourceText, Func<string, bool>? generatedTextAssertion = null)
        {
            var compilation = CreateCompilation(sourceText);
            var diagnostics = compilation.GetDiagnostics();
            //AssertNoDiagnostics(diagnostics);
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