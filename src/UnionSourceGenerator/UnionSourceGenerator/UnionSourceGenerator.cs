using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using CATypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace UnionTypes.Generators
{
    [Generator]
    public class UnionSourceGenerator : ISourceGenerator
    {
        public static readonly string TagUnionAttributeName = "TagUnionAttribute";
        public static readonly string TypeUnionAttributeName = "TypeUnionAttribute";
        public static readonly string CaseAttributeName = "CaseAttribute";
        public static readonly string TagUnionAnnotation = "@TagUnion";
        public static readonly string TypeUnionAnnotation = "@TypeUnion";

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // search entire compilation for all types that look like partial declared unions.
            var unionTypes = GetUnionTypes(context.Compilation.GlobalNamespace);

            // try to generate source code for each union type
            foreach (var unionType in unionTypes)
            {
                if (TryGenerateUnionType(unionType, out var result))
                {
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

        private static bool IsUnionType(INamedTypeSymbol symbol)
        {
            return IsTypeUnion(symbol, out _)
                || IsTagUnion(symbol, out _);
        }

        private static bool IsTypeUnion(INamedTypeSymbol symbol, out AttributeData? attribute)
        {
            if (symbol.TryGetAttribute(TypeUnionAttributeName, out attribute))
                return true;
            attribute = null;
            return ContainsInTrivia(symbol, TypeUnionAnnotation);
        }

        private static bool IsTagUnion(INamedTypeSymbol symbol, out AttributeData? attribute)
        {
            if (symbol.TryGetAttribute(TagUnionAttributeName, out attribute))
                return true;
            attribute = null;
            return ContainsInTrivia(symbol, TagUnionAnnotation);
        }

        private static bool ContainsInTrivia(ISymbol symbol, string text)
        {
            return GetDeclarationNodes(symbol).Any(d => ContainsInTrivia(d, text));
        }

        private static bool ContainsInTrivia(SyntaxNode node, string text)
        {
            var commentTrivia = node.GetLeadingTrivia().Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia) || t.IsKind(SyntaxKind.MultiLineCommentTrivia)).ToArray();
            return commentTrivia.Any(t => t.ToString().Contains(text));
        }

        private static IEnumerable<SyntaxNode> GetDeclarationNodes(ISymbol symbol)
        {
            foreach (var location in symbol.Locations.Where(loc => loc.IsInSource))
            {
                if (location.SourceTree is SyntaxTree sourceTree
                    && sourceTree.GetRoot() is SyntaxNode root)
                {
                    var declaration = root.FindNode(location.SourceSpan);
                    if (declaration != null)
                        yield return declaration;
                }
            }
        }

        private bool TryGenerateUnionType(INamedTypeSymbol unionType, out GenerateResult result)
        {
            var diagnostics = new List<Diagnostic>();

            string namespaceName = null!;
            if (unionType.ContainingNamespace != null)
            {
                namespaceName = GetNamespaceName(unionType.ContainingNamespace);
            }

            var usingDirectives = GetDeclaredUsings(unionType);
            var usings = usingDirectives.Select(uz => uz.ToString()).ToArray();
            var usesToolkit = usings.Any(u => u.Contains(UnionGenerator.ToolkitNamespace));

            var accessibility = GetAccessibility(unionType.DeclaredAccessibility);
            string text;

            if (IsTypeUnion(unionType, out var typeUnionAttribute))
            {
                var options = GetOptionsFromUnionAttribute(typeUnionAttribute);
                options = options.WithUseToolkit(usesToolkit);

                // get all cases declared for union type
                var cases =
                    GetTypeCasesFromCaseAttributesOnUnion(unionType, options, diagnostics)
                    .Concat(GetTypeCasesFromNestedRecords(unionType, diagnostics))
                    .Concat(GetTypeCasesFromPartialFactoryMethods(unionType, options, diagnostics))
                    .Concat(GetTypeCasesFromPartialFactoryProperties(unionType, options, diagnostics))
                    .ToList();

                if (cases.Count > 0)
                {
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
                    result = new GenerateResult(text, diagnostics);
                    return true;
                }
            }
            else if (IsTagUnion(unionType, out var tagUnionAttribute))
            {
                var options = GetOptionsFromUnionAttribute(tagUnionAttribute);

                // get all cases declared for union type
                var cases =
                    GetTagCasesFromPartialFactoryMethods(unionType, options, diagnostics)
                    .Concat(GetTagCasesFromPartialFactoryProperties(unionType, options, diagnostics))
                    .Concat(GetTagCasesFromCaseAttributesOnUnion(unionType, options, diagnostics))
                    .ToList();

                if (cases.Count > 0)
                {
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
                    result = new GenerateResult(text, diagnostics);
                    return true;
                }
            }

            result = default!;
            return false;
        }

        private string? GetInferredCaseName(IMethodSymbol method, string? namePrefix)
        {
            if (namePrefix == null)
                return null;
            var name = method.Name;
            if (name.StartsWith(namePrefix))
            {
                return name.Substring(namePrefix.Length);
            }
            return null;
        }

        private static string? GetPrefix(string name)
        {
            // find the case shift back to Upper
            if (name.Length > 2 && char.IsUpper(name[0]) && char.IsLower(name[1]))
            {
                for (int i = 2; i < name.Length; i++)
                {
                    if (char.IsUpper(name[i]))
                    {
                        return name.Substring(0, i);
                    }
                }
            }

            return null;
        }

        private static string? GetCommonPrefix(IReadOnlyList<string> names)
        {
            string? commonPrefix = null;
            foreach (var name in names)
            {
                var prefix = GetPrefix(name);
                if (commonPrefix == null)
                {
                    commonPrefix = prefix;
                    continue;
                }
                else if (commonPrefix == prefix)
                {
                    continue;
                }
                else
                {
                    return null;
                }
            }
            return commonPrefix;
        }

        /// <summary>
        /// Gets the using directives from the file that contains the type declaration.
        /// </summary>
        private IReadOnlyList<UsingDirectiveSyntax> GetDeclaredUsings(INamedTypeSymbol type)
        {
            if (type.Locations.FirstOrDefault(loc => loc.IsInSource) is Location sourceLocation
                && sourceLocation.SourceTree is SyntaxTree sourceTree
                && sourceTree.GetRoot() is SyntaxNode root)
            {
                return root.DescendantNodes()
                    .OfType<UsingDirectiveSyntax>()
                    .Where(u => u.GlobalKeyword == default)
                    .ToList();
            }

            return Array.Empty<UsingDirectiveSyntax>();
        }

        private bool RefersToToolkit(UsingDirectiveSyntax usingDirective)
        {
            return usingDirective.ToString().Contains("UnionTypes.Toolkit");
        }

        private UnionOptions GetOptionsFromUnionAttribute(AttributeData? unionAttribute)
        {
            var options = UnionOptions.Default;

            if (unionAttribute == null)
                return options;

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

            if (unionAttribute.TryGetNamedArgument("GenerateToString", out arg)
                && arg.Kind == TypedConstantKind.Primitive
                && arg.Value is bool generateToString)
            {
                options = options.WithGenerateToString(generateToString);
            }

            if (unionAttribute.TryGetNamedArgument("GenerateMatch", out arg)
                && arg.Kind == TypedConstantKind.Primitive
                && arg.Value is bool generateMatch)
            {
                options = options.WithGenerateMatch(generateMatch);
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


        private class CaseData
        {
            public string? Name { get; }
            public int TagValue { get; }
            public ITypeSymbol? Type { get; }
            public string? FactoryName { get; }
            public bool? FactoryIsProperty { get; }
            public bool? FactoryIsInternal { get; }
            public string? AccessorName { get; }
            public bool? HasAccessor { get; }

            public CaseData(
                string? name, 
                int tagValue, 
                ITypeSymbol? type, 
                string? factoryName, 
                bool? factoryIsProperty, 
                bool? factoryIsInternal, 
                string? accessorName, 
                bool? hasAccessor)
            {
                this.Name = name;
                this.TagValue = tagValue;
                this.Type = type;
                this.FactoryName = factoryName;
                this.FactoryIsProperty = factoryIsProperty;
                this.FactoryIsInternal = factoryIsInternal;
                this.AccessorName = accessorName;
                this.HasAccessor = hasAccessor;
            }

            public static readonly CaseData Default = 
                new CaseData(
                    name: null, 
                    tagValue: -1, 
                    type: null, 
                    factoryName: null, 
                    factoryIsProperty: null, 
                    factoryIsInternal: null, 
                    accessorName: null, 
                    hasAccessor: null
                    );
        }

        private CaseData GetCaseInfo(AttributeData? attr)
        {
            if (attr != null)
            {
                return new CaseData(
                    name: attr.TryGetNamedArgument("Name", out var nameArg)
                        && nameArg.Kind == TypedConstantKind.Primitive
                        && nameArg.Value is string vName
                        ? vName
                        : null,
                    tagValue: attr.TryGetNamedArgument("TagValue", out var valueArg)
                        && valueArg.Kind == TypedConstantKind.Primitive
                        && valueArg.Value is int vValue
                        ? vValue
                        : -1,
                    type: attr.TryGetNamedArgument("Type", out var typeArg)
                        && typeArg.Kind == TypedConstantKind.Type
                        && typeArg.Value is INamedTypeSymbol vType
                        ? vType
                        : null,
                    factoryName: attr.TryGetNamedArgument("FactoryName", out var factoryNameArg)
                        && factoryNameArg.Kind == TypedConstantKind.Primitive
                        && factoryNameArg.Value is string vFactoryName
                        ? vFactoryName
                        : null,
                    factoryIsProperty: attr.TryGetNamedArgument("FactoryIsProperty", out var factoryIsPropertyArg)
                        && factoryIsPropertyArg.Kind == TypedConstantKind.Primitive
                        && factoryIsPropertyArg.Value is bool vFactoryIsProperty
                        ? vFactoryIsProperty
                        : null,
                    factoryIsInternal: attr.TryGetNamedArgument("FactoryIsInternal", out var factoryIsInternalArg)
                        && factoryIsInternalArg.Kind == TypedConstantKind.Primitive
                        && factoryIsInternalArg.Value is bool vFactoryIsInternal
                        ? vFactoryIsInternal
                        : null,
                    accessorName: attr.TryGetNamedArgument("AccessorName", out var accessorNameArg)
                        && accessorNameArg.Kind == TypedConstantKind.Primitive
                        && accessorNameArg.Value is string vAccessorName
                        ? vAccessorName
                        : null,
                    hasAccessor: attr.TryGetNamedArgument("HasAccessor", out var hasAccessorArg)
                        && hasAccessorArg.Kind == TypedConstantKind.Primitive
                        && hasAccessorArg.Value is bool vHasAccessor
                        ? vHasAccessor
                        : true
                );
            }
            else
            {
                return CaseData.Default;
            }
        }

        private IReadOnlyList<UnionCase> GetTypeCasesFromCaseAttributesOnUnion(
            INamedTypeSymbol unionType, UnionOptions options, List<Diagnostic> diagnostics)
        {
            var cases = new List<UnionCase>();

            foreach (var attr in unionType.GetAttributes(CaseAttributeName))
            {
                var caseInfo = GetCaseInfo(attr);
                
                if (caseInfo.Type != null)
                {
                    var caseName = caseInfo.Name;
                    if (caseName == null)
                    {
                        caseName = caseInfo.Type.Name;
                    }

                    var caseType = GetValueType(caseInfo.Type);
                    var isProperty = caseInfo.FactoryIsProperty == true && caseType.SingletonAccessor != null;

                    var factoryParameters = isProperty
                        ? Array.Empty<UnionCaseValue>() 
                        : new[] { GetCaseValue("value", caseInfo.Type) };

                    var typeCase = new UnionCase(
                        name: caseName,
                        type: caseType,
                        tagValue: caseInfo.TagValue,
                        factoryName: caseInfo.FactoryName,
                        factoryParameters: factoryParameters,
                        factoryIsPartial: false,
                        factoryIsProperty: isProperty,
                        factoryAccessibility: caseInfo.FactoryIsInternal == true ? "internal" : "public",
                        accessorName: caseInfo.AccessorName,
                        hasAccessor: caseInfo.HasAccessor ?? true
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
                    && (nt.DeclaredAccessibility == Accessibility.Public
                        || nt.DeclaredAccessibility == Accessibility.Internal))
                .ToList();

            var cases = new List<UnionCase>();
            foreach (var nestedType in nestedTypes)
            {
                if (nestedType.TryGetAttribute(CaseAttributeName, out var attr))
                {
                    var caseInfo = GetCaseInfo(attr);

                    var caseName = caseInfo.Name ?? nestedType.Name;

                    var caseType = GetValueType(nestedType);
                    var isProperty = caseInfo.FactoryIsProperty == true && caseType.SingletonAccessor != null;
                    var parameters = isProperty ? Array.Empty<UnionCaseValue>() : new[] { GetCaseValue("value", nestedType) };
                    var factoryAccessibility = GetAccessibility(nestedType.DeclaredAccessibility);

                    var typeCase = new UnionCase(
                        name: caseName,
                        type: caseType,
                        tagValue: caseInfo.TagValue,
                        caseInfo.FactoryName,
                        factoryParameters: parameters,
                        factoryIsPartial: false,
                        factoryIsProperty: isProperty,
                        factoryAccessibility: factoryAccessibility,
                        accessorName: caseInfo.AccessorName,
                        hasAccessor: caseInfo.HasAccessor ?? true
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
                         && (m.DeclaredAccessibility == Accessibility.Public || m.DeclaredAccessibility == Accessibility.Internal)
                         && m.TypeParameters.Length == 0
                         && SymbolEqualityComparer.Default.Equals(m.ReturnType, unionType)
                         && m.Parameters.Length == 1)
                .ToList();

            var uniqueNames = new HashSet<string>(factoryMethods.Select(m => m.Name));
            var namePrefix = GetCommonPrefix(uniqueNames.ToList());

            foreach (var method in factoryMethods)
            {
                method.TryGetAttribute(CaseAttributeName, out var attribute);
                var caseInfo = GetCaseInfo(attribute);

                var type = caseInfo.Type;

                // parameter type is always the correct type.
                if (method.Parameters.Length == 1)
                    type = method.Parameters[0].Type;

                var caseType = type != null ? GetValueType(type) : null;

                var inferredCaseName = GetInferredCaseName(method, namePrefix);
                var caseName = caseInfo.Name ?? inferredCaseName ?? (type is INamedTypeSymbol namedType ? namedType.Name : method.Name);

                var factoryName = method.Name;
                var factoryParameters = GetCaseParameters(method.Parameters);
                var factoryAccessibility = GetAccessibility(method.DeclaredAccessibility);

                var typeCase = new UnionCase(
                    name: caseName,
                    type: caseType,
                    tagValue: caseInfo.TagValue,
                    factoryName: factoryName,
                    factoryParameters: factoryParameters,
                    factoryIsPartial: true,
                    factoryIsProperty: false,
                    factoryAccessibility: factoryAccessibility,
                    accessorName: caseInfo.AccessorName,
                    hasAccessor: caseInfo.HasAccessor ?? true
                    );

                cases.Add(typeCase);
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
                         && p.SetMethod == null
                         && (p.DeclaredAccessibility == Accessibility.Public || p.DeclaredAccessibility == Accessibility.Internal)
                         && p.IsStatic
                         && SymbolEqualityComparer.Default.Equals(p.Type, unionType))
                .ToList();

            foreach (var prop in factoryProperties)
            {
                prop.TryGetAttribute(CaseAttributeName, out var attr);
                var caseInfo = GetCaseInfo(attr);

                var caseType = caseInfo.Type != null ? GetValueType(caseInfo.Type) : null;
                var caseName = caseInfo.Name ?? (caseInfo.Type is INamedTypeSymbol namedType ? namedType.Name : prop.Name);

                var factoryName = prop.Name;
                var factoryParameters = GetCaseParameters(prop.Parameters);
                var factoryAccessibility = GetAccessibility(prop.DeclaredAccessibility);

                var typeCase = new UnionCase(
                    name: caseName,
                    type: caseType,
                    tagValue: caseInfo.TagValue,
                    factoryName: factoryName,
                    factoryParameters: factoryParameters,
                    factoryIsPartial: true,
                    factoryIsProperty: true,
                    factoryAccessibility: factoryAccessibility,
                    accessorName: caseInfo.AccessorName,
                    hasAccessor: caseInfo.HasAccessor ?? true
                    );

                cases.Add(typeCase);
            }

            return cases;
        }

        private IReadOnlyList<UnionCase> GetTagCasesFromCaseAttributesOnUnion(
            INamedTypeSymbol unionSymbol, UnionOptions options, List<Diagnostic> diagnostics)
        {
            var cases = new List<UnionCase>();

            foreach (var attr in unionSymbol.GetAttributes(CaseAttributeName))
            {
                var caseInfo = GetCaseInfo(attr);
                
                var caseName = caseInfo.Name ?? "Case" + (cases.Count + 1);

                var tagCase = new UnionCase(
                    caseName,
                    type: null,
                    tagValue: caseInfo.TagValue,
                    factoryName: caseInfo.FactoryName,
                    factoryParameters: null,
                    factoryIsPartial: false,
                    factoryIsProperty: caseInfo.FactoryIsProperty ?? true, // default to property if not specified
                    factoryAccessibility: caseInfo.FactoryIsInternal == true ? "internal" : "public",
                    accessorName: caseInfo.AccessorName,
                    hasAccessor: caseInfo.HasAccessor ?? true
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

            var uniqueNames = new HashSet<string>(factoryMethods.Select(m => m.Name));
            if (uniqueNames.Count == factoryMethods.Count)
            {
                var namePrefix = GetCommonPrefix(uniqueNames.ToList());

                foreach (var method in factoryMethods)
                {
                    // only consider methods with TagCase attribute
                    method.TryGetAttribute(CaseAttributeName, out var attr);

                    var inferredCaseName = GetInferredCaseName(method, namePrefix);
                    var caseInfo = GetCaseInfo(attr);

                    var caseName = caseInfo.Name ?? inferredCaseName ?? method.Name;

                    // the factory is already specified, so it must use the member name.
                    var factoryName = method.Name;

                    var factoryParameters =
                        method.Parameters.Select(p => new UnionCaseValue(p.Name, GetValueType(p.Type))).ToArray();

                    var factoryAccessibility = GetAccessibility(method.DeclaredAccessibility);

                    var tagCase = new UnionCase(
                        name: caseName,
                        type: null,
                        tagValue: caseInfo.TagValue,
                        factoryName: factoryName,
                        factoryParameters: factoryParameters,
                        factoryIsPartial: true,
                        factoryIsProperty: false,
                        factoryAccessibility: factoryAccessibility,
                        accessorName: caseInfo.AccessorName,
                        hasAccessor: caseInfo.HasAccessor ?? true
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
                // only consider properties with TagCase attribute
                if (property.TryGetAttribute(CaseAttributeName, out var attr))
                {
                    var caseInfo = GetCaseInfo(attr);
                    var caseName = caseInfo.Name ?? property.Name;

                    // the factory is already specified, so it must use the member name.
                    var factoryName = property.Name;
                    var factoryAccessibility = GetAccessibility(property.DeclaredAccessibility);

                    var tagCase =
                        new UnionCase(
                            name: caseName,
                            type: null,
                            tagValue: caseInfo.TagValue,
                            factoryName: factoryName,
                            factoryParameters: new UnionCaseValue[] { },
                            factoryIsPartial: true,
                            factoryIsProperty: true,
                            factoryAccessibility: factoryAccessibility,
                            accessorName: caseInfo.AccessorName,
                            hasAccessor: caseInfo.HasAccessor ?? true
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
            return GetCaseValue(parameter.Name, parameter.Type);
        }

        private UnionCaseValue GetCaseValue(string name, ITypeSymbol type, string? singletonAccessor = null)
        {
            var kind = GetTypeKind(type);
            var typeName = GetTypeFullName(type);
            var vtype = new UnionValueType(typeName, kind, singletonAccessor);
            var nestedParameters = GetNestedParameters(type);
            return new UnionCaseValue(name, vtype, nestedParameters);
        }

        private UnionValueType GetValueType(ITypeSymbol type)
        {
            var kind = GetTypeKind(type);
            var typeName = GetTypeFullName(type);
            TryGetSingletonAccessor(type, out var accessor);
            return new UnionValueType(typeName, kind, accessor);
        }

        private bool TryGetSingletonAccessor(ITypeSymbol type, out string accessor)
        {
            accessor = null!;

            // must be a named type
            if (!(type is INamedTypeSymbol namedType))
                return false;

            // must be a reference type
            if (!type.IsReferenceType)
                return false;

            // must have at least one constructor
            if (namedType.Constructors.Length == 0)
                return false;

            // type must have no non-private constructors
            if (!namedType.Constructors.All(c => c.DeclaredAccessibility == Accessibility.Private))
                return false;

            foreach (var member in type.GetMembers())
            {
                if (member.DeclaredAccessibility == Accessibility.Private)
                    continue;

                switch (member)
                {
                    case IPropertySymbol p:
                        // cannot have members that return this type that are not the singleton accessor
                        if (SymbolEqualityComparer.Default.Equals(p.Type, type))
                            return false;
                        break;
                    case IMethodSymbol p:
                        // cannot have members that return this type that are not the singleton accessor
                        if (SymbolEqualityComparer.Default.Equals(p.ReturnType, type))
                            return false;
                        break;
                    case IFieldSymbol f:
                        if (f.DeclaredAccessibility == Accessibility.Public
                            && f.IsStatic
                            && f.IsReadOnly
                            && SymbolEqualityComparer.Default.Equals(f.Type, type))
                        {
                            // cannot have more than one public static field
                            if (accessor != null)
                                return false;
                            accessor = f.Name;
                        }
                        break;
                }
            }

            return accessor != null;
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

            return GetTypeKind(type);
        }

        private static TypeKind GetTypeKind(ITypeSymbol type)
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