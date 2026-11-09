using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(ParserPatternMatchingBenchmarks).Assembly).Run(args);

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
public class ParserPatternMatchingBenchmarks
{
    private readonly StateStruct _structState = default;
    private readonly StateClass _classState = StateClass.Instance;

    [Benchmark(Baseline = true), BenchmarkCategory("Parser struct")]
    public int OldStruct() => OldParser(_structState);

    [Benchmark, BenchmarkCategory("Parser struct")]
    public int PatternStruct() => PatternParser(_structState);

    [Benchmark(Baseline = true), BenchmarkCategory("Parser class")]
    public int OldClass() => OldParser(_classState);

    [Benchmark, BenchmarkCategory("Parser class")]
    public int PatternClass() => PatternParser(_classState);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int OldParser<TState>(TState state)
    {
        if (state is IReadOnlyList<KeyValuePair<string, object?>>)
            return ((IReadOnlyList<KeyValuePair<string, object?>>)state).Count;

        return 0;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int PatternParser<TState>(TState state)
    {
        if (state is IReadOnlyList<KeyValuePair<string, object?>> properties)
            return properties.Count;

        return 0;
    }
}

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
public class LoggerPatternMatchingBenchmarks
{
    private readonly StateStruct _structState = default;
    private readonly StateClass _classState = StateClass.Instance;

    [Benchmark(Baseline = true), BenchmarkCategory("Logger struct")]
    public int OldStruct() => OldLogger(_structState);

    [Benchmark, BenchmarkCategory("Logger struct")]
    public int PatternStruct() => PatternLogger(_structState);

    [Benchmark(Baseline = true), BenchmarkCategory("Logger class")]
    public int OldClass() => OldLogger(_classState);

    [Benchmark, BenchmarkCategory("Logger class")]
    public int PatternClass() => PatternLogger(_classState);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int OldLogger<TState>(TState state)
    {
        if (state is IReadOnlyList<KeyValuePair<string, object?>>)
        {
            int count = ((IReadOnlyList<KeyValuePair<string, object?>>)state).Count;
            int checksum = 0;
            for (int i = 0; i < count; ++i)
                checksum += ((IReadOnlyList<KeyValuePair<string, object?>>)state)[i].Value?.GetHashCode() ?? 0;
            return checksum;
        }

        return 0;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int PatternLogger<TState>(TState state)
    {
        if (state is IReadOnlyList<KeyValuePair<string, object?>> properties)
        {
            int count = properties.Count;
            int checksum = 0;
            for (int i = 0; i < count; ++i)
                checksum += properties[i].Value?.GetHashCode() ?? 0;
            return checksum;
        }

        return 0;
    }
}

public readonly struct StateStruct : IReadOnlyList<KeyValuePair<string, object?>>
{
    private static readonly KeyValuePair<string, object?>[] Properties =
    {
        new("First", "alpha"),
        new("Second", "bravo"),
        new("Third", "charlie"),
    };

    public int Count => Properties.Length;

    public KeyValuePair<string, object?> this[int index] => Properties[index];

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, object?>>)Properties).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Properties.GetEnumerator();
}

public sealed class StateClass : IReadOnlyList<KeyValuePair<string, object?>>
{
    private static readonly KeyValuePair<string, object?>[] Properties =
    {
        new("First", "alpha"),
        new("Second", "bravo"),
        new("Third", "charlie"),
    };

    public static StateClass Instance { get; } = new();

    private StateClass()
    {
    }

    public int Count => Properties.Length;

    public KeyValuePair<string, object?> this[int index] => Properties[index];

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, object?>>)Properties).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Properties.GetEnumerator();
}