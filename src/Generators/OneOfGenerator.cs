// <#+
#if !T4
namespace UnionTypes.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
#endif

#nullable enable

    public class OneOfGenerator : Generator
    {
        private readonly int _maxTypeArgs;

        private OneOfGenerator(int maxTypeArgs)
        {
            _maxTypeArgs = maxTypeArgs;
        }

        public static string Generate(int maxTypeArgs)
        {
            var generator = new OneOfGenerator(maxTypeArgs);
            generator.WriteFile();
            return generator.GeneratedText;
        }

        private void WriteFile()
        {
            WriteLine("using System;");
            WriteLine("using System.Collections.Generic;");
            WriteLine("using System.Diagnostics.CodeAnalysis;");
            WriteLine("#nullable enable");
            WriteLine();
            WriteLine("namespace UnionTypes");
            WriteBraceNested(() =>
            {
                WriteOneOfTypes();
            });
        }

        private void WriteOneOfTypes()
        {
            for (int nTypeArgs = 2; nTypeArgs <= _maxTypeArgs; nTypeArgs++)
            {
                WriteOneOfType(nTypeArgs);
                if (nTypeArgs < _maxTypeArgs)
                    WriteLine();
            }
        }

        private int _nTypeArgs = 0;
        private string _typeArgList = "";
        private string _oneOfType = "";

        private void WriteOneOfType(int nTypeArgs)
        {
            _nTypeArgs = nTypeArgs;
            _typeArgList = string.Join(", ", Enumerable.Range(1, nTypeArgs).Select(n => $"T{n}"));
            _oneOfType = $"OneOf<{_typeArgList}>";

            WriteLine($"public struct {_oneOfType}");
            WriteLineNested($": IOneOf<{_oneOfType}>, IEquatable<{_oneOfType}>");
            WriteBraceNested(() =>
            {
                WriteLine("/// <summary>The type case for the union's value; 1 == T1, 2 == T2, etc.</summary>");
                WriteLine("public int Kind { get; }");
                WriteLine("/// <summary>The underlying value of the union.</summary>");
                WriteLine("public object Value { get; }");

                WriteLine($"private OneOf(int kind, object value) {{ this.Kind = kind; this.Value = value; }}");
                WriteLine($"static {_oneOfType} IOneOf<{_oneOfType}>.Construct(int kind, object value) => new {_oneOfType}(kind, value);");

                for (int i = 1; i <= _nTypeArgs; i++)
                {
                    WriteLine($"public static {_oneOfType} Create(T{i} value) => new {_oneOfType}({i}, value!);");
                }

                WriteLine($"public static bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out {_oneOfType} union) => OneOf.TryCreate<{_oneOfType}, TValue>(value, out union);");
                WriteLine($"public static {_oneOfType} Create<TValue>(TValue value) => TryCreate(value, out var union) ? union : throw new InvalidCastException(\"Invalid type for union.\");");

                for (int i = 1; i <= _nTypeArgs; i++)
                {
                    WriteLine($"/// <summary>The union's value as type <typeparamref name=\"T{i}\"/>.</summary>");
                    WriteLine($"public T{i} Type{i}Value => (this.Value is T{i} value || this.TryGet(out value)) ? value : default!;");
                }

                WriteLine("public Type Type => this.Value?.GetType() ?? typeof(object);");

                var typeList = string.Join(", ", Enumerable.Range(1, nTypeArgs).Select(n => $"typeof(T{n})"));
                WriteLine($"private static IReadOnlyList<Type> _types = [{typeList}];");
                WriteLine($"static IReadOnlyList<Type> IClosedTypeUnion<{_oneOfType}>.Types => _types;");

                WriteLine($"public bool TryGet<TValue>([NotNullWhen(true)] out TValue value)");
                WriteBraceNested(() =>
                {
                    WriteLine("if (this.Value is TValue tval) { value = tval; return true; }");
                    WriteLine("return TypeUnion.TryCreate(this.Value, out value);");
                });
            
                WriteLine("/// <summary>Returns the ToString() result of the value held by the union.</summary>");
                WriteLine("public override string ToString() => this.Value?.ToString() ?? \"\";");

                for (int i = 1; i <= _nTypeArgs; i++)
                {
                    WriteLine($"public static implicit operator {_oneOfType}(T{i} value) => Create(value);");
                }

                for (int i = 1; i <= _nTypeArgs; i++)
                {
                    WriteLine($"public static explicit operator T{i}({_oneOfType} union) => union.Kind == {i} ? union.Type{i}Value : throw new InvalidCastException();");
                }

                // IEquality
                WriteLine($"public bool Equals({_oneOfType} other) => object.Equals(this.Value, other.Value);");
                WriteLine($"public bool Equals<TValue>(TValue value) => (value is {_oneOfType} other || TryCreate(value, out other)) && Equals(other);");
                WriteLine($"public override bool Equals(object? obj) => Equals<object?>(obj);");
                WriteLine($"public override int GetHashCode() => this.Value?.GetHashCode() ?? 0;");
                WriteLine($"public static bool operator ==({_oneOfType} a, {_oneOfType} b) => a.Equals(b);");
                WriteLine($"public static bool operator !=({_oneOfType} a, {_oneOfType} b) => !a.Equals(b);");

                // select
                var parameters = string.Join(", ", Enumerable.Range(1, _nTypeArgs).Select(i => $"Func<T{i}, TResult> match{i}"));
                WriteLine($"public TResult Select<TResult>({parameters})");
                WriteBraceNested(
                    () =>
                    {
                        WriteLine("switch (this.Kind)");
                        WriteBraceNested(
                            () =>
                            {
                                for (int i = 1; i <= _nTypeArgs; i++)
                                {
                                    WriteLine($"case {i}: return match{i}(Type{i}Value);");
                                }
                                WriteLine("""default: throw new InvalidOperationException("Invalid union state.");""");
                            });
                    });

                // match
                parameters = string.Join(", ", Enumerable.Range(1, _nTypeArgs).Select(i => $"Action<T{i}> match{i}")); 
                WriteLine($"public void Match<TResult>({parameters})");
                WriteBraceNested(
                    () =>
                    {
                        WriteLine("switch (this.Kind)");
                        WriteBraceNested(
                            () =>
                            {
                                for (int i = 1; i <= _nTypeArgs; i++)
                                {
                                    WriteLine($"case {i}: match{i}(Type{i}Value); break;");
                                }
                                WriteLine("""default: throw new InvalidOperationException("Invalid union state.");""");
                            });
                    });
            });
        }
    }

#if !T4
}
#endif
// #>