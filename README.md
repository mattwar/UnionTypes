# UnionTypes
#### Tools for experimenting with union types.
This repo is intended to be used for discussion and experimentation during design of discriminated unions in C# future.


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

## Using pre-declared `OneOf` types.

A series of overloaded versions of `OneOf<...>` are predeclared in `UnionTypes.dll`.
Use them to declare type unions of existing types only.

### Examples

#### Assigning values
```CSharp
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

