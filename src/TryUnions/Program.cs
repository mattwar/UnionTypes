using UnionTypes;

var pet1 = CatDogBird.Cat("fluffy", CatState.Sleeping);
var pet2 = CatDogBird.Cat("fluffy", CatState.Sleeping);

var equals = pet1 == pet2;

Console.ReadLine();


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
