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

    public abstract class UnionCaseAttribute : Attribute
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
        /// True if the factory used to construct a union of this case is a property instead of a method.
        /// </summary>
        public bool FactoryIsProperty { get; set; }

        /// <summary>
        /// True if the factory should be generated as internal, not public.
        /// </summary>
        public bool FactoryIsInternal { get; set; }

        /// <summary>
        /// The name of the member to access the value of this case.
        /// If not specified it is [AccessorPrefix][CaseName].
        /// </summary>
        public string? AccessorName { get; set; } = null;

        /// <summary>
        /// False if the accessor of a case without data should not exist.
        /// </summary>
        public bool HasAccessor { get; set; } = true;

        public UnionCaseAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true)]
    public class TypeCaseAttribute : UnionCaseAttribute
    {
        /// <summary>
        /// The type of the type union case when it cannot be infered.
        /// </summary>
        public Type? Type { get; set; } = null;

        /// <summary>
        /// True if the type is singleton value and does require storage,
        /// and the singleton value can be accessed from the type via a static property or field named 'Singleton'.
        /// </summary>
        public bool IsSingleton { get; set; } = false;

        public TypeCaseAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true)]
    public class TagCaseAttribute : UnionCaseAttribute
    {
    }
}