# UnionTypes

### A collection of common union types for C# and a source generator for creating custom union types.
This repo started as an exploration of union types for introduction into C# as part of the C# LDM (Language Design Meeting).
It is now published (by me) for anyone to use.

You can download the nuget package for using these union types and the source generator for making custome ones at [Nuget](TBD).


## Included Union Types

- [OneOf](#Using-Predefined-OneOf-Types)
- Option
- Result
- [Custom](#Custom-Union-Types)

## Using Predefined OneOf Types

A series of overloaded versions of `OneOf<T1, T2, ...>` are predeclared in `UnionTypes.dll`.
Use them to declare type unions of any class, struct, interface or array types.

*Not supported: refs, ref structs and pointers.  That means no `Span<T>.`*

### Examples

#### Assigning values
``` CSharp
OneOf<int, string> intOrString = 5;
OneOf<int, double> intOrDouble = OneOf<int, double>.CreateFrom(intOrString);
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

#### Convert between compatible unions
``` CSharp
OneOf<int, string> intOrString = 5;
OneOf<int, string, double> intOrStringOrDouble = OneOf<int, string, double>.Convert(intOrstring);
```
#### Compare for equality between union and values
``` CSharp
OneOf<int, string> intValue = 5;
OneOf<int, string> stringValue = "five";
var areEqual = intValue == 5;
var areEqual2 = intValue == intValue;
var notEqual = intValue == stringValue;
```

#### Compare equality between different unions
``` CSharp
OneOf<int, string> value1 = 5;
OneOf<string, int> value2 = 5;
var areEqual = value1.Equals(value2);
```

### Variant


### Custom Union Types

Custom union types can be generated for you from a short partial struct definition with some attributes.
You can either generate a type union or a tag union.

Type unions are types that can hold a single instance of one of a set of unique types.
For instance, if you wanted to have a variable assignable to only either a Cat, Dog or Bird, you could use a type union to constrain the value to only one of those types.

Tag unions (aka tagged unions, discriminated unions, sum types, et al) are subtly different. 
They are not limited to a set of unique types, but instead are limited to a set of uniquely named states (tags) like an enum,
but each may be associated with its own unique set of variables.
If you are like me then you will have noticed that a unique name with associated variables sounds like a type, a class or struct in C#.
But for reasons we pretend that these cases are not types.

The benefit of a type union is your union type holds onto values that are freely usuable and conveyable outside the union without losing the knowledge of what they represent.
You can freely start with a custom type union containing a Cat, pull it out and pass it to a function that expects a Cat or a different type union including a Cat.
> Use a type union when the purpose is to bring together values of types that are useful in your application seperately from the union.

The benefit of a tag union is that you can skip the part about defining the individual case types,
but at the cost of losing the ability to pass around the values outside the union without losing their tag.
> Use a tag union when the purpose is to represent a set of states that only make sense together.

Both type and tag unions share the same generator because they end up being fundementally the same thing under the hood.
The generated type unions still have tags for efficiency sake, and generated tag unions still allow you to access the associated values as one unit via tuples.

#### Why use custom unions over OneOf.

The `OneOf` type is a general type union that can hold an instance of any one of the types specified as type arguments.
However, the downside of using `OneOf` is that its implementation options are limited. 
Because the case types are generic, the union must contain either a unique field for each case type or force them all into a single field by boxing any structs.
This can lead to inefficiencies either way.
The implementation of `OneOf` provided uses a single field and forces boxing. 
This may be satisfactory for most cases, but if allocation caused by boxing is a problem you may want to use a custom type union.

#### Declaring a Type Union
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

You determine the case via the `Kind` property and can access the value for each case using its associated `Value` property.
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


#### Declaring a Tag Union.

You declare a tag union by declaring a partial struct with a `TagUnion` attribute and `TagCase` attributes for each case.
By default the names of the methods become the names of the tags, and the parameters to the methods
become the associated values.

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

- If the case has multiple values, the property is named XXXValues and a tuple of all the values are returned.
- If the case has only one value, then the property is named XXXValue and just that one value is returned.
- If the case has no values, an IsXXX method is generated instead and returns `bool`.

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

#### Declaring a valueless Tag Union case as a Property.

If the case has no associated values, it's factory can be declared as a property instead of a method.
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
You can override it to become a parameterless method by using `FactoryIsProperty=false` in the `TagCase` attribute.

```CSharp
   Pet pet = Pet.Unknown;
```

#### Declaring a singleton Type Union case as a Property.

If the type used in a type union is a singleton, you can have the factory that constructs the union in that state be a property.
To do this, you will need to identify the case as a singleton with the IsSingleton property, 
request the factory be a property with the FactoryIsProperty property and give the factory a unique name
with the FactoryName property.

* The singleton type must declare exactly one public static read-only property or field on the type that returns an instance of the type to be considered a singleton.
* The type may also implement `ISingleton<T>` to allow generic access to the singleton value.*

```CSharp

    public class Unknown { public static readonly Unknown Singleton = new Unknown(); }

    [TypeUnion]
    [TypeCase(Type=typeof(Cat)]
    [TypeCase(Type=typeof(Dog))]
    [TypeCase(Type=typeof(Bird)]
    [TypeCase(Type=typeof(Unknown), IsSingleton=true, FactoryName="Unknown", FactoryIsProperty=true)]
    public struct Pet
    {
    }
```

You can use either the factory or direct assignement with the singleton.

```CSharp
    Pet pet = Pet.Unknown;
    Pet pet2 = Unknown.Singleton;
```

#### Declaring a Type Union with Nested Types.

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

#### Declaring a Type Union with Factory methods.

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

#### Assigning Specific Tag Values to Cases.

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


#### Declaring a Default Case for a Type or Tag Union

If your custom type or tag union is a struct you may encounter problems that occur when the union is in its default state,
because it is not yet assigned or it was assigned `default`.

You can choose one of your cases to be the default case for this situtation by assigning a `TagValue` of `0`,
since the tag value will otherwise be zero when it is not yet assigned one of the known cases.
>You should only do this only for a value-less tag case or a singleton type case.

*By default, all tag values are assigned postive, non-zero values to avoid being associated with the default state.*

















# OLD README
*To Be Removed*

## Contents

1. *UnionTypes*  
A project building `UnionTypes.dll` containing type definitions, interfaces and custom attributes needed for projects to use and declare union types.

2. *Generators*  
A project defining the core generators for OneOf and custom union types.
These generators can be used by T4 templates to pre-build union types.

3. *UnionSourceGenerator*  
A project that builds the Roslyn source generator for building custom union types.

4. *UnionSourceGenerator.Targets*  
A project that builds the nuget package "UnionSourceGeneratior_x.x.x.nupkg" that contains the source generator for custom union types.

5. *UnionTests*  
A project with tests for verifying the correctness of generating and using union types.

6. *TryUnions*  
A seperate projects with its own solution `TryUnions.sln`. This is intended to be used as a scratch pad for experimenting with union types.
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

## Using pre-declared `OneOf` types.

A series of overloaded versions of `OneOf<...>` are predeclared in `UnionTypes.dll`.
Use them to declare type unions of existing types only.

### Examples

#### Assigning values
``` CSharp
OneOf<int, string> intOrString = 5;
OneOf<int, double> intOrDouble = OneOf<int, double>.Convert(intOrString);
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

#### Convert between compatible unions
``` CSharp
OneOf<int, string> intOrString = 5;
OneOf<int, string, double> intOrStringOrDouble = OneOf<int, string, double>.Convert(intOrstring);
```
#### Compare for equality between union and values
``` CSharp
OneOf<int, string> intValue = 5;
OneOf<int, string> stringValue = "five";
var areEqual = intValue == 5;
var areEqual2 = intValue == intValue;
var notEqual = intValue == stringValue;
```

#### Compare equality between different unions
``` CSharp
OneOf<int, string> value1 = 5;
OneOf<string, int> value2 = 5;
var areEqual = value1.Equals(value2);
```
<br/>

## Declaring Custom Union Types

You can declare a new union type by declaring a partial struct type with a `Union` attribute using one of the following methods.  
The source generator will fill out the rest of the API for you.

### Declaring type unions from the `UnionTypes` attribute
All types listed in the `UnionTypes` attribute declared on the union type becomes a type case of the type union.

```CSharp
[Union]
[UnionTypes(typeof(Cat), typeof(Dog), typeof(Bird)]
public partial struct CatDogBird
{
}

public record struct Cat(string name, CatState state);
public record struct Dog(string name, DogState state);
public record struct Bird(string name, BirdState state, string[] thingsItSays);

```

### Declaring type unions from nested record type declarations
Any record type declared within the body of the union type declaration becomes a case type of a type union.

```CSharp
[Union]
public partial struct CatDogBird
{
    public record struct Cat(string name, CatState state);
    public record struct Dog(string name, DogState state);
    public record struct Bird(string name, BirdState state, string[] thingsItSays);
}
```

### Declaring type union from factory methods
Any partial factory method with the name "Create" declares a type case.

```CSharp
[Union]
public partial struct CatDogBird
{
    public static partial CatDogBird Create(Cat cat);
    public static partial CatDogBird Create(Dog dog);
    public static partial CatDogBird Create(Bird bird);
}

public record struct Cat(string name, CatState state);
public record struct Dog(string name, DogState state);
public record struct Bird(string name, BirdState state, string[] thingsItSays);
```

### Declaring tag unions from `UnionTags` attribute
All tag names listed in the `UnionTags` attribute declared on the union type become parameterless tag cases of the tag union.

```CSharp
[Union]
[UnionTags("Cat", "Dog", "Bird")]
public partial struct CatDogBird
{
}
```

### Declaring tag unions from factory methods
Any partial factory method with a unique name declares a tag case.

```CSharp
[Union]
public partial struct CatDogBird
{
    public static partial CatDogBird Cat(string name, CatState state);
    public static partial CatDogBird Dog(string name, DogState state, bool friendly);
    public static partial CatDogBird Bird(string name, BirdState state, string[] thingsItSays);
}
```

## Using custom tag unions

For the following examples, the union type `CatDogBird` is defined as a tag union.

```CSharp
[Union]
public partial struct CatDogBird
{
    public static partial CatDogBird Cat(string name, CatState state);
    public static partial CatDogBird Dog(string name, DogState state, bool friendly);
    public static partial CatDogBird Bird(string name, BirdState state, string[] thingsItSays);
}

public enum CatState { Eating, Sleeping, Playing, Hunting, Annoyed }
public enum DogState { Eating, Sleeping, Playing }
public enum BirdState { Quiet, Chirping }
```

### Using factory methods to construct a tag union.

```CSharp
var animal = CatDogBird.Cat("Fluffy", CatState.Sleeping);
```

### Test for specific tag case using IsXXX methods.
```CSharp
var animal = CatDogBird.Cat("Fluffy", CatState.Sleeping);
var isCatOrDog = animal.IsCat || animal.IsDog;
```

### Test and get specific tag case parameter values.
```CSharp
var animal = CatDogBird.Cat("Fluffy", CatState.Sleeping);
var isCat = animal.TryGetCat(out var name, out var state);
```

### Get specific tag case parameter values with possible exception.
```CSharp
var animal = CatDogBird.Cat("Fluffy", CatState.Sleeping);
var (name, state) = animal.GetCat();
```

### Get specific tag case paramter values or default.
```CSharp
CatDogBird animal = CatDogBird.Cat("Fluffy", CatState.Sleeping);
var (name, state) = animal.AsCat();
```

### Compare for equality between tag unions of same union type.
```CSharp
CatDogBird first = CatDogBird.Cat("Fluffy", CatState.Sleeping);
CatDogBird second = CatDogBird.Dog("Snoopy", DogState.Eating, friendly: true);
var same = first == second; 
```



## Using custom type unions

For the following examples, the union type `CatDogBird` is defined as a type union.

```Csharp
[Union]
public partial struct CatDogBird
{
    public static partial CatDogBird Create(Cat cat);
    public static partial CatDogBird Create(Dog dog);
    public static partial CatDogBird Create(Bird bird);
}

public record struct Cat(string name, CatState state);
public record struct Dog(string name, DogState state);
public record struct Bird(string name, BirdState state, string[] thingsItSays);
```

### Using factory methods to construct a type union.

```Csharp
var pet = CatDogBird.Create(new Cat("Fluffy", CatState.Sleeping));
```

### Using implicit coercion to assign type case values to a type union.
```Csharp
CatDogBird pet = new Cat("Fluffy", CatState.Sleeping);
```

### Test for specific type cases using `Is<T>` method.
```CSharp
CatDogBird pet = new Cat("Fluffy", CatState.Sleeping);
var isCatOrDog = pet.Is<Cat>() || pet.Is<Dog>();
```

### Test and get specific type case value using `TryGet<T>` method.
```CSharp
CatDogBird pet = new Cat("Fluffy", CatState.Sleeping);
var isCat = cat.TryGet<Cat>(out var cat);
```

### Get specific type case value with possible exception using `Get<T>` method.
```CSharp
CatDogBird pet = new Cat("Fluffy", CatState.Sleeping);
Cat cat = cat.Get<Cat>();
```

### Convert between compatible type unions using `Convert` method.
```CSharp
OneOf<Cat, Dog> catOrDog = new new Cat("Fluffy", CatState.Sleeping);
CatDogBird pet = CatDogBird.Convert(catOrDog);
```

### Compare for equality between type unions of same union type.
```CSharp
CatDogBird cat = new Cat("Fluffy", CatState.Sleeping);
CatDogBird dog = new Dog("Snoopy", DogState.Eating, friendly: true);
var same = cat == dog; 
```

### Compare for equality between type unions and case types.
```CSharp
CatDogBird cat = new Cat("Fluffy", CatState.Sleeping);
Dog dog = new Dog("Snoopy", DogState.Eating);
var same = cat == dog; 
```

### Compare for equality between different type union types.
```CSharp
OneOf<Cat, Dog> catOrDog = new new Cat("Fluffy", CatState.Sleeping);
CatDogBird pet = new Cat("Fluffy", CatState.Sleeping);
var same = pet == catOrDog;
```

