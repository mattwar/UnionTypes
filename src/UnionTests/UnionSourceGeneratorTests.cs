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
        public void TestUnionNestedCases()
        {
            TestUnion(
                """
                using UnionTypes;

                [Union]
                public partial struct MyUnion
                {
                    public record struct OptionA(int x);
                    public record struct OptionB(string y);
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = new MyUnion.OptionA(10);
                        MyUnion unionB = new MyUnion.OptionB("ten");
                        var isA = unionA.IsOptionA;
                        var isB = unionB.IsOptionB;
                        var areEqual = unionA == unionB;
                        var optA = unionA.ToOptionA();
                        var optB = unionB.ToOptionB();
                        isA = unionA.TryGetOptionA(out var maybeA);
                        var boxedValue = unionB.GetValue();
                    }
                }
                """,
                newText => newText.Contains("partial"));
        }

        [TestMethod]
        public void TestUnionExternalCases()
        {
            TestUnion(
                """
                using UnionTypes;

                public record struct OptionA(int x);
                public record struct OptionB(string y);

                [Union]
                [UnionTypes(typeof(OptionA), typeof(OptionB))]
                public partial struct MyUnion
                {
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = new OptionA(10);
                        MyUnion unionB = new OptionB("ten");
                        var isA = unionA.IsOptionA;
                        var isB = unionB.IsOptionB;
                        var areEqual = unionA == unionB;
                        var optA = unionA.ToOptionA();
                        var optB = unionB.ToOptionB();
                        isA = unionA.TryGetOptionA(out var maybeA);
                    }
                }
                """);
        }

        [TestMethod]
        public void TestUnionExternalFromOtherType()
        {
            TestUnion(
                """
                using UnionTypes;

                public static class OtherType
                {
                    public record struct OptionA(int x);
                    public record struct OptionB(string y);
                }

                [Union]
                [UnionTypes(typeof(OtherType.OptionA), typeof(OtherType.OptionB))]
                public partial struct MyUnion
                {
                }
                """);
        }

        [TestMethod]
        public void TestUnionTagCases()
        {
            TestUnion(
                """
                using UnionTypes;

                [Union]
                [UnionTags("OptionA", "OptionB")]
                public partial struct MyUnion
                {
                }

                public class Test
                {
                    public void TestMethod()
                    {
                        MyUnion unionA = MyUnion.OptionA;
                        MyUnion unionB = MyUnion.OptionB;
                        var isA = unionA.IsOptionA;
                        var isB = unionB.IsOptionB;
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
                        public record struct OptionA(int x);
                        public record struct OptionB(string y);
                    }
                }
                """,
                newText => newText.Contains("namespace MyNamespace"));
        }

        [TestMethod]
        public void TestUnionInternal()
        {
            TestUnion(
                """
                using UnionTypes;

                namespace MyNamespace
                {
                    [Union]
                    internal partial struct MyUnion
                    {
                        public record struct OptionA(int x);
                        public record struct OptionB(string y);
                    }
                }
                """,
                newText => newText.Contains("internal partial struct MyUnion"));
        }

        [TestMethod]
        public void TestUnionInternalCases()
        {
            TestUnion(
                """
                using UnionTypes;

                namespace MyNamespace
                {
                    [Union]
                    internal partial struct MyUnion
                    {
                        internal record struct OptionA(int x);
                        internal record struct OptionB(string y);
                    }
                }
                """,
                newText => newText.Contains("internal OptionA ToOptionA"));
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