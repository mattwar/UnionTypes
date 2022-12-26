using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
                GenerateUnionType(unionType, context);
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
            return symbol.GetAttributes().Any(ad => ad.AttributeClass?.Name == "UnionAttribute");
        }

        private void GenerateUnionType(INamedTypeSymbol unionType, GeneratorExecutionContext context)
        {
            var text = GenerateUnionType(unionType);
            context.AddSource($"{unionType.Name}_UnionImplementation.cs", text);
        }

        public string GenerateUnionType(INamedTypeSymbol unionType)
        {
            // get all cases declared for union type
            var cases = GetCasesFromNestedRecords(unionType)
                .Concat(GetCasesFromPartialFactories(unionType))
                .Concat(GetCasesFromTypesAttribute(unionType))
                .Concat(GetCasesFromTagsAttribute(unionType))
                .ToArray();

            string namespaceName = null!;
            if (unionType.ContainingNamespace != null)
            {
                namespaceName = GetNamespaceName(unionType.ContainingNamespace);
            }

            var usings = GetDeclaredUsings(unionType);

            var accessibility = GetAccessibility(unionType.DeclaredAccessibility);

            var union = new Union(unionType.Name, GetTypeParameterList(unionType), accessibility, cases);

            var generator = new UnionGenerator(
                namespaceName,
                usings,
                generateCaseTypes: false /* already declared in source */
                );

            var text = generator.GenerateFile(union);

            return text;
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

        private IReadOnlyList<Case> GetCasesFromNestedRecords(INamedTypeSymbol unionType)
        {
            return unionType
                .GetTypeMembers()
                .OfType<INamedTypeSymbol>()
                .Where(nt => nt.IsRecord
                    && (nt.DeclaredAccessibility == Accessibility.Public || nt.DeclaredAccessibility == Accessibility.Internal))
                .Select(nt => CreateTypeCase(nt.Name, nt, isNested: true, isPartial: false, factoryName: "Create" + nt.Name))
                .ToArray();
        }

        private IReadOnlyList<Case> GetCasesFromTypesAttribute(INamedTypeSymbol unionType)
        {
            var unionTypesAttribute = unionType.GetAttributes().FirstOrDefault(ad => ad.AttributeClass?.Name == "UnionTypesAttribute");
            if (unionTypesAttribute != null
                && unionTypesAttribute.ConstructorArguments.Length > 0
                && unionTypesAttribute.ConstructorArguments[0].Kind == TypedConstantKind.Array)
            {
                var cases = new List<Case>();

                foreach (var val in unionTypesAttribute.ConstructorArguments[0].Values)
                {
                    if (val.Kind == TypedConstantKind.Type && val.Value is INamedTypeSymbol nt)
                    {
                        cases.Add(CreateTypeCase(nt.Name, nt, isNested: false, isPartial: false, factoryName: nt.Name));
                    }
                }

                return cases;
            }

            return Array.Empty<Case>();
        }

        private IReadOnlyList<Case> GetCasesFromTagsAttribute(INamedTypeSymbol unionType)
        {
            var cases = new List<Case>();

            var unionTagsAttribute = unionType.GetAttributes().FirstOrDefault(ad => ad.AttributeClass?.Name == "UnionTagsAttribute");
            if (unionTagsAttribute != null
                && unionTagsAttribute.ConstructorArguments.Length > 0
                && unionTagsAttribute.ConstructorArguments[0].Kind == TypedConstantKind.Array)
            {
                foreach (var val in unionTagsAttribute.ConstructorArguments[0].Values)
                {
                    if (val.Kind == TypedConstantKind.Primitive && val.Value is string tagName)
                    {
                        cases.Add(new Case(TypeKind.Tag, tagName));
                    }
                }
            }

            return cases;
        }

        private IReadOnlyList<Case> GetCasesFromPartialFactories(INamedTypeSymbol unionType)
        {
            var cases = new List<Case>();

            // from partial CreateXXX methods
            var factoryMethods = unionType.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.IsPartialDefinition 
                         && m.IsStatic
                         && m.TypeParameters.Length == 0 
                         && SymbolEqualityComparer.Default.Equals(m.ReturnType, unionType))
                .ToList();

            foreach (var method in factoryMethods)
            {
                if (method.Parameters.Length == 1 
                    && method.Parameters[0].Type is INamedTypeSymbol nt
                    && method.Name == nt.Name)
                {
                    // this is an external type
                    cases.Add(CreateTypeCase(method.Name, nt, isNested: false, isPartial: true, factoryName: method.Name));
                }
                else
                {
                    // this is a tag with possible values
                    cases.Add(CreateTagCase(method.Name, method, isPartial: true, factoryName: method.Name));
                }
            }

            return cases;
        }

        private Case CreateTypeCase(string caseName, INamedTypeSymbol caseSymbol, bool isNested, bool isPartial, string factoryName)
        {
            var typeName = isNested ? caseSymbol.Name : GetTypeFullName(caseSymbol);
            var caseValues = Array.Empty<Value>();

            var primaryConstructor = caseSymbol.Constructors.FirstOrDefault(c => c.Parameters.Length > 0);
            if (primaryConstructor != null)
            {
                caseValues = primaryConstructor.Parameters.Select(p =>
                    new Value(GetValueTypeKind(p.Type), p.Name, GetTypeFullName(p.Type)))
                    .ToArray();
            }

            return new Case(
                GetCaseTypeKind(caseSymbol),
                caseName,
                typeName,
                GetAccessibility(caseSymbol.DeclaredAccessibility),
                isPartial: isPartial,
                factoryName: factoryName,
                generate: false,
                values: caseValues);
        }

        private Case CreateTagCase(string caseName, IMethodSymbol caseMethod, bool isPartial, string factoryName)
        {
            var values = caseMethod.Parameters.Select(p => new Value(GetValueTypeKind(p.Type), p.Name, GetTypeFullName(p.Type))).ToArray();
            return new Case(TypeKind.Tag, caseName, "", "public", isPartial: true, factoryName: factoryName, generate: false, values);
        }

        private static TypeKind GetCaseTypeKind(ITypeSymbol type)
        {
            if (type is INamedTypeSymbol nt && nt.IsRecord && nt.IsValueType)
                return TypeKind.RecordStruct;
            return GetValueTypeKind(type);
        }

        private static TypeKind GetValueTypeKind(ITypeSymbol type)
        {
            switch (type.TypeKind)
            {
                case CATypeKind.Enum:
                    return TypeKind.Primitive;
                case CATypeKind.Struct:
                    return TypeKind.Struct;
                case CATypeKind.Interface:
                    return TypeKind.Interface;
                case CATypeKind.Class:
                case CATypeKind.Array:
                case CATypeKind.Dynamic:
                case CATypeKind.Delegate:
                    return TypeKind.Class;
                case CATypeKind.TypeParameter:
                    return TypeKind.TypeParameter;

                default:
                    if (type is INamedTypeSymbol nt && IsPrimitive(nt))
                        return TypeKind.Primitive;
                    return TypeKind.Unknown;
            }
        }

        private static bool IsPrimitive(INamedTypeSymbol type)
        {
            switch (type.SpecialType)
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
                case SpecialType.System_String:
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
            if (type is INamedTypeSymbol nt)
            {
                var shortName = GetTypeShortName(nt);

                if (nt.ContainingType != null)
                {
                    return GetTypeFullName(nt.ContainingType) + "." + shortName;
                }
                else if (nt.ContainingNamespace != null
                        && !nt.ContainingNamespace.IsGlobalNamespace)
                {
                    return GetNamespaceName(nt.ContainingNamespace) + "." + shortName;
                }
                else
                {
                    return shortName;
                }
            }
            else
            {
                return type.Name;
            }
        }

        private static string GetTypeShortName(ITypeSymbol type)
        {
            if (type is INamedTypeSymbol nt)
            {
                var typeParameterList = GetTypeParameterList(nt);
                return string.IsNullOrEmpty(typeParameterList) ? nt.Name : nt.Name + typeParameterList;
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
    }
}