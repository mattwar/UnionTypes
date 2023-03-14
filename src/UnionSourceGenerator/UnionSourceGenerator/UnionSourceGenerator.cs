using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
                .Select(nt => CreateTypeCase(nt.Name, nt, isNested: true, isPartial: false, factoryName: "Create"))
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
                        cases.Add(CreateTypeCase(nt.Name, nt, isNested: false, isPartial: false, factoryName: "Create"));
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
                        var tagCase = new Case(
                            CaseKind.Tag,
                            tagName,
                            accessibility: "public",
                            isPartial: false,
                            factoryName: tagName,
                            generateCaseType: false,
                            parameters: null);
                        cases.Add(tagCase);
                    }
                }
            }

            return cases;
        }

        private IReadOnlyList<Case> GetCasesFromPartialFactories(INamedTypeSymbol unionType)
        {
            var cases = new List<Case>();

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
                if (method.Parameters.Length == 1
                    && method.Parameters[0].Type is INamedTypeSymbol nt)
                {
                    if (method.Name == nt.Name)
                    {
                        cases.Add(CreateFactoryTypeCase(method.Name, method, isNested: false, isPartial: true, factoryName: method.Name));
                        continue;
                    }
                    else if (method.Name == "Create")
                    {
                        cases.Add(CreateFactoryTypeCase(nt.Name, method, isNested: false, isPartial: true, factoryName: method.Name));
                        continue;
                    }
                    else if (method.Name == "Create" + nt.Name)
                    {
                        cases.Add(CreateFactoryTypeCase(nt.Name, method, isNested: false, isPartial: true, factoryName: method.Name));
                        continue;
                    }
                }

                // this is a tag with possible values
                var tagCaseName = method.Name.StartsWith("Create") && method.Name.Length > 6
                    ? method.Name.Substring(6)
                    : method.Name;

                cases.Add(CreateTagCase(tagCaseName, method, isPartial: true, factoryName: method.Name));
            }

            return cases;
        }

        private Case CreateFactoryTypeCase(string caseName, IMethodSymbol methodSymbol, bool isNested, bool isPartial, string factoryName)
        {
            var caseParameters = GetCaseParameters(methodSymbol.Parameters);

            return new Case(
                CaseKind.Type,
                caseName,
                GetAccessibility(methodSymbol.DeclaredAccessibility),
                isPartial: isPartial,
                factoryName: factoryName,
                generateCaseType: false,
                caseParameters);
        }

        private Case CreateTypeCase(string caseName, ITypeSymbol caseTypeSymbol, bool isNested, bool isPartial, string factoryName)
        {
            var caseParameters = new[] { GetCaseParameter(caseTypeSymbol, "value") };

            return new Case(
                CaseKind.Type,
                caseName,
                GetAccessibility(caseTypeSymbol.DeclaredAccessibility),
                isPartial: isPartial,
                factoryName: factoryName,
                generateCaseType: false,
                caseParameters);
        }

        private IReadOnlyList<CaseParameter> GetCaseParameters(IReadOnlyList<IParameterSymbol> parameters)
        {
            return parameters.Select(p => GetCaseParameter(p)).ToArray();
        }

        private CaseParameter GetCaseParameter(IParameterSymbol parameter)
        {
            return GetCaseParameter(parameter.Type, parameter.Name);
        }

        private CaseParameter GetCaseParameter(ITypeSymbol type, string name)
        {
            var kind = GetParameterKind(type);
            var typeName = GetTypeFullName(type);
            var nestedParameters = GetNestedParameters(type);
            return new CaseParameter(kind, name, typeName, nestedParameters);
        }

        private IReadOnlyList<CaseParameter> GetNestedParameters(ITypeSymbol caseSymbol)
        {
            if (caseSymbol.IsValueType && caseSymbol.IsRecord && caseSymbol is INamedTypeSymbol nt)
            {
                // look for nested case parameters from records and tuple arguments
                var primaryConstructor = nt.Constructors.FirstOrDefault(c => c.Parameters.Length > 0);
                if (primaryConstructor != null)
                {
                    return primaryConstructor.Parameters.Select(p => GetCaseParameter(p)).ToArray();
                }
            }

            return Array.Empty<CaseParameter>();
        }

        private Case CreateTagCase(string caseName, IMethodSymbol caseMethod, bool isPartial, string factoryName)
        {
            var values = caseMethod.Parameters.Select(p => new CaseParameter(GetParameterKind(p.Type), p.Name, GetTypeFullName(p.Type))).ToArray();
            return new Case(CaseKind.Tag, caseName, "public", isPartial: isPartial, factoryName: factoryName, generateCaseType: false, values);
        }

        private static ParameterKind GetCaseTypeKind(ITypeSymbol type)
        {
            if (type is INamedTypeSymbol nt && nt.IsRecord && nt.IsValueType)
                return ParameterKind.RecordStruct;
            return GetParameterKind(type);
        }

        private static ParameterKind GetParameterKind(ITypeSymbol type)
        {
            switch (type.TypeKind)
            {
                case CATypeKind.Enum:
                    return ParameterKind.PrimitiveStruct;
                case CATypeKind.Struct:
                    if (IsPrimitiveValType(type))
                        return ParameterKind.PrimitiveStruct;
                    else if (type.IsTupleType)
                        return ParameterKind.Tuple;
                    else if (type.IsRecord)
                        return ParameterKind.RecordStruct;
                    else if (IsOverlappableValStruct(type))
                        return ParameterKind.OverlappableValStruct;
                    //else if (IsOverlappableRefStruct(type))            // these don't align well when overlayed
                    //    return ParameterKind.OverlappableRefStruct;
                    else
                        return ParameterKind.NonOverlappableStruct;
                case CATypeKind.Interface:
                    return ParameterKind.Interface;
                case CATypeKind.Class:
                case CATypeKind.Array:
                case CATypeKind.Dynamic:
                case CATypeKind.Delegate:
                    return ParameterKind.Class;
                case CATypeKind.TypeParameter:
                    var tp = (ITypeParameterSymbol)type;
                    if (tp.HasReferenceTypeConstraint)
                        return ParameterKind.TypeParameter_RefConstrained;
                    return ParameterKind.TypeParameter;
                default:
                    return ParameterKind.Unknown;
            }
        }

        private static bool IsDecomposableType(ITypeSymbol type)
        {
            return type.IsValueType
                && (type.IsTupleType || type.IsRecord);
        }

        private static bool IsOverlappableRefType(ITypeSymbol type)
        {
            switch (type.TypeKind)
            {
                case CATypeKind.Enum:
                    return false;
                case CATypeKind.Struct:
                    return IsOverlappableRefStruct(type);
                case CATypeKind.Interface:
                case CATypeKind.Class:
                case CATypeKind.Array:
                case CATypeKind.Dynamic:
                case CATypeKind.Delegate:
                    return true;
                case CATypeKind.TypeParameter:
                    return false; // what about constrained type parameters?
                default:
                    return false;
            }
        }

        private static bool IsOverlappableValType(ITypeSymbol type)
        {
            switch (type.TypeKind)
            {
                case CATypeKind.Enum:
                    return true;
                case CATypeKind.Struct:
                    return IsPrimitiveValType(type)
                        || IsOverlappableValStruct(type);
                case CATypeKind.Interface:
                case CATypeKind.Class:
                case CATypeKind.Array:
                case CATypeKind.Dynamic:
                case CATypeKind.Delegate:
                    return false;
                case CATypeKind.TypeParameter:
                    return false; // what about constrained type parameters?
                default:
                    return false;
            }
        }

        private static bool IsOverlappableRefStruct(ITypeSymbol type)
        {
            return type.IsValueType
                && type.GetMembers().OfType<IFieldSymbol>().All(f => IsOverlappableRefType(f.Type));
        }

        private static bool IsOverlappableValStruct(ITypeSymbol type)
        {
            return type.IsValueType
                && type.GetMembers().OfType<IFieldSymbol>().All(f => IsOverlappableValType(f.Type));
        }

        private static bool IsPrimitiveValType(ITypeSymbol type)
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
    }
}