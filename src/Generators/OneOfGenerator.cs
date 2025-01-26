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
        private readonly string _namespaceName;

        private OneOfGenerator(int maxTypeArgs, string namespaceName)
        {
            _maxTypeArgs = maxTypeArgs;
            _namespaceName = namespaceName;
        }

        public static string Generate(int maxTypeArgs, string namespaceName)
        {
            var generator = new OneOfGenerator(maxTypeArgs, namespaceName);
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
            if (!string.IsNullOrEmpty(_namespaceName))
            {
                WriteLine($"namespace {_namespaceName}");
                WriteBraceNested(() =>
                {
                    WriteOneOfTypes();
                });
            }
            else
            {
                WriteOneOfTypes();
            }
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
            WriteLineNested($": IOneOf, IClosedTypeUnion<{_oneOfType}>, IEquatable<{_oneOfType}>");
            WriteBraceNested(() =>
            {
                WriteLineSeparatedBlocks(() =>
                {
                    WriteBlock(() =>
                    {
                        WriteLine("/// <summary>The type case for the union's value; 1 == T1, 2 == T2, etc.</summary>");
                        WriteLine("public int Kind { get; }");
                    });

                    WriteBlock(() =>
                    {
                        WriteLine("/// <summary>The underlying value of the union.</summary>");
                        WriteLine("public object Value { get; }");
                    });

                    WriteBlock(() =>
                    {
                        WriteLine($"private OneOf(int kind, object value) {{ this.Kind = kind; this.Value = value; }}");
                    });

                    for (int i = 1; i <= _nTypeArgs; i++)
                    {
                        WriteBlock(() =>
                        {
                            WriteLine($"public static {_oneOfType} Create(T{i} value)");
                            WriteBraceNested(() =>
                            {
                                WriteLine($"if (value is ITypeUnion u && u.TryGet(out object obj)) return new {_oneOfType}({i}, obj!);");
                                WriteLine($"return new {_oneOfType}({i}, value!);");
                            });
                        });
                    }

                    WriteBlock(() =>
                    {
                        WriteLine($"public static bool TryCreate<TValue>(TValue value, [NotNullWhen(true)] out {_oneOfType} union)");
                        WriteBraceNested(() =>
                        {
                            WriteLine("if (value != null)");
                            WriteBraceNested(() =>
                            {
                                WriteLine("switch (value)");
                                WriteBraceNested(() =>
                                {
                                    for (int i = 1; i <= _nTypeArgs; i++)
                                    {
                                        WriteLine($"case T{i} v{i}: union = Create(v{i}); return true;");
                                    }
                                });

                                WriteLine("return TypeUnion.TryCreateFromUnion(value, out union);");
                            });

                            WriteLine("union = default!;");
                            WriteLine("return false;");
                        });
                    });

                    WriteBlock(() =>
                    {
                        WriteLine($"public static {_oneOfType} Create<TValue>(TValue value) => TryCreate(value, out var union) ? union : throw new InvalidCastException(\"Invalid type for union.\");");
                    });

                    for (int i = 1; i <= _nTypeArgs; i++)
                    {
                        WriteBlock(() =>
                        {
                            WriteLine($"/// <summary>The union's value as type <typeparamref name=\"T{i}\"/>.</summary>");
                            WriteLine($"public T{i} Type{i}Value => (this.Value is T{i} value || this.TryGet(out value)) ? value : default!;");
                        });
                    }

                    WriteBlock(() =>
                    {
                        WriteLine("public Type Type => this.Value?.GetType() ?? typeof(object);");
                    });

                    WriteBlock(() =>
                    {
                        var typeList = string.Join(", ", Enumerable.Range(1, nTypeArgs).Select(n => $"typeof(T{n})"));
                        WriteLine($"static IReadOnlyList<Type> IClosedTypeUnion<{_oneOfType}>.Types {{ get; }} =");
                        WriteLineNested($"new [] {{ {typeList} }};");
                    });

                    WriteBlock(() =>
                    {
                        WriteLine($"public bool TryGet<TValue>([NotNullWhen(true)] out TValue value)");
                        WriteBraceNested(() =>
                        {
                            WriteLine("if (this.Value is TValue tval) { value = tval; return true; }");
                            WriteLine("return TypeUnion.TryCreate(this.Value, out value);");
                        });
                    });

                    WriteBlock(() =>
                    {
                        WriteLine("/// <summary>Returns the ToString() result of the value held by the union.</summary>");
                        WriteLine("public override string ToString() => this.Value?.ToString() ?? \"\";");
                    });

                    WriteBlock(() =>
                    {
                        for (int i = 1; i <= _nTypeArgs; i++)
                        {
                            WriteLine($"public static implicit operator {_oneOfType}(T{i} value) => Create(value);");
                        }
                        for (int i = 1; i <= _nTypeArgs; i++)
                        {
                            WriteLine($"public static explicit operator T{i}({_oneOfType} union) => union.Kind == {i} ? union.Type{i}Value : throw new InvalidCastException();");
                        }
                    });

                    // IEquality
                    WriteBlock(() =>
                    {
                        WriteLine($"public bool Equals({_oneOfType} other) => object.Equals(this.Value, other.Value);");
                        WriteLine($"public bool Equals<TValue>(TValue value) => (value is {_oneOfType} other || TryCreate(value, out other)) && Equals(other);");
                        WriteLine($"public override bool Equals(object? obj) => Equals<object?>(obj);");
                        WriteLine($"public override int GetHashCode() => this.Value?.GetHashCode() ?? 0;");
                        WriteLine($"public static bool operator ==({_oneOfType} a, {_oneOfType} b) => a.Equals(b);");
                        WriteLine($"public static bool operator !=({_oneOfType} a, {_oneOfType} b) => !a.Equals(b);");
                    });

                    // select
                    WriteBlock(() =>
                    {
                        var parameters = Enumerable.Range(1, _nTypeArgs).Select(i => $"Func<T{i}, TResult> whenType{i}").ToList();
                        parameters.Add("Func<TResult>? whenUndefined = null");

                        WriteLine($"public TResult Select<TResult>({string.Join(", ", parameters)})");
                        WriteBraceNested(
                            () =>
                            {
                                WriteLine("switch (this.Kind)");
                                WriteBraceNested(
                                    () =>
                                    {
                                        for (int i = 1; i <= _nTypeArgs; i++)
                                        {
                                            WriteLine($"case {i}: return whenType{i}(Type{i}Value);");
                                        }

                                        WriteLine("""default: return whenUndefined != null ? whenUndefined() : throw new InvalidOperationException("Undefined union state.");""");
                                    });
                            });

                    });

                    // match
                    WriteBlock(() =>
                    {
                        var parameters = Enumerable.Range(1, _nTypeArgs).Select(i => $"Action<T{i}> whenType{i}").ToList();
                        parameters.Add("Action? whenUndefined = null");

                        WriteLine($"public void Match<TResult>({string.Join(", ", parameters)})");
                        WriteBraceNested(
                            () =>
                            {
                                WriteLine("switch (this.Kind)");
                                WriteBraceNested(
                                    () =>
                                    {
                                        for (int i = 1; i <= _nTypeArgs; i++)
                                        {
                                            WriteLine($"case {i}: whenType{i}(Type{i}Value); break;");
                                        }
                                        WriteLine("""default: if (whenUndefined != null) whenUndefined(); else throw new InvalidOperationException("Invalid union state."); break;""");
                                    });
                            });
                    });

                });
            });
        }
    }

#if !T4
}
#endif
// #>