#if false
global using IntOrString = UnionTypes.OneOf<int, string>;
global using IntOrStringOrDouble = UnionTypes.OneOf<int, string, double>;

using UnionTypes;

// Types union declare with nested record declarations
[Union]
public partial struct TypesUnion
{
    public record struct A(int X);
    public record struct B(string Y);
    public record struct C(double Z);
}

// Tag Union
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


// Type union as a tag union?
[Union]
public partial struct Animal
{
    public static partial Animal Dog(Dog value);
    public static partial Animal Cat(Cat value);
    public static partial Animal Bird(Bird value);
}

public record Dog(int X);
public record Cat(string Y);
public record Bird(double Z);


[Union]
public partial struct Animal
{
    public static partial Animal Dog(Dog value);
    public static partial Animal Cat(Cat value);
    public static partial Animal Bird(Bird value);
}


// Tags union declared with partial factory methods
[Union]
public partial struct TagsUnion
{
    public static partial TagsUnion A(int x);
    public static partial TagsUnion B(string y);
    public static partial TagsUnion C();
    public static partial TagsUnion D(int p, int q);
}

//// Result<T> as types union
//[Union]
//public partial struct Result<T>
//{
//    public record struct Success(T Value);
//    public record struct Failure(string Reason);
//}

// Result<T> as tags union
[Union]
public partial struct Result<T>
{
    public static partial Result<T> Success(T value);
    public static partial Result<T> Failure(string reason);
}

// Option<T> as tags union
[Union]
public partial struct Option<T>
{
    public static partial Option<T> Some(T value);
    public static partial Option<T> None();
}
#endif