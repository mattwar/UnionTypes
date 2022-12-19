// <#+
#if !T4
namespace UnionTypes.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#endif

#nullable enable

    public class Union
    {
        /// <summary>
        /// The name of the union.
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// The type parameters (if any)
        /// </summary>
        public string TypeParameterList { get; }

        /// <summary>
        /// The name of the union type (with type parameters)
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Accessibility of the union: public, internal, private
        /// </summary>
        public string Accessibility { get; }

        public IReadOnlyList<Case> Cases { get; }

        public Union(string name, string typeParameterList, string accessibility, params Case[] cases)
        {
            this.Name = name;
            this.TypeParameterList = typeParameterList ?? "";
            this.TypeName = string.IsNullOrEmpty(typeParameterList) ? name : name + typeParameterList;
            this.Accessibility = accessibility;
            this.Cases = cases;
        }

        public Union(string name, params Case[] cases)
            : this(name, "", "public", cases)
        {
        }
    }

    public enum TypeKind
    {
        /// <summary>
        /// The type kind is unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// Just a tag, contains no values
        /// </summary>
        Tag,

        /// <summary>
        /// Just a single value (not class/struct holding it)
        /// </summary>
        Primitive,

        /// <summary>
        /// struct containing one or more values that can be deconstructed
        /// </summary>
        RecordStruct,

        /// <summary>
        /// class containing zero or more values
        /// </summary>
        Class,

        /// <summary>
        /// struct containing zero or more values
        /// </summary>
        Struct,

        /// <summary>
        /// An interface
        /// </summary>
        Interface,

        /// <summary>
        /// A type parameter
        /// </summary>
        TypeParameter
    }

    public class Case
    {
        public TypeKind Kind { get; }
        public string Name { get; }
        public string Type { get; }
        public string Accessibilty { get; }
        public bool IsPartial { get; }
        public IReadOnlyList<Value> Values { get; }
        public bool Generate { get; }

        public Case(TypeKind kind, string name, string type, string accessibility, bool isPartial, bool generate, IReadOnlyList<Value>? values)
        {
            this.Kind = kind;
            this.Name = name;
            this.Type = type ?? name;
            this.Accessibilty = accessibility ?? "";
            this.IsPartial = isPartial;
            this.Generate = generate;
            this.Values = values ?? Array.Empty<Value>();
        }

        public Case(TypeKind kind, string name, string type, bool generate = true)
            : this(kind, name, type, "public", isPartial: false, generate, null)
        {
        }

        public Case(TypeKind kind, string name, string type, params Value[] values)
            : this(kind, name, type, "public", isPartial: false, generate: true, values)
        {
        }

        public Case(TypeKind kind, string name, params Value[] values)
            : this(kind, name, name, values)
        {
        }

        public Case(string name, params Value[] values)
            : this(values.Length > 0 ? TypeKind.RecordStruct : TypeKind.Tag, name, name, values)
        {
        }
    }

    public class Value
    {
        public TypeKind Kind { get; }
        public string Name { get; }
        public string Type { get; }

        public Value(TypeKind kind, string name, string type)
        {
            this.Kind = kind;
            this.Name = name;
            this.Type = type;
        }

        public Value(string name, string type)
            : this(TypeKind.Primitive, name, type)
        {
        }
    }

    public class UnionGenerator : Generator
    {
        private readonly List<Union> _unions;
        private readonly List<Field> _fields;
        private readonly List<Field> _constructorParameters;
        private readonly List<CaseInfo> _caseInfos;
        private Union _union;
        private bool _isAllTags;
        private bool _generateCaseTypes;
        private string? _namespace;
        private readonly IReadOnlyList<string>? _usings;

        public UnionGenerator(string? namespaceName = null, IReadOnlyList<string>? usings = null, bool generateCaseTypes = true)
        {
            _namespace = namespaceName;
            _generateCaseTypes = generateCaseTypes;
            _usings = usings;
            _unions = new List<Union>();
            _fields = new List<Field>();
            _union = null!;
            _constructorParameters = new List<Field>();
            _caseInfos = new List<CaseInfo>();
        }

        public string GenerateFile(params Union[] unions)
        {
            this.WriteFile(unions);
            return this.GeneratedText;
        }

        public string GenerateFile(string typeName, params Case[] cases)
        {
            return GenerateFile(new Union(typeName, cases));
        }

        private class Field
        {
            public string Name { get; }
            public string Type { get; }

            public Field(string name, string type)
            {
                this.Name = name;
                this.Type = type;
            }
        }

        private class CaseInfo
        {
            public Case Case { get; }
            public Field Field { get; set; }
            public Dictionary<Value, Field> CaseValueToFieldMap { get; }
            public Dictionary<Field, Value> FieldToCaseValueMap { get; }

            public TypeKind Kind => this.Case.Kind;
            public bool Generate => this.Case.Generate;
            public string Name => this.Case.Name;
            public string Type => this.Case.Type;
            public string Accessibility => this.Case.Accessibilty;
            public bool IsPartial => this.Case.IsPartial;
            public IReadOnlyList<Value> Values => this.Case.Values;

            public CaseInfo(Case @case)
            {
                this.Case = @case;
                this.Field = null!;
                this.CaseValueToFieldMap = new Dictionary<Value, Field>();
                this.FieldToCaseValueMap = new Dictionary<Field, Value>();
            }
        }

        private void InitUnion(Union union)
        {
            _union = union;
            _caseInfos.Clear();
            _caseInfos.AddRange(union.Cases.Select(c => new CaseInfo(c)));
            _fields.Clear();
            _constructorParameters.Clear();
            _isAllTags = _caseInfos.All(c => c.Kind == TypeKind.Tag);

            var usedFields = new HashSet<Field>();

            // figure out what fields all the case values are mapping to
            foreach (var caseInfo in _caseInfos)
            {
                usedFields.Clear();

                if (caseInfo.Kind == TypeKind.RecordStruct
                    || caseInfo.Kind == TypeKind.Tag)
                {
                    foreach (var caseValue in caseInfo.Values)
                    {
                        var type = GetFieldType(caseValue.Kind, caseValue.Type, isCase: false);
                        var field = GetField(caseValue.Kind, type, usedFields);

                        // init maps
                        AddCaseValueToFieldRelation(caseInfo, caseValue, field);
                    }
                }
                else
                {
                    var type = GetFieldType(caseInfo.Kind, caseInfo.Type, isCase: true);
                    var field = GetField(caseInfo.Kind, type, usedFields);
                    caseInfo.Field = field;
                }
            }
        }

        private string GetFieldType(TypeKind kind, string type, bool isCase)
        {
            switch (kind)
            {
                case TypeKind.Class:
                case TypeKind.Interface:
                    return "object";
                case TypeKind.Struct:
                    return isCase ? "object" : type;
                case TypeKind.Primitive:
                    return GetFieldTypeFromPrimitive(type);
                case TypeKind.Tag:
                    return "Tag";
                default:
                    return type;
            }
        }

        private string GetFieldTypeFromPrimitive(string primitiveType)
        {
            switch (primitiveType)
            {
                case "string":
                    return "object";
                default:
                    return primitiveType;
            }
        }

        private void AddCaseValueToFieldRelation(CaseInfo caseInfo, Value caseValue, Field field)
        {
            if (!caseInfo.CaseValueToFieldMap.ContainsKey(caseValue))
            {
                caseInfo.CaseValueToFieldMap.Add(caseValue, field);
            }

            if (!caseInfo.FieldToCaseValueMap.ContainsKey(field))
            {
                caseInfo.FieldToCaseValueMap.Add(field, caseValue);
            }
        }

        private Field GetField(TypeKind kind, string type, HashSet<Field> usedFields)
        {
            // find next unused field with same type
            var field = _fields.FirstOrDefault(f => f.Type == type && !usedFields.Contains(f));

            // if no such field exists, add a new one
            if (field == null)
            {
                var index = _fields.Count;
                _fields.Add(field = new Field("_value" + index, type));
                _constructorParameters.Add(new Field("value" + index, type));
            }

            // mark it as used for this case
            usedFields.Add(field);

            return field;
        }

        private void WriteFile(IReadOnlyList<Union> unions)
        {
            WriteLine("using System;");
            WriteLine("using UnionTypes;");

            if (_usings != null)
            {
                foreach (var uzing in _usings)
                {
                    var uz = (uzing.StartsWith("using") ? uzing : $"using {uzing}")
                           + (uzing.EndsWith(";") ? "" : ";");

                    if (uz != "using System;"
                        && uz != "using UnionTypes;")
                    {
                        WriteLine(uz);
                    }
                }
            }

            WriteLine("#nullable enable");
            WriteLine();

            if (string.IsNullOrEmpty(_namespace))
            {
                WriteUnions(unions);
            }
            else
            {
                WriteLine($"namespace {_namespace}");
                WriteBraceNested(() =>
                {
                    WriteUnions(unions);
                });
            }
        }

        private void WriteUnions(IReadOnlyList<Union> unions)
        {
            _unions.Clear();
            _unions.AddRange(unions);

            var lastUnion = _unions.LastOrDefault();
            foreach (var union in _unions)
            {
                WriteUnion(union);

                if (union != lastUnion)
                    Console.WriteLine();
            }
        }

        private void WriteUnion(Union union)
        {
            InitUnion(union);

            var oneOf = _isAllTags ? "" : ", IOneOf";

            WriteLine($"{_union.Accessibility} partial struct {_union.TypeName} : IEquatable<{_union.TypeName}>{oneOf}");
            WriteBraceNested(() =>
            {
                WriteLineSeparated(
                    WriteTagDeclaration,
                    WriteFields,
                    WriteConstructor,
                    WriteCaseCreateMethods,
                    WriteCreateOfT,
                    WriteConvert,
                    WriteIsProperites,
                    WriteTryGetCaseMethods,
                    WriteTryGetCaseValueMethods,
                    WriteGetMethods,
                    WriteOneOfMethods,
                    WriteImplicitCastOperators,
                    WriteExplicitCastOperators,
                    WriteEquatableEquals,
                    WriteObjectEquals,
                    WriteGetHashCode,
                    WriteEqualityOperators,
                    WriteToString,
                    WriteCaseTypes
                    );
            });
        }

        private void WriteTagDeclaration()
        {
            WriteLine("private enum Tag");
            WriteBraceNested(() =>
            {
                for (int i = 0; i < _caseInfos.Count; i++)
                {
                    WriteLine($"{_caseInfos[i].Name} = {i + 1},");
                }
            });
        }

        private void WriteFields()
        {
            WriteLine("private readonly Tag _tag;");

            foreach (var field in _fields)
            {
                WriteLine($"private readonly {field.Type} {field.Name};");
            }
        }

        private void WriteConstructor()
        {
            var parameters = new List<string>();
            parameters.Add("Tag tag");
            parameters.AddRange(_constructorParameters.Select(p => $"{p.Type} {p.Name}"));
            var parameterList = string.Join(", ", parameters);

            WriteLine($"private {_union.Name}({parameterList})");
            WriteBraceNested(() =>
            {
                WriteLine("_tag = tag;");

                for (int i = 0; i < _fields.Count; i++)
                {
                    WriteLine($"{_fields[i].Name} = {_constructorParameters[i].Name};");
                }
            });
        }

        private void WriteCaseCreateMethods()
        {
            var args = new List<string>();

            foreach (var caseInfo in _caseInfos)
            {
                args.Clear();
                args.Add($"Tag.{caseInfo.Name}");

                switch (caseInfo.Kind)
                {
                    case TypeKind.RecordStruct:
                        args.AddRange(_fields.Select(f => caseInfo.FieldToCaseValueMap.TryGetValue(f, out var cv) ? $"value.{cv.Name}" : "default!"));
                        break;
                    case TypeKind.Class:
                    case TypeKind.Interface:
                    case TypeKind.Struct:
                    case TypeKind.Primitive:
                        args.AddRange(_fields.Select(f => f == caseInfo.Field ? "value" : "default!"));
                        break;
                    case TypeKind.Tag:
                        args.AddRange(_fields.Select(f => caseInfo.FieldToCaseValueMap.TryGetValue(f, out var cv) ? $"{LowerName(cv.Name)}" : "default!"));
                        break;
                }

                string argsList = string.Join(", ", args);
                var partiality = caseInfo.IsPartial ? "partial " : "";

                if (caseInfo.Kind == TypeKind.Tag)
                {
                    // create from values
                    var paramList = string.Join(", ", caseInfo.Values.Select(v => $"{v.Type} {LowerName(v.Name)}"));
                    WriteLine($"public static {partiality}{_union.TypeName} Create{caseInfo.Name}({paramList}) => new {_union.TypeName}({argsList});");

                    if (caseInfo.Values.Count == 0)
                    {
                        // also tag field
                        WriteLine($"public static readonly {_union.TypeName} {caseInfo.Name} = new {_union.TypeName}({argsList});");
                    }
                }
                else
                {
                    // create from case type
                    WriteLine($"{caseInfo.Accessibility} static {partiality}{_union.TypeName} Create{caseInfo.Name}({caseInfo.Type} value) => new {_union.TypeName}({argsList});");

                    if (caseInfo.Kind == TypeKind.RecordStruct)
                    {
                        // create from values too
                        var valuesParamList = string.Join(", ", caseInfo.Values.Select(v => $"{v.Type} {LowerName(v.Name)}"));
                        var valuesArgsList = string.Join(", ", caseInfo.Values.Select(v => LowerName(v.Name)));
                        WriteLine($"public static {_union.TypeName} Create{caseInfo.Name}({valuesParamList}) => Create{caseInfo.Name}(new {caseInfo.Type}({valuesArgsList}));");
                    }
                }
            }
        }

        private void WriteCreateOfT()
        {
            if (_isAllTags)
                return;

            WriteLine($"public static {_union.TypeName} Create<TValue>(TValue value)");
            WriteBraceNested(() =>
            {
                WriteLine("switch (value)");
                WriteBraceNested(() =>
                {
                    foreach (var caseInfo in _caseInfos)
                    {
                        if (caseInfo.Kind != TypeKind.Tag)
                        {
                            WriteLine($"case {caseInfo.Type} c_{caseInfo.Name}: return Create{caseInfo.Name}(c_{caseInfo.Name});");
                        }
                    }
                });
                WriteLine();

                WriteLine("throw new InvalidCastException();");
            });
        }

        private void WriteConvert()
        {
            WriteLine($"public static {_union.TypeName} Convert<TOneOf>(TOneOf oneOf) where TOneOf : IOneOf");
            WriteBraceNested(() =>
            {
                WriteLine($"if (oneOf is {_union.TypeName} me) return me;");

                for (int i = 0; i < _caseInfos.Count; i++)
                {
                    var caseInfo = _caseInfos[i];
                    if (caseInfo.Kind != TypeKind.Tag)
                    {
                        WriteLine($"if (oneOf.TryGet(out {caseInfo.Type} value{i})) return Create{caseInfo.Name}(value{i});");
                    }
                }

                WriteLine("throw new InvalidCastException();");
            });
        }

        private void WriteIsProperites()
        {
            foreach (var caseInfo in _caseInfos)
            {
                WriteLine($"public bool Is{caseInfo.Name} => _tag == Tag.{caseInfo.Name};");
            }
        }

        private string GetCaseValueFromFieldArgument(CaseInfo caseInfo, Value value)
        {
            var field = caseInfo.CaseValueToFieldMap[value];
            if (field.Type != value.Type)
            {
                return $"({value.Type}){field.Name}";
            }
            else
            {
                return field.Name;
            }
        }

        private void WriteGetMethods()
        {
            foreach (var caseInfo in _caseInfos)
            {
                if (caseInfo.Kind != TypeKind.Tag)
                {
                    WriteLine($"{caseInfo.Accessibility} {caseInfo.Type} Get{caseInfo.Name}() => TryGet{caseInfo.Name}(out var value) ? value : throw new InvalidCastException();");
                }
            }
        }

        private void WriteTryGetCaseMethods()
        {
            var first = true;
            foreach (var caseInfo in _caseInfos)
            {
                if (caseInfo.Kind != TypeKind.Tag)
                {
                    if (!first)
                        WriteLine();
                    first = false;

                    var construction = GetCaseTypeAccessExpression(caseInfo);
                    WriteLine($"{caseInfo.Accessibility} bool TryGet{caseInfo.Name}(out {caseInfo.Type} value)");
                    WriteBraceNested(() =>
                    {
                        WriteLine($"if (Is{caseInfo.Name}) {{ value = {construction}; return true; }}");
                        WriteLine("value = default!; return false;");
                    });
                }
                else if (caseInfo.Values.Count > 0)
                {
                    if (!first)
                        WriteLine();
                    first = false;

                    var paramList = string.Join(", ", caseInfo.Values.Select(cv => $"out {cv.Type} {LowerName(cv.Name)}"));

                    WriteLine($"{caseInfo.Accessibility} bool TryGet{caseInfo.Name}({paramList})");
                    WriteBraceNested(() =>
                    {
                        var assignments = string.Join("; ", caseInfo.Values.Select(v => $"{LowerName(v.Name)} = {GetCaseValueFromFieldArgument(caseInfo, v)}"));
                        var defAssignments = string.Join("; ", caseInfo.Values.Select(v => $"{LowerName(v.Name)} = default!"));
                        WriteLine($"if (Is{caseInfo.Name}) {{ {assignments}; return true; }}");
                        WriteLine($"{defAssignments}; return false;");
                    });
                }
            }
        }

        private void WriteTryGetCaseValueMethods()
        {
            var first = true;
            foreach (var caseInfo in _caseInfos)
            {
                if (caseInfo.Kind != TypeKind.Tag
                    && caseInfo.Values.Count > 0)
                {
                    if (!first)
                        WriteLine();
                    first = false;

                    var paramList = string.Join(", ", caseInfo.Values.Select(cv => $"out {cv.Type} {LowerName(cv.Name)}"));

                    WriteLine($"{caseInfo.Accessibility} bool TryGet{caseInfo.Name}Values({paramList})");
                    WriteBraceNested(() =>
                    {
                        var assignments = string.Join("; ", caseInfo.Values.Select(v => $"{LowerName(v.Name)} = {GetCaseValueFromFieldArgument(caseInfo, v)}"));
                        var defAssignments = string.Join("; ", caseInfo.Values.Select(v => $"{LowerName(v.Name)} = default!"));
                        WriteLine($"if (Is{caseInfo.Name}) {{ {assignments}; return true; }}");
                        WriteLine($"{defAssignments}; return false;");
                    });
                }
            }
        }

        /// <summary>
        /// Gets the text of an expression that accesses the case type from the union's fields.
        /// </summary>
        private string GetCaseTypeAccessExpression(CaseInfo caseInfo)
        {
            switch (caseInfo.Kind)
            {
                case TypeKind.RecordStruct:
                    // construct the record struct case type from the value fields
                    var argList = string.Join(", ", caseInfo.Values.Select(v => GetCaseValueFromFieldArgument(caseInfo, v)));
                    return $"new {caseInfo.Type}({argList})";
                case TypeKind.Class:
                case TypeKind.Interface:
                case TypeKind.Struct:
                case TypeKind.Primitive:
                    // pull the reference type from a single field.
                    return $"({caseInfo.Type}){caseInfo.Field.Name}";
                case TypeKind.Tag:
                default:
                    return ""; // no type
            }
        }

        private void WriteOneOfMethods()
        {
            // all tags has no case types, so cannot implement IOneOf
            if (_isAllTags)
                return;

            // Is<T>
            WriteLine("public bool Is<TType>()");
            WriteBraceNested(() =>
            {
                Write("switch (_tag)");
                WriteBraceNested(() =>
                {
                    foreach (var caseInfo in _caseInfos)
                    {
                        Write($"case Tag.{caseInfo.Name}: ");
                        switch (caseInfo.Kind)
                        {
                            case TypeKind.RecordStruct:
                                WriteLineNested($"return typeof(TType) == typeof({caseInfo.Type});");
                                break;
                            case TypeKind.Class:
                            case TypeKind.Interface:
                            case TypeKind.Struct:
                            case TypeKind.Primitive:
                                WriteLineNested($"{caseInfo.Field.Name} is TType");
                                break;
                        }
                    }
                    WriteLine("default: return false;");
                });
            });
            WriteLine();

            // TryGet<T>()
            WriteLine("public bool TryGet<TType>(out TType value)");
            WriteBraceNested(() =>
            {
                Write("switch (_tag)");
                WriteBraceNested(() =>
                {
                    foreach (var caseInfo in _caseInfos)
                    {
                        if (caseInfo.Kind != TypeKind.Tag)
                        {
                            WriteLine($"case Tag.{caseInfo.Name}: ");
                            WriteLineNested($"if (TryGet{caseInfo.Name}(out {caseInfo.Type} c_{caseInfo.Name}) && c_{caseInfo.Name} is TType t_{caseInfo.Name}) {{ value = t_{caseInfo.Name}; return true; }}");
                            WriteLineNested("break;");
                        }
                    }
                });

                WriteLine("value = default!;");
                WriteLine("return false;");
            });
            WriteLine();

            // Get<T>()
            WriteLine($"public TType Get<TType>() => TryGet<TType>(out TType t) ? t : throw new InvalidCastException();");
        }

        private void WriteImplicitCastOperators()
        {
            // implicit cast value to union
            foreach (var caseInfo in _caseInfos)
            {
                if (caseInfo.Kind != TypeKind.Tag
                    && caseInfo.Kind != TypeKind.Interface)
                {
                    WriteLine($"public static implicit operator {_union.TypeName}({caseInfo.Type} value) => Create{caseInfo.Name}(value);");
                }
            }
        }

        private void WriteExplicitCastOperators()
        {
            // explicit cast union to value
            foreach (var caseInfo in _caseInfos)
            {
                if (caseInfo.Kind != TypeKind.Tag
                    && caseInfo.Kind != TypeKind.Interface)
                {
                    WriteLine($"public static explicit operator {caseInfo.Type} ({_union.TypeName} union) => union.Get{caseInfo.Name}();");
                }
            }
        }

        private void WriteEquatableEquals()
        {
            // IEquatable<Union>.Equals
            WriteLine($"public bool Equals({_union.TypeName} other)");
            WriteBraceNested(() =>
            {
                if (_isAllTags)
                {
                    WriteLine("return _tag == other._tag;");
                }
                else
                {
                    WriteLine("if (_tag != other._tag) return false;");
                    WriteLine();
                    WriteLine("switch (_tag)");
                    WriteBraceNested(() =>
                    {
                        foreach (var caseInfo in _caseInfos)
                        {
                            WriteLine($"case Tag.{caseInfo.Name}:");
                            if (caseInfo.Kind == TypeKind.Tag)
                            {
                                WriteLineNested($"return true;");
                            }
                            else
                            {
                                WriteLineNested($"return Get{caseInfo.Name}().Equals(other.Get{caseInfo.Name}());");
                            }
                        }

                        WriteLine("default:");
                        WriteLineNested("throw new InvalidOperationException();");
                    });
                }
            });
        }

        private void WriteObjectEquals()
        {
            // Equals<TValue> 
            WriteLine("public bool Equals<TValue>(TValue value)");
            WriteBraceNested(() =>
            {
                WriteLine("switch (value)");
                WriteBraceNested(() =>
                {
                    WriteLine($"case {_union.TypeName} union:");
                    WriteLineNested("return this.Equals(union);");

                    foreach (var caseInfo in _caseInfos)
                    {
                        if (caseInfo.Kind != TypeKind.Tag)
                        {
                            WriteLine($"case {caseInfo.Type} v_{LowerName(caseInfo.Name)}:");
                            WriteLineNested($"return _tag == Tag.{caseInfo.Name} && value.Equals(Get{caseInfo.Name}());");
                        }
                    }

                    if (!_isAllTags)
                    {
                        // cannot match other unions if this one does not have any case types.
                        WriteLine("case IOneOf oneOf:");
                        WriteLineNested("return Equals(oneOf.Get<object>());");
                    }

                    WriteLine("default:");
                    WriteLineNested("return false;");
                });
            });
            WriteLine();

            // object.Equals
            WriteLine("public override bool Equals(object? other)");
            WriteBraceNested(() =>
            {
                WriteLine("return other is object obj && Equals<object>(obj);");
            });
        }

        private void WriteGetHashCode()
        {
            // object.GetHashCode()
            WriteLine("public override int GetHashCode()");
            WriteBraceNested(() =>
            {
                if (_isAllTags)
                {
                    WriteLine("return (int)_tag;");
                }
                else
                {
                    WriteLine("switch (_tag)");
                    WriteBraceNested(() =>
                    {
                        foreach (var caseInfo in _caseInfos)
                        {
                            WriteLine($"case Tag.{caseInfo.Name}:");

                            if (caseInfo.Kind == TypeKind.Tag)
                            {
                                WriteLineNested($"return (int)_tag;");
                            }
                            else
                            {
                                WriteLineNested($"return Get{caseInfo.Name}().GetHashCode();");
                            }
                        }

                        WriteLine("default:");
                        WriteLineNested("return 0;");
                    });
                }
            });
        }

        private void WriteEqualityOperators()
        {
            WriteLine($"public static bool operator == ({_union.TypeName} left, {_union.TypeName} right) =>");
            WriteLineNested($"left.Equals(right);");
            WriteLine();

            WriteLine($"public static bool operator != ({_union.TypeName} left, {_union.TypeName} right) =>");
            WriteLineNested($"!left.Equals(right);");
        }

        private void WriteToString()
        {
            // object.ToString()
            WriteLine("public override string ToString()");
            WriteBraceNested(() =>
            {
                if (_isAllTags)
                {
                    WriteLine("return _tag.ToString();");
                }
                else
                {
                    WriteLine("switch (_tag)");
                    WriteBraceNested(() =>
                    {
                        foreach (var caseInfo in _caseInfos)
                        {
                            WriteLine($"case Tag.{caseInfo.Name}:");
                            if (caseInfo.Kind == TypeKind.Tag)
                            {
                                WriteLineNested($"return _tag.ToString();");
                            }
                            else
                            {
                                WriteLineNested($"return Get{caseInfo.Name}().ToString();");
                            }
                        }

                        WriteLine("default:");
                        WriteLineNested("return \"\";");
                    });
                }
            });
        }

        private void WriteCaseTypes()
        {
            if (!_generateCaseTypes)
                return;

            foreach (var caseInfo in _caseInfos)
            {
                if (caseInfo.Generate)
                {
                    var propList = string.Join(", ", caseInfo.Values.Select(v => $"{v.Type} {v.Name}"));
                    switch (caseInfo.Kind)
                    {
                        case TypeKind.RecordStruct:
                            WriteLine($"{caseInfo.Accessibility} record struct {caseInfo.Type}({propList});");
                            break;
                        case TypeKind.Class:
                            WriteLine($"{caseInfo.Accessibility} record {caseInfo.Type}({propList});");
                            break;
                    }
                }
            }
        }
    }

#if !T4
}
#endif
// #>