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

            WriteLine($"public class {_oneOfType}");
            WriteLineNested($": IOneOf, IEquatable<{_oneOfType}>");
            WriteBraceNested(() =>
            {
                WriteLineSeparated(
                    WriteFields,
                    WriteConstructor,
                    WriteCreateMethods,
                    WriteConversionOperators,
                    WriteIUnionImplementation,
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
            for (int i = 1; i <= _nTypeArgs; i++)
            {
                WriteLine($"public static {_oneOfType} Create(T{i} value)");
                WriteBraceNested(() =>
                {
                    WriteLine($"return new {_oneOfType}(value!);");
                });
                WriteLine();
            }

            // Create(object)
            //WriteLine($"public static {_oneOfType} Create(object value)");
            //WriteBraceNested(() =>
            //{
            //    WriteLine("if (value is IOneOf oneOf)");
            //    WriteLineNested("value = oneOf.GetValue();");
            //    WriteLine();

            //    WriteLine("switch (value)");
            //    WriteBraceNested(() =>
            //    {
            //        for (int i = 1; i <= _nTypeArgs; i++)
            //        {
            //            WriteLine($"case T{i} _:");
            //        }

            //        WriteNested(() => WriteLine($"return new {_oneOfType}(value);"));
            //        WriteLine();

            //        WriteLine("default:");
            //        WriteNested(() => WriteLine("throw new ArgumentException($\"The value is not one of the expected types\");"));
            //    });
            //});
            //WriteLine();

            // Create<TOneOf>
            WriteLine($"public static {_oneOfType} Create<TValue>(TValue value)");
            WriteBraceNested(() =>
            {
                WriteLine("switch (value)");
                WriteBraceNested(() =>
                {
                    WriteLine($"case {_oneOfType} thisOneOf: return thisOneOf;");
                    for (int i = 1; i <= _nTypeArgs; i++)
                    {
                        WriteLine($"case T{i} value{i}: return Create(value{i});");
                    }
                    WriteLine($"case IOneOf otherOneOf: return Create(otherOneOf.GetValue());");
                    WriteLine("default: throw new InvalidCastException();");
                });
            });
        }

        private void WriteConversionOperators()
        {
            for (int i = 1; i <= _nTypeArgs; i++)
            {
                WriteLine($"public static implicit operator {_oneOfType}(T{i} value)");
                WriteBraceNested(() =>
                {
                    WriteLine($"return new {_oneOfType}(value!);");
                });
                WriteLine();

                WriteLine($"public static explicit operator T{i}({_oneOfType} oneOf)");
                WriteBraceNested(() =>
                {
                    WriteLine($"return oneOf.TryGetValue(out T{i} value) ? value : throw new InvalidCastException();");
                });

                if (i < _nTypeArgs)
                    WriteLine();
            }
        }

        private void WriteIUnionImplementation()
        {
            // IsType
            WriteLine("public bool IsType<T>() => _value is T;");
            WriteLine();

            // TryGetValue
            WriteLine("public bool TryGetValue<T>(out T value)");
            WriteBraceNested(() =>
            {
                WriteLine("if (_value is T)");
                WriteBraceNested(() =>
                {
                    WriteLine("value = (T)_value;");
                    WriteLine("return true;");
                });
                WriteLine("else");
                WriteBraceNested(() =>
                {
                    WriteLine("value = default!;");
                    WriteLine("return false;");
                });
            });
            WriteLine();

            // GetValue
            WriteLine("public object GetValue()");
            WriteBraceNested(() =>
            {
                WriteLine("return _value;");
            });
        }

        private void WriteEquality()
        {
            // equals
            WriteLine("public bool Equals<TValue>(TValue value)");
            WriteBraceNested(() =>
            {
                WriteLine($"if (value is {_oneOfType} thisOneOf)");
                WriteLineNested("return object.Equals(this.GetValue(), thisOneOf.GetValue());");
                WriteLine("else if (value is IOneOf otherOneOf)");
                WriteLineNested("return object.Equals(this.GetValue(), otherOneOf.GetValue());");
                WriteLine("else");
                WriteLineNested("return object.Equals(this.GetValue(), value);");
            });
            WriteLine();

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
                WriteLine("return GetValue()?.GetHashCode() ?? 0;");
            });
            WriteLine();

            // IEquatable
            WriteLine($"public bool Equals({_oneOfType}? other)");
            WriteBraceNested(() =>
            {
                WriteLine("return object.Equals(GetValue(), other?.GetValue());");
            });
            WriteLine();

            // operators
            WriteLine($"public static bool operator ==({_oneOfType} oneOf, IOneOf? other)");
            WriteBraceNested(() =>
            {
                WriteLine("return object.Equals(oneOf.GetValue(), other?.GetValue());");
            });
            WriteLine();
            WriteLine($"public static bool operator !=({_oneOfType} oneOf, IOneOf? other)");
            WriteBraceNested(() =>
            {
                WriteLine("return !object.Equals(oneOf.GetValue(), other?.GetValue());");
            });
            WriteLine();

            for (int i = 1; i <= _nTypeArgs; i++)
            {
                WriteLine($"public static bool operator ==({_oneOfType} oneOf, T{i} other)");
                WriteBraceNested(() =>
                {
                    WriteLine("return object.Equals(oneOf.GetValue(), other);");
                });
                WriteLine();
                WriteLine($"public static bool operator !=({_oneOfType} oneOf, T{i} other)");
                WriteBraceNested(() =>
                {
                    WriteLine("return !object.Equals(oneOf.GetValue(), other);");
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
                WriteLine("return GetValue()?.ToString() ?? \"\";");
            });
        }
    }

#if !T4
}
#endif
// #>