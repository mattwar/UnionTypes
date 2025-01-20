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

    #region public types to describe unions
    public class Union
    {
        public UnionKind Kind { get; }

        /// <summary>
        /// The name of the union type (w/o type parameters).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The name of the union type (with type parameters)
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Accessibility of the union: public, internal, private
        /// </summary>
        public string? Accessibility { get; }

        public UnionOptions Options { get; }

        public IReadOnlyList<UnionCase> Cases { get; }

        /// <summary>
        /// If true, then parameters that are overlappable value types are collected into overlappable structs.
        /// </summary>
        public bool OverlapFields { get; }

        public Union(
            UnionKind kind,
            string name, 
            string typeName, 
            string? accessibility,
            IReadOnlyList<UnionCase> cases,
            UnionOptions options)
        {
            this.Kind = kind;
            this.Name = name;
            this.TypeName = typeName;
            this.Accessibility = accessibility;
            this.Cases = cases;
            this.Options = options;
        }
    }

    public enum UnionKind
    {
        /// <summary>
        /// A union of individual type values
        /// </summary>
        TypeUnion,

        /// <summary>
        /// A union of different states with optional parameters
        /// </summary>
        TagUnion
    }

    public class UnionOptions
    {
        /// <summary>
        /// Allow data fields of the same type are shared across all cases.
        /// </summary>
        public bool ShareSameTypeFields { get; }

        /// <summary>
        /// Allow data fields of reference type be shared across all cases.
        /// </summary>
        public bool ShareReferenceFields { get; }

        /// <summary>
        /// Allow structs with only overlappable fields to be overlapped with other case data.
        /// </summary>
        public bool OverlapStructs { get; }

        /// <summary>
        /// Allow structs declared outside the current compilation unit to be overlapped with other case data.
        /// </summary>
        public bool OverlapForeignStructs { get; }

        /// <summary>
        /// Decompose decomposable structs (records and tuples) to allow better data compaction.
        /// </summary>
        public bool DecomposeStructs { get; }

        /// <summary>
        /// Allow structs defined outside the compilation unit to be decomposed.
        /// </summary>
        public bool DecomposeForeignStructs { get; }

        /// <summary>
        /// Generate pass-through equality for the union type.
        /// </summary>
        public bool GenerateEquality { get; }

        /// <summary>
        /// The name of the generated tag enum.
        /// </summary>
        public string TagTypeName { get; }

        /// <summary>
        /// The name of the tag property on the union type.
        /// </summary>
        public string TagPropertyName { get; }

        private UnionOptions(
            bool shareFields,
            bool shareReferenceFields,
            bool overlapStructs,
            bool overlapForeignStructs,
            bool decomposeStructs,
            bool decomposeForeignStructs,
            bool generateEquality,
            string tagTypeName,
            string tagPropertyName)
        {
            this.ShareSameTypeFields = shareFields;
            this.ShareReferenceFields = shareReferenceFields;
            this.OverlapStructs = overlapStructs;
            this.OverlapForeignStructs = overlapForeignStructs;
            this.DecomposeStructs = decomposeStructs;
            this.DecomposeForeignStructs = decomposeForeignStructs;
            this.GenerateEquality = generateEquality;
            this.TagTypeName = tagTypeName;
            this.TagPropertyName = tagPropertyName;
        }

        public UnionOptions WithShareFields(bool share) =>
            With(shareFields: share);

        public UnionOptions WithShareReferenceFields(bool share) =>
            With(shareReferenceFields: share);

        public UnionOptions WithOverlapStructs(bool overlap) =>
            With(overlapStructs: overlap);

        public UnionOptions WithDecomposeStructs(bool decompose) =>
            With(decomposeStructs: decompose);

        public UnionOptions WithDecomposeForeignStructs(bool decompose) =>
            With(decomposeForeignStructs: decompose);

        public UnionOptions WithOverlapForeignStructs(bool overlap) =>
            With(overlapForeignStructs: overlap);

        public UnionOptions WithGenerateEquality(bool generate) =>
            With(generateEquality: generate);

        public UnionOptions WithTagTypeName(string name) =>
            With(tagTypeName: name);

        public UnionOptions WithTagPropertyName(string name) =>
            With(tagPropertyName: name);

        private UnionOptions With(
            bool? shareFields = null,
            bool? shareReferenceFields = null,
            bool? overlapStructs = null,
            bool? overlapForeignStructs = null,
            bool? decomposeStructs = null,
            bool? decomposeForeignStructs = null,
            bool? generateEquality = null,
            string? tagTypeName = null,
            string? tagPropertyName = null)
        {
            var newShareFields = shareFields ?? this.ShareSameTypeFields;
            var newShareReferenceFields = shareReferenceFields ?? this.ShareReferenceFields;
            var newOverlapStructs = overlapStructs ?? this.OverlapStructs;
            var newDecomposeStructs = decomposeStructs ?? this.DecomposeStructs;
            var newDecomposeForeignStructs = decomposeForeignStructs ?? this.DecomposeForeignStructs;
            var newOverlapForeignStructs = overlapForeignStructs ?? this.OverlapForeignStructs;
            var newGenerateEquality = generateEquality ?? this.GenerateEquality;
            var newTagTypeName = tagTypeName ?? this.TagTypeName;
            var newTagPropertyName = tagPropertyName ?? this.TagPropertyName;

            if (newShareFields != this.ShareSameTypeFields
                || newShareReferenceFields != this.ShareReferenceFields
                || newOverlapStructs != this.OverlapStructs
                || newDecomposeStructs != this.DecomposeStructs
                || newDecomposeForeignStructs != this.DecomposeForeignStructs
                || newOverlapForeignStructs != this.OverlapForeignStructs
                || newGenerateEquality != this.GenerateEquality
                || newTagTypeName != this.TagTypeName
                || newTagPropertyName != this.TagPropertyName)
            {
                return new UnionOptions(
                    shareFields: newShareFields,
                    shareReferenceFields: newShareReferenceFields,
                    overlapStructs: newOverlapStructs,
                    overlapForeignStructs: newOverlapForeignStructs,
                    decomposeStructs: newDecomposeStructs,
                    decomposeForeignStructs: newDecomposeForeignStructs,
                    generateEquality: newGenerateEquality,
                    tagTypeName: newTagTypeName,
                    tagPropertyName: newTagPropertyName
                    );
            }

            return this;
        }

        public static UnionOptions Default =
            new UnionOptions(
                shareFields: true, 
                shareReferenceFields: true, 
                overlapStructs: true, 
                overlapForeignStructs: true, 
                decomposeStructs: true,
                decomposeForeignStructs: true,
                generateEquality: false,
                tagTypeName: "Case",
                tagPropertyName: "Kind"
                );
    }

    public class UnionCase
    {
        public string Name { get; }
        public string? Type { get; }
        public int TagValue { get; } 
        public string FactoryName { get; }
        public IReadOnlyList<UnionCaseValue> FactoryParameters { get; }
        public bool FactoryIsPartial { get; }
 
        public UnionCase(
            string name,
            string? type,
            int tagValue,
            string factoryName,
            IReadOnlyList<UnionCaseValue>? factoryParameters = null,
            bool factoryIsPartial = false)
        {
            this.Name = name;
            this.Type = type;
            this.TagValue = tagValue;
            this.FactoryName = factoryName;
            this.FactoryParameters = factoryParameters ?? Array.Empty<UnionCaseValue>();
            this.FactoryIsPartial = factoryIsPartial;
        }
    }

    /// <summary>
    /// The declaration of an individual union case value.
    /// </summary>
    public class UnionCaseValue
    {
        /// <summary>
        /// The name of the case value.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type name of the case value.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// The kind of type.
        /// </summary>
        public TypeKind Kind { get; }

        /// <summary>
        /// The members of the value it can be decomposed into.
        /// </summary>
        public IReadOnlyList<UnionCaseValue> Members { get; }

        public UnionCaseValue(
            string name,
            string type,
            TypeKind kind,
            IReadOnlyList<UnionCaseValue>? members = null)
        {
            this.Kind = kind;
            this.Name = name;
            this.Type = type;
            this.Members = members ?? Array.Empty<UnionCaseValue>();
        }
    }

    /// <summary>
    /// The kind of type the case value represents.
    /// Useful for determining how the value can be stored in the union's data fields.
    /// </summary>
    public enum TypeKind
    {
        /// <summary>
        /// The kind is unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// A tuple containing one or more values that can be decomposed
        /// </summary>
        ValueTuple,

        /// <summary>
        /// A record struct defined within the current compilation unit.
        /// </summary>
        DecomposableLocalRecordStruct,

        /// <summary>
        /// A record struct defined outside the current compilation unit.
        /// </summary>
        DecomposableForeignRecordStruct,

        /// <summary>
        /// struct containing one or more reference values and one or more value-type values
        /// </summary>
        NonOverlappableStruct,

        /// <summary>
        /// struct containing only overlappable field members,
        /// declared within the current compilation unit.
        /// </summary>
        OverlappableLocalStruct,

        /// <summary>
        /// A struct containing only overlappable field members, 
        /// not declared within the current compilation unit.
        /// </summary>
        OverlappableForeignStruct,

        /// <summary>
        /// Just a single overlappable primitive value type
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
        /// The object type
        /// </summary>
        Object,

        /// <summary>
        /// A type parameter
        /// </summary>
        TypeParameter_Unconstrained,

        /// <summary>
        /// A ref constrained type parameter
        /// </summary>
        TypeParameter_RefConstrained,

        /// <summary>
        /// A value-type constrained type parameter
        /// </summary>
        TypeParameter_ValConstrained
    }
    #endregion

    /// <summary>
    /// Generates the source code for a union type, given its description.
    /// </summary>
    public class UnionGenerator : Generator
    {
        private string? _namespace;
        private readonly IReadOnlyList<string>? _usings;

        public UnionGenerator(
            string? namespaceName = null, 
            IReadOnlyList<string>? usings = null
            )
        {
            _namespace = namespaceName;
            _usings = usings;
        }

        public string GenerateFile(params Union[] unions)
        {
            var infos = unions.Select(u => CreateUnionLayout(u)).ToList();
            this.WriteFile(infos);
            return this.GeneratedText;
        }

        #region code generation info models
        /// <summary>
        /// The field layout for the union type.
        /// </summary>
        private class UnionLayout
        {
            /// <summary>
            /// The original description of the union.
            /// </summary>
            public Union Union { get; }

            /// <summary>
            /// The layouts of the individual union cases.
            /// </summary>
            public IReadOnlyList<UnionCaseLayout> Cases { get; }

            /// <summary>
            /// All non-overlapped fields in the union type.
            /// </summary>
            public IReadOnlyList<DataField> NonOverlappedFields { get; }

            /// <summary>
            /// The field in the union type that holds overlapped data.
            /// </summary>
            public DataField? OverlappedDataField { get; }

            public UnionKind Kind => this.Union.Kind;
            public string Name => this.Union.Name;
            public string TypeName => this.Union.TypeName;
            public string Accessibility => this.Union.Accessibility ?? "public";
            public UnionOptions Options => this.Union.Options;

            public UnionLayout(
                Union union,
                IReadOnlyList<UnionCaseLayout> cases,
                IReadOnlyList<DataField> nonOverlapedFields,
                DataField? overlappedDataField)
            {
                this.Union = union;
                this.Cases = cases;
                this.NonOverlappedFields = nonOverlapedFields;
                this.OverlappedDataField = overlappedDataField;
            }
        }

        /// <summary>
        /// The field layout for the individual union cases.
        /// </summary>
        private class UnionCaseLayout
        {
            public UnionCase Case { get; }

            /// <summary>
            /// The layout for the parameters to the factory method for the case.
            /// </summary>
            public IReadOnlyList<CaseValueLayout> FactoryParameters { get; }

            /// <summary>
            /// The type of the case, if the union is a type union.
            /// </summary>
            public string? Type { get; }

            /// <summary>
            /// The value used for the tag associated with the case.
            /// </summary>
            public int TagValue { get; }

            /// <summary>
            /// The field in the union's overlapped data that holds either
            /// a single value or a struct with all the case's overlappable fields.
            /// </summary>
            public DataField? OverlappedCaseField { get; }

            /// <summary>
            /// The individual fields for the overlapped data for the case,
            /// if more than one parameter is overlappable.
            /// </summary>
            public IReadOnlyList<DataField> OverlappedCaseDataFields { get; }

            public string Name => this.Case.Name;
            public string FactoryName => this.Case.FactoryName;
            public bool FactoryIsPartial => this.Case.FactoryIsPartial;

            public UnionCaseLayout(
                UnionCase unionCase,
                IReadOnlyList<CaseValueLayout> factoryParameters,
                string? type,
                int tagValue,
                DataField? overlappedCaseField,
                IReadOnlyList<DataField> overlappedCaseDataFields)
            {
                this.Case = unionCase;
                this.FactoryParameters = factoryParameters;
                this.Type = type;
                this.TagValue = tagValue;
                this.OverlappedCaseField = overlappedCaseField;
                this.OverlappedCaseDataFields = overlappedCaseDataFields;
            }

            /// <summary>
            /// Gets the parameter associated with the field.
            /// </summary>
            public bool TryGetParameter(DataField field, out CaseValueLayout parameter)
            {
                foreach (var p in this.FactoryParameters)
                {
                    if (p.Field == field)
                    {
                        parameter = p;
                        return true;
                    }
                    else if (p.DecomposedMembers.Count > 0
                        && TryGetNestedParameter(p.DecomposedMembers, field, out parameter))
                    {
                        return true;
                    }
                }

                parameter = default!;
                return false;

                bool TryGetNestedParameter(IReadOnlyList<CaseValueLayout> ps, DataField field, out CaseValueLayout parameter)
                {
                    foreach (var np in ps)
                    {
                        if (np.Field == field)
                        {
                            parameter = np;
                            return true;
                        }
                        if (TryGetNestedParameter(np.DecomposedMembers, field, out parameter))
                            return true;
                    }
                    parameter = default!;
                    return false;
                }
            }
        }

        /// <summary>
        /// The field layout for an individual factory parameter or 
        /// decomposable members of a factory parameter.
        /// </summary>
        private class CaseValueLayout
        {
            public UnionCaseValue Parameter { get; }
            public string? PathFromFactoryArg { get; }
            public DataField? Field { get; }
            public DataKind DataKind { get; }
            public IReadOnlyList<CaseValueLayout> DecomposedMembers { get; }

            public CaseValueLayout(
                UnionCaseValue parameter,
                DataKind dataKind,
                DataField? field,
                string? pathFromFactoryArg, 
                IReadOnlyList<CaseValueLayout>? nestedParameters)
            {
                this.Parameter = parameter;
                this.DataKind = dataKind;
                this.Field = field;
                this.PathFromFactoryArg = pathFromFactoryArg;
                this.DecomposedMembers = nestedParameters ?? Array.Empty<CaseValueLayout>();
            }

            public TypeKind Kind => this.Parameter.Kind;
            public string Name => this.Parameter.Name;
            public string Type => this.Parameter.Type;
        }

        private class DataField
        {
            public DataKind DataKind { get; }

            /// <summary>
            /// The name of the field.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// The type of this field.
            /// </summary>
            public string Type { get; }

            public string? ConstructorArg { get; }

            public DataField(
                DataKind kind,
                string name,
                string type,
                string? constructorArg)
            {
                this.DataKind = kind;
                this.Name = name;
                this.Type = type;
                this.ConstructorArg = constructorArg;
            }
        }

        private static IEnumerable<CaseValueLayout> Find(IEnumerable<CaseValueLayout> values, Func<CaseValueLayout, bool> predicate)
        {
            var list = new List<CaseValueLayout>();
            Find(values, list);
            return list;

            void Find(IEnumerable<CaseValueLayout> values, List<CaseValueLayout> list)
            {
                foreach (var value in values)
                {
                    if (predicate(value))
                        list.Add(value);

                    Find(value.DecomposedMembers, list);
                }
            }
        }

        private static CaseValueLayout? FindFirst(IEnumerable<CaseValueLayout> values, Func<CaseValueLayout, bool> predicate)
        {
            foreach (var value in values)
            {
                if (predicate(value))
                    return value;
                var found = FindFirst(value.DecomposedMembers, predicate);
                if (found != null)
                    return found;
            }
            return null;
        }

        /// <summary>
        /// Gets the path to the data field.
        /// </summary>
        private static string GetPathToData(UnionLayout union, DataField field)
        {
            if (field == union.OverlappedDataField
                || union.NonOverlappedFields.Contains(field))
            {
                return field.Name;
            }

            if (union.OverlappedDataField != null)
            {
                foreach (var unionCase in union.Cases)
                {
                    if (unionCase.OverlappedCaseField == field)
                    {
                        return CombinePath(union.OverlappedDataField.Name, field.Name);
                    }

                    if (unionCase.OverlappedCaseField != null 
                        && unionCase.OverlappedCaseDataFields.Contains(field))
                    {
                        return CombinePath(GetPathToData(union, unionCase.OverlappedCaseField), field.Name);
                    }
                }
            }

            throw new InvalidOperationException("Data field not in model");           
        }

        private static string CombinePath(params string[] parts)
        {
            return string.Join(".", parts.Where(p => !string.IsNullOrEmpty(p)));
        }

        private static string CombineName(params string[] parts)
        {
            return string.Join("_", parts.Where(p => !string.IsNullOrEmpty(p)));
        }

        /// <summary>
        /// The kind of data a case parameter represents.
        /// </summary>
        public enum DataKind
        {
            /// <summary>
            /// Any type that can share the same field with a parameter from a difference case
            /// with the same type.
            /// </summary>
            SameTypeSharable,

            /// <summary>
            /// Any reference type that can share the same field with any other reference type,
            /// with the field typed as object.
            /// </summary>
            ReferenceSharable,

            /// <summary>
            /// Any type that can be decomposed into separate values; ValueTuple or record struct.
            /// </summary>
            Decomposable,

            /// <summary>
            /// Any type that can be overlapped with other overlappable types.
            /// </summary>
            Overlappable,

            /// <summary>
            /// Any type that must have its own unique field.
            /// </summary>
            Unique
        }
        #endregion

        #region create data layouts 

        private UnionLayout CreateUnionLayout(Union union)
        {
            if (union.Options.OverlapStructs)
            {
                // only use overlap fields if the number of fields is less than without overlap.
                // this can occur when overlappable fields also type match with other fields.
                var layoutNoOverlap = CreateUnionLayout(union, allowOverlappedFields: false);
                var layoutWithOverlap = CreateUnionLayout(union, allowOverlappedFields: true);

                var fieldsNoOverlap = layoutNoOverlap.NonOverlappedFields.Count + (layoutNoOverlap.OverlappedDataField != null ? 1 : 0);
                var fieldsWithOverlap = layoutWithOverlap.NonOverlappedFields.Count + (layoutWithOverlap.OverlappedDataField != null ? 1 : 0);

                return fieldsWithOverlap < fieldsNoOverlap
                    ? layoutWithOverlap
                    : layoutNoOverlap;
            }
            else
            {
                return CreateUnionLayout(union, allowOverlappedFields: false);
            }
        }

        private UnionLayout CreateUnionLayout(Union union, bool allowOverlappedFields)
        {
            var numberOfCasesWithOverlappableData = allowOverlappedFields 
                ? GetOverlappedCaseCount(union) 
                : 0;

            var unionFields = new List<DataField>();
            var cases = new List<UnionCaseLayout>();

            // only have overlapped data if more than one case has overlappable data
            DataField? overlappedUnionField = numberOfCasesWithOverlappableData > 1
                ? new DataField(DataKind.Unique, "_overlapped", "OverlappedData", "overlapped")
                : null;

            var usedTagValues = new SortedSet<int>(union.Cases.Where(c => c.TagValue >= 0).Select(c => c.TagValue));

            foreach (var unionCase in union.Cases)
            {
                var allocatedFields = new HashSet<DataField>();

                // if the case has move than one overlapped data field, then create a unique field for the case
                DataField? overlappedCaseField = (overlappedUnionField != null
                    && GetOverlappedCaseDataFieldCount(union, unionCase) > 1)
                    ? new DataField(DataKind.Unique, unionCase.Name, unionCase.Name + "Data", null)
                    : null;

                var factoryParamLayouts = new List<CaseValueLayout>();
                foreach (var param in unionCase.FactoryParameters)
                {
                    var caseParam = CreateCaseValueLayout(
                        union,
                        unionCase,
                        param,
                        unionFields,
                        allocatedFields,
                        overlappedUnionField,
                        overlappedCaseField,
                        parentPath: "",
                        parentName: "",
                        pathFromFactoryArg: ""
                        );
                    factoryParamLayouts.Add(caseParam);
                }

                var overlappedCaseDataFields = GetOverlappedFields(factoryParamLayouts);

                if (overlappedCaseField == null 
                    && overlappedCaseDataFields.Count == 1)
                {
                    // if there is only one overlapped case data field then use it as the case field
                    overlappedCaseField = overlappedCaseDataFields[0];
                    overlappedCaseDataFields = Array.Empty<DataField>();
                }

                var type = union.Kind == UnionKind.TypeUnion && factoryParamLayouts.Count == 1
                    ? factoryParamLayouts[0].Type
                    : null;

                var tagValue = unionCase.TagValue;
                if (tagValue < 0)
                    tagValue = GetNextTagValue(usedTagValues);
                usedTagValues.Add(tagValue);

                var info = new UnionCaseLayout(
                    unionCase,
                    factoryParamLayouts,
                    type,
                    tagValue,
                    overlappedCaseField,
                    overlappedCaseDataFields
                    );

                cases.Add(info);
            }

            // order by tag value
            cases.Sort((a, b) => a.TagValue.CompareTo(b.TagValue));

            return new UnionLayout(
                union,
                cases,
                unionFields,
                overlappedUnionField
                );
        }

        private int GetNextTagValue(SortedSet<int> usedTagValues)
        {
            // start at 1
            if (usedTagValues.Count == 0)
                return 1;

            // look for gap in already used values           
            var lastTagValue = -1;

            foreach (var tagValue in usedTagValues)
            {
                if (lastTagValue >= 0 && tagValue > lastTagValue + 1)
                    break;
                lastTagValue = tagValue;
            }

            return lastTagValue + 1;
        }


        /// <summary>
        /// Gets the list overlapped fields in the value layouts.
        /// </summary>
        private static IReadOnlyList<DataField> GetOverlappedFields(IEnumerable<CaseValueLayout> layouts)
        {
            var overlappedFields = new List<DataField>();
            GetOverlappedFields(layouts);
            return overlappedFields;

            void GetOverlappedFields(IEnumerable<CaseValueLayout> layouts)
            {
                foreach (var layout in layouts)
                {
                    if (layout.Field != null && layout.Field.DataKind == DataKind.Overlappable)
                    {
                        overlappedFields.Add(layout.Field);
                    }

                    GetOverlappedFields(layout.DecomposedMembers);
                }
            }
        }

        /// <summary>
        /// Gets the number of cases that have overlapped case values.
        /// </summary>
        private int GetOverlappedCaseCount(Union union)
        {
            var caseCount = 0;

            foreach (var unionCase in union.Cases)
            {
                if (GetOverlappedCaseDataFieldCount(union, unionCase) > 0)
                    caseCount++;    
            }

            return caseCount;
        }

        /// <summary>
        /// Gets the number of fields an individual case requires.
        /// </summary>
        private int GetOverlappedCaseDataFieldCount(
            Union union, UnionCase unionCase)
        {
            return CountOverlappedFields(unionCase.FactoryParameters);

            int CountOverlappedFields(IReadOnlyList<UnionCaseValue> caseValues)
            {
                int count = 0;
                foreach (var caseValue in caseValues)
                {
                    var kind = GetDataKind(union.Options, caseValue.Kind);
                    if (kind == DataKind.Overlappable)
                    {
                        count++;
                    }
                    else if (kind == DataKind.Decomposable)
                    {
                        count += CountOverlappedFields(caseValue.Members);
                    }
                }
                return count;
            }
        }

        private CaseValueLayout CreateCaseValueLayout(
            Union union,
            UnionCase unionCase,
            UnionCaseValue caseValue,
            List<DataField> unionFields,
            HashSet<DataField> allocatedUnionFields,
            DataField? overlappedUnionField,
            DataField? overlappedCaseField,
            string parentPath,
            string parentName,
            string pathFromFactoryArg)
        {
            DataField? field = null;
            List<CaseValueLayout>? memberLayouts = null;
            var dataKind = GetDataKind(union.Options, caseValue.Kind, overlappedUnionField != null);

            pathFromFactoryArg = CombinePath(pathFromFactoryArg, caseValue.Name);

            switch (dataKind)
            {
                case DataKind.Decomposable:
                    var isFactoryParameter = unionCase.FactoryParameters.Contains(caseValue);
                    memberLayouts = new List<CaseValueLayout>(caseValue.Members.Count);
                    foreach (var np in caseValue.Members)
                    {
                        // ignore initial parent name for type case (since it will be 'value')
                        var newParentName = isFactoryParameter && union.Kind == UnionKind.TypeUnion
                            ? parentName
                            : CombineName(parentName, caseValue.Name);

                        var newMemberLayout = CreateCaseValueLayout(
                            union,
                            unionCase,
                            np,
                            unionFields,
                            allocatedUnionFields,
                            overlappedUnionField,
                            overlappedCaseField,
                            parentPath,
                            newParentName,
                            pathFromFactoryArg
                            );

                        memberLayouts.Add(newMemberLayout);
                    }
                    break;

                case DataKind.Overlappable:
                    {
                        // the name of the field includes all the name of the 
                        // containing case values, so that the field name is unique.
                        var name = CombineName(parentName, caseValue.Name);

                        // if their is not case field for this case
                        // then add the name of the case to the field
                        if (overlappedCaseField == null)
                        {
                            name = CombineName(unionCase.Name, name);
                        }

                        field = new DataField(DataKind.Overlappable, name, caseValue.Type, null);
                    }
                    break;

                case DataKind.SameTypeSharable:
                    field = FindOrAllocateUnionField(
                        DataKind.SameTypeSharable,
                        caseValue.Type,
                        unionFields,
                        allocatedUnionFields
                        );
                    break;

                case DataKind.ReferenceSharable:
                    field = FindOrAllocateUnionField(
                        DataKind.ReferenceSharable,
                        "object",
                        unionFields,
                        allocatedUnionFields
                        );
                    break;

                case DataKind.Unique:
                    field = FindOrAllocateUnionField(
                        DataKind.Unique,
                        caseValue.Type,
                        unionFields,
                        allocatedUnionFields
                        );
                    break;

                default:
                    throw new NotImplementedException();
            }

            return new CaseValueLayout(
                caseValue,
                dataKind,
                field,
                pathFromFactoryArg,
                memberLayouts
                );
        }

        private DataField FindOrAllocateUnionField(
            DataKind dataKind,
            string type,
            List<DataField> unionFields,
            HashSet<DataField> allocatedUnionFields)
        {
            // find existing unallocated union field.
            for (int i = 0; i < unionFields.Count; i++)
            {
                var field = unionFields[i];
                if (((dataKind == DataKind.SameTypeSharable && field.Type == type)
                    || (dataKind == DataKind.ReferenceSharable && field.DataKind == DataKind.ReferenceSharable))
                    && !allocatedUnionFields.Contains(field))
                {
                    allocatedUnionFields.Add(field);
                    return field;
                }
            }

            var newFieldName = "_field" + unionFields.Count;
            var argName = "field" + unionFields.Count;
            var newField = new DataField(dataKind, newFieldName, type, argName);

            unionFields.Add(newField);
            allocatedUnionFields.Add(newField);
            return newField;
        }

        private DataKind GetDataKind(UnionOptions options, TypeKind kind, bool allowOverlappingData = true)
        {
            var dataKind = GetBaseKind();
            
            if (dataKind == DataKind.Overlappable && (!options.OverlapStructs || !allowOverlappingData))
                dataKind = DataKind.SameTypeSharable;
            if (dataKind == DataKind.Decomposable && !options.DecomposeStructs)
                dataKind = DataKind.SameTypeSharable;
            if (dataKind == DataKind.ReferenceSharable && !options.ShareReferenceFields)
                dataKind = DataKind.SameTypeSharable;
            if (dataKind == DataKind.SameTypeSharable && !options.ShareSameTypeFields)
                dataKind = DataKind.Unique;

            return dataKind;

            DataKind GetBaseKind()
            {
                switch (kind)
                {
                    case TypeKind.TypeParameter_Unconstrained:
                    case TypeKind.TypeParameter_ValConstrained:
                    case TypeKind.NonOverlappableStruct:
                    case TypeKind.Unknown:
                        return DataKind.SameTypeSharable;

                    case TypeKind.DecomposableLocalRecordStruct:
                    case TypeKind.ValueTuple:
                        return DataKind.Decomposable;

                    case TypeKind.DecomposableForeignRecordStruct:
                        if (options.DecomposeForeignStructs)
                        {
                            return DataKind.Decomposable;
                        }
                        else
                        {
                            return DataKind.SameTypeSharable;
                        }

                    case TypeKind.Class:
                    case TypeKind.Interface:
                    case TypeKind.Object:
                    case TypeKind.TypeParameter_RefConstrained:
                        return DataKind.ReferenceSharable;

                    case TypeKind.PrimitiveStruct:
                    case TypeKind.OverlappableLocalStruct:
                        return DataKind.Overlappable;

                    case TypeKind.OverlappableForeignStruct:
                        if (options.OverlapForeignStructs)
                        {
                            return DataKind.Overlappable;
                        }
                        else
                        {
                            return DataKind.SameTypeSharable;
                        }

                    default:
                        return DataKind.SameTypeSharable;
                }
            }
        }

        private static bool IsPossibleReference(TypeKind kind)
        {
            switch (kind)
            {
                case TypeKind.Class:
                case TypeKind.Interface:
                case TypeKind.Object:
                case TypeKind.TypeParameter_Unconstrained:
                case TypeKind.TypeParameter_RefConstrained:
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        private static readonly List<string> _defaultUsings = new List<string>
        {
            "using System;",
            "using System.Collections.Generic;",
            "using System.Diagnostics.CodeAnalysis;",
            "using System.Runtime.InteropServices;",
            "using UnionTypes;"
        };

        private static string FormatUsing(string uzing)
        {
            return (uzing.StartsWith("using") ? uzing : $"using {uzing}")
                   + (uzing.EndsWith(";") ? "" : ";");
        }

        private void WriteFile(IReadOnlyList<UnionLayout> unions)
        {
            foreach (var u in _defaultUsings)
            {
                WriteLine(u);
            }

            if (_usings != null)
            {
                foreach (var uzing in _usings)
                {
                    var uz = FormatUsing(uzing);
                    if (!_defaultUsings.Contains(uz))
                    {
                        WriteLine(uz);
                    }
                }
            }

            WriteLine("#nullable enable");
            WriteLine();

            if (!string.IsNullOrEmpty(_namespace))
            {
                WriteLine($"namespace {_namespace};");
                WriteLine();
            }

            WriteUnions(unions);
        }

        #region write union type declarations
        private void WriteUnions(IReadOnlyList<UnionLayout> unions)
        {
            var lastUnion = unions.LastOrDefault();
            foreach (var union in unions)
            {
                WriteUnion(union);

                if (union != lastUnion)
                    Console.WriteLine();
            }
        }

        private void WriteUnion(UnionLayout union)
        {
            if (union.Kind == UnionKind.TypeUnion)
            {
                var interfaces = new List<string>();
                interfaces.Add($"IClosedTypeUnion<{union.TypeName}>");
                if (union.Options.GenerateEquality)
                    interfaces.Add($"IEquatable<{union.TypeName}>");
                var interfaceList = string.Join(", ", interfaces);
                if (interfaceList.Length > 0)
                    interfaceList = " : " + interfaceList;

                WriteLine($"{union.Accessibility} partial struct {union.TypeName}{interfaceList}");
                WriteBraceNested(() =>
                {
                    WriteLineSeparated(
                        () => WriteTagDeclaration(union),
                        () => WriteFields(union),
                        () => WriteOverlappedDataTypes(union),
                        () => WriteConstructor(union),
                        () => WriteFactoryMethods(union),
                        () => WriteImplicitCastOperators(union),
                        () => WriteValueProperties(union),
                        () => WriteITypeUnionMethods(union),
                        () => WriteMatchMethods(union)
                        );
                });
            }
            else
            {
                var interfaces = new List<string>();
                if (union.Options.GenerateEquality)
                    interfaces.Add($"IEquatable<{union.TypeName}>");
                var interfaceList = string.Join(", ", interfaces);
                if (interfaceList.Length > 0)
                    interfaceList = " : " + interfaceList;

                WriteLine($"{union.Accessibility} partial struct {union.TypeName}{interfaceList}");
                WriteBraceNested(() =>
                {
                    WriteLineSeparated(
                        () => WriteTagDeclaration(union),
                        () => WriteFields(union),
                        () => WriteOverlappedDataTypes(union),
                        () => WriteConstructor(union),
                        () => WriteFactoryMethods(union),
                        () => WriteValueProperties(union)
                        //() => WriteMatchMethods(union)
                        );
                });
            }
        }

        private void WriteTagDeclaration(UnionLayout union)
        {
            WriteLine($"public enum {GetTagTypeName(union)}");
            WriteBraceNested(() =>
            {
                foreach (var c in union.Cases)
                {
                    WriteLine($"{GetCaseTagName(c)} = {c.TagValue},");
                }
            });
        }

        private void WriteFields(UnionLayout union)
        {
            WriteLine($"public {GetTagTypeName(union)} {GetTagPropertyName(union)} {{ get; }}");

            if (union.OverlappedDataField != null)
            {
                WriteLine($"private readonly {union.OverlappedDataField.Type} {union.OverlappedDataField.Name};");
            }

            foreach (var field in union.NonOverlappedFields)
            {
                WriteLine($"private readonly {field.Type} {field.Name};");
            }
        }

        private void WriteOverlappedDataTypes(UnionLayout union)
        {
            if (union.OverlappedDataField != null)
            {
                WriteLineSeparated(() =>
                {
                    WriteLine("[StructLayout(LayoutKind.Explicit)]");
                    WriteLine($"private struct {union.OverlappedDataField.Type}");
                    WriteBraceNested(() =>
                    {
                        WriteLineSeparatedBlocks(() =>
                        {
                            WriteBlock(() =>
                            {
                                foreach (var unionCase in union.Cases)
                                {
                                    if (unionCase.OverlappedCaseField != null)
                                    {
                                        WriteLine($"[FieldOffset(0)]");
                                        WriteLine($"public {unionCase.OverlappedCaseField.Type} {unionCase.OverlappedCaseField.Name};");
                                    }
                                }
                            });

                        });
                    });

                    foreach (var unionCase in union.Cases)
                    {
                        if (unionCase.OverlappedCaseField != null
                            && unionCase.OverlappedCaseDataFields.Count > 0)
                        {
                            WriteBlock(() =>
                            {
                                WriteLine($"public struct {unionCase.OverlappedCaseField.Type}");
                                WriteBraceNested(() =>
                                {
                                    foreach (var field in unionCase.OverlappedCaseDataFields)
                                    {
                                        WriteLine($"public {field.Type} {field.Name};");
                                    }
                                });
                            });
                        }
                    }
                });
            }
        }

        private void WriteConstructor(UnionLayout union)
        {
            var args = new List<string>();
            args.Add($"{GetTagTypeName(union)} {GetTagArgumentName(union)}");

            if (union.OverlappedDataField != null)
                args.Add($"{union.OverlappedDataField.Type} {union.OverlappedDataField.ConstructorArg}");

            foreach (var field in union.NonOverlappedFields)
            {
                args.Add($"{field.Type} {field.ConstructorArg}");
            }

            var argsList = string.Join(", ", args);

            WriteLine($"private {union.Name}({argsList})");
            WriteBraceNested(() =>
            {
                WriteLine($"this.{GetTagPropertyName(union)} = {GetTagArgumentName(union)};");

                if (union.OverlappedDataField != null)
                    WriteLine($"{union.OverlappedDataField.Name} = {union.OverlappedDataField.ConstructorArg};");

                foreach (var field in union.NonOverlappedFields)
                {
                    WriteLine($"{field.Name} = {field.ConstructorArg};");
                }
            });
        }

        private void WriteFactoryMethods(UnionLayout union)
        {
            foreach (var unionCase in union.Cases)
            {
                var partial = unionCase.FactoryIsPartial ? "partial " : "";
                var parameters = string.Join(", ", unionCase.FactoryParameters.Select(p => $"{p.Type} {p.Name}"));
                var unionConstruction = GetUnionCaseConstructionExpression(union, unionCase);
                WriteLine($"public static {partial}{union.TypeName} {unionCase.FactoryName}({parameters}) => {unionConstruction};");
            }
        }

        /// <summary>
        /// Constructs the union type for a given case from factory arguments.
        /// </summary>
        private string GetUnionCaseConstructionExpression(UnionLayout union, UnionCaseLayout unionCase)
        {
            var args = new List<string>();
            args.Add($"{GetTagArgumentName(union)}: {GetTagValueExpression(union, unionCase)}");

            if (union.OverlappedDataField != null)
            {
                args.Add($"{union.OverlappedDataField.ConstructorArg}: {GetOverlappedConstructionExpression(union, unionCase)}");
            }

            foreach (var field in union.NonOverlappedFields)
            {
                if (field.ConstructorArg != null)
                {
                    if (unionCase.TryGetParameter(field, out var param))
                    {
                        args.Add($"{field.ConstructorArg}: {param.PathFromFactoryArg}");
                    }
                    else
                    {
                        args.Add($"{field.ConstructorArg}: default!");
                    }
                }
            }

            var argList = string.Join(", ", args);
            return $"new {union.TypeName}({argList})";

            string GetOverlappedConstructionExpression(UnionLayout union, UnionCaseLayout unionCase)
            {
                if (union.OverlappedDataField == null)
                    throw new InvalidOperationException("No overlapped data.");

                if (unionCase.OverlappedCaseField != null)
                {
                    var caseConstruction = GetOverlappedCaseConstructionExpression();
                    return $"new {union.OverlappedDataField.Type} {{ {unionCase.OverlappedCaseField.Name} = {caseConstruction} }}";
                }
                else
                {
                    return "default!";
                }

                string GetOverlappedCaseConstructionExpression()
                {
                    if (unionCase.OverlappedCaseDataFields.Count == 0)
                    {
                        // there is only one value, so no struct is defined
                        // find the value that references the overlapped case field
                        var value = FindFirst(unionCase.FactoryParameters, p => p.Field == unionCase.OverlappedCaseField);
                        if (value != null && value.PathFromFactoryArg != null)
                        {
                            return value.PathFromFactoryArg;
                        }
                        else
                        {
                            throw new InvalidOperationException("No overlapped case field value found.");
                        }
                    }
                    else
                    {
                        var assignments = new List<string>();
                        GetOverlappedCaseDataFieldAssignments(unionCase.FactoryParameters, assignments);
                        var assignmentList = string.Join(", ", assignments);
                        return $"new {unionCase.OverlappedCaseField!.Type} {{ {assignmentList} }}";

                        void GetOverlappedCaseDataFieldAssignments(IEnumerable<CaseValueLayout> values, List<string> assigments)
                        {
                            foreach (var value in values)
                            {
                                if (value.Field != null
                                    && value.DataKind == DataKind.Overlappable
                                    && value.PathFromFactoryArg != null)
                                {
                                    assigments.Add($"{value.Field.Name} = {value.PathFromFactoryArg}");
                                }
                                else if (value.DataKind == DataKind.Decomposable)
                                {
                                    GetOverlappedCaseDataFieldAssignments(value.DecomposedMembers, assigments);
                                }
                            }
                        }
                    }
                }
            }
        }


        private void WriteImplicitCastOperators(UnionLayout union)
        {
            if (union.Kind != UnionKind.TypeUnion)
                return;

            // implicit cast value to union
            foreach (var unionCase in union.Cases)
            {
                var param = unionCase.FactoryParameters[0];
                if (param.Kind != TypeKind.Interface
                    && param.Kind != TypeKind.Object)
                {
                    WriteLine($"public static implicit operator {union.TypeName}({param.Type} value) => {unionCase.FactoryName}(value);");
                }
            }
        }

        private void WriteValueProperties(UnionLayout union)
        {
            foreach (var unionCase in union.Cases)
            {
                if (unionCase.FactoryParameters.Count == 1)
                {
                    var param = unionCase.FactoryParameters[0];
                    WriteLine($"public {param.Type} Value{unionCase.Name} => {GetTagComparison(union, unionCase)} ? {GetCaseValueAccessExpression(union, unionCase)} : default!;");
                }
                else if (unionCase.FactoryParameters.Count > 1)
                {
                    var tupleType = GetTupleTypeExpression(unionCase.FactoryParameters);
                    var tupleConstruction = GetTupleConstructionExpression(union, unionCase.FactoryParameters);
                    WriteLine($"public {tupleType} Value{unionCase.Name} => {GetTagComparison(union, unionCase)} ? {tupleConstruction} : default!;");
                }
                else if (unionCase.FactoryParameters.Count == 0)
                {
                    WriteLine($"public bool Value{unionCase.Name} => {GetTagComparison(union, unionCase)};");
                }
            }
        }

        private void WriteGetMethods(UnionLayout union)
        {
            WriteLineSeparatedBlocks(() =>
            {
                foreach (var unionCase in union.Cases)
                {
                    WriteBlock(() => WriteGetMethods(unionCase));
                }
            });

            void WriteGetMethods(UnionCaseLayout unionCase)
            {
                CaseValueLayout param;

                if (union.Kind == UnionKind.TypeUnion)
                {
                    param = unionCase.FactoryParameters[0];
                    WriteLine($"public bool TryGet{unionCase.Name}(out {param.Type} value)");
                    WriteBraceNested(() =>
                    {
                        WriteLine($"if ({GetTagComparison(union, unionCase)})");
                        WriteBraceNested(() =>
                        {
                            WriteLine($"value = {GetCaseValueAccessExpression(union, unionCase)};");
                            WriteLine("return true;");
                        });
                        WriteLine("value = default!;");
                        WriteLine("return false;");
                    });

                    WriteLine($"public {param.Type} Get{unionCase.Name}() => TryGet{unionCase.Name}(out var value) ? value : throw new InvalidCastException();");
                    WriteLine($"public {param.Type} Get{unionCase.Name}OrDefault() => TryGet{unionCase.Name}(out var value) ? value : default!;");
                }
                else
                {
                    if (unionCase.FactoryParameters.Count > 0)
                    {
                        // this is a tag case (w/ values)
                        var paramList = string.Join(", ", unionCase.FactoryParameters.Select(cv => $"out {cv.Type} {cv.Name}"));

                        WriteLine($"public bool TryGet{unionCase.Name}({paramList})");
                        WriteBraceNested(() =>
                        {
                            WriteLine($"if ({GetTagComparison(union, unionCase)})");
                            WriteBraceNested(() =>
                            {
                                foreach (var param in unionCase.FactoryParameters)
                                {
                                    WriteLine($"{param.Name} = {GetCaseValueAccessExpression(union, param)};");
                                }
                                WriteLine("return true;");
                            });
                            WriteLine("else");
                            WriteBraceNested(() =>
                            {
                                foreach (var param in unionCase.FactoryParameters)
                                {
                                    WriteLine($"{param.Name} = default!;");
                                }
                                WriteLine("return false;");
                            });
                        });

                        if (unionCase.FactoryParameters.Count > 1)
                        {
                            WriteLine($"public {GetTupleTypeExpression(unionCase.FactoryParameters)} Get{unionCase.Name}() => {GetTagComparison(union, unionCase)} ? {GetTupleConstructionExpression(union, unionCase.FactoryParameters)} : throw new InvalidCastException();");
                            WriteLine($"public {GetTupleTypeExpression(unionCase.FactoryParameters)} Get{unionCase.Name}OrDefault() => {GetTagComparison(union, unionCase)} ? {GetTupleConstructionExpression(union, unionCase.FactoryParameters)} : default!;");
                        }
                        else if (unionCase.FactoryParameters.Count == 1)
                        {
                            // param0 GetCase()
                            param = unionCase.FactoryParameters[0];
                            WriteLine($"public {param.Type} Get{unionCase.Name}() => {GetTagComparison(union, unionCase)} ? {GetCaseValueAccessExpression(union, param)} : throw new InvalidCastException();");
                            WriteLine($"public {param.Type} Get{unionCase.Name}OrDefault() => {GetTagComparison(union, unionCase)} ? {GetCaseValueAccessExpression(union, param)} : default!;");
                        }
                    }
                    else
                    {
                        // tag case (w/o values) does not get a TryGet method
                    }
                }
            }
        }

        private void WriteITypeUnionMethods(UnionLayout union)
        {
            if (union.Kind != UnionKind.TypeUnion)
                return;

            WriteLine("#region ITypeUnion, ITypeUnion<TUnion>, ICloseTypeUnion, ICloseTypeUnion<TUnion> implementation.");

            WriteLineSeparatedBlocks(() =>
            {
                WriteBlock(() =>
                {
                    WriteLine($"public static bool TryCreateFrom<TValue>(TValue value, out {union.TypeName} union)");
                    WriteBraceNested(() =>
                    {
                        WriteLine("switch (value)");
                        WriteBraceNested(() =>
                        {
                            foreach (var unionCase in union.Cases)
                            {
                                if (unionCase.Type != null)
                                {
                                    WriteLine($"case {unionCase.Type} v: union = {unionCase.FactoryName}(v); return true;");
                                }
                            }
                        });

                        WriteLine();
                        WriteLine($"if (value is ITypeUnion u && u.TryGet<object>(out var uvalue))");
                        WriteBraceNested(() =>
                        {
                            WriteLine("return TryCreateFrom(uvalue, out union);");
                        });

                        WriteLine();
                        WriteLine($"var index = TypeUnion.GetTypeIndex<{union.TypeName}, TValue>(value);");
                        WriteLine("switch (index)");
                        WriteBraceNested(() =>
                        {
                            // this should be the same order that the case types are listed in the Types property.
                            for (int i = 0; i < union.Cases.Count; i++)
                            {
                                var unionCase = union.Cases[i];
                                if (unionCase.Type != null)
                                {
                                    WriteLine($"case {i} when TypeUnion.TryCreateFrom<TValue, {unionCase.Type}>(value, out var v{i}): union = {unionCase.FactoryName}(v{i}); return true;");
                                }
                            }
                        });

                        WriteLine();
                        WriteLine("union = default!; return false;");
                    });

                });

                WriteBlock(() =>
                {
                    var types = union.Cases.Select(c => c.Type).OfType<string>();
                    var typeList = string.Join(", ", types.Select(t => $"typeof({t})"));
                    WriteLine($"private static IReadOnlyList<Type> _types = new [] {{{typeList}}};");
                    WriteLine($"static IReadOnlyList<Type> IClosedTypeUnion<{union.TypeName}>.Types => _types;");

                    WriteLine($"private int GetTypeIndex()");
                    WriteBraceNested(() =>
                    {
                        // translate tag to index
                        WriteLine($"switch ({GetTagPropertyName(union)})");
                        WriteBraceNested(() =>
                        {
                            for (int i = 0; i < union.Cases.Count; i++)
                            {
                                var unionCase = union.Cases[i];
                                WriteLine($"case {GetTagValueExpression(union, unionCase)}: return {i};");
                            }
                            WriteLine("default: return -1;");
                        });
                    });

                    WriteLine($"int IClosedTypeUnion<{union.TypeName}>.TypeIndex => this.GetTypeIndex();");
                    WriteLine("Type ITypeUnion.Type { get { var index = this.GetTypeIndex(); return index >= 0 && index < _types.Count ? _types[index] : typeof(object); } }");
                });

                WriteBlock(() =>
                {
                    WriteLine($"public bool TryGet<TValue>([NotNullWhen(true)] out TValue value)");
                    WriteBraceNested(() =>
                    {
                        WriteLine("switch (this.Kind)");
                        WriteBraceNested(
                            () =>
                            {
                                foreach (var unionCase in union.Cases)
                                {
                                    WriteLine($"case {GetTagValueExpression(union, unionCase)}:");
                                    WriteNested(() =>
                                    {
                                        WriteLine($"if (Value{unionCase.Name} is TValue tv{unionCase.Name})");
                                        WriteBraceNested(() =>
                                        {
                                            WriteLine($"value = tv{unionCase.Name};");
                                            WriteLine("return true;");
                                        });
                                        WriteLine($"return TypeUnion.TryCreateFrom(Value{unionCase.Name}, out value);");
                                    });
                                }
                            });

                        WriteLine("value = default!; return false;");
                    });
                });
   
            });

            WriteLine("#endregion");
        }


        private void WriteMatchMethods(UnionLayout union)
        {
            if (union.Kind == UnionKind.TypeUnion)
            {
                WriteLineSeparatedBlocks(() =>
                {
                    WriteBlock(() =>
                    {
                        var parameters = union.Cases.Select(c => $"Action<{c.Type}>? when{c.Name}").ToList();
                        parameters.Add("Action? invalid = null");
                        var parameterList = string.Join(", ", parameters);

                        WriteLine($"public void Match({parameterList})");
                        WriteBraceNested(() =>
                        {
                            WriteLine($"switch ({GetTagPropertyName(union)})");
                            WriteBraceNested(() =>
                            {
                                foreach (var unionCase in union.Cases)
                                {
                                    WriteLine($"case {GetTagValueExpression(union, unionCase)} when when{unionCase.Name} != null : when{unionCase.Name}(Value{unionCase.Name}); break;");
                                }

                                WriteLine("default: invalid?.Invoke(); break;");
                            });
                        });
                    });

                    WriteBlock(() =>
                    {
                        var parameters = union.Cases.Select(c => $"Func<{c.Type}, TResult>? when{c.Name}").ToList();
                        parameters.Add("Func<TResult>? invalid = null");
                        var parameterList = string.Join(", ", parameters);


                        WriteLine($"public TResult Match<TResult>({parameterList})");
                        WriteBraceNested(() =>
                        {
                            WriteLine($"switch ({GetTagPropertyName(union)})");
                            WriteBraceNested(() =>
                            {
                                foreach (var unionCase in union.Cases)
                                {
                                    WriteLine($"case {GetTagValueExpression(union, unionCase)} when when{unionCase.Name} != null: return when{unionCase.Name}(Value{unionCase.Name});");
                                }

                                WriteLine("default: return invalid != null ? invalid() : throw new InvalidOperationException(\"Unhandled union state.\");");
                            });
                        });
                    });
                });
            }
        }

        /// <summary>
        /// Gets the text of an expression that accesses or constructs the case value from fields.
        /// </summary>
        private string GetCaseValueAccessExpression(UnionLayout union, UnionCaseLayout unionCase)
        {
            if (unionCase.FactoryParameters.Count == 1)
            {
                // treat as single parameter value
                return GetCaseValueAccessExpression(union, unionCase.FactoryParameters[0]);
            }
            else if (unionCase.FactoryParameters.Count > 1)
            {
                // treat as tuple of parameter values
                return GetTupleConstructionExpression(union, unionCase.FactoryParameters);
            }

            // cannot access tag members that do not have any parameters
            throw new InvalidOperationException();
        }

        private string GetCaseValueAccessExpression(UnionLayout union, CaseValueLayout value)
        {
            if (value.DataKind == DataKind.Decomposable)
            {
                switch (value.Kind)
                {
                    case TypeKind.DecomposableForeignRecordStruct:
                    case TypeKind.DecomposableLocalRecordStruct:
                        return GetRecordConstructionExpression(union, value);

                    case TypeKind.ValueTuple:
                        return GetTupleConstructionExpression(union, value.DecomposedMembers);
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (value.Field != null)
            {
                var path = GetPathToData(union, value.Field);

                // pull the entire reference type case instance from a single value field
                if (value.DataKind == DataKind.ReferenceSharable
                    && value.Field.Type != value.Type)
                {
                    return $"({value.Type}){path}";
                }
                else
                {
                    return path;
                }
            }
            else
            {
                throw new InvalidOperationException($"Parameter {value.Name} has no field.");
            }
        }

        private string GetRecordConstructionExpression(UnionLayout union, CaseValueLayout value)
        {
            var members = value.DecomposedMembers.Select(v => GetCaseValueAccessExpression(union, v));
            return $"new {value.Type}(" + string.Join(", ", members) + ")";
        }

        private string GetTupleTypeExpression(IReadOnlyList<CaseValueLayout> factoryParameters)
        {
            return "(" + string.Join(", ", factoryParameters.Select(cv => $"{cv.Type} {cv.Name}")) + ")";
        }

        private string GetTupleConstructionExpression(UnionLayout union, IReadOnlyList<CaseValueLayout> factoryParameters)
        {
            var values = factoryParameters.Select(p => GetCaseValueAccessExpression(union, p));
            return "(" + string.Join(", ", values) + ")";
        }

        private string GetTagTypeName(UnionLayout union)
        {
            return union.Options.TagTypeName;
        }

        private string GetCaseTagName(UnionCaseLayout unionCase)
        {
            return unionCase.Name;
        }

        private string GetTagPropertyName(UnionLayout union)
        {
            return union.Options.TagPropertyName;
        }

        private string GetTagArgumentName(UnionLayout union)
        {
            return LowerName(GetTagPropertyName(union));
        }

        private string GetTagValueExpression(UnionLayout union, UnionCaseLayout unionCase)
        {
            return $"{GetTagTypeName(union)}.{GetCaseTagName(unionCase)}";
        }

        private string GetTagComparison(UnionLayout union, UnionCaseLayout unionCase)
        {
            return $"this.{GetTagPropertyName(union)} == {GetTagValueExpression(union, unionCase)}";
        }


#if false
        private void WriteEquatableEquals()
        {
            // IEquatable<Union>.Equals
            WriteLine($"public bool Equals({_union.TypeName} other)");
            WriteBraceNested(() =>
            {
                if (_isParameterlessTagsOnly)
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
                        foreach (var unionCase in _unionCases)
                        {
                            WriteLine($"case Tag.{unionCase.Name}:");
                            if (unionCase.Kind == CaseKind.Tag && unionCase.Parameters.Count == 0)
                            {
                                WriteLineNested($"return true;");
                            }
                            else
                            {
                                if (unionCase.Parameters.Count == 1
                                    && IsPossibleReference(unionCase.Parameters[0].Kind))
                                {
                                    WriteBraceNested(() =>
                                    {
                                        WriteLine($"var value = Get{unionCase.Name}();");
                                        WriteLine($"var otherValue = other.Get{unionCase.Name}();");
                                        WriteLine($"return (value != null && otherValue != null)");
                                        WriteLineNested("|| (value != null && otherValue != null && value.Equals(otherValue));");
                                    });
                                }
                                else
                                {
                                    // will be a struct
                                    WriteBraceNested(() =>
                                    {
                                        WriteLine($"var value = Get{unionCase.Name}();");
                                        WriteLine($"var otherValue = other.Get{unionCase.Name}();");
                                        WriteLine($"return value.Equals(otherValue);");
                                    });
                                }
                            }
                        }

                        // same tag value, but not a known tag.. probably default
                        WriteLine("default:");
                        WriteLineNested("return true;");
                    });
                }
            });
        }

        private void WriteObjectEquals()
        {
            WriteLine("public override bool Equals(object? other)");
            WriteBraceNested(() =>
            {
                if (_hasTypeCases)
                {
                    // defer to ITypeUnion.Equals
                    WriteLine("return other is object obj && Equals<object>(obj);");
                }
                else
                {
                    // defer to IEquatable.Equals
                    WriteLine($"return other is {_union.TypeName} union && Equals(union);");
                }
            });
        }

        private void WriteGetHashCode()
        {
            // object.GetHashCode()
            WriteLine("public override int GetHashCode()");
            WriteBraceNested(() =>
            {
                if (_isParameterlessTagsOnly)
                {
                    WriteLine("return (int)_tag;");
                }
                else
                {
                    WriteLine("switch (_tag)");
                    WriteBraceNested(() =>
                    {
                        foreach (var unionCase in _unionCases)
                        {
                            WriteLine($"case Tag.{unionCase.Name}:");

                            if (unionCase.Kind == CaseKind.Tag && unionCase.Parameters.Count == 0)
                            {
                                WriteLineNested($"return (int)_tag;");
                            }
                            else if (unionCase.Parameters.Count == 1 && IsPossibleReference(unionCase.Parameters[0].Kind))
                            {
                                WriteLineNested($"return Get{unionCase.Name}()?.GetHashCode() ?? 0;");
                            }
                            else
                            {
                                WriteLineNested($"return Get{unionCase.Name}().GetHashCode();");
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
                if (_isParameterlessTagsOnly)
                {
                    WriteLine("return _tag.ToString();");
                }
                else
                {
                    WriteLine("switch (_tag)");
                    WriteBraceNested(() =>
                    {
                        foreach (var unionCase in _unionCases)
                        {
                            WriteLine($"case Tag.{unionCase.Name}:");
                            if (unionCase.Kind == CaseKind.Tag)
                            {
                                if (unionCase.Parameters.Count == 0)
                                {
                                    WriteLineNested($"return _tag.ToString();");
                                }
                                else if (unionCase.Parameters.Count == 1)
                                {
                                    WriteNested(() =>
                                    {
                                        WriteLine($"return $\"{unionCase.Name}({{Get{unionCase.Name}()}})\";");
                                    });
                                }
                                else
                                {
                                    WriteNested(() =>
                                    {
                                        WriteLine($"var v_{unionCase.Name} = Get{unionCase.Name}();");
                                        var props = string.Join(", ", unionCase.Parameters.Select(p => $"{p.Name}: {{v_{unionCase.Name}.{p.Name}}}"));
                                        WriteLine($$"""return $"{{unionCase.Name}}({{props}})";""");
                                    });
                                }
                            }
                            else
                            {
                                WriteLineNested($"return Get{unionCase.Name}().ToString();");
                            }
                        }

                        WriteLine("default:");
                        WriteLineNested("return \"\";");
                    });
                }
            });
        }
#endif
        #endregion
    }

#if !T4
}
#endif
// #>