# UnionTypes
#### Tools for experimenting with union types.
This repo is intended to be used for discussion and experimentation during design of discriminated unions in C# future.


## Contents

#### UnionTypes
A project building `UnionTypes.dll` containing type definitions, interfaces and custom attributes needed for projects to use and declare union types.

#### Generators
A project defining the core generators for OneOf and custom union types.
These generators can be used by T4 templates to pre-build union types.

#### UnionSourceGenerator
A project that builds the Roslyn source generator for building custom union types.

#### UnionSourceGenerator.Targets
A project that builds the nuget package "UnionSourceGeneratior_x.x.x.nupkg" that contains the source generator for custom union types.

#### UnionTests
A project with tests for verifying the correctness of generating and using union types.

#### TryUnions
A seperate projects with its own solution `TryUnions.sln`. This is intended to be used as a scratch pad for expirimenting with union types.
<br/>

## Getting Started

1) Open UnionTypes.sln in Visual Studio
2) Restore Nuget packages
3) Build solution (debug is okay)

4) Open TryUnions.sln
5) Fix references to point to UnionTypes.dll built by UnionTypes.sln
6) Open Manage Nuget packages for TryUnions project.
7) Create a package source (in settings gear) to point to build directory of UnionSourceGenerator_x.x.x.nupkg package.
8) Switch to new package source in drop down
9) Install UnionSourceGenerator_x.x.x.nupkg.

10) Open program.cs in TryUnions.sln
11) Mess about with defining union types
<br/>

## Using pre-declared OneOf&lt;T&gt; types.

A series of overloaded versions of OneOf&lt;T&gt; are predeclared in `UnionTypes.dll'.
Use them to declare unions of existing types only.

### Examples

#### Assigning values
```CSharp
OneOf<int, string> intOrString = 5;
OneOf<int, double> intOrDouble = intOrDouble.Create(intOrString);
```

#### Test for specific type
```CSharp
if (intOrString.IsType<int>()) { }
```

#### Test for specific type and get value
```CSharp
if (intOrString.TryGetValue<int>(out var value)) { }
```

#### Convert to specific type
```CSharp
var value = (int)intOrString;
```

### Convert between compatible unions
``` CSharp
OneOf<int, string> intOrString = 5;
OneOf<int, string, double> intOrStringOrDouble = OneOf<int, string, double>.Create(intOrstring);
```
### Compare for equality between union and values
``` CSharp
OneOf<int, string> intValue = 5;
OneOf<int, string> stringValue = "five";
var areEqual = intValue == 5;
var areEqual2 = intValue == intValue;
var notEqual = intValue == stringValue;
```

### Compare equality between different unions
``` CSharp
OneOf<int, string> value1 = 5;
OneOf<string, int> value2 = 5;
var areEqual = value1.Equals(value2);
```
<br/>

## Declaring custom union types

### Union type attributes

#### UnionAttribute
Use this on any type (partial struct) you want to be a union.
By default all public/internal nested record types are seen as discriminated union cases.

#### UnionTypesAttribute
Use this attribute (in addition to Union attribute) to indicate other types to be include as discriminated union cases.

#### UnionTags
Use this attribute (in addition to Union attribiute) to declare names of individual cases that carry no state (equivalent to enum names)


### Examples

#### Union type with nested case types
```CSharp
[Union]
public partial struct MyUnion
{
    public record struct A(int X);
    public record struct B(string Y);
    public record struct C(double Z);
}
```

#### Union type with external case types
```CSharp
public record struct A(int X);
public record struct B(string Y);
public record struct C(double Z);

[Union, UnionTypes(typeof(A), typeof(B), typeof(C))]
public partial struct MyUnion { }
```

#### Union type with tag-only cases
```CSharp
[Union, UnionTags("A", "B", "C")]
public partial struct MyUnion { }
```

#### Union type with mix of nested, external and tag-only cases
```CSharp
[Union, UnionTypes(typeof(B)), UnionTags("C")]
public partial struct MyUnion
{
    public record struct A(int X);
}

public record struct B(string Y);
```

### Using custom union types

#### Creating values
```CSharp
[Union, UnionTypes(typeof(B)), UnionTags("C")]
public partial struct ABC
{
    public record struct A(int X);
}
public record struct B(string Y);

var a = ABC.Create(new ABC.A(10));
ABC b = ABC.Create(new B("ten"));
ABC c = ABC.C;
```

#### Assigning values via implicit coercion
```CSharp
[Union, UnionTypes(typeof(B)), UnionTags("C")]
public partial struct ABC
{
    public record struct A(int X);
}
public record struct B(string Y);

ABC a = new ABC.A(10);
ABC b = new B("ten");
ABC c = ABC.C;
```

#### Test for specific case
```CSharp
[Union, UnionTypes(typeof(B)), UnionTags("C")]
public partial struct ABC
{
    public record struct A(int X);
}
public record struct B(string Y);

ABC abc = ...;
var isA = abc.IsA;
var isB = abc.IsB;
var isC = abc.IsC;
```

#### Test for specific case and get value
```CSharp
[Union, UnionTypes(typeof(B)), UnionTags("C")]
public partial struct ABC
{
    public record struct A(int X);
}
public record struct B(string Y);

ABC abc = ...;

var isA = abc.TryGetA(out ABC.A a);
var isA2 = abc.TryGetAValues(out int x);
var isA3 = abc.TryGetValue<ABC.A>(out a);

var isB = abc.TryGetB(out B b);
var isB2 = abc.TryGetBValues(out string y);
var isB3 = abc.TryGetValue<B>(out b);
```

#### Convert to specific case type and get value without failure
```CSharp
[Union, UnionTypes(typeof(B)), UnionTags("C")]
public partial struct ABC
{
    public record struct A(int X);
}
public record struct B(string Y);

ABC abc = ...;
ABC.A? a = abc.AsA();
B? b = abc.AsB();
```

#### Convert to specific case type with possible failure
```CSharp
[Union, UnionTypes(typeof(B)), UnionTags("C")]
public partial struct ABC
{
    public record struct A(int X);
}
public record struct B(string Y);

ABC abc = ...;
ABC.A a = abc.ToA();
ABC.A2 a = (ABC.A)abc;

B b = abc.ToB();
B b2 = (B)abc;
```

#### Convert between compatible unions
```CSharp

[Union]
public partial struct ABC
{
    public record A(int X);
    public record B(string Y);
    public record C(double Z);
}

[Union, UnionTypes(typeof(ABC.A), typeof(ABC.C))]
public partial struct AB { }

ABC abc = ...;
AB ab = AB.Create(abc);
```

#### Compare for equality between same union types
```CSharp

[Union]
public partial struct ABC
{
    public record A(int X);
    public record B(string Y);
    public record C(double Z);
}

[Union, UnionTypes(typeof(ABC.A), typeof(ABC.C))]
public partial struct AB { }

ABC value1 = new ABC.A(10);
ABC value2 = new B("ten");
va areEqual = value1 == value2;
```

#### Compare for equality between union and case value
```CSharp

[Union]
public partial struct ABC
{
    public record A(int X);
    public record B(string Y);
    public record C(double Z);
}

ABC abc = new ABC.A(10);
B b = new B("ten");
va areEqual = abc == b;
```

#### Compare for equality between diffent union types
```CSharp

[Union]
public partial struct ABC
{
    public record A(int X);
    public record B(string Y);
    public record C(double Z);
}

[Union, UnionTypes(typeof(ABC.A), typeof(ABC.C))]
public partial struct AB { }

ABC abc = new ABC.A(10);
AB ab = new B("ten");
va areEqual = abc.Equals(ab);
```
