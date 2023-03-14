// <#+
#if !T4
namespace UnionTypes.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
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

    public enum CaseKind
    {
        /// <summary>
        /// The case is a single unique type across all cases
        /// </summary>
        Type,

        /// <summary>
        /// The case is a tag and optional parameters
        /// </summary>
        Tag
    }

    public enum ParameterKind
    {
        /// <summary>
        /// The kind is unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// A tuple containing one or more values that can be decomposed
        /// </summary>
        Tuple,

        /// <summary>
        /// struct containing one or more values that can be decomposed
        /// </summary>
        RecordStruct,

        /// <summary>
        /// struct containing only reference type members
        /// </summary>
        OverlappableRefStruct,

        /// <summary>
        /// struct containing only overlappable value type members
        /// </summary>
        OverlappableValStruct,

        /// <summary>
        /// Just a single primitive value type
        /// </summary>
        PrimitiveStruct,

        /// <summary>
        /// class containing zero or more values
        /// </summary>
        Class,

        /// <summary>
        /// An interface
        /// </summary>
        Interface,

        /// <summary>
        /// struct containing one or more reference values and one or more value-type values
        /// </summary>
        NonOverlappableStruct,

        /// <summary>
        /// A type parameter
        /// </summary>
        TypeParameter, 

        /// <summary>
        /// A ref constrained type parameter
        /// </summary>
        TypeParameter_RefConstrained
    }

    public class Case
    {
        public CaseKind Kind { get; }
        public string Name { get; }
        public string Accessibilty { get; }
        public bool IsPartial { get; }
        public string FactoryName { get; }
        public IReadOnlyList<CaseParameter> Parameters { get; }
        public bool GenerateCaseType { get; }

        public Case(
            CaseKind kind, 
            string name,
            string accessibility, 
            bool isPartial, 
            string factoryName, 
            bool generateCaseType, 
            IReadOnlyList<CaseParameter>? parameters)
        {
            this.Kind = kind;
            this.Name = name;
            this.Accessibilty = accessibility ?? "";
            this.IsPartial = isPartial;
            this.FactoryName = factoryName;
            this.GenerateCaseType = generateCaseType;
            this.Parameters = parameters ?? Array.Empty<CaseParameter>();
        }
    }

    public class CaseParameter
    {
        public ParameterKind Kind { get; }
        public string Name { get; }
        public string Type { get; }
        public IReadOnlyList<CaseParameter> NestedParameters { get; }

        public CaseParameter(ParameterKind kind, string name, string type, IReadOnlyList<CaseParameter>? nestedParameters = null)
        {
            this.Kind = kind;
            this.Name = name;
            this.Type = type;
            this.NestedParameters = nestedParameters ?? Array.Empty<CaseParameter>();
        }
    }

    public class UnionGenerator : Generator
    {
        private readonly List<Union> _unions;
        private readonly List<CaseInfo> _caseInfos;
        private Union _union;
        private bool _isAllTags;       
        private bool _hasOverlappingRefData;
        private bool _hasOverlappingValData;
        private List<Field> _nonOverlappingFields;
        private bool _generateCaseTypes;
        private string? _namespace;
        private readonly IReadOnlyList<string>? _usings;

        public UnionGenerator(string? namespaceName = null, IReadOnlyList<string>? usings = null, bool generateCaseTypes = true)
        {
            _namespace = namespaceName;
            _generateCaseTypes = generateCaseTypes;
            _usings = usings;
            _unions = new List<Union>();
            _union = null!;
            _caseInfos = new List<CaseInfo>();
            _nonOverlappingFields = new List<Field>();
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
            public string ConstructorArg { get; }

            public Field(string fieldName, string type, string constructorArg)
            {
                this.Name = fieldName;
                this.Type = type;
                this.ConstructorArg = constructorArg;
            }
        }

        private class CaseInfo
        {
            public Case Case { get; }
            public IReadOnlyList<CaseParameterInfo> Parameters { get; }
            public IReadOnlyList<CaseParameterInfo> OverlappingRefParameters { get; }
            public IReadOnlyList<CaseParameterInfo> OverlappingValParameters { get; }
            public IReadOnlyList<CaseParameterInfo> NonOverlappingParameters { get; }

            public CaseKind Kind => this.Case.Kind;
            public bool GenerateCaseType => this.Case.GenerateCaseType;
            public string Name => this.Case.Name;
            public string Accessibility => this.Case.Accessibilty;
            public bool IsPartial => this.Case.IsPartial;
            public string FactoryName => this.Case.FactoryName;

            public CaseInfo(
                Case unionCase, 
                IReadOnlyList<CaseParameterInfo> parameters, 
                IReadOnlyList<CaseParameterInfo> overlappingRefParameters,
                IReadOnlyList<CaseParameterInfo> overlappingValParameters,
                IReadOnlyList<CaseParameterInfo> nonOverlappingParameters)
            {
                this.Case = unionCase;
                this.Parameters = parameters;
                this.OverlappingRefParameters = overlappingRefParameters;
                this.OverlappingValParameters = overlappingValParameters;
                this.NonOverlappingParameters = nonOverlappingParameters;
            }

            public bool TryGetNonOverlappingParameter(Field field, out CaseParameterInfo param)
            {
                param = this.NonOverlappingParameters.FirstOrDefault(p => p.Field == field);
                return param != null;
            }
        }

        private class CaseParameterInfo
        {
            public CaseParameter Parameter { get; }
            public string? PathFromFactoryArg { get; }
            public string? PathFromField { get; }
            public Field? Field { get; }
            public IReadOnlyList<CaseParameterInfo> NestedParameters { get; }

            public CaseParameterInfo(
                CaseParameter parameter,
                Field? field,
                string? pathFromFactoryArg, 
                string? pathFromField,
                IReadOnlyList<CaseParameterInfo>? nestedParameters)
            {
                this.Parameter = parameter;
                this.Field = field;
                this.PathFromFactoryArg = pathFromFactoryArg;
                this.PathFromField = pathFromField;
                this.NestedParameters = nestedParameters ?? Array.Empty<CaseParameterInfo>();
            }

            public ParameterKind Kind => this.Parameter.Kind;
            public ParameterCategory Category => GetParameterCategory(this.Kind);
            public string Name => this.Parameter.Name;
            public string Type => this.Parameter.Type;
        }

        private static ParameterCategory GetParameterCategory(ParameterKind kind)
        {
            switch (kind)
            {
                case ParameterKind.TypeParameter:
                case ParameterKind.NonOverlappableStruct:
                case ParameterKind.Unknown:
                    return ParameterCategory.NonOverlappable;

                case ParameterKind.RecordStruct:
                case ParameterKind.Tuple:
                    return ParameterCategory.Decomposable;

                case ParameterKind.Class:
                case ParameterKind.Interface:
                case ParameterKind.TypeParameter_RefConstrained:
                case ParameterKind.OverlappableRefStruct:
                    return ParameterCategory.OverlappableRefData;

                case ParameterKind.PrimitiveStruct:
                case ParameterKind.OverlappableValStruct:
                    return ParameterCategory.OverlappableValData;

                default:
                    throw new NotImplementedException();
            }
        }

        private static Field RefDataField = new Field("_refData", "RefData", "refData");
        private static Field ValDataField = new Field("_valData", "ValData", "valData");

        private void InitUnion(Union union)
        {
            _union = union;
            _caseInfos.Clear();
            _hasOverlappingRefData = false;
            _hasOverlappingValData = false;
            _nonOverlappingFields.Clear();

            var overlappingRefParameters = new List<CaseParameterInfo>();
            var overlappingValParameters = new List<CaseParameterInfo>();
            var nonOverlappingParameters = new List<CaseParameterInfo>();

            foreach (var unionCase in union.Cases)
            {
                _caseInfos.Add(InitCase(unionCase));
            }

            _isAllTags = _caseInfos.All(c => c.Kind == CaseKind.Tag);

            CaseInfo InitCase(Case unionCase)
            {
                overlappingRefParameters.Clear();
                overlappingValParameters.Clear();
                nonOverlappingParameters.Clear();

                List<CaseParameterInfo> caseParams = new List<CaseParameterInfo>();
                foreach (var param in unionCase.Parameters)
                {
                    var caseParam = InitCaseParameter(
                            unionCase, param,
                            pathFromField: "",
                            pathFromFactoryArg: "",
                            isRoot: true);
                    Declare(caseParam);
                    caseParams.Add(caseParam);
                }

                var info = new CaseInfo(
                    unionCase, 
                    caseParams, 
                    overlappingRefParameters.ToList(), 
                    overlappingValParameters.ToList(), 
                    nonOverlappingParameters.ToList());

                _hasOverlappingRefData |= overlappingRefParameters.Count > 0;
                _hasOverlappingValData |= overlappingValParameters.Count > 0;

                return info;
            }

            void Declare(CaseParameterInfo param)
            {
                switch (GetParameterCategory(param.Kind))
                {
                    case ParameterCategory.OverlappableRefData:
                        overlappingRefParameters.Add(param);
                        break;
                    case ParameterCategory.NonOverlappable:
                        nonOverlappingParameters.Add(param);
                        break;
                    case ParameterCategory.OverlappableValData:
                        overlappingValParameters.Add(param);
                        break;
                }
            }

            CaseParameterInfo InitCaseParameter(
                Case unionCase, CaseParameter caseParam, string pathFromField, string pathFromFactoryArg, bool isRoot)
            {
                Field? field = null;
                List<CaseParameterInfo>? nestedParameters = null;

                pathFromField = CombineName(pathFromField, caseParam.Name);
                pathFromFactoryArg = CombinePath(pathFromFactoryArg, caseParam.Name);

                switch (GetParameterCategory(caseParam.Kind))
                {
                    case ParameterCategory.Decomposable:
                        nestedParameters = new List<CaseParameterInfo>(caseParam.NestedParameters.Count);
                        foreach (var np in caseParam.NestedParameters)
                        {
                            var newCaseParamInfo = InitCaseParameter(unionCase, np, pathFromField, pathFromFactoryArg, isRoot: false);
                            Declare(newCaseParamInfo);
                            nestedParameters.Add(newCaseParamInfo);
                        }
                        break;

                    case ParameterCategory.OverlappableValData:
                        pathFromField = CombinePath(ValDataField.Name, unionCase.Name, pathFromField);
                        break;

                    case ParameterCategory.OverlappableRefData:
                        pathFromField = CombinePath(RefDataField.Name, unionCase.Name, pathFromField);
                        break;

                    case ParameterCategory.NonOverlappable:
                        var constructorArg = "field" + _nonOverlappingFields.Count;
                        var fieldName = "_" + constructorArg;
                        pathFromField = fieldName;
                        field = new Field(fieldName, caseParam.Type, constructorArg);
                        _nonOverlappingFields.Add(field);
                        break;

                    default:
                        throw new NotImplementedException();
                }

                return new CaseParameterInfo(caseParam, field, pathFromFactoryArg, pathFromField, nestedParameters);
            }
        }

        private static string CombinePath(params string[] parts)
        {
            return string.Join(".", parts.Where(p => !string.IsNullOrEmpty(p)));
        }

        private static string CombineName(params string[] parts)
        {
            return string.Join("_", parts.Where(p => !string.IsNullOrEmpty(p)));
        }


        public enum ParameterCategory
        {
            OverlappableRefData,
            Decomposable,
            OverlappableValData,
            NonOverlappable
        }

        private void WriteFile(IReadOnlyList<Union> unions)
        {
            WriteLine("using System;");
            WriteLine("using System.Runtime.InteropServices;");
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

            var oneOf = _isAllTags ? "" : ", ITypeUnion";

            WriteLine($"{_union.Accessibility} partial struct {_union.TypeName} : IEquatable<{_union.TypeName}>{oneOf}");
            WriteBraceNested(() =>
            {
                WriteLineSeparated(
                    WriteTagDeclaration,
                    WriteFields,
                    WriteDataTypes,
                    WriteConstructor,
                    WriteFactoryMethods,
                    WriteTryConvert,
                    WriteConvert,
                    WriteIsProperites,
                    WriteTryGetMethods,
                    WriteGetMethods,
                    WriteGetOrDefaultMethods,
                    WriteCreateOfT, 
                    WriteITypeUnionMethods,
                    WriteImplicitCastOperators,
                    WriteExplicitCastOperators,
                    WriteEquatableEquals,
                    WriteObjectEquals,
                    WriteGetHashCode,
                    WriteEqualityOperators,
                    WriteToString,
                    WriteGeneratedCaseTypes
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
            
            if (_hasOverlappingRefData)
                WriteLine($"private readonly {RefDataField.Type} {RefDataField.Name};");
            
            if (_hasOverlappingValData)
                WriteLine($"private readonly {ValDataField.Type} {ValDataField.Name};");

            if (_nonOverlappingFields.Count > 0)
            {
                foreach (var field in _nonOverlappingFields)
                {
                    WriteLine($"private readonly {field.Type} {field.Name};");
                }
            }
        }

        private void WriteDataTypes()
        {
            WriteLineSeparatedBlocks(() =>
            {
                if (_hasOverlappingRefData)
                {
                    WriteBlock(() => WriteRefDataType());
                }

                if (_hasOverlappingValData)
                {
                    WriteBlock(() => WriteValDataType());
                }
            });

            void WriteRefDataType()
            {
                WriteLine("[StructLayout(LayoutKind.Explicit)]");
                WriteLine($"private struct {RefDataField.Type}");
                WriteBraceNested(() =>
                {
                    WriteLineSeparatedBlocks(() =>
                    {
                        WriteBlock(() =>
                        {
                            foreach (var caseInfo in _caseInfos)
                            {
                                if (caseInfo.OverlappingRefParameters.Count > 0)
                                {
                                    WriteLine("[FieldOffset(0)]");
                                    WriteLine($"public {caseInfo.Name}_Data {caseInfo.Name};");
                                }
                            }
                        });

                        WriteCaseDataTypes();
                    });
                });

                void WriteCaseDataTypes()
                {
                    foreach (var caseInfo in _caseInfos)
                    {
                        if (caseInfo.OverlappingRefParameters.Count == 0)
                            continue;

                        WriteBlock(() =>
                        {
                            WriteLine($"public struct {caseInfo.Name}_Data");
                            WriteBraceNested(() =>
                            {
                                foreach (var param in caseInfo.Parameters)
                                {
                                    WriteFields(param, param.Name);
                                }
                            });
                        });
                    }

                    void WriteFields(CaseParameterInfo param, string name)
                    {
                        switch (param.Category)
                        {
                            case ParameterCategory.Decomposable:
                                foreach (var np in param.NestedParameters)
                                {
                                    WriteFields(np, CombineName(name, np.Name));
                                }
                                break;

                            case ParameterCategory.OverlappableRefData:
                                WriteLine($"public {param.Type} {name};");
                                break;

                            case ParameterCategory.OverlappableValData:
                            case ParameterCategory.NonOverlappable:
                                break;

                            default:
                                throw new NotImplementedException();
                        }
                    }
                }
            }

            void WriteValDataType()
            {
                WriteLine("[StructLayout(LayoutKind.Explicit)]");
                WriteLine($"private struct {ValDataField.Type}");
                WriteBraceNested(() =>
                {
                    WriteLineSeparatedBlocks(() =>
                    {
                        WriteBlock(() =>
                        {
                            foreach (var caseInfo in _caseInfos)
                            {
                                if (caseInfo.OverlappingValParameters.Count > 0)
                                {
                                    WriteLine("[FieldOffset(0)]");
                                    WriteLine($"public {caseInfo.Name}_Data {caseInfo.Name};");
                                }
                            }
                        });

                        WriteCaseDataTypes();
                    });

                    void WriteCaseDataTypes()
                    {
                        foreach (var caseInfo in _caseInfos)
                        {
                            if (caseInfo.OverlappingValParameters.Count == 0)
                                continue;

                            WriteBlock(() =>
                            {
                                WriteLine($"public struct {caseInfo.Name}_Data");
                                WriteBraceNested(() =>
                                {
                                    foreach (var param in caseInfo.Parameters)
                                    {
                                        WriteFields(param, param.Name);
                                    }
                                });
                            });
                        }

                        void WriteFields(CaseParameterInfo param, string name)
                        {
                            switch (param.Category)
                            {
                                case ParameterCategory.Decomposable:
                                    foreach (var np in param.NestedParameters)
                                    {
                                        WriteFields(np, CombineName(name, np.Name));
                                    }
                                    break;

                                case ParameterCategory.OverlappableValData:
                                    WriteLine($"public {param.Type} {name};");
                                    break;

                                case ParameterCategory.OverlappableRefData:
                                case ParameterCategory.NonOverlappable:
                                    break;

                                default:
                                    throw new NotImplementedException();
                            }
                        }
                    }
                });
            }
        }

        private void WriteConstructor()
        {
            var args = new List<string>() { "Tag tag" };

            if (_hasOverlappingRefData)
                args.Add($"{RefDataField.Type} {RefDataField.ConstructorArg}");

            if (_hasOverlappingValData)
                args.Add($"{ValDataField.Type} {ValDataField.ConstructorArg}");

            foreach (var field in _nonOverlappingFields)
            {
                args.Add($"{field.Type} {field.ConstructorArg}");
            }

            var argsList = string.Join(", ", args);

            WriteLine($"private {_union.Name}({argsList})");
            WriteBraceNested(() =>
            {
                WriteLine("_tag = tag;");

                if (_hasOverlappingRefData)
                    WriteLine($"{RefDataField.Name} = {RefDataField.ConstructorArg};");

                if (_hasOverlappingValData)
                    WriteLine($"{ValDataField.Name} = {ValDataField.ConstructorArg};");

                foreach (var field in _nonOverlappingFields)
                {
                    WriteLine($"this.{field.Name} = {field.ConstructorArg};");
                }
            });
        }

        private void WriteFactoryMethods()
        {
            WriteLineSeparatedBlocks(() =>
            {
                foreach (var caseInfo in _caseInfos)
                {
                    WriteBlock(() => WriteFactoryMethod(caseInfo));
                }
            });

            void WriteFactoryMethod(CaseInfo caseInfo)
            {
                var access = string.IsNullOrEmpty(caseInfo.Accessibility) ? "public" : caseInfo.Accessibility;
                var decl = $"{access} static {(caseInfo.IsPartial ? "partial " + _union.TypeName : _union.TypeName)} {caseInfo.FactoryName}";

                switch (caseInfo.Kind)
                {
                    case CaseKind.Tag:
                        var parameters = string.Join(", ", caseInfo.Parameters.Select(p => $"{p.Type} {p.Name}"));
                        WriteFactory(parameters);
                        break;

                    case CaseKind.Type:
                        var param = caseInfo.Parameters[0];
                        WriteFactory($"{param.Type} {param.Name}");
                        break;
                }

                void WriteFactory(string parameters)
                {
                    Write($"{decl}({parameters})");
                    WriteBraceNested(() =>
                    {
                        WriteLine($"return new {_union.TypeName}(");
                        WriteNested(() =>
                        {
                            WriteCommaList(() =>
                            {
                                WriteCommaListElement(() => WriteLine($"Tag.{caseInfo.Name}"));

                                if (_hasOverlappingRefData)
                                {
                                    WriteCommaListElement(() =>
                                    {
                                        if (caseInfo.OverlappingRefParameters.Count > 0)
                                        {
                                            WriteLine($"new RefData");
                                            WriteBraceNested(() =>
                                            {
                                                Write($"{caseInfo.Name} = ");
                                                WriteRefCaseTypeConstruction(caseInfo);
                                            });
                                        }
                                        else
                                        {
                                            WriteLine("new RefData {}");
                                        }
                                    });
                                }

                                if (_hasOverlappingValData)
                                {
                                    WriteCommaListElement(() =>
                                    {
                                        if (caseInfo.OverlappingValParameters.Count > 0)
                                        {
                                            WriteLine($"new ValData");
                                            WriteBraceNested(() =>
                                            {
                                                Write($"{caseInfo.Name} = ");
                                                WriteValCaseTypeConstruction(caseInfo);
                                            });
                                        }
                                        else
                                        {
                                            WriteLine("new ValData {}");
                                        }
                                    });
                                }

                                foreach (var field in _nonOverlappingFields)
                                {
                                    WriteCommaListElement(() =>
                                    {
                                        if (caseInfo.TryGetNonOverlappingParameter(field, out var param))
                                        {
                                            WriteLine($"{field.ConstructorArg}: {param.PathFromFactoryArg}");
                                        }
                                        else
                                        {
                                            WriteLine($"{field.ConstructorArg}: default!");
                                        }
                                    });
                                }
                            });

                            WriteLine(");");
                        });
                    });
                }
            }

            void WriteRefCaseTypeConstruction(CaseInfo caseInfo)
            {
                WriteLine($"new RefData.{caseInfo.Name}_Data");
                WriteBraceNested(() =>
                {
                    WriteCommaList(() =>
                    {
                        foreach (var param in caseInfo.Parameters)
                        {
                            WriteParameter(param, param.Name);
                        }
                    });
                });

                void WriteParameter(CaseParameterInfo param, string name)
                {
                    switch (param.Category)
                    {
                        case ParameterCategory.Decomposable:
                            foreach (var np in param.NestedParameters)
                            {
                                WriteParameter(np, CombineName(name, np.Name));
                            }
                            break;

                        case ParameterCategory.OverlappableRefData:
                            WriteCommaListElement(() =>
                            {
                                WriteLine($"{name} = {param.PathFromFactoryArg}");
                            });
                            break;

                        case ParameterCategory.OverlappableValData:
                        case ParameterCategory.NonOverlappable:
                        default:
                            break;
                    }
                }
            }

            void WriteValCaseTypeConstruction(CaseInfo caseInfo)
            {
                WriteLine($"new ValData.{caseInfo.Name}_Data");
                WriteBraceNested(() =>
                {
                    WriteCommaList(() =>
                    {
                        foreach (var param in caseInfo.Parameters)
                        {
                            WriteParameter(param, param.Name);
                        }
                    });
                });

                void WriteParameter(CaseParameterInfo param, string name)
                {
                    switch (param.Category)
                    {
                        case ParameterCategory.Decomposable:
                            foreach (var np in param.NestedParameters)
                            {
                                WriteParameter(np, CombineName(name, np.Name));
                            }
                            break;

                        case ParameterCategory.OverlappableValData:
                            WriteCommaListElement(() =>
                            {
                                WriteLine($"{name} = {param.PathFromFactoryArg}");
                            });
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void WriteTryConvert()
        {
            if (_isAllTags)
                return;

            WriteLine($"public static bool TryConvert<TUnion>(TUnion union, out {_union.TypeName} converted) where TUnion : ITypeUnion");
            WriteBraceNested(() =>
            {
                WriteLine($"if (union is {_union.TypeName} me) {{ converted = me; return true; }}");

                foreach (var caseInfo in _caseInfos)
                {
                    if (caseInfo.Kind == CaseKind.Type)
                    {
                        var param = caseInfo.Parameters[0];
                        WriteLine($"if (union.TryGet(out {param.Type} v_{caseInfo.Name})) {{ converted = {caseInfo.FactoryName}(v_{caseInfo.Name}); return true; }}");
                    }
                }

                WriteLine("converted = default!; return false;");
            });
        }

        private void WriteConvert()
        {
            if (_isAllTags)
                return;

            WriteLine($"public static {_union.TypeName} Convert<TUnion>(TUnion union) where TUnion : ITypeUnion");
            WriteBraceNested(() =>
            {
                WriteLine($"return TryConvert(union, out var converted) ? converted : throw new InvalidCastException();");
            });
        }

        private void WriteIsProperites()
        {
            foreach (var caseInfo in _caseInfos)
            {
                WriteLine($"public bool Is{caseInfo.Name} => _tag == Tag.{caseInfo.Name};");
            }
        }

        private void WriteTryGetMethods()
        {
            WriteLineSeparatedBlocks(() =>
            {
                foreach (var caseInfo in _caseInfos)
                {
                    WriteBlock(() => WriteTryGetMethod(caseInfo));
                }
            });

            void WriteTryGetMethod(CaseInfo caseInfo)
            {
                switch (caseInfo.Kind)
                {
                    case CaseKind.Type:
                        var param = caseInfo.Parameters[0];
                        WriteLine($"{caseInfo.Accessibility} bool TryGet{caseInfo.Name}(out {param.Type} value)");
                        WriteBraceNested(() =>
                        {
                            WriteLine($"if (Is{caseInfo.Name})");
                            WriteBraceNested(() =>
                            {
                                Write("value = ");
                                WriteCaseAccessOrConstruction(caseInfo);
                                WriteLine(";");
                                WriteLine("return true;");
                            });
                            WriteLine("else");
                            WriteBraceNested(() =>
                            {
                                WriteLine("value = default!;");
                                WriteLine("return false;");
                            });
                        });
                        break;

                    case CaseKind.Tag:
                        if (caseInfo.Parameters.Count > 0)
                        {
                            // this is a tag case (w/ values)
                            var paramList = string.Join(", ", caseInfo.Parameters.Select(cv => $"out {cv.Type} {cv.Name}"));

                            WriteLine($"{caseInfo.Accessibility} bool TryGet{caseInfo.Name}({paramList})");
                            WriteBraceNested(() =>
                            {
                                WriteLine($"if (Is{caseInfo.Name})");
                                WriteBraceNested(() =>
                                {
                                    foreach (var param in caseInfo.Parameters)
                                    {
                                        Write($"{param.Name} = ");
                                        WriteCaseParamAccessOrConstruction(param);
                                        WriteLine(";");
                                    }
                                    WriteLine("return true;");
                                });
                                WriteLine("else");
                                WriteBraceNested(() =>
                                {
                                    foreach (var param in caseInfo.Parameters)
                                    {
                                        Write($"{param.Name} = default!;");
                                    }
                                    WriteLine("return false;");
                                });
                            });
                        }
                        else
                        {
                            // tag case (w/o values) does not get a TryGet method
                        }
                        break;
                }
            }
        }

        private void WriteGetMethods()
        {
            WriteLineSeparatedBlocks(() =>
            {
                foreach (var caseInfo in _caseInfos)
                {
                    WriteBlock(() => WriteMethod(caseInfo));
                }
            });

            void WriteMethod(CaseInfo caseInfo)
            {
                switch (caseInfo.Kind)
                {
                    case CaseKind.Type:
                        var param = caseInfo.Parameters[0];
                        WriteLine($"{caseInfo.Accessibility} {param.Type} Get{caseInfo.Name}() =>");
                        WriteLineNested($"TryGet{caseInfo.Name}(out var value) ? value : throw new InvalidCastException();");
                        break;

                    case CaseKind.Tag:
                        if (caseInfo.Parameters.Count > 1)
                        {
                            // this is a tag case (w/ more than one value)
                            var tupleType = "(" + string.Join(", ", caseInfo.Parameters.Select(c => $"{c.Type} {c.Name}")) + ")";
                            var tupleInitializer = "(" + string.Join(", ", caseInfo.Parameters.Select(c => c.Name)) + ")";
                            var outArgs = string.Join(", ", caseInfo.Parameters.Select(c => $"out var {c.Name}"));
                            WriteLine($"public {tupleType} Get{caseInfo.Name}() =>");
                            WriteLineNested($"TryGet{caseInfo.Name}({outArgs}) ? {tupleInitializer} : throw new InvalidCastException();");
                        }
                        else if (caseInfo.Parameters.Count == 1)
                        {
                            // this is a tag case (w/ only one value)
                            var valueInfo = caseInfo.Parameters[0];
                            WriteLine($"public {valueInfo.Type} Get{caseInfo.Name}() =>");
                            WriteLineNested($"TryGet{caseInfo.Name}(out var {valueInfo.Name}) ? {valueInfo.Name} : throw new InvalidCastException();");
                        }
                        else
                        {
                            // tag case with no values does not get a Get method.
                        }
                        break;
                }
            }
        }

        private void WriteGetOrDefaultMethods()
        {
            WriteLineSeparatedBlocks(() =>
            {
                foreach (var caseInfo in _caseInfos)
                {
                    WriteBlock(() => WriteMethod(caseInfo));
                }
            });

            void WriteMethod(CaseInfo caseInfo)
            {
                switch (caseInfo.Kind)
                {
                    case CaseKind.Type:
                        var param = caseInfo.Parameters[0];
                        WriteLine($"{caseInfo.Accessibility} {param.Type} Get{caseInfo.Name}OrDefault() =>");
                        WriteLineNested($"TryGet{caseInfo.Name}(out var value) ? value : default!;");
                        break;

                    case CaseKind.Tag:
                        if (caseInfo.Parameters.Count > 1)
                        {
                            // this is a tag case (w/ more than one value)
                            var tupleType = "(" + string.Join(", ", caseInfo.Parameters.Select(c => $"{c.Type} {c.Name}")) + ")";
                            var tupleInitializer = "(" + string.Join(", ", caseInfo.Parameters.Select(c => c.Name)) + ")";
                            var outArgs = string.Join(", ", caseInfo.Parameters.Select(c => $"out var {c.Name}"));
                            WriteLine($"public {tupleType} Get{caseInfo.Name}OrDefault() =>");
                            WriteLineNested($"TryGet{caseInfo.Name}({outArgs}) ? {tupleInitializer} : default!;");
                        }
                        else if (caseInfo.Parameters.Count == 1)
                        {
                            // this is a tag case (w/ only one value)
                            var valueInfo = caseInfo.Parameters[0];
                            WriteLine($"public {valueInfo.Type} Get{caseInfo.Name}OrDefault() =>");
                            WriteLineNested($"TryGet{caseInfo.Name}(out var {valueInfo.Name}) ? {valueInfo.Name} : default!;");
                        }
                        else
                        {
                            // tag case with no values does not get a Get method.
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the text of an expression that accesses the type case instance from the union's fields.
        /// </summary>
        private void WriteCaseAccessOrConstruction(CaseInfo caseInfo)
        {
            switch (caseInfo.Kind)
            {
                case CaseKind.Type:
                    WriteCaseParamAccessOrConstruction(caseInfo.Parameters[0]);
                    break;

                case CaseKind.Tag:
                    if (caseInfo.Parameters.Count == 1)
                    {
                        // treat as single parameter value
                        WriteCaseParamAccessOrConstruction(caseInfo.Parameters[0]);
                    }
                    else if (caseInfo.Parameters.Count > 1)
                    {
                        // treat as tuple of parameter values
                        WriteTupleConstruction(caseInfo.Parameters);
                    }
                    else
                    {
                        // cannot access tag members that do not have any parameters
                        throw new InvalidOperationException();
                    }
                    break;
            }
        }

        private void WriteCaseParamAccessOrConstruction(CaseParameterInfo param)
        {
            if (param.Category == ParameterCategory.Decomposable)
            {
                switch (param.Kind)
                {
                    case ParameterKind.RecordStruct:
                        WriteRecordConstruction(param);
                        break;

                    case ParameterKind.Tuple:
                        WriteTupleConstruction(param.NestedParameters);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                // pull the entire reference type case instance from a single value field
                Write(param.PathFromField!);
            }
        }

        private void WriteRecordConstruction(CaseParameterInfo param)
        {
            Write($"new {param.Type}(");
            WriteCommaList(() =>
            {
                foreach (var np in param.NestedParameters)
                {
                    WriteCommaListElement(() => WriteCaseParamAccessOrConstruction(np));
                }
            });
            Write(")");
        }

        private void WriteTupleConstruction(IReadOnlyList<CaseParameterInfo> parameters)
        {
            Write($"(");
            WriteCommaList(() =>
            {
                foreach (var np in parameters)
                {
                    WriteCommaListElement(() => WriteCaseParamAccessOrConstruction(np));
                }
            });
            Write(")");
        }

        private void WriteCreateOfT()
        {
            // cannot create from type case instance if the union has no type cases
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
                        if (caseInfo.Kind == CaseKind.Type)
                        {
                            var param = caseInfo.Parameters[0];
                            WriteLine($"case {param.Type} c_{caseInfo.Name}: return {caseInfo.FactoryName}(c_{caseInfo.Name});");
                        }
                    }
                });
                WriteLine();
                WriteLine("throw new InvalidCastException();");
            });
        }

        private void WriteITypeUnionMethods()
        {
            // cannot implement ITypeUnion if the union has no type cases
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
                        if (caseInfo.Kind == CaseKind.Type)
                        {
                            Write($"case Tag.{caseInfo.Name}: ");
                            var param = caseInfo.Parameters[0];
                            switch (param.Kind)
                            {
                                case ParameterKind.Class:
                                case ParameterKind.Interface:
                                case ParameterKind.TypeParameter_RefConstrained:
                                    // known reference types
                                    WriteLineNested($"return {param.PathFromField} is TType;");
                                    break;
                                default:
                                    WriteLineNested($"return typeof(TType) == typeof({param.Type});");
                                    break;
                            }
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
                        if (caseInfo.Kind == CaseKind.Type)
                        {
                            var param = caseInfo.Parameters[0];
                            WriteLine($"case Tag.{caseInfo.Name}: ");
                            WriteLineNested($"if (TryGet{caseInfo.Name}(out {param.Type} c_{caseInfo.Name}) && c_{caseInfo.Name} is TType t_{caseInfo.Name}) {{ value = t_{caseInfo.Name}; return true; }}");
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
            WriteLine();

            // Equals<TValue> 
            WriteLine("public bool Equals<TValue>(TValue value)");
            WriteBraceNested(() =>
            {
                WriteLine("switch (value)");
                WriteBraceNested(() =>
                {
                    // value is same union type as this
                    WriteLine($"case {_union.TypeName} sameUnion:");
                    WriteLineNested("return this.Equals(sameUnion);");

                    // value is one of the type cases for this union type
                    foreach (var caseInfo in _caseInfos)
                    {
                        if (caseInfo.Kind == CaseKind.Type)
                        {
                            var param = caseInfo.Parameters[0];
                            WriteLine($"case {param.Type} v_{caseInfo.Name}:");
                            WriteLineNested($"return _tag == Tag.{caseInfo.Name} && v_{caseInfo.Name}.Equals(Get{caseInfo.Name}());");
                        }
                    }

                    // value is some other type union with type cases (implements ITypeUnion)
                    if (!_isAllTags)
                    {
                        // we don't know what types the other union supports, so get value as object
                        WriteLine("case ITypeUnion otherUnion:");
                        WriteLineNested("return Equals(otherUnion.Get<object>());");
                    }

                    WriteLine("default:");
                    WriteLineNested("return false;");
                });
            });
        }

        private void WriteImplicitCastOperators()
        {
            // implicit cast value to union
            foreach (var caseInfo in _caseInfos)
            {
                // only applicable to type cases and cannot work with interface types.
                if (caseInfo.Kind == CaseKind.Type)
                {
                    var param = caseInfo.Parameters[0];
                    if (param.Kind != ParameterKind.Interface
                        && caseInfo.Accessibility == "public")
                    {
                        WriteLine($"public static implicit operator {_union.TypeName}({param.Type} value) => {caseInfo.FactoryName}(value);");
                    }
                }
            }
        }

        private void WriteExplicitCastOperators()
        {
            // explicit cast union to value
            foreach (var caseInfo in _caseInfos)
            {
                // only applicable to type cases and cannot work with interface types.
                if (caseInfo.Kind == CaseKind.Type)
                {
                    var param = caseInfo.Parameters[0];
                    if (param.Kind != ParameterKind.Interface
                        && caseInfo.Accessibility == "public")
                    {
                        WriteLine($"public static explicit operator {param.Type} ({_union.TypeName} union) => union.Get{caseInfo.Name}();");
                    }
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
                            if (caseInfo.Kind == CaseKind.Tag && caseInfo.Parameters.Count == 0)
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
            WriteLine("public override bool Equals(object? other)");
            WriteBraceNested(() =>
            {
                if (_isAllTags)
                {
                    // defer to IEquatable.Equals
                    WriteLine($"return other is {_union.TypeName} union && Equals(union);");
                }
                else
                {
                    // defer to ITypeUnion.Equals
                    WriteLine("return other is object obj && Equals<object>(obj);");
                }
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

                            if (caseInfo.Kind == CaseKind.Tag && caseInfo.Parameters.Count == 0)
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
                if (_isAllTags && _union.Cases.All(c => c.Parameters.Count == 0))
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
                            if (caseInfo.Kind == CaseKind.Tag)
                            {
                                if (caseInfo.Parameters.Count == 0)
                                {
                                    WriteLineNested($"return _tag.ToString();");
                                }
                                else if (caseInfo.Parameters.Count == 1)
                                {
                                    WriteNested(() =>
                                    {
                                        WriteLine($"return $\"{caseInfo.Name}({{Get{caseInfo.Name}()}})\";");
                                    });
                                }
                                else
                                {
                                    WriteNested(() =>
                                    {
                                        WriteLine($"var v_{caseInfo.Name} = Get{caseInfo.Name}();");
                                        var props = string.Join(", ", caseInfo.Parameters.Select(p => $"{p.Name}: {{v_{caseInfo.Name}.{p.Name}}}"));
                                        WriteLine($$"""return $"{{caseInfo.Name}}({{props}})";""");
                                    });
                                }
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

        private void WriteGeneratedCaseTypes()
        {
            if (!_generateCaseTypes)
                return;

            foreach (var caseInfo in _caseInfos)
            {
                if (caseInfo.GenerateCaseType && caseInfo.Kind == CaseKind.Type)
                {
                    var param = caseInfo.Parameters[0];

                    var propList = string.Join(", ", param.NestedParameters.Select(v => $"{v.Type} {v.Name}"));
                    switch (param.Kind)
                    {
                        case ParameterKind.RecordStruct:
                            WriteLine($"{caseInfo.Accessibility} record struct {param.Type}({propList});");
                            break;
                        case ParameterKind.Class:
                            WriteLine($"{caseInfo.Accessibility} record {param.Type}({propList});");
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