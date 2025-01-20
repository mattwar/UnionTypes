global using IntOrString = UnionTypes.OneOf<int, string>;
using UnionTypes;

Variant a = 100;
Variant b = 10.0;

Console.WriteLine(a == b);
Console.ReadLine();


[Union]
[UnionTags("A", "B", "C")]
public partial struct TagUnion
{
}

[Union]
[UnionTypes(typeof(string), typeof(int), typeof(double), typeof(float), typeof(long), typeof(object))]
public partial struct Variant
{
}