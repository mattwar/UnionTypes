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
            WriteLineNested($": ITypeUnion, IEquatable<{_oneOfType}>");
            WriteBraceNested(() =>
            {
                WriteLineSeparated(
                    WriteFields,
                    WriteConstructor,
                    WriteCreateMethods,
                    WriteConversionOperators,
                    WriteITypeUnionImplementation,
                    WriteEquality,
                    WriteMiscMethods
                    );
            });
        }

        private void WriteFields()
        {
            WriteLine("private readonly object _value;");
        }

        private void WriteConstructor()
        {
            WriteLine("private OneOf(object value) { _value = value; }");
        }

        private void WriteCreateMethods()
        {
            // Create(T)
            for (int i = 1; i <= _nTypeArgs; i++)
            {
                WriteLine($"public static {_oneOfType} Create(T{i} value)");
                WriteBraceNested(() =>
                {
                    WriteLine($"return new {_oneOfType}(value!);");
                });
                WriteLine();
            }

            // Create<TOneOf>
            WriteLine($"public static {_oneOfType} Create<TValue>(TValue value)");
            WriteBraceNested(() =>
            {
                WriteLine("switch (value)");
                WriteBraceNested(() =>
                {
                    for (int i = 1; i <= _nTypeArgs; i++)
                    {
                        WriteLine($"case T{i} value{i}: return Create(value{i});");
                    }
                    WriteLine($"case ITypeUnion otherOneOf: return Create(otherOneOf.Get<object>());");
                    WriteLine("default: throw new InvalidCastException();");
                });
            });
            WriteLine();

            // Convert<TOneOf>
            WriteLine($"public static {_oneOfType} Convert<TOneOf>(TOneOf oneOf) where TOneOf : ITypeUnion");
            WriteBraceNested(() =>
            {
                WriteLine($"return TryConvert(oneOf, out var thisOneOf) ? thisOneOf : throw new InvalidCastException();");
            });
            WriteLine();

            // TryConvert<TOneOf>
            WriteLine($"public static bool TryConvert<TOneOf>(TOneOf oneOf, out {_oneOfType} thisOnOf) where TOneOf : ITypeUnion");
            WriteBraceNested(() =>
            {
                WriteLine($"if (oneOf is {_oneOfType} me) {{ thisOnOf = me; return true; }}");

                for (int i = 1; i <= _nTypeArgs; i++)
                {
                    WriteLine($"if (oneOf.TryGet(out T{i} value{i})) {{ thisOnOf = Create(value{i}); return true; }}");
                }

                WriteLine("thisOnOf = default!;");
                WriteLine("return false;");
            });
        }

        private void WriteConversionOperators()
        {
            for (int i = 1; i <= _nTypeArgs; i++)
            {
                WriteLine($"public static implicit operator {_oneOfType}(T{i} value)");
                WriteBraceNested(() =>
                {
                    WriteLine($"return Create<T{i}>(value);");
                });
                WriteLine();

                WriteLine($"public static explicit operator T{i}({_oneOfType} oneOf)");
                WriteBraceNested(() =>
                {
                    WriteLine($"return oneOf.Get<T{i}>();");
                });

                if (i < _nTypeArgs)
                    WriteLine();
            }
        }

        private void WriteITypeUnionImplementation()
        {
            // Is<T>
            WriteLine("public bool Is<T>() => _value is T;");

            // Get<T>
            WriteLine("public T Get<T>() => _value is T t ? t : throw new InvalidCastException();");

            // TryGet<T>
            WriteLine("public bool TryGet<T>(out T value) { if (_value is T t) { value = t; return true; } else { value = default!; return false; } }");

            // equals
            WriteLine("public bool Equals<TValue>(TValue value)");
            WriteBraceNested(() =>
            {
                WriteLine($"if (value is {_oneOfType} thisOneOf)");
                WriteLineNested("return object.Equals(this.Get<object>(), thisOneOf.Get<object>());");
                WriteLine("else if (value is ITypeUnion otherOneOf)");
                WriteLineNested("return object.Equals(this.Get<object>(), otherOneOf.Get<object>());");
                WriteLine("else");
                WriteLineNested("return object.Equals(this.Get<object>(), value);");
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