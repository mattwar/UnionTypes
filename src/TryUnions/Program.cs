
using UnionTypes;

MyUnion mu = new MyUnion.OptionA(10);
Console.WriteLine(mu);


Result<int> result = Result.Success(10);
Console.WriteLine(result);

if (result.TryGetSuccessValue(out var value))
{
    Console.WriteLine($"result value = {value}");
}

[Union]
public partial struct MyUnion
{
    public record struct OptionA(int X);
    public record struct OptionB(string Y);
    public record struct OptionC(double Z);
}

[Union]
public partial struct Result<T>
{
    public record struct Success(T Value);
    public record struct Failure(string Reason);
}

public static class Result
{
    public static Result<T> Success<T>(T value) => new Result<T>.Success(value);
    public static Result<T> Failure<T>(string reason) => new Result<T>.Failure(reason);
}