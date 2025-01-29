MyUnion x = 3;

if (x.Kind == MyUnion.Case.A)
{
    Console.WriteLine($"A: {x.AValue}");
}
else if (x.Kind == MyUnion.Case.B)
{
    Console.WriteLine($"B: {x.BValue}");
}

Console.ReadLine();
