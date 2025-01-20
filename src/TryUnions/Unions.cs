#if false
global using IntOrString = UnionTypes.OneOf<int, string>;
global using IntOrStringOrDouble = UnionTypes.OneOf<int, string, double>;

using UnionTypes;

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

// Type union
[Union]
public partial struct CatDogBird
{
    public static partial Animal Create(Dog value);
    public static partial Animal Create(Cat value);
    public static partial Animal Create(Bird value);
}

public enum CatState { Eating, Sleeping, Playing, Hunting, Annoyed }
public enum DogState { Eating, Sleeping, Playing }
public enum BirdState { Quiet, Chirping }

public record struct Cat(string name, CatState state);
public record struct Dog(string name, DogState state, bool friendly);
public record struct Bird(string name, BirdState state, string[] thingsItSays);


// Result<T> as type union w/ nested types
[Union]
public partial struct Result<T>
{
    public record struct Success(T Value);
    public record struct Failure(string Reason);
}

// Result<T> as tag union
[Union]
public partial struct Result<T>
{
    public static partial Result<T> Success(T value);
    public static partial Result<T> Failure(string reason);
}

// Option<T> as tag union
[Union]
public partial struct Option<T>
{
    public static partial Option<T> Some(T value);
    public static partial Option<T> None();
}

#endif