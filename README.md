# UnionTypes.Toolkit

A collection of common union types for C# and a source generator for creating custom union types.

This repo started as an exploration of union types for introduction into C# and the dotnet runtime as part of the C# LDM (Language Design Meeting).
It is now published (by me) for anyone to use.

---
Table of Contents
- [Download the Toolkit](#TBD)
- [Using the included union types](#Using-the-Included-Union-Types)
- [Generating Custom Types](#Custom-Union-Types)
- [What is in the Repo?](#What-is-in-the-Repo?)
- [Debugging the Source Generators](#Debuggin-the-Source-Generators)


---

## Using the Included Union Types

The `UnionTypes.dll` includes many predefined union types ready to use.

- [OneOf](#Using-OneOf-Types)
- [Variant](#Using-Variant-Types)
- Option
- Result
- ITypeUnion

---
## Using OneOf Types

The `OneOf` type is a series of overloaded generic types.
Each `OneOf` type is a closed type union, only allowing values of types specified in the type arguments.
Use them to declare type unions of any class, struct, interface or array types.

*Not supported: refs, ref structs and pointers. That means no `Span<T>.`*

*When instances of value types (structs in C#) are placed in the union, they are stored by boxing the value, which may cause GC pressure.*

### Creating an Instance

You can create an instance of a `OneOf` type by calling one of the `Create` factory method or via assignment and implicit coercion.

```CSharp
    var number = OneOf<int, double>.Create(5);  //  using factory
    OneOf<int, double> number = 5;              //  using assignment.
```

You can also attempt to create an instance using the `TryCreate<T>` factory method that attempts to create an instance of the OneOf type
from an arbitrary value.

```CSharp
    object someValue = 5;
    if (OneOf<int, double>.TryCreate(someValue, out var number)) { ... })
```

You can also use `TryCreate` to attempt to create a OneOf from a different type union.

```CSharp
    OneOf<int, string> intOrString = 5;
    if (OneOf<int, double>.TryCreate(intOrString, out var number)) { ... })
```

### Accessing the Value

There are multiple ways to access the underlying value.

You can access it in a weakly-typed way by using the `Value` property.
```CSharp
   object value = number.Value;
```

You can access it in a strongly-typed way via explicit coersion to one of the known types.
However, this may throw an `InvalidCastException` if you use the wrong type.
```CSharp
    int x = (int)number;
```

You can access each possible strongly-typed value via additional value properties.
Every `OneOf` type has a value property for each type argument; `Type1Value`, `Type2Value`, etc.
You can determine which one of these is currently valid by checking the `Kind` property.
Accessing an invalid value property will return default.
```CSharp
    var isLessThan5 = number.Kind switch { 
        1 => number.Type1Value < 5,
        2 => number.Type2Value < 5.0
        _ => false
        };
```

Lastly, you can access arbitrarilly typed values via the `TryGet<T>` method.
```CSharp
    if (number.TryGet<int>(out var intValue)) { ... }
```
This technique is useful when you don't now the exact union type, and have a reference to `ITypeUnion`,
or when you want to access the value using a base class or interface, or via a different union type.

```CSharp
if (intOrDouble.TryGet<OneOf<int, string>>(out var intOrString)) { ... })
```

### Comparing Equality

Each `OneOf` type declares pass-through equality operators and implements `IEquatable<T>` so you can compare two instances of the same OneOf type.
If the values are the same, the OneOf instances will be considered the same.

``` CSharp
OneOf<int, double> number1 = 5;
OneOf<int, double> number2 = "five";
if (number1 == number2) { ... }
if (number1.Equals(number2)) { ... }
```
Since there are also implicit coercion operators, you can compare a OneOf instance with a value of one of the case types.

```CSharp
OneOf<int, double> number = 5;
if (number == 5) { ... }
if (number.Equals(5)) { ... }
```

You can also compare an instance of one `OneOf` type with an instance of another `OneOf` type, or any other type union that implements `ITypeUnion` 
using the generic `Equals` method.

```CSharp
OneOf<int, double> number = 5;
OneOf<int, string> value = 5;
if (number.Equals(value)) { ... }

### Compare equality between different unions

``` CSharp
OneOf<int, string> value1 = 5;
OneOf<string, int> value2 = 5;
var areEqual = value1.Equals(value2);
```

---
## Using Variant Types

The `Variant` type is a type union that is not closed to a fixed number of case types.
Any value of any type can be assigned or converted into a `Variant`, even the value `null`.
What makes a variant interesting is that it is designed to avoid boxing of many value and struct types.
If the value or struct type constains no reference type members and fits withing 64 bits, it will be stored within the variant without boxing.

### Creating an Instance

You can create an instance of a `Variant` by calling one of the `Create` factory methods or 
via assignment and implicit coercion if the value is one of the known primitives.

```CSharp
    var number = Variant.Create(5);  //  using factory
    Variant number = 5;              //  using assignment.
    Variant point = Variant.Create(new Point(3, 4)); // factory only if its not a known primitive.
```

You can also create a variant using the `TryCreate<T>` method. However, it will always succeed, since variants may contain any value.
It exists on the type to satisfy the contract of the `ITypeUnion<T>` interface.

### Accessing the Value

You can access the value in a weakly-typed way by using the `Value` property.
However, this may cause boxing if the value is a struct type.

```CSharp
   object value = variant.Value;
```

You can access the strongly-typed value via any of the specialized value properties.
The `Variant` type has a specialized value properties for many well known types; `Int32Value`, `DoubleValue`, `StringValue` etc.
You can determine which of these is the correct property to access using the `Kind` property.

```CSharp
    var isLessThan5 = variant.Kind switch { 
        VariantKind.Int32 => variant.Int32Value < 5,
        VariantKind.Double => variant.DoubleValue < 5.0,
        VariantKind.Int64 => variant.Int64Value < 5L,
        _ => false
        };
```

You can attempt to access arbitrary strongly-typed values using the `TryGet<T>` method.

```CSharp
    if (variant.TryGet<Point>(out var point)) { ... }
```
There is also `Get<T>` that returns the value if possible or throws an `InvalidCastException`,
`GetOrDefault<T>` that returns the value if possible or `default`,
and `CanGet<T>` that tells you if the `TryGet<T>` method will succeed.

If you want to know the value's type without potential boxing, you can use the `Type` property.

```CSharp
    var currentType = variant.Type;
```

### Nulls

The `Variant` can store a null value.

You can determine if the current value is `null`, by checking the `IsNull` property or by comparing the `Kind` property to `VariantKind.Null`.
```CSharp
    if (variant.IsNull) { ... }
    if (variant.Kind == VariantKind.Null) { ... }
```

*The `TryGet<T>` and `CanGet<T>` methods will return false when the value is null, even if the type `T` can contain a null value; for example, `int?` or `string?`.*


---
## Custom Union Types

Custom union types can be generated for you from a short `partial struct` definition with some attributes.

You can either generate a *type union* or a *tag union*.

- Type unions are types that can hold a single instance of one of a set of unique types.
For instance, if you wanted to have a variable assignable to only either a Cat, Dog or Bird, you could use a type union to constrain the value to only one of those types.

- Tag unions (aka tagged unions, discriminated unions, sum types, et al) are subtly different. 
They are not limited to a set of unique types, but instead are limited to a set of uniquely named states (tags) like an enum,
but each may be carry with it its own unique set of variables.
If you are like me then you will have noticed that a uniquely named set of variables sounds like a type, like a class or struct in C#.
But for reasons we pretend that these cases are not types.

The benefit of a type union is that your union type holds onto values that are freely usuable and conveyable outside the union without losing the knowledge of what they represent.
You can freely start with a custom type union containing a Cat, pull it out and pass it to a function that expects a Cat or put it into a different type union that allows a Cat.
> Use a type union when the case types are meaningful in your application outside the definition of the union.

The benefit of a tag union is that you can skip the part about defining the individual case types,
but at the cost of losing the ability to pass around the values outside the union without losing their tag.
> Use a tag union when use of the cases as types outside the union is not meaningful.

Both type and tag unions share the same generator because they end up being fundementally the same thing under the hood.
The generated type unions still have tags for efficiency sake, and generated tag unions still allow you to access the case values as one unit via tuples.

### Why Generate Custom Type Unions?

Instead of generating a custom union, you could use one of the predefined `OneOf` types.
The `OneOf` type is a general type union that can hold an instance of any one of the types specified in its type arguments.
However, the downside of using `OneOf` is that its implementation options are limited. 
Because the case types are generic, the union must contain either a unique field for each case type or force them all into a single field by boxing any structs.
This can lead to inefficiencies either way.
The implementation of `OneOf` provided uses a single field and forces boxing. 
This may be satisfactory for most cases, but if allocation caused by boxing is a problem you may want to use a custom type union.

### Declaring a Type Union
You can declare a type union by declaring a partial struct with a `TypeUnion` attribute and `TypeCase` attributes for each case.

```CSharp
    [TypeUnion]
    [TypeCase(Type=typeof(Cat)]
    [TypeCase(Type=typeof(Dog)]
    [TypeCase(Type=typeof(Bird)]
    public struct Pet
    {
    }
```

As soon as its declared you can start using the type like this.

```CSharp
    Pet pet = Pet.Create(new Cat("Mr Fluffy")); // call factory method
    Pet pet = new Cat("Mr Fluffy"); // or use direct assignment
    ...
    switch (pet.Kind)
    {
        case Pet.Case.Cat: 
            Console.WriteLine($"Cat's name is {pet.CatValue.Name}");
            break;
        case Pet.Case.Dog:
            Console.WriteLine($"Dog's name is {pet.DogValue.Name}");
            break;
        case Pet.Case.Bird:
            Console.WriteLine($"Bird's name is {pet.BirdValue.Name}");
            break;
    }
```

You determine the case via the `Kind` property and can access the value for each case using its corresponding `Value` property.
A factory exists for each type case, each called `Create` by default, but you can customize it via the attribute or declaring the factory manually.

If you enable generation of `Match` functions via `TypeUnion(GenerateMatch=true)` you can write the following instead,
and skip refering to the kind and value properties, but potentially cause delegate allocations due to capture.

```CSharp
    pet.Match(
        cat => Console.WriteLine($"Cat's name is {cat.Name}"),
        dog => Console.WriteLine($"Dog's name is {dog.Name}"),
        bird => Console.WriteLine($"Bird's name is {bird.Name}")
    );
```
There is both void returning and value returning versions of `Match`. 
They are not generated by default.


### Declaring a Tag Union.

You declare a tag union by declaring a partial struct with a `TagUnion` attribute and `TagCase` attributes for each case
placed on partial static methods serving as factory methods.
By default the names of the methods become the names of the tags, and the parameters to the methods
become the case values.

```CSharp
    [TagUnion]
    public struct Pet
    {
        [TagCase]
        public static partial Pet Cat(string name, int toys);

        [TagCase]
        public static partial Pet Dog(string name, bool friendly);

        [TagCase]
        public static partial Pet Bird(string name, string[] thingsItSays);
    }
```

You use it similarly to how type unions are used. 
However, you must always construct the tag union via the factory since you have no separate instance to assign
and you must identify the tag when you create it.
In addition, some of the generated accessor properties have different names than for type unions.

- If the case has multiple values, the property is named [case]Values and a tuple of all the values are returned.
- If the case has only one value, then the property is named [case]Value and just that one value is returned.
- If the case has no values, an Is[case] method is generated instead and returns `bool`.

*It is possible to customize all the factory and property names.*

```CSharp
    Pet pet = Pet.Cat("Mr Fluffy");
    ...
    switch (pet.Kind)
    {
        case Pet.Case.Cat: 
            Console.WriteLine($"Cat's name is {pet.CatValues.name}");
            break;
        case Pet.Case.Dog:
            Console.WriteLine($"Dog's name is {pet.DogValues.name}");
            break;
        case Pet.Case.Bird:
            Console.WriteLine($"Bird's name is {pet.BirdValues.name}");
            break;
    }
```

You can also generate `Match` functions for tag unions using the `GenerateMatch=true` property in the `TagUnion` attribute.

### Declaring a Tag Union Case as a Property.

If the case has no case values, it's factory can be declared as a property instead of a method.
If you are not able to declare partial properties, you can place the `TagCase` attribute for the case on the union type itself.

```CSharp
    [TagUnion]
    [TagCase(Name="Unknown")]
    public struct Pet
    {
        [TagCase]
        public static partial Pet Cat(string name, int toys);

        [TagCase]
        public static partial Pet Dog(string name, bool friendly);

        [TagCase]
        public static partial Pet Bird(string name, string[] thingsItSays);
    }
```

The new `Unknown` state is defined as a property, since it has no values.
You can override this behavior to make it a parameterless method instead by using `FactoryIsProperty=false` in the `TagCase` attribute.

```CSharp
   Pet pet = Pet.Unknown;
```

### Declaring a Type Union case as a Property.

If the type used in a type union is a singleton, you can have the factory that constructs the union in that state be a property.
To do this, you will need to identify the case as a singleton with the IsSingleton property, 
and request the factory be a property with the FactoryIsProperty property.

* The singleton type must declare exactly one public static read-only property or field on the type that returns an instance of the type to be considered a singleton.
* The type may also implement `ISingleton<T>` to allow generic access to the singleton value.*

```CSharp

    public class Unknown { public static readonly Unknown Singleton = new Unknown(); }

    [TypeUnion]
    [TypeCase(Type=typeof(Cat)]
    [TypeCase(Type=typeof(Dog))]
    [TypeCase(Type=typeof(Bird)]
    [TypeCase(Type=typeof(Unknown), IsSingleton=true, FactoryIsProperty=true)]
    public struct Pet
    {
    }
```

You can use either the factory or direct assignement with the singleton.

```CSharp
    Pet pet = Pet.Unknown;
    Pet pet2 = Unknown.Singleton;
```

### Declaring a Union Case without an Accessor.

If a type case is a singleton type or a tag case has no values, you can omit the generation of the
accessor property by setting `HasAccess=false` in the `TypeCase` or `TagCase` attribute.

For example, normally a tag union will generate `Is` properties for cases without values.

```CSharp
    [TagUnion]
    [TagCase(Type=typeof(Cat, HasAccessor=false)]
    public struct Pet
    {
        [TagCase(HasAccessor=false)]
        public static partial Pet Dog();

        [TagCase]
        public static partial Pet Bird(int numberOfThingsItSays);
    }
```

Now only the bird case as a value accessor, since its the only one with values.


### Declaring a Type Union with Nested Types.

If your partial type declaration includes nested types that you want to be cases of the union,
you can alternatively put the `TypeCase` attribute on those types directly and avoid respecifying the type.

```CSharp
    [TypeUnion]
    public struct Pet
    {
        [TypeCase]
        public record struct Cat(string Name, int Toys);
        [TypeCase]
        public record struct Dog(string Name, bool Friendly);
        [TypeCase]
        public record struct Bird(string Name, string[] ThingsItSays);
    }
```

### Declaring a Type Union with Factory Methods.

You can also have custom factory methods specified for the type union if you find that easier to reason about.

```CSharp
    [TypeUnion]
    public struct Pet
    {
        [TypeCase]
        public static partial Pet MakeCat(Cat cat);
        [TypeCase]
        public static partial Pet MakeDog(Dog dog);
        [TypeCase]
        public static partial Pet MakeBird(Bird bird);
    }
```

### Assigning Specific Tag Values to Cases.

You may want to avoid versioning problems by future-proofing your union types by assigning them specific tag values for each case.
This is the same as you might do with an enum.

By giving each case a unique value, you can keep some binary compatability with past versions of the union type,
when new cases are added. 

Of course, this won't solve the real problem of code not handling the new cases, 
but at least it keeps the code that does handle the old cases from breaking.

```CSharp
    [TagUnion]
    [TagCase(Name="Cat", TagValue=1)]
    [TagCase(Name="Dog", TagValue=2)]
    [TagCase(Name="Bird", TagValue=3)]
    public struct Pet
    {
        [TagCase]
        public static partial Pet Cat(string name, int toys);
        [TagCase]
        public static partial Pet Dog(string name, bool friendly);
        [TagCase]
        public static partial Pet Bird(string name, string[] thingsItSays);
    }
```

*You can do this for both type and tag unions.*


### Declaring a Default Case for a Type or Tag Union

If your custom type or tag union is a struct you may encounter problems that occur when the union is in its default state,
because it is not yet assigned or it was assigned `default`.

You can choose one of your cases to be the default case for this situtation by assigning a `TagValue` of `0`,
since the tag value will otherwise be zero when it is not yet assigned one of the known cases.
>You should only do this only for a value-less tag case or a singleton type case.

*By default, all tag values are assigned postive, non-zero values to avoid being associated with the default state.*

---
## What is in the Repo?

- *UnionTypes*  
A project building `UnionTypes.dll` containing type definitions, interfaces and custom attributes needed for projects to use and declare union types.

- *Generators*  
A project defining the core generators for OneOf and custom union types.
These generators can be used by T4 templates to pre-build union types.

- *UnionSourceGenerator*  
A project that builds the Roslyn source generator for building custom union types.

- *UnionSourceGenerator.Targets*  
A project that builds the nuget package "UnionSourceGenerator_x.x.x.nupkg" that contains the source generator for custom union types.

- *UnionTypesTests*  
A project with tests for verifying the correctness of all predefined union types.

- *UnionGeneratorTests*  
A project with tests for verifying the correctness of the union generators as T4 template generators.

- *UnionSourceGeneratorTests*  
A project with tests for verifying the correctness of the union source generator as compiler plug-in.

- *TryUnions*  
A seperate project with its own solution `TryUnions.sln` for experimenting with unpublished changes.


## Debugging the Source Generators

For the most part, the source generators are debugged using the tests in the UnionTests project.

If you need to experiment with using the generators, or changes to the generators not yet publish,
you can configure the `TryUnions` project to use the local build of the source generator.

- Open TryUnions.sln
- Fix references to point to UnionTypes.dll built by UnionTypes.sln
- Open Manage Nuget packages for TryUnions project.
- Create a package source (in settings gear) to point to build directory of UnionSourceGenerator_x.x.x.nupkg package.
- Switch to new package source in drop down
- Install UnionSourceGenerator_x.x.x.nupkg.
- Open program.cs in TryUnions.sln
- Mess about with defining union types
