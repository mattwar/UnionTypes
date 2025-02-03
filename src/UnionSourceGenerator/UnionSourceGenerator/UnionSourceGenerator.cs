using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CATypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace UnionTypes.Generators
{
    [Generator(LanguageNames.CSharp)]
    public class UnionSourceGenerator: IIncrementalGenerator
    {
        public static readonly string TagUnionAttributeName = "TagUnionAttribute";
        public static readonly string TypeUnionAttributeName = "TypeUnionAttribute";
        public static readonly string CaseAttributeName = "CaseAttribute";
        public static readonly string TagUnionAnnotation = "@TagUnion";
        public static readonly string TypeUnionAnnotation = "@TypeUnion";
        public static readonly string ToolkitNamespace = UnionGenerator.ToolkitNamespace;

        public static readonly string GetInfoStepName = "GetInfo";
        public static readonly string GenerateStepName = "Generate";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider =
                context.SyntaxProvider.CreateSyntaxProvider(IsGenerationCandiate, GetGenerationInfo)
                .WithTrackingName(GetInfoStepName)
                .Select(Generate)
                .WithTrackingName(GenerateStepName)
                .Where(info => info != null);

            context.RegisterSourceOutput(provider, GenerateOutput);
        }

        public bool IsGenerationCandiate(SyntaxNode node, CancellationToken ct)
        {
            if (node is StructDeclarationSyntax decl)
            {
                return HasUnionAttribute(decl);
            }
            return false;
        }

        private bool HasUnionAttribute(StructDeclarationSyntax decl)
        {
            var hasAttr = decl.AttributeLists.Any(al => al.Attributes.Any(IsUnionAttribute))
                || ContainsInTrivia(decl, TagUnionAnnotation)
                || ContainsInTrivia(decl, TypeUnionAnnotation);
            return hasAttr;
        }

        private bool IsUnionAttribute(AttributeSyntax attr)
        {
            var attrName = attr.Name.ToString();
            return attrName == TagUnionAttributeName
                || attrName == TypeUnionAttributeName
                || TagUnionAttributeName.StartsWith(attrName)
                || TypeUnionAttributeName.StartsWith(attrName);
        }

        /// <summary>
        /// Gets the info that drives the code generation for the union type.
        /// </summary>
        public GenerationInfo? GetGenerationInfo(GeneratorSyntaxContext context, CancellationToken ct)
        {
            var decl = (StructDeclarationSyntax)context.Node;
            
            var symbol = context.SemanticModel.GetDeclaredSymbol(decl, ct);
            if (symbol != null
                && TryGetGenerationInfo(symbol, out var union))
            {
                return union;
            }

            return null!;
        }

        private GenerateResult? Generate(GenerationInfo? info, CancellationToken ct)
        {
            if (info != null)
            {
                if (info.Diagnostics.Count > 0)
                {
                    return new GenerateResult("", "", info.Diagnostics);
                }
                else
                {
                    var generator = new UnionGenerator(info.Namespace, info.Usings);
                    var text = generator.GenerateFile(info.Union);
                    var name = (string.IsNullOrEmpty(info.Namespace) ? info.Union.Name : $"{info.Namespace}_{info.Union.Name}").Replace('.', '_');
                    var fileName = $"{name}_UnionImplementation.cs";
                    return new GenerateResult(text, fileName, info.Diagnostics);
                }
            }

            return null;
        }

        private void GenerateOutput(SourceProductionContext context, GenerateResult? resultx)
        {
            if (resultx is GenerateResult result)
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
                    context.AddSource(result.FileName, result.Text);
                }
            }
        }

        public class GenerationInfo : IEquatable<GenerationInfo>
        {
            public string Namespace { get; }
            public IReadOnlyList<string> Usings { get; }
            public Union Union { get; }
            public IReadOnlyList<Diagnostic> Diagnostics { get; }

            public GenerationInfo(
                string @namespace, 
                IReadOnlyList<string> usings, 
                Union union,
                IReadOnlyList<Diagnostic> diagnostics
                )
            {
                this.Namespace = @namespace;
                this.Usings = usings;
                this.Union = union;
                this.Diagnostics = diagnostics;
            }

            public bool Equals(GenerationInfo generationInfo)
            {
                var isEqual = Namespace == generationInfo.Namespace
                    && Usings.SequenceEqual(generationInfo.Usings)
                    && Union.Equals(generationInfo.Union)
                    && Diagnostics.SequenceEqual(generationInfo.Diagnostics);
                return isEqual;
            }

            public override bool Equals(object obj) =>
                obj is GenerationInfo info && Equals(info);

            public override int GetHashCode() =>
                this.Union.GetHashCode();
        }

        private class GenerateResult
        {
            public string Text { get; }
            public string FileName { get; }
            public IReadOnlyList<Diagnostic> Diagnostics { get; }

            public GenerateResult(
                string text, 
                string fileName,
                IReadOnlyList<Diagnostic> diagnostics)
            {
                Text = text;
                FileName = fileName;
                Diagnostics = diagnostics;
            }
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

        /// <summary>
        /// Gets the info that drives the code generation for the union type.
        /// </summary>
        private bool TryGetGenerationInfo(INamedTypeSymbol unionType, out GenerationInfo info)
        {
            var cases = new List<UnionCase>();
            var diagnostics = new List<Diagnostic>();

            string namespaceName = null!;
            if (unionType.ContainingNamespace != null)
            {
                namespaceName = GetNamespaceName(unionType.ContainingNamespace);
            }

            var usingDirectives = GetDeclaredUsings(unionType);
            var usings = usingDirectives.Select(uz => uz.ToString()).ToArray();
            var usesToolkit = usings.Any(u => u.Contains(UnionGenerator.ToolkitNamespace));

            var modifiers = GetModifiers(unionType);

            if (IsTypeUnion(unionType, out var typeUnionAttribute))
            {
                var options = GetOptionsFromUnionAttribute(typeUnionAttribute);
                options = options.WithUseToolkit(usesToolkit);

                // get all cases declared for union type
                GetTypeCasesFromCaseAttributesOnUnion(unionType, options, cases, diagnostics);
                GetTypeCasesFromNestedTypes(unionType, cases, diagnostics);
                GetTypeCasesFromPartialFactoryMethods(unionType, options, cases, diagnostics);
                GetTypeCasesFromPartialFactoryProperties(unionType, options, cases, diagnostics);

                if (cases.Count > 0)
                {
                    var name = unionType.Name; // name w/o type parameters or namespace
                    var typeName = GetTypeShortName(unionType); // name w/o namespace

                    var union = new Union(
                        UnionKind.TypeUnion,
                        name,
                        typeName,
                        modifiers,
                        cases,
                        options
                        );

                    info = new GenerationInfo(namespaceName, usings, union, diagnostics);
                    return true;
                }
            }
            else if (IsTagUnion(unionType, out var tagUnionAttribute))
            {
                var options = GetOptionsFromUnionAttribute(tagUnionAttribute);

                // get all cases declared for union type
                GetTagCasesFromPartialFactoryMethods(unionType, options, cases, diagnostics);
                GetTagCasesFromPartialFactoryProperties(unionType, options, cases, diagnostics);
                GetTagCasesFromCaseAttributesOnUnion(unionType, options, cases, diagnostics);

                if (cases.Count > 0)
                {
                    var union = new Union(
                        UnionKind.TagUnion,
                        unionType.Name,
                        GetTypeShortName(unionType),
                        modifiers,
                        cases,
                        options
                        );

                    info = new GenerationInfo(namespaceName, usings, union, diagnostics);
                    return true;
                }
            }

            info = default!;
            return false;
        }

        private string? GetInferredCaseName(string name, string? namePrefix)
        {
            if (namePrefix == null)
                return null;
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

        private class CaseInfo
        {
            public string? Name { get; }
            public int TagValue { get; }
            public ITypeSymbol? Type { get; }
            public string? FactoryName { get; }
            public CaseFactoryKind? FactoryKind { get; }
            public string? AccessorName { get; }
            public CaseAccessorKind? AccessorKind { get; }

            public CaseInfo(
                string? name, 
                int tagValue, 
                ITypeSymbol? type, 
                string? factoryName, 
                CaseFactoryKind? factoryKind, 
                string? accessorName, 
                CaseAccessorKind? accessorKind)
            {
                this.Name = name;
                this.TagValue = tagValue;
                this.Type = type;
                this.FactoryName = factoryName;
                this.FactoryKind = factoryKind;
                this.AccessorName = accessorName;
                this.AccessorKind = accessorKind;
            }

            public static readonly CaseInfo Default = 
                new CaseInfo(
                    name: null, 
                    tagValue: -1, 
                    type: null, 
                    factoryName: null, 
                    factoryKind: null, 
                    accessorName: null, 
                    accessorKind: null
                    );
        }

        private CaseInfo GetCaseInfo(AttributeData? attr)
        {
            if (attr != null)
            {
                var name = attr.TryGetNamedArgument("Name", out var nameArg)
                    && nameArg.Kind == TypedConstantKind.Primitive
                    && nameArg.Value is string vName
                    ? vName
                    : null;

                var tagValue = attr.TryGetNamedArgument("TagValue", out var valueArg)
                    && valueArg.Kind == TypedConstantKind.Primitive
                    && valueArg.Value is int vValue
                    ? vValue
                    : -1;

                var type = attr.TryGetNamedArgument("Type", out var typeArg)
                    && typeArg.Kind == TypedConstantKind.Type
                    && typeArg.Value is INamedTypeSymbol vType
                    ? vType
                    : null;

                var factoryName = attr.TryGetNamedArgument("FactoryName", out var factoryNameArg)
                    && factoryNameArg.Kind == TypedConstantKind.Primitive
                    && factoryNameArg.Value is string vFactoryName
                    ? vFactoryName
                    : null;

                CaseFactoryKind? factoryKind = attr.TryGetNamedArgument("FactoryKind", out var factoryKindArg)
                    && factoryKindArg.Kind == TypedConstantKind.Primitive
                    && factoryKindArg.Value is string vFactoryKind && Enum.TryParse(vFactoryKind, ignoreCase: true, out CaseFactoryKind vFactoryKindValue)
                    ? vFactoryKindValue
                    : null;

                var accessorName = attr.TryGetNamedArgument("AccessorName", out var accessorNameArg)
                    && accessorNameArg.Kind == TypedConstantKind.Primitive
                    && accessorNameArg.Value is string vAccessorName
                    ? vAccessorName
                    : null;

                CaseAccessorKind? accessorKind = attr.TryGetNamedArgument("AccessorKind", out var accessorKindArg)
                    && accessorKindArg.Kind == TypedConstantKind.Primitive
                    && accessorKindArg.Value is string vAccessorKind && Enum.TryParse(vAccessorKind, ignoreCase: true, out CaseAccessorKind vAccessorKindValue)
                    ? vAccessorKindValue
                    : null;

                return new CaseInfo(
                    name: name,
                    tagValue: tagValue,
                    type: type,
                    factoryName: factoryName,
                    factoryKind: factoryKind,
                    accessorName: accessorName,
                    accessorKind: accessorKind
                );
            }
            else
            {
                return CaseInfo.Default;
            }
        }

        private void GetTypeCasesFromCaseAttributesOnUnion(
            INamedTypeSymbol unionType, 
            UnionOptions options, 
            List<UnionCase> cases,
            List<Diagnostic> diagnostics)
        {
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

                    var factoryKind =
                        caseInfo.FactoryKind == CaseFactoryKind.Property && caseType.SingletonAccessor != null ? CaseFactoryKind.Property
                        : caseInfo.FactoryKind == CaseFactoryKind.None && caseType.SingletonAccessor != null && caseInfo.TagValue == 0 ? CaseFactoryKind.None
                        : CaseFactoryKind.Method;

                    var factoryParameters = factoryKind == CaseFactoryKind.Method
                        ? new[] { GetCaseValue("value", caseInfo.Type) }
                        : Array.Empty<UnionCaseValue>();

                    var accessorKind =
                        caseInfo.AccessorKind == CaseAccessorKind.None && caseType.SingletonAccessor != null ? CaseAccessorKind.None
                        : caseInfo.AccessorKind != null ? caseInfo.AccessorKind.Value
                        : CaseAccessorKind.Property;

                    var accessibility = GetMemberAccessibilityForType(caseInfo.Type);

                    var typeCase = new UnionCase(
                        name: caseName,
                        type: caseType,
                        tagValue: caseInfo.TagValue,
                        factoryName: caseInfo.FactoryName,
                        factoryParameters: factoryParameters,
                        factoryKind: factoryKind,
                        accessorName: caseInfo.AccessorName,
                        accessorKind: accessorKind,
                        accessibility: accessibility
                        );

                    cases.Add(typeCase);
                }
            }
        }

        private void GetTypeCasesFromNestedTypes(
            INamedTypeSymbol unionType, 
            List<UnionCase> cases,
            List<Diagnostic> diagnostics)
        {
            var nestedTypes = unionType
                .GetTypeMembers()
                .OfType<INamedTypeSymbol>()
                .Where(nt => nt.DeclaredAccessibility == Accessibility.Public
                        || nt.DeclaredAccessibility == Accessibility.Internal)
                .ToList();

            foreach (var nestedType in nestedTypes)
            {
                if (nestedType.TryGetAttribute(CaseAttributeName, out var attr))
                {
                    var caseInfo = GetCaseInfo(attr);

                    var caseName = caseInfo.Name ?? nestedType.Name;

                    var caseType = GetValueType(nestedType);

                    var factoryKind =
                        caseInfo.FactoryKind == CaseFactoryKind.Property && caseType.SingletonAccessor != null ? CaseFactoryKind.Property
                        : caseInfo.FactoryKind == CaseFactoryKind.None && caseType.SingletonAccessor != null && caseInfo.TagValue == 0 ? CaseFactoryKind.None
                        : CaseFactoryKind.Method;

                    var factoryParameters = factoryKind == CaseFactoryKind.Method
                        ? new[] { GetCaseValue("value", nestedType) }
                        : Array.Empty<UnionCaseValue>();

                    var accessorKind =
                        caseInfo.AccessorKind == CaseAccessorKind.None && caseType.SingletonAccessor != null ? CaseAccessorKind.None
                        : caseInfo.AccessorKind != null ? caseInfo.AccessorKind.Value
                        : CaseAccessorKind.Property;

                    var accessibility = GetAccessibility(nestedType.DeclaredAccessibility);

                    var typeCase = new UnionCase(
                        name: caseName,
                        type: caseType,
                        tagValue: caseInfo.TagValue,
                        caseInfo.FactoryName,
                        factoryParameters: factoryParameters,
                        factoryKind: factoryKind,
                        accessorName: caseInfo.AccessorName,
                        accessorKind: accessorKind,
                        accessibility: accessibility
                        );

                    cases.Add(typeCase);
                }
            }
        }

        private void GetTypeCasesFromPartialFactoryMethods(
            INamedTypeSymbol unionType, 
            UnionOptions options, 
            List<UnionCase> cases,
            List<Diagnostic> diagnostics)
        {
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

            var inferredCaseNames = new Dictionary<IMethodSymbol, string>(SymbolEqualityComparer.Default);
            var factoryNames = factoryMethods.Select(m => m.Name).ToList();
            var typeNames = factoryMethods.Select(GetCaseType).OfType<INamedTypeSymbol>().Select(nt => nt.Name).ToList();
            var parameterNames = factoryMethods.Where(m => m.Parameters.Length == 1).Select(m => m.Parameters[0].Name).ToList();

            if (factoryNames.Distinct().Count() == factoryMethods.Count)
            {
                // use just the unique part of the factory name
                var namePrefix = GetCommonPrefix(factoryNames);
                foreach (var method in factoryMethods)
                {
                    var name = GetInferredCaseName(method.Name, namePrefix) ?? method.Name;
                    inferredCaseNames.Add(method, MakePascalCase(name));
                }
            }
            else if (parameterNames.Distinct().Count() == factoryMethods.Count)
            {
                for (int i = 0; i < factoryMethods.Count; i++)
                {
                    inferredCaseNames.Add(factoryMethods[i], MakePascalCase(parameterNames[i]));
                }
            }
            else if (typeNames.Distinct().Count() == factoryMethods.Count)
            {
                for (int i = 0; i < factoryMethods.Count; i++)
                {
                    inferredCaseNames.Add(factoryMethods[i], MakePascalCase(typeNames[i]));
                }
            }

            foreach (var method in factoryMethods)
            {
                method.TryGetAttribute(CaseAttributeName, out var attribute);
                var caseInfo = GetCaseInfo(attribute);

                var type = caseInfo.Type;

                // parameter type is always the correct type, regardless of attribute
                if (method.Parameters.Length == 1)
                    type = method.Parameters[0].Type;

                if (type != null)
                {
                    var caseType = GetValueType(type);

                    var caseName = caseInfo.Name
                        ?? (inferredCaseNames.TryGetValue(method, out var name) 
                            ? name 
                            : "Type" + (cases.Count + 1));

                    var factoryName = method.Name;
                    var factoryParameters = GetCaseParameters(method.Parameters);
                    var factoryModifiers = GetModifiers(method);
                    var accessibility = GetAccessibility(method.DeclaredAccessibility);

                    var accessorKind =
                        caseInfo.AccessorKind == CaseAccessorKind.None && caseType.SingletonAccessor != null ? CaseAccessorKind.None
                        : caseInfo.AccessorKind != null ? caseInfo.AccessorKind.Value
                        : CaseAccessorKind.Property;

                    var typeCase = new UnionCase(
                        name: caseName,
                        type: caseType,
                        tagValue: caseInfo.TagValue,
                        factoryName: factoryName,
                        factoryParameters: factoryParameters,
                        factoryKind: CaseFactoryKind.Method,
                        factoryModifiers: factoryModifiers,
                        accessorName: caseInfo.AccessorName,
                        accessorKind: accessorKind,
                        accessibility: accessibility
                        );

                    cases.Add(typeCase);
                }

            }
        }

        private static string MakePascalCase(string name)
        {
            if (name.Length > 1 && !char.IsUpper(name[0]))
            {
                return char.ToUpper(name[0]) + name.Substring(1);
            }
            else if (name.Length == 1 && !char.IsUpper(name[0]))
            {
                return name.ToUpper();
            }
            else
            {
                return name;
            }
        }

        private ITypeSymbol? GetCaseType(ISymbol factory)
        {
            factory.TryGetAttribute(CaseAttributeName, out var attribute);
            var caseInfo = GetCaseInfo(attribute);

            if (caseInfo.Type != null)
                return caseInfo.Type;

            if (factory is IMethodSymbol method
                && method.Parameters.Length == 1)
            {
                return method.Parameters[0].Type;
            }

            return null;
        }

        private void GetTypeCasesFromPartialFactoryProperties(
            INamedTypeSymbol unionType, 
            UnionOptions options, 
            List<UnionCase> cases,
            List<Diagnostic> diagnostics)
        {
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

                var type = caseInfo.Type;
                if (type != null)
                {
                    var caseType = GetValueType(type);
                    var caseName = caseInfo.Name 
                        ?? (caseInfo.Type is INamedTypeSymbol namedType && namedType.TypeParameters.Length == 0 
                            ? namedType.Name 
                            : prop.Name);

                    var factoryName = prop.Name;
                    var factoryParameters = GetCaseParameters(prop.Parameters);
                    var factoryModifiers = GetModifiers(prop);
                    var accessibility = GetAccessibility(prop.DeclaredAccessibility);

                    var accessorKind =
                        caseInfo.AccessorKind == CaseAccessorKind.None && caseType.SingletonAccessor != null ? CaseAccessorKind.None
                        : caseInfo.AccessorKind != null ? caseInfo.AccessorKind.Value
                        : CaseAccessorKind.Property;

                    var typeCase = new UnionCase(
                        name: caseName,
                        type: caseType,
                        tagValue: caseInfo.TagValue,
                        factoryName: factoryName,
                        factoryParameters: factoryParameters,
                        factoryKind: CaseFactoryKind.Property,
                        factoryModifiers: factoryModifiers,
                        accessorName: caseInfo.AccessorName,
                        accessorKind: accessorKind,
                        accessibility: accessibility
                        );

                    cases.Add(typeCase);
                }
            }
        }

        private void GetTagCasesFromCaseAttributesOnUnion(
            INamedTypeSymbol unionSymbol, 
            UnionOptions options, 
            List<UnionCase> cases,
            List<Diagnostic> diagnostics)
        {
            foreach (var attr in unionSymbol.GetAttributes(CaseAttributeName))
            {
                var caseInfo = GetCaseInfo(attr);               
                var caseName = caseInfo.Name ?? "Case" + (cases.Count + 1);

                var factoryKind =
                    caseInfo.FactoryKind == CaseFactoryKind.Method ? CaseFactoryKind.Method
                    : caseInfo.FactoryKind == CaseFactoryKind.None && caseInfo.TagValue == 0 ? CaseFactoryKind.None
                    : CaseFactoryKind.Property;

                var accessorKind =
                    caseInfo.AccessorKind == CaseAccessorKind.None ? CaseAccessorKind.None
                    : caseInfo.AccessorKind == CaseAccessorKind.Property ? CaseAccessorKind.Property
                    : caseInfo.AccessorKind == CaseAccessorKind.Method ? CaseAccessorKind.Method
                    : CaseAccessorKind.Property;

                var tagCase = new UnionCase(
                    caseName,
                    type: null,
                    tagValue: caseInfo.TagValue,
                    factoryName: caseInfo.FactoryName,
                    factoryParameters: null,
                    factoryKind: factoryKind,
                    accessorName: caseInfo.AccessorName,
                    accessorKind: accessorKind,
                    accessibility: "public"
                    );

                cases.Add(tagCase);
            }
        }

        private void GetTagCasesFromPartialFactoryMethods(
            INamedTypeSymbol unionType, 
            UnionOptions options, 
            List<UnionCase> cases,
            List<Diagnostic> diagnostics)
        {
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

                    var inferredCaseName = GetInferredCaseName(method.Name, namePrefix);
                    var caseInfo = GetCaseInfo(attr);

                    var caseName = caseInfo.Name ?? inferredCaseName ?? method.Name;

                    // the factory is already specified, so it must use the member name.
                    var factoryName = method.Name;
                    var factoryParameters = GetCaseParameters(method.Parameters);
                    var factoryModifiers = GetModifiers(method);
                    var accessibility = GetAccessibility(method.DeclaredAccessibility);

                    var accessorKind =
                        caseInfo.AccessorKind == CaseAccessorKind.None && factoryParameters.Count == 0 ? CaseAccessorKind.None
                        : caseInfo.AccessorKind == CaseAccessorKind.Method ? CaseAccessorKind.Method
                        : CaseAccessorKind.Property;

                    var tagCase = new UnionCase(
                        name: caseName,
                        type: null,
                        tagValue: caseInfo.TagValue,
                        factoryName: factoryName,
                        factoryParameters: factoryParameters,
                        factoryKind: CaseFactoryKind.Method,
                        factoryModifiers: factoryModifiers,
                        accessorName: caseInfo.AccessorName,
                        accessorKind: accessorKind,
                        accessibility: accessibility
                        );

                    cases.Add(tagCase);
                }
            }
        }

        private static bool CanAccessAsTuple(IReadOnlyList<UnionCaseValue> caseValues)
        {
            return caseValues.All(v => v.Type.Kind != TypeKind.RefStruct);
        }

        private void GetTagCasesFromPartialFactoryProperties(
            INamedTypeSymbol unionType, 
            UnionOptions options, 
            List<UnionCase> cases,
            List<Diagnostic> diagnostics)
        {
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
                    var factoryModifiers = GetModifiers(property);
                    var accessibility = GetAccessibility(property.DeclaredAccessibility);

                    var accessorKind =
                        caseInfo.AccessorKind == CaseAccessorKind.None ? CaseAccessorKind.None
                        : caseInfo.AccessorKind == CaseAccessorKind.Method ? CaseAccessorKind.Method
                        : CaseAccessorKind.Property;

                    var tagCase =
                        new UnionCase(
                            name: caseName,
                            type: null,
                            tagValue: caseInfo.TagValue,
                            factoryName: factoryName,
                            factoryParameters: new UnionCaseValue[] { },
                            factoryKind: CaseFactoryKind.Property,
                            factoryModifiers: factoryModifiers,
                            accessorName: caseInfo.AccessorName,
                            accessorKind: accessorKind,
                            accessibility: accessibility
                            );

                    cases.Add(tagCase);
                }
            }
        }

        private IReadOnlyList<UnionCaseValue> GetCaseParameters(IReadOnlyList<IParameterSymbol> parameters)
        {
            return parameters.Select(p => GetCaseValue(p)).ToArray();
        }

        private UnionCaseValue GetCaseValue(IParameterSymbol parameter, string? name = null)
        {
            return GetCaseValue(name ?? parameter.Name, parameter.Type);
        }

        private UnionCaseValue GetCaseValue(string name, ITypeSymbol type, string? singletonAccessor = null)
        {
            var kind = GetTypeKind(type);
            var typeName = GetTypeFullName(type);
            var vtype = new UnionValueType(typeName, kind, singletonAccessor);
            var nestedParameters = GetNestedMembers(type);
            return new UnionCaseValue(name, vtype, nestedParameters);
        }

        private UnionValueType GetValueType(ITypeSymbol type)
        {
            var kind = GetTypeKind(type);
            var typeName = GetTypeFullName(type);
            TryGetSingletonAccessor(type, out var accessor);
            return new UnionValueType(typeName, kind, accessor);
        }

        private IReadOnlyList<UnionCaseValue> GetNestedMembers(ITypeSymbol caseSymbol)
        {
            if (caseSymbol.IsValueType
                && caseSymbol is INamedTypeSymbol nt)
            {
                if (caseSymbol.IsRecord)
                {
                    // break down record into members based on primary constructor..
                    // For records, the names of the parameters are the same as the names of the properties.
                    var primaryConstructor = nt.Constructors.FirstOrDefault(c => c.Parameters.Length > 0);
                    if (primaryConstructor != null)
                    {
                        return primaryConstructor.Parameters.Select(p => GetCaseValue(p)).ToArray();
                    }
                }
                else if (caseSymbol.IsTupleType)
                {
                    var constructor = nt.Constructors.FirstOrDefault(c => c.Parameters.Length > 0);
                    if (constructor != null)
                    {
                        return constructor.Parameters.Select(p => GetCaseValue(p, GetTuplePropertyName(p))).ToArray();
                    }
                }
            }

            return Array.Empty<UnionCaseValue>();
        }

        private static string GetTuplePropertyName(IParameterSymbol parameter)
        {
            if (parameter.Name.StartsWith("item"))
            {
                return "Item" + parameter.Name.Substring(4);
            }
            else
            {
                return parameter.Name;
            }
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
                        // trust value tuples to not have hidden metadata
                        if (IsOverlappableStruct(type))
                        {
                            return TypeKind.OverlappableTuple;
                        }
                        else
                        {
                            return TypeKind.NonOverlappableTuple;
                        }
                    }
                    else if (type.IsRecord)
                    {
                        var isDecomposable = IsDecomposableRecordStruct(type);
                        var isOverlappable = IsOverlappableStruct(type);
                        
                        if (type.IsDeclaredInSource())
                        {
                            if (isOverlappable && isDecomposable)
                            {
                                return TypeKind.OverlappableDecomposableLocalRecordStruct;
                            }
                            else if (isOverlappable)
                            {
                                return TypeKind.OverlappableLocalRecordStruct;
                            }
                            else if (isDecomposable)
                            {
                                return TypeKind.DecomposableLocalRecordStruct;
                            }
                            else
                            {
                                return TypeKind.NonOverlappableStruct;
                            }
                        }
                        else
                        {
                            if (isOverlappable && isDecomposable)
                            {
                                return TypeKind.OverlappableDecomposableForeignRecordStruct;
                            }
                            else if (isOverlappable)
                            {
                                return TypeKind.OverlappableForeignRecordStruct;
                            }
                            else if (isDecomposable)
                            {
                                return TypeKind.DecomposableForeignRecordStruct;
                            }
                            else
                            {
                                return TypeKind.NonOverlappableStruct;
                            }
                        }
                    }
                    else if (type.IsRefLikeType)
                    {
                        return TypeKind.RefStruct;
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

        private static bool IsDecomposableRecordStruct(ITypeSymbol type)
        {
            if (type.IsValueType && type.IsRecord && type is INamedTypeSymbol nt)
            {
                // check to see if there are the same number of fields as parameters in the deconstructor.
                // If there are more, then the user has added extra 'ghost' data that we may not be able to 
                // access via deconstruction or to be able to reconstruct later.
                // note: not sure how to easily determine which is primary constructors, since use may add additional constructors,
                // so are using the deconstructor, since user will not define one.
                var fieldCount = nt.GetMembers().OfType<IFieldSymbol>().Count(f => !f.IsStatic);
                var deconstructor = nt.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(m => m.Name == "Deconstruct");
                if (deconstructor != null)
                {
                    return deconstructor.Parameters.Length == fieldCount;
                }
                else
                {
                    return false;
                }
            }

            return false;
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

        private static string GetModifiers(ISymbol symbol)
        {
            // try to get full set of modifiers as in source
            var location = symbol.Locations.FirstOrDefault(loc => loc.IsInSource);
            if (location != null && location.SourceTree != null)
            {
                var declNode = location.SourceTree.GetRoot().FindNode(location.SourceSpan);
                switch (declNode)
                {
                    case StructDeclarationSyntax decl:
                        return GetModifiers(decl.Modifiers);
                    case ClassDeclarationSyntax decl:
                        return GetModifiers(decl.Modifiers);
                    case RecordDeclarationSyntax decl:
                        return GetModifiers(decl.Modifiers);
                    case InterfaceDeclarationSyntax decl:
                        return GetModifiers(decl.Modifiers);
                    case MethodDeclarationSyntax decl:
                        return GetModifiers(decl.Modifiers);
                    case PropertyDeclarationSyntax decl:
                        return GetModifiers(decl.Modifiers);
                    case FieldDeclarationSyntax decl:
                        return GetModifiers(decl.Modifiers);
                }
            }

            return GetAccessibility(symbol.DeclaredAccessibility);

        }

        private static string GetModifiers(SyntaxTokenList modifiers)
        {
            // don't use actual source, so we are not dependent on trivia.
            return string.Join(" ", modifiers.Select(m => m.Text));
        }

        /// <summary>
        /// Returns the accessibilty that should be used for members that refer to this type in their signature.
        /// </summary>
        private static string GetMemberAccessibilityForType(ITypeSymbol symbol)
        {
            if (symbol.DeclaredAccessibility == Accessibility.Public)
            {
                if (symbol.ContainingType != null)
                {
                    return GetMemberAccessibilityForType(symbol.ContainingType);
                }
                else
                {
                    return "public";
                }
            }

            // if type is not public then the generated member must not be declared public or suffer the wrath of C#.
            // internal is okay, since otherwise the type is somehow accessible to the location the union type is being generated to.
            return "internal";
        }

        /// <summary>
        /// Gets the accessibility as C# text.
        /// </summary>
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
            var name = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return name;
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