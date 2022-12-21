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
OneOf<int, double> intOrDouble = intOrDouble.Convert(intOrString);
```

#### Test for specific type
```CSharp
if (intOrString.Is<int>()) { }
```

#### Test for specific type and get value
```CSharp
if (intOrString.TryGet<int>(out var value)) { }
```

#### Convert to specific type
```CSharp
var value = (int)intOrString;
```

### Convert between compatible unions
``` CSharp
OneOf<int, string> intOrString = 5;
OneOf<int, string, double> intOrStringOrDouble = OneOf<int, string, double>.Convert(intOrstring);
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
Instead of using this attribute, you can alternatively declare a partial factory method for the external type.

#### UnionTags
Use this attribute (in addition to Union attribiute) to declare names of individual cases that carry no state (equivalent to enum names)
Instead of using this attribute, you can alternatively declare a partial parameterless factory method for this case.


### Examples

#### Union type with nested case types
Any record type declared within the body of the struct automatically becomes a case type of the union.

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
Any partial factory method with the same name as the external type and a single parameter with the same type as the external type becomes a case type for the union.

```CSharp
public record struct A(int X);
public record struct B(string Y);
public record struct C(double Z);

[Union]
public partial struct MyUnion 
{ 
    public static partial MyUnion A(A a);
    public static partial MyUnion B(B b);
    public static partial MyUnion C(C c);
}
```

#### Union type with tag-only cases

Any partial factory method that does not refer to an external type is considered a tag case (non type case).

```CSharp
[Union]
public partial struct MyUnion 
{ 
    public static partial MyUnion A(int x);
    public static partial MyUnion B(string y);
    public static partial MyUnion C(double z);
    public static partial MyUnion D();
    public static partial MyUnion E(int x, string y, double z);
}
```

#### Union type with mix of nested, external and tag-only cases
Unions can contain any mix of nested type, external type or tag cases.

```CSharp
public record struct B(string Y);

[Union]
public partial struct MyUnion
{
    public record struct A(int X);
    public static partial MyUnion B(B b);
    public static partial MyUnion C(double z);
    public static partial MyUnion D();
    public static partial MyUnion E(int x, string y, double z);
}
```

### Using custom union types

#### Creating values via factories
```CSharp
public record struct B(string Y);

[Union]
public partial struct ABC
{
    public record struct A(int X);
    public static partial ABC B(B b);
    public static partial ABC C(double z);
    public static partial ABC D();
}

var a = ABC.Create(new ABC.A(10));
ABC b = ABC.Create(new B("ten"));
ABC c = ABC.C(5.0);
ABC d = AbC.D();
```

#### Assigning values via implicit coercion
```CSharp
public record struct B(string Y);

[Union]
public partial struct ABC
{
    public record struct A(int X);
    public static partial ABC B(B b);
}

ABC a = new ABC.A(10);
ABC b = new B("ten");
```

#### Test for specific case
```CSharp
public record struct B(string Y);

[Union]
public partial struct ABC
{
    public record struct A(int X);
    public static partial ABC B(B b);
    public static partial ABC C(double z);
    public static partial ABC D();
}

ABC abc = ...;
var isA = abc.IsA;
var isA2 = abc.Is<ABC.A>();
var isB = abc.IsB;
var isB2 = abc.Is<B>();
var isC = abc.IsC;
var isD = abc.IsD;
```

#### Test and get specific case value
```CSharp
public record struct B(string Y);

[Union]
public partial struct ABC
{
    public record struct A(int X);
    public static partial ABC B(B b);
    public static partial ABC C(double z);
    public static partial ABC D(int x, string y);
}

ABC abc = ...;
var tryA = abc.TryGetA(out ABC.A a);
var tryA3 = abc.TryGet<ABC.A>(out a);
var tryB = abc.TryGetB(out B b);
var tryB3 = abc.TryGet<B>(out b);
var tryC = abc.TryGetC(out double z);
var tryD = abc.TryGetD(out int x, out string y);
```

#### Get specific case value(s) with possible failure
```CSharp
public record struct B(string Y);

[Union]
public partial struct ABC
{
    public record struct A(int X);
    public static partial ABC B(B b);
    public static partial ABC C(double z);
    public static partial ABC D(int x, string y);
}

ABC abc = ...;
ABC.A a = abc.GetA();
ABC.A a2 = abc.Get<ABC.A>();
ABC.A a3 = (ABC.A)abc;
B b = abc.GetB();
B b2 = abc.Get<B>();
B b2 = (B)abc;
double z = abc.GetC();
(int x, string y) dTuple = abc.GetD();
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

[Union]
public partial struct AB 
{ 
    public static partial AB A(ABC.A a);
    public static partial AB B(ABC.B b);
}

ABC abc = ...;
AB ab = AB.Convert(abc);
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

[Union]
public partial struct AB
{ 
    public static partial AB A(ABC.A a);
    public static partial AB B(ABC.B b);
}

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

[Union]
public partial struct AB 
{ 
    public static partial AB A(ABC.A a);
    public static partial AB B(ABC.B b);
}

ABC abc = new ABC.A(10);
AB ab = new B("ten");
va areEqual = abc.Equals(ab);
```
