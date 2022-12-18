using System;
using System.Collections.Generic;

namespace UnionTypes
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class UnionAttribute : Attribute
    {
        public UnionAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Struct)]
    public class UnionTypesAttribute : Attribute
    {
        public IReadOnlyList<Type> ExternalTypes { get; }

        public UnionTypesAttribute(params Type[] externalTypes)
        {
            this.ExternalTypes = externalTypes ?? Array.Empty<Type>();
        }
    }

    [AttributeUsage(AttributeTargets.Struct)]
    public class UnionTagsAttribute : Attribute
    {
        public IReadOnlyList<string> TagNames { get; }
        public UnionTagsAttribute(params string[] tagNames)
        {
            this.TagNames = tagNames ?? Array.Empty<string>();
        }
    }
}