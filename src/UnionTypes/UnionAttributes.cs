using System;
using System.Collections.Generic;

namespace UnionTypes.Toolkit
{
    public abstract class UnionAttributeBase : Attribute
    {
        /// <summary>
        /// Share data fields of the same type across cases.
        /// </summary>
        public bool ShareSameTypeFields { get; set; }

        /// <summary>
        /// Share data fields of reference type across cases.
        /// </summary>
        public bool ShareReferenceFields { get; set; }

        /// <summary>
        /// Overlap structs with only overlappable fields with other case data.
        /// </summary>
        public bool OverlapStructs { get; set; }

        /// <summary>
        /// Allow structs declared outside the current compilation unit to be overlapped with other case data.
        /// </summary>
        public bool OverlapForeignStructs { get; set; }

        /// <summary>
        /// Decompose structs (records and tuples) into their constituent data to allow better data compaction.
        /// </summary>
        public bool DecomposeStructs { get; set; }

        /// <summary>
        /// Allow structs declared outside the current compilation unit to be decomposed.
        /// </summary>
        public bool DecomposeForeignStructs { get; set; }

        /// <summary>
        /// Generate pass through implementations of IEquatable, Equals and GetHashCode.
        /// </summary>
        public bool GenerateEquality { get; set; }

        /// <summary>
        /// Generate Match methods that force handling of all cases.
        /// </summary>
        public bool GenerateMatch { get; set; }

        /// <summary>
        /// Generate pass through implemention of ToString()
        /// </summary>
        public bool GenerateToString { get; set; }

        /// <summary>
        /// The type name of the tag enum generated within the union type.
        /// Default is 'Case'.
        /// </summary>
        public string? TagTypeName { get; set; }

        /// <summary>
        /// The property name for the union's current tag, generated on the union type.
        /// Default is 'Kind'.
        /// </summary>
        public string? TagPropertyName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Struct)]
    public class TypeUnionAttribute : UnionAttributeBase
    {
        public TypeUnionAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Struct)]
    public class TagUnionAttribute : UnionAttributeBase
    {
        public TagUnionAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true)]
    public class CaseAttribute : Attribute
    {
        /// <summary>
        /// The name of the case used in factory methods, accessors and tags.
        /// If not assigned the name is derived from the member it is associated with.
        /// </summary>
        public string? Name { get; set; } = null;

        /// <summary>
        /// The value of the tag associated with the case.
        /// A value of 0 will allow the case to be default when the union is defaulted or unassigned.
        /// </summary>
        public int TagValue { get; set; } = -1;

        /// <summary>
        /// The name of the factory method to construct a union of this case.
        /// If not specified it is [FactoryPrefix][CaseName].
        /// </summary>
        public string? FactoryName { get; set; } = null;

        /// <summary>
        /// The kind of factory to generate when the case attribute is not placed on the factory.
        /// </summary>
        public string? FactoryKind { get; set; } = null;

        /// <summary>
        /// The name of the member to access the value(s) of this case.
        /// </summary>
        public string? AccessorName { get; set; } = null;

        /// <summary>
        /// The kind of accessor to generate for the case.
        /// </summary>
        public string? AccessorKind { get; set; } = null;

        /// <summary>
        /// The type of the type union case when it cannot be infered.
        /// </summary>
        public Type? Type { get; set; } = null;

        public CaseAttribute()
        {
        }
    }

    public static class FactoryKind
    {
        /// <summary>
        /// The factory generated is a method taking zero or more arguments corresponding the the case values.
        /// </summary>
        public const string Method = nameof(Method);

        /// <summary>
        /// No factory is generated for this case.
        /// This is only available for cases that have no data and correspond to the default state.
        /// </summary>
        public const string None = nameof(None);

        /// <summary>
        /// The factory generated is a property.
        /// This is only available for cases with no data or a single value of a singleton type.
        /// </summary>
        public const string Property = nameof(Property);
    }

    public static class AccessorKind
    {
        /// <summary>
        /// The accessor generated is a method returning a single value.
        /// Cases with multiple values return a tuple.
        /// </summary>
        public const string Method = nameof(Method);

        /// <summary>
        /// No accessor is generated for this case. 
        /// This is possible for cases without data.
        /// </summary>
        public const string None = nameof(None);

        /// <summary>
        /// The accessor generated is a property.
        /// Cases with multiple values return a tuple.
        /// </summary>
        public const string Property = nameof(Property);
    }
}