namespace UnionTypes;

public class None : ISingleton<None>
{
    private None() {}
    public static None Singleton { get; } = new None();
}

#if false
public class Nothing : ISingleton<Nothing>
{
    private Nothing() { }
    public static Nothing Singleton { get; } = new Nothing();
}

public class Null : ISingleton<Null>
{
    private Null() { }
    public static Null Singleton { get; } = new Null();
}

public class Unknown : ISingleton<Unknown>
{
    private Unknown() { }
    public static Unknown Singleton { get; } = new Unknown();
}

public class Undefined : ISingleton<Undefined>
{
    private Undefined() { }
    public static Undefined Singleton { get; } = new Undefined();
}
#endif