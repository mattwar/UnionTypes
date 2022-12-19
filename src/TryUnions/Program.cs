
using UnionTypes;

var some = Option<int>.Some(10);
var none = Option<int>.None;

[Union]
public partial struct Option<T>
{
    public static partial Option<T> Some(T value);
    public static partial Option<T> None();
}










#if false

// Types union declare with nested record declarations
[Union]
public partial struct TypesUnion
{
    public record struct A(int X);
    public record struct B(string Y);
    public record struct C(double Z);
}

// Tags union declared with partial factory methods
[Union]
public partial struct TagsUnion
{
    public static partial TagsUnion A(int X);
    public static partial TagsUnion B(string Y);
    public static partial TagsUnion C();
}

// Result<T> as types union
[Union]
public partial struct Result<T>
{
    public record struct Success(T Value);
    public record struct Failure(string Reason);
}

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
