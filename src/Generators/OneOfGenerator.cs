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

                //WriteLine($"public static bool CanCreateFrom<TValue>(TValue value) => OneOfCore<{_oneOfType}>.CanCreateFrom(value);");
                WriteLine($"public static bool TryCreateFrom<TValue>(TValue value, [NotNullWhen(true)] out {_oneOfType} union) => OneOfCore<{_oneOfType}>.TryCreateFrom(value, out union);");
                //WriteLine($"public static {_oneOfType} CreateFrom<TValue>(TValue value) => OneOfCore<{_oneOfType}>.CreateFrom(value);");

                for (int i = 1; i <= _nTypeArgs; i++)
                {
                    WriteLine($"public static {_oneOfType} Create{i}(T{i} value) => new {_oneOfType}(new OneOfCore<{_oneOfType}>({i}, value!));");
                }

                for (int i = 1; i <= _nTypeArgs; i++)
                {
                    WriteLine("/// <summary>The union's value as type <typeparamref name=\"T1\"/>.</summary>");
                    WriteLine($"public T{i} Value{i} => _core.Get<T{i}>();");
                }

                WriteLine("public object BoxedValue => _core.Value;");
                WriteLine("public Type Type => _core.GetIndexType();");
                WriteLine("public int TypeIndex => _core.GetTypeIndex();");

                var typeList = string.Join(", ", Enumerable.Range(1, nTypeArgs).Select(n => $"typeof(T{n})"));
                WriteLine($"private static IReadOnlyList<Type> _types = [{typeList}];");
                WriteLine($"public static IReadOnlyList<Type> Types => _types;");

                //WriteLine($"public bool CanGet<T>() => _core.CanGet<T>();");
                WriteLine($"public bool TryGet<T>([NotNullWhen(true)] out T value) => _core.TryGet(out value);");
                //WriteLine($"public T Get<T>() => _core.Get<T>();");
                //WriteLine("public T GetOrDefault<T>() => _core.GetOrDefault<T>();");

                WriteLine("public override string ToString() => _core.ToString();");

                for (int i = 1; i <= _nTypeArgs; i++)
                {
                    WriteLine($"public static implicit operator {_oneOfType}(T{i} value) => Create{i}(value);");
                }

                // match function
                Write("public TResult Match<TResult>(");
                for (int i = 1; i <= _nTypeArgs; i++)
                {
                    Write($"Func<T{i}, TResult> match{i}");
                    Write((i < _nTypeArgs) ? ", " : ")");
                }
                WriteLine();
                WriteBraceNested(
                    () =>
                    {
                        WriteLine("switch (TypeIndex)");
                        WriteBraceNested(
                            () =>
                            {
                                for (int i = 1; i <= _nTypeArgs; i++)
                                {
                                    WriteLine($"case {i}: return match{i}(Value{i});");
                                }
                                WriteLine("""default: throw new InvalidOperationException("Invalid union state.");""");
                            });
                    });

                // match action
                Write("public void Match<TResult>(");
                for (int i = 1; i <= _nTypeArgs; i++)
                {
                    Write($"Action<T{i}> match{i}");
                    Write((i < _nTypeArgs) ? ", " : ")");
                }
                WriteLine();
                WriteBraceNested(
                    () =>
                    {
                        WriteLine("switch (TypeIndex)");
                        WriteBraceNested(
                            () =>
                            {
                                for (int i = 1; i <= _nTypeArgs; i++)
                                {
                                    WriteLine($"case {i}: match{i}(Value{i}); break;");
                                }
                                WriteLine("""default: throw new InvalidOperationException("Invalid union state.");""");
                            });
                    });

            });
        }

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
    }

#if !T4
}
#endif
// #>