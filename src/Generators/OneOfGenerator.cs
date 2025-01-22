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
            WriteLineNested($": IOneOf<{_oneOfType}>");
            WriteBraceNested(() =>
            {
                WriteLine($"private readonly OneOfCore<{_oneOfType}> _core;");
                WriteLine($"private OneOf(OneOfCore<{_oneOfType}> core) => _core = core;");
                WriteLine($"static {_oneOfType} IOneOf<{_oneOfType}>.Construct(OneOfCore<{_oneOfType}> core) => new {_oneOfType}(core);");

                for (int i = 1; i <= _nTypeArgs; i++)
                {
                    WriteLine($"public static {_oneOfType} Create(T{i} value) => new {_oneOfType}(new OneOfCore<{_oneOfType}>({i}, value!));");
                }

                WriteLine($"public static bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out {_oneOfType} union) => OneOfCore<{_oneOfType}>.TryCreateFrom(value, out union);");
                WriteLine($"public static {_oneOfType} Create<TValue>(TValue value) => TryCreate(value, out var union) ? union : throw new InvalidCastException(\"Invalid type for union.\");");

                for (int i = 1; i <= _nTypeArgs; i++)
                {
                    WriteLine($"/// <summary>The union's value as type <typeparamref name=\"T{i}\"/>.</summary>");
                    WriteLine($"public T{i} Type{i}Value => _core.GetOrDefault<T{i}>();");
                }

                WriteLine("/// <summary>The type case for the union's value; 1 == T1, 2 == T2, etc.</summary>");
                WriteLine($"public int Kind => _core.Kind;");
                WriteLine("public Type Type => _core.Type;");
                WriteLine("public object Value => _core.Value;");

                var typeList = string.Join(", ", Enumerable.Range(1, nTypeArgs).Select(n => $"typeof(T{n})"));
                WriteLine($"private static IReadOnlyList<Type> _types = [{typeList}];");
                WriteLine($"static IReadOnlyList<Type> IClosedTypeUnion<{_oneOfType}>.Types => _types;");

                WriteLine($"public bool TryGet<T>([NotNullWhen(true)] out T value) => _core.TryGet(out value);");

                WriteLine("/// <summary>Returns the ToString() result of the value held by the union.</summary>");
                WriteLine("public override string ToString() => _core.ToString();");

                for (int i = 1; i <= _nTypeArgs; i++)
                {
                    WriteLine($"public static implicit operator {_oneOfType}(T{i} value) => Create(value);");
                }

                // match function
                var parameters = string.Join(", ", Enumerable.Range(1, _nTypeArgs).Select(i => $"Func<T{i}, TResult> match{i}"));
                WriteLine($"public TResult Match<TResult>({parameters})");
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

                // match action
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

#if false
        private void WriteEquality()
        {
            // equals
            WriteLine("public override bool Equals(object? obj)");
            WriteBraceNested(() =>
            {
                WriteLine("return obj is object other && Equals<object>(other);");
            });
            WriteLine();

            // GetHashCode
            WriteLine("public override int GetHashCode()");
            WriteBraceNested(() =>
            {
                WriteLine("return _value?.GetHashCode() ?? 0;");
            });
            WriteLine();

            // IEquatable
            WriteLine($"public bool Equals({_oneOfType} other)");
            WriteBraceNested(() =>
            {
                WriteLine("return object.Equals(_value, other._value);");
            });
            WriteLine();

            // operators
            WriteLine($"public static bool operator ==({_oneOfType} oneOf, ITypeUnion? other)");
            WriteBraceNested(() =>
            {
                WriteLine("return object.Equals(oneOf._value, other?.Get<object>());");
            });
            WriteLine();
            WriteLine($"public static bool operator !=({_oneOfType} oneOf, ITypeUnion? other)");
            WriteBraceNested(() =>
            {
                WriteLine("return !object.Equals(oneOf._value, other?.Get<object>());");
            });
            WriteLine();

            for (int i = 1; i <= _nTypeArgs; i++)
            {
                WriteLine($"public static bool operator ==({_oneOfType} oneOf, T{i} other)");
                WriteBraceNested(() =>
                {
                    WriteLine("return object.Equals(oneOf._value, other);");
                });
                WriteLine();
                WriteLine($"public static bool operator !=({_oneOfType} oneOf, T{i} other)");
                WriteBraceNested(() =>
                {
                    WriteLine("return !object.Equals(oneOf._value, other);");
                });

                if (i < _nTypeArgs)
                    WriteLine();
            }
        }

        private void WriteMiscMethods()
        {
            WriteLine("public override string ToString()");
            WriteBraceNested(() =>
            {
                WriteLine("return _value?.ToString() ?? \"\";");
            });
        }
#endif
    }

#if !T4
}
#endif
// #>