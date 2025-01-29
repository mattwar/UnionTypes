using UnionTypes.Toolkit;

[TypeUnion]
public partial struct MyUnion
{
    [TypeCase]
    public static partial MyUnion CreateA(int x);

    [TypeCase]
    public static partial MyUnion CreateB(double x);
}
