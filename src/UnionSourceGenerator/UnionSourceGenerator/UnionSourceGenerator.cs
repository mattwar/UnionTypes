using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using CATypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace UnionTypes.Generators
{
    [Generator]
    public class UnionSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var unionTypes = GetUnionTypes(context.Compilation.GlobalNamespace);

            foreach (var unionType in unionTypes)
            {
                var result = GenerateUnionType(unionType);

                if (result.Diagnostics.Count > 0)
                {
                    foreach (var dx in result.Diagnostics)
                    {
                        context.ReportDiagnostic(dx);
                    }
                }
                else
                {
                    context.AddSource($"{unionType.Name}_UnionImplementation.cs", result.Text);
                }
            }
        }

        private class GenerateResult
        {
            public string Text { get; }
            public IReadOnlyList<Diagnostic> Diagnostics { get; }

            public GenerateResult(string text, IReadOnlyList<Diagnostic> diagnostics)
            {
                Text = text;
                Diagnostics = diagnostics;
            }
        }

        private IReadOnlyList<INamedTypeSymbol> GetUnionTypes(INamespaceSymbol @namespace)
        {
            var unionTypes = new List<INamedTypeSymbol>();
            GetTypesFromNamespace(@namespace);
            return unionTypes;

            void GetTypesFromNamespace(INamespaceSymbol ns)
            {
                unionTypes.AddRange(ns.GetTypeMembers().OfType<INamedTypeSymbol>().Where(IsUnionType));
                foreach (var subns in ns.GetNamespaceMembers())
                {
                    GetTypesFromNamespace(subns);
                }
            }
        }

        private bool IsUnionType(INamedTypeSymbol symbol)
        {
            return TryGetTypeUnionAttribute(symbol, out _)
                || TryGetTagUnionAttribute(symbol, out _);
        }

        private bool TryGetTypeUnionAttribute(INamedTypeSymbol symbol, out AttributeData attribute)
        {
            return symbol.TryGetAttribute("TypeUnionAttribute", out attribute);
        }

        private bool TryGetTagUnionAttribute(INamedTypeSymbol symbol, out AttributeData attribute)
        {
            return symbol.TryGetAttribute("TagUnionAttribute", out attribute);
        }

        private GenerateResult GenerateUnionType(INamedTypeSymbol unionType)
        {
            var diagnostics = new List<Diagnostic>();

            string namespaceName = null!;
            if (unionType.ContainingNamespace != null)
            {
                namespaceName = GetNamespaceName(unionType.ContainingNamespace);
            }

            var usings = GetDeclaredUsings(unionType);
            var accessibility = GetAccessibility(unionType.DeclaredAccessibility);
            string text;

            if (TryGetTypeUnionAttribute(unionType, out var typeUnionAttribute))
            {
                var options = GetOptionsFromUnionAttribute(typeUnionAttribute);

                // get all cases declared for union type
                var cases =
                    GetTypeCasesFromCaseAttributesOnUnion(unionType, options, diagnostics)
                    .Concat(GetTypeCasesFromNestedRecords(unionType, diagnostics))
                    .Concat(GetTypeCasesFromPartialFactoryMethods(unionType, options, diagnostics))
                    .Concat(GetTypeCasesFromPartialFactoryProperties(unionType, options, diagnostics))
                    .ToArray();

                var name = unionType.Name; // name w/o type parameters or namespace
                var typeName = GetTypeShortName(unionType); // name w/o namespace

                var union = new Union(
                    UnionKind.TypeUnion,
                    name,
                    typeName,
                    accessibility,
                    cases,
                    options
                    );

                var generator = new UnionGenerator(
                    namespaceName,
                    usings
                    );

                text = generator.GenerateFile(union);
            }
            else if (TryGetTagUnionAttribute(unionType, out var tagUnionAttribute))
            {
                var options = GetOptionsFromUnionAttribute(tagUnionAttribute);

                // get all cases declared for union type
                var cases =
                    GetTagCasesFromPartialFactoryMethods(unionType, options, diagnostics)
                    .Concat(GetTagCasesFromPartialFactoryProperties(unionType, options, diagnostics))
                    .Concat(GetTagCasesFromCaseAttributesOnUnion(unionType, options, diagnostics))
                    .ToArray();

                var union = new Union(
                    UnionKind.TagUnion,
                    unionType.Name,
                    GetTypeFullName(unionType),
                    accessibility,
                    cases,
                    options
                    );

                var generator = new UnionGenerator(
                    namespaceName,
                    usings
                    );

                text = generator.GenerateFile(union);                
            }
            else
            {
                text = "";
            }

            return new GenerateResult(text, diagnostics);
        }

        /// <summary>
        /// Gets the using directives from the file that contains the type declaration.
        /// </summary>
        private IReadOnlyList<string> GetDeclaredUsings(INamedTypeSymbol type)
        {
            if (type.Locations.FirstOrDefault(loc => loc.IsInSource) is Location sourceLocation
                && sourceLocation.SourceTree is SyntaxTree sourceTree
                && sourceTree.GetRoot() is SyntaxNode root)
            {
                var usingDirectives =
                    root.DescendantNodes()
                    .OfType<UsingDirectiveSyntax>()
                    .Where(u => u.GlobalKeyword == default)
                    .ToList();

                return 
                    usingDirectives
                    .Select(uz => uz.ToString()).ToArray();
            }

            return Array.Empty<string>();
        }

        private UnionOptions GetOptionsFromUnionAttribute(AttributeData unionAttribute)
        {
            var options = UnionOptions.Default;

            if (unionAttribute.TryGetNamedArgument("ShareSameTypeFields", out var arg)
                && arg.Kind == TypedConstantKind.Primitive
                && arg.Value is bool shareSameFields)
            {
                options = options.WithShareFields(shareSameFields);
            }

            if (unionAttribute.TryGetNamedArgument("ShareReferenceFields", out arg)
                && arg.Kind == TypedConstantKind.Primitive
                && arg.Value is bool shareReferenceFields)
            {
                options = options.WithShareReferenceFields(shareReferenceFields);
            }

            if (unionAttribute.TryGetNamedArgument("OverlapStructs", out arg)
                && arg.Kind == TypedConstantKind.Primitive
                && arg.Value is bool overlapStructs)
            {
                options = options.WithOverlapStructs(overlapStructs);
            }

            if (unionAttribute.TryGetNamedArgument("OverlapForeignStructs", out arg)
                && arg.Kind == TypedConstantKind.Primitive
                && arg.Value is bool overlapForeignStructs)
            {
                options = options.WithOverlapForeignStructs(overlapForeignStructs);
            }

            if (unionAttribute.TryGetNamedArgument("DecomposeStructs", out arg)
                && arg.Kind == TypedConstantKind.Primitive
                && arg.Value is bool decomposeStructs)
            {
                options = options.WithDecomposeStructs(decomposeStructs);
            }

            if (unionAttribute.TryGetNamedArgument("DecomposeForeignStructs", out arg)
                && arg.Kind == TypedConstantKind.Primitive
                && arg.Value is bool decomposeForeignStructs)
            {
                options = options.WithDecomposeForeignStructs(decomposeForeignStructs);
            }

            if (unionAttribute.TryGetNamedArgument("GenerateEquality", out arg)
                && arg.Kind == TypedConstantKind.Primitive
                && arg.Value is bool generateEquality)
            {
                options = options.WithGenerateEquality(generateEquality);
            }

            if (unionAttribute.TryGetNamedArgument("TagTypeName", out arg)
                && arg.Kind == TypedConstantKind.Primitive
                && arg.Value is string tagTypeName)
            {
                options = options.WithTagTypeName(tagTypeName);
            }

            if (unionAttribute.TryGetNamedArgument("TagPropertyName", out arg)
                && arg.Kind == TypedConstantKind.Primitive
                && arg.Value is string tagPropertyName)
            {
                options = options.WithTagPropertyName(tagPropertyName);
            }

            return options;
        }

        private void GetUnionCaseData(
            AttributeData attr, 
            out string? name, 
            out int value, 
            out ITypeSymbol? type, 
            out bool isSingleton,
            out string? factoryName,
            out bool? factoryIsProperty,
            out string? accessorName)
        {
            name = attr.TryGetNamedArgument("Name", out var nameArg)
                && nameArg.Kind == TypedConstantKind.Primitive
                && nameArg.Value is string vName
                ? vName
                : null;

            value = attr.TryGetNamedArgument("Value", out var valueArg)
                && valueArg.Kind == TypedConstantKind.Primitive
                && valueArg.Value is int vValue
                ? vValue
                : -1;

            type = attr.TryGetNamedArgument("Type", out var typeArg)
                && typeArg.Kind == TypedConstantKind.Type
                && typeArg.Value is INamedTypeSymbol vType
                ? vType
                : null;

            isSingleton = attr.TryGetNamedArgument("IsSingleton", out var isSingletonArg)
                && isSingletonArg.Kind == TypedConstantKind.Primitive
                && isSingletonArg.Value is bool vIsSingleton
                ? vIsSingleton
                : false;

            factoryName = attr.TryGetNamedArgument("FactoryName", out var factoryNameArg)
                && factoryNameArg.Kind == TypedConstantKind.Primitive
                && factoryNameArg.Value is string vFactoryName
                ? vFactoryName
                : null;

            factoryIsProperty = attr.TryGetNamedArgument("FactoryIsProperty", out var factoryIsPropertyArg)
                && factoryIsPropertyArg.Kind == TypedConstantKind.Primitive
                && factoryIsPropertyArg.Value is bool vFactoryIsProperty
                ? vFactoryIsProperty
                : null;

            accessorName = attr.TryGetNamedArgument("AccessorName", out var accessorNameArg)
                && accessorNameArg.Kind == TypedConstantKind.Primitive
                && accessorNameArg.Value is string vAccessorName
                ? vAccessorName
                : null;
        }

        private IReadOnlyList<UnionCase> GetTypeCasesFromCaseAttributesOnUnion(
            INamedTypeSymbol unionType, UnionOptions options, List<Diagnostic> diagnostics)
        {
            var cases = new List<UnionCase>();

            foreach (var attr in unionType.GetAttributes("UnionCaseAttribute"))
            {
                GetUnionCaseData(attr, 
                    out var caseName, 
                    out var tagValue, 
                    out var type, 
                    out var isSingleton,
                    out var factoryName,
                    out var factoryIsProperty,
                    out var accessorName
                    );
                
                if (type != null)
                {
                    if (caseName == null)
                    {
                        caseName = type.Name;
                    }

                    var caseType = GetTypeFullName(type);

                    var factoryParameters = isSingleton
                        ? Array.Empty<UnionCaseValue>() 
                        : new[] { GetCaseValue(type, "value") };

                    var typeCase = new UnionCase(
                        name: caseName,
                        type: caseType,
                        tagValue: tagValue,
                        factoryName: factoryName,
                        factoryParameters: factoryParameters,
                        factoryIsPartial: false,
                        factoryIsProperty: factoryIsProperty ?? isSingleton, // default to property if not specified and singleton
                        accessorName: accessorName,
                        isSingleton: isSingleton
                        );

                    cases.Add(typeCase);
                }
            }

            return cases;
        }

        private IReadOnlyList<UnionCase> GetTypeCasesFromNestedRecords(
            INamedTypeSymbol unionType, List<Diagnostic> diagnostics)
        {
            var nestedTypes = unionType
                .GetTypeMembers()
                .OfType<INamedTypeSymbol>()
                .Where(nt => nt.IsRecord
                    && nt.DeclaredAccessibility == Accessibility.Public)
                .ToList();

            var cases = new List<UnionCase>();
            foreach (var nestedType in nestedTypes)
            {
                if (nestedType.TryGetAttribute("UnionCaseAttribute", out var attr))
                {
                    GetUnionCaseData(attr, 
                        out var caseName, 
                        out var tagValue, 
                        type: out _, // type is the nested record
                        out var isSingleton,
                        out var factoryName,
                        out var factoryIsProperty,
                        out var accessorName);

                    if (caseName == null)
                        caseName = nestedType.Name;

                    var caseType = GetTypeFullName(nestedType);
                    var parameters = new[] { GetCaseValue(nestedType, "value") };

                    var typeCase = new UnionCase(
                        name: caseName,
                        type: caseType,
                        tagValue: tagValue,
                        factoryName,
                        factoryParameters: parameters,
                        factoryIsPartial: false,
                        factoryIsProperty: isSingleton && (factoryIsProperty ?? isSingleton), // property if singleton and requested
                        accessorName: accessorName,
                        isSingleton: isSingleton
                        );

                    cases.Add(typeCase);
                }
            }

            return cases;
        }

        private IReadOnlyList<UnionCase> GetTypeCasesFromPartialFactoryMethods(
            INamedTypeSymbol unionType, UnionOptions options, List<Diagnostic> diagnostics)
        {
            var cases = new List<UnionCase>();

            // any static partial method returning the union type is a factory
            var factoryMethods = unionType.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.IsPartialDefinition
                         && m.IsStatic
                         && m.TypeParameters.Length == 0
                         && SymbolEqualityComparer.Default.Equals(m.ReturnType, unionType)
                         && m.Parameters.Length == 1)
                .ToList();

            foreach (var method in factoryMethods)
            {
                if (method.TryGetAttribute("UnionCaseAttribute", out var attr))
                {
                    GetUnionCaseData(attr, 
                        out var caseName, 
                        out var tagValue, 
                        out var type, 
                        out var isSingleton,
                        factoryName: out var _, // factory name is the factory name
                        factoryIsProperty: out var _,
                        out var accessorName
                        );

                    // parameter type is always the correct type.
                    if (method.Parameters.Length == 1)
                        type = method.Parameters[0].Type;

                    var namedType = type as INamedTypeSymbol;
                    var typeName = namedType != null ? GetTypeFullName(namedType) : null;
                    if (caseName == null)
                    {
                        // infer case name from type name or factory name.
                        caseName = namedType?.Name ?? method.Name;
                    }

                    var factoryName = method.Name;
                    var factoryParameters = GetCaseParameters(method.Parameters);

                    var typeCase = new UnionCase(
                        name: caseName,
                        type: typeName,
                        tagValue: tagValue,
                        factoryName: factoryName,
                        factoryParameters: factoryParameters,
                        factoryIsPartial: true,
                        factoryIsProperty: false,
                        accessorName: accessorName,
                        isSingleton: isSingleton
                        ); 

                    cases.Add(typeCase);
                }
            }

            return cases;
        }

        private IReadOnlyList<UnionCase> GetTypeCasesFromPartialFactoryProperties(
            INamedTypeSymbol unionType, UnionOptions options, List<Diagnostic> diagnostics)
        {
            var cases = new List<UnionCase>();

            // any static partial method returning the union type is a factory
            var factoryProperties = unionType.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.GetMethod != null && p.GetMethod.IsPartialDefinition
                         && p.IsStatic
                         && SymbolEqualityComparer.Default.Equals(p.Type, unionType))
                .ToList();

            foreach (var method in factoryProperties)
            {
                if (method.TryGetAttribute("UnionCaseAttribute", out var attr))
                {
                    GetUnionCaseData(attr,
                        out var caseName,
                        out var tagValue,
                        out var type,
                        out var isSingleton,
                        factoryName: out var _, // factory name is the factory name
                        factoryIsProperty: out var _,
                        out var accessorName
                        );

                    var namedType = type as INamedTypeSymbol;
                    var typeName = namedType != null ? GetTypeFullName(namedType) : null;
                    if (caseName == null)
                    {
                        // infer case name from type name or factory name
                        caseName = namedType?.Name ?? method.Name;
                    }

                    var factoryName = method.Name;
                    var factoryParameters = GetCaseParameters(method.Parameters);

                    var typeCase = new UnionCase(
                        name: caseName,
                        type: typeName,
                        tagValue: tagValue,
                        factoryName: factoryName,
                        factoryParameters: factoryParameters,
                        factoryIsPartial: true,
                        factoryIsProperty: true,
                        accessorName: accessorName,
                        isSingleton: isSingleton
                        );

                    cases.Add(typeCase);
                }
            }

            return cases;
        }

        private IReadOnlyList<UnionCase> GetTagCasesFromCaseAttributesOnUnion(
            INamedTypeSymbol unionSymbol, UnionOptions options, List<Diagnostic> diagnostics)
        {
            var cases = new List<UnionCase>();

            foreach (var attr in unionSymbol.GetAttributes("UnionCaseAttribute"))
            {
                GetUnionCaseData(attr, 
                    out var caseName, 
                    out var value, 
                    type: out _,  // there is no type for a tag union
                    isSingleton: out _,
                    out var factoryName,
                    out var factoryIsProperty,
                    out var accessorName
                    );
                
                if (caseName == null)
                {
                    caseName = "Case" + (cases.Count + 1);
                }

                var tagCase = new UnionCase(
                    caseName,
                    type: null,
                    tagValue: value,
                    factoryName: factoryName,
                    factoryParameters: null,
                    factoryIsPartial: false,
                    factoryIsProperty: factoryIsProperty ?? true, // default to property if not specified
                    accessorName: accessorName,
                    isSingleton: false
                    );

                cases.Add(tagCase);

            }

            return cases;
        }


        private IReadOnlyList<UnionCase> GetTagCasesFromPartialFactoryMethods(
            INamedTypeSymbol unionType, UnionOptions options, List<Diagnostic> diagnostics)
        {
            var cases = new List<UnionCase>();
            var caseNames = new HashSet<string>();

            // any static partial method returning the union type is a factory
            var factoryMethods = unionType.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.IsPartialDefinition
                         && m.IsStatic
                         && m.TypeParameters.Length == 0
                         && SymbolEqualityComparer.Default.Equals(m.ReturnType, unionType))
                .ToList();

            foreach (var method in factoryMethods)
            {
                // only consider methods with UnionCase attribute
                if (method.TryGetAttribute("UnionCaseAttribute", out var attr))
                {
                    GetUnionCaseData(attr, 
                        out var caseName, 
                        out var tagValue, 
                        type: out _,    // there is no type for a tag union
                        isSingleton: out _, // there is no singleton for a tag union
                        factoryName: out _,  // factory is declared
                        factoryIsProperty: out _,
                        out var accessorName
                        );
                    
                    if (caseName == null)
                    {
                        // infer case name from method name
                        caseName = method.Name;
                    }

                    // the factory is already specified, so it must use the member name.
                    var factoryName = method.Name;

                    var factoryParameters =
                        method.Parameters.Select(p => new UnionCaseValue(p.Name, GetTypeFullName(p.Type), GetParameterKind(p.Type))).ToArray();

                    var tagCase =
                        new UnionCase(
                            name: caseName,
                            type: null,
                            tagValue: tagValue,
                            factoryName: factoryName,
                            factoryParameters: factoryParameters,
                            factoryIsPartial: true,
                            factoryIsProperty: false,
                            accessorName: accessorName,
                            isSingleton: false
                            );

                    cases.Add(tagCase);
                }
            }

            return cases;
        }

        private IReadOnlyList<UnionCase> GetTagCasesFromPartialFactoryProperties(
            INamedTypeSymbol unionType, UnionOptions options, List<Diagnostic> diagnostics)
        {
            var cases = new List<UnionCase>();
            var caseNames = new HashSet<string>();

            // any static partial method returning the union type is a factory
            var factoryProperties = unionType.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.GetMethod != null && p.GetMethod.IsPartialDefinition
                         && p.IsStatic
                         && SymbolEqualityComparer.Default.Equals(p.Type, unionType))
                .ToList();

            foreach (var property in factoryProperties)
            {
                // only consider properties with UnionCase attribute
                if (property.TryGetAttribute("UnionCaseAttribute", out var attr))
                {
                    GetUnionCaseData(attr,
                        out var caseName,
                        out var tagValue,
                        type: out _,    // there is no type for a tag union
                        isSingleton: out _, // there is no singleton for a tag union
                        factoryName: out _,  // factory is declared
                        factoryIsProperty: out _,
                        out var accessorName
                        );

                    if (caseName == null)
                    {
                        // infer case name from property name
                        caseName = property.Name;
                    }

                    // the factory is already specified, so it must use the member name.
                    var factoryName = property.Name;

                    var tagCase =
                        new UnionCase(
                            name: caseName,
                            type: null,
                            tagValue: tagValue,
                            factoryName: factoryName,
                            factoryParameters: new UnionCaseValue[] { },
                            factoryIsPartial: true,
                            factoryIsProperty: true,
                            accessorName: accessorName,
                            isSingleton: false
                            );

                    cases.Add(tagCase);
                }
            }

            return cases;
        }

        private IReadOnlyList<UnionCaseValue> GetCaseParameters(IReadOnlyList<IParameterSymbol> parameters)
        {
            return parameters.Select(p => GetCaseValue(p)).ToArray();
        }

        private UnionCaseValue GetCaseValue(IParameterSymbol parameter)
        {
            return GetCaseValue(parameter.Type, parameter.Name);
        }

        private UnionCaseValue GetCaseValue(ITypeSymbol type, string name)
        {
            var kind = GetParameterKind(type);
            var typeName = GetTypeFullName(type);
            var nestedParameters = GetNestedParameters(type);
            return new UnionCaseValue(name, typeName, kind, nestedParameters);
        }

        private IReadOnlyList<UnionCaseValue> GetNestedParameters(ITypeSymbol caseSymbol)
        {
            if (caseSymbol.IsValueType && caseSymbol.IsRecord && caseSymbol is INamedTypeSymbol nt)
            {
                // look for nested case parameters from records and tuple arguments
                var primaryConstructor = nt.Constructors.FirstOrDefault(c => c.Parameters.Length > 0);
                if (primaryConstructor != null)
                {
                    return primaryConstructor.Parameters.Select(p => GetCaseValue(p)).ToArray();
                }
            }

            return Array.Empty<UnionCaseValue>();
        }

        private static TypeKind GetCaseTypeKind(ITypeSymbol type)
        {
            if (type is INamedTypeSymbol nt && nt.IsRecord && nt.IsValueType)
            {
                if (type.Locations.Any(loc => loc.IsInSource))
                {
                    return TypeKind.DecomposableLocalRecordStruct;
                }
                else
                {
                    return TypeKind.DecomposableForeignRecordStruct;
                }
            }

            return GetParameterKind(type);
        }

        private static TypeKind GetParameterKind(ITypeSymbol type)
        {
            switch (type.TypeKind)
            {
                case CATypeKind.Enum:
                    return TypeKind.PrimitiveStruct;
                case CATypeKind.Struct:
                    if (IsPrimitiveStruct(type))
                    {
                        return TypeKind.PrimitiveStruct;
                    }
                    else if (type.IsTupleType)
                    {
                        return TypeKind.ValueTuple;
                    }
                    else if (type.IsRecord)
                    {
                        if (type.IsDeclaredInSource())
                        {
                            return TypeKind.DecomposableLocalRecordStruct;
                        }
                        else
                        {
                            return TypeKind.DecomposableForeignRecordStruct;
                        }
                    }
                    else if (IsOverlappableStruct(type))
                    {
                        if (type.IsDeclaredInSource())
                        {
                            return TypeKind.OverlappableLocalStruct;
                        }
                        else
                        {
                            return TypeKind.OverlappableForeignStruct;
                        }
                    }
                    else
                    {
                        return TypeKind.NonOverlappableStruct;
                    }
                case CATypeKind.Interface:
                    return TypeKind.Interface;
                case CATypeKind.Class:
                case CATypeKind.Array:
                case CATypeKind.Dynamic:
                case CATypeKind.Delegate:
                    return TypeKind.Class;
                
                case CATypeKind.TypeParameter:
                    var tp = (ITypeParameterSymbol)type;
                    if (tp.HasReferenceTypeConstraint)
                        return TypeKind.TypeParameter_RefConstrained;
                    return TypeKind.TypeParameter_Unconstrained;
                default:
                    return TypeKind.Unknown;
            }
        }

        private static bool IsOverlappableType(ITypeSymbol type)
        {
            switch (type.TypeKind)
            {
                case CATypeKind.Enum:
                    return true;
                case CATypeKind.Struct:
                    return IsPrimitiveStruct(type)
                        || IsOverlappableStruct(type);
                default:
                    return false;
            }
        }

        private static bool IsOverlappableStruct(ITypeSymbol type)
        {
            return type.IsValueType
                && type.GetMembers().OfType<IFieldSymbol>().All(f => IsOverlappableType(f.Type));
        }

        private static bool IsPrimitiveStruct(ITypeSymbol type)
        {
            if (!(type is INamedTypeSymbol nt))
                return false;

            switch (nt.SpecialType)
            {
                case SpecialType.System_Boolean:
                case SpecialType.System_Byte:
                case SpecialType.System_Char:
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_SByte:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_Decimal:
                case SpecialType.System_DateTime:
                    return true;

                default:
                    return false;
            }
        }

        private static string GetAccessibility(Accessibility acc)
        {
            switch (acc)
            {
                case Accessibility.Public:
                    return "public";
                case Accessibility.Protected:
                    return "protected";
                case Accessibility.Private:
                    return "private";
                case Accessibility.Internal:
                    return "internal";
                case Accessibility.ProtectedAndInternal:
                    return "private protected";
                case Accessibility.ProtectedOrInternal:
                    return "internal protected";
                default:
                    return "";
            }
        }

        private static string GetTypeFullName(ITypeSymbol type)
        {
            var shortName = GetTypeShortName(type);

            if (type is ITypeParameterSymbol
                || type.IsTupleType)
                return shortName;

            if (type.ContainingType != null)
            {
                return GetTypeFullName(type.ContainingType) + "." + shortName;
            }
            else if (type.ContainingNamespace != null
                    && !type.ContainingNamespace.IsGlobalNamespace)
            {
                return GetNamespaceName(type.ContainingNamespace) + "." + shortName;
            }
            else
            {
                return shortName;
            }
        }

        private static string GetTypeShortName(ITypeSymbol type)
        {
            if (type is INamedTypeSymbol nt)
            {
                var typeParameterList = GetTypeParameterList(nt);
                return string.IsNullOrEmpty(typeParameterList) ? nt.Name : nt.Name + typeParameterList;
            }
            else if (type is IArrayTypeSymbol at)
            {
                var elementType = GetTypeShortName(at.ElementType);
                if (at.Rank == 1)
                {
                    return elementType + "[]";
                }
                else if (at.Rank == 2)
                {
                    return elementType + "[,]";
                }
                else
                {
                    return elementType + "[" + new string(',', at.Rank - 1) + "]";
                }
            }
            else
            {
                return type.Name;
            }
        }

        private static string GetTypeParameterList(INamedTypeSymbol type)
        {
            if (type.TypeParameters.Length > 0)
            {
                return $"<{string.Join(", ", type.TypeParameters.Select(tp => tp.Name))}>";
            }
            else
            {
                return "";
            }
        }

        private static string GetNamespaceName(INamespaceSymbol ns)
        {
            if (ns.ContainingNamespace != null
                && !ns.ContainingNamespace.IsGlobalNamespace)
            {
                return GetNamespaceName(ns.ContainingNamespace) + "." + ns.Name;
            }

            return ns.Name;
        }

        private static DiagnosticDescriptor DuplicateCaseNameDiagnostic = new DiagnosticDescriptor(
#pragma warning disable RS2008 // Enable analyzer release tracking
            "UT0001",
#pragma warning restore RS2008 // Enable analyzer release tracking
            "Duplicate case name",
            "Case name '{0}' is already used in the union type",
            "UnionTypes",
            DiagnosticSeverity.Error,
            true
            );
    }
}