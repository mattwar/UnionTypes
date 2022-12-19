
using UnionTypes;

var some = Option<int>.CreateSome(10);
var none = Option<int>.None;




[Union]
public partial struct TypesUnion
{
    public record struct A(int X);
    public record struct B(string Y);
    public record struct C(double Z);
}

[Union]
public partial struct TagsUnion
{
    public static partial TagsUnion CreateA(int X);
    public static partial TagsUnion CreateB(string Y);
    public static partial TagsUnion CreateC();
}

[Union]
public partial struct Result<T>
{
    public record struct Success(T Value);
    public record struct Failure(string Reason);
}

[Union]
public partial struct Result2<T>
{
    public static partial Result2<T> CreateSuccess(T value);
    public static partial Result2<T> CreateFailure(string reason);
}

[Union]
public partial struct Option<T>
{
    public static partial Option<T> CreateSome(T value);
    public static partial Option<T> CreateNone();
}
