
using UnionTypes;

MyUnion mu = new MyUnion.A(10);
Console.WriteLine(mu);

var obj = mu.Get<object>();


Result<int> result = new Result<int>.Success(10);
Console.WriteLine(result);

if (result.TryGetSuccessValues(out var value))
{
    Console.WriteLine($"result value = {value}");
}

[Union]
public partial struct MyUnion
{
    public record struct A(int X);
    public record struct B(string Y);
    public record struct C(double Z);
}

[Union]
public partial struct Result<T>
{
    public record struct Success(T Value);
    public record struct Failure(string Reason);
}

[Union]
public partial struct ValuesUnion
{
    public static partial ValuesUnion CreateA(int X);
    public static partial ValuesUnion CreateB(string Y);
    public static partial ValuesUnion CreateC();
}