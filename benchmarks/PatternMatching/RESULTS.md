# Pattern-Matching Benchmark Results

BenchmarkDotNet 0.15.3 `ShortRun` results on Windows 11 with an Intel Core i7-8750H. Each run used one launch, three warmup iterations, and three measured iterations. Allocations are bytes per operation.

The `net462` benchmark executable ran on the installed .NET Framework 4.8 runtime. .NET Framework is an in-place updated runtime, so this does not measure the 4.6.2 CLR specifically.

| Target runtime | Path | State | Old mean (ns) | Pattern mean (ns) | Old alloc (B/op) | Pattern alloc (B/op) |
|---|---|---:|---:|---:|---:|---:|
| .NET Framework 4.8 (`net462` target) | Parser | Class | 53.37 | 55.43 | 0 | 0 |
| .NET Framework 4.8 (`net462` target) | Parser | Struct | 112.15 | 57.16 | 48 | 24 |
| .NET Framework 4.8 (`net462` target) | Logger | Class | 92.32 | 92.30 | 0 | 0 |
| .NET Framework 4.8 (`net462` target) | Logger | Struct | 328.86 | 101.09 | 120 | 24 |
| .NET 8.0 | Parser | Class | 3.66 | 3.05 | 0 | 0 |
| .NET 8.0 | Parser | Struct | 0.57 | 3.32 | 0 | 24 |
| .NET 8.0 | Logger | Class | 28.36 | 29.05 | 0 | 0 |
| .NET 8.0 | Logger | Struct | 19.12 | 22.85 | 0 | 24 |
| .NET 10.0 | Parser | Class | 1.06 | 0.83 | 0 | 0 |
| .NET 10.0 | Parser | Struct | 0.53 | 0.53 | 0 | 0 |
| .NET 10.0 | Logger | Class | 27.19 | 26.39 | 0 | 0 |
| .NET 10.0 | Logger | Struct | 17.88 | 17.48 | 0 | 0 |

## Takeaways

- On .NET 8.0, pattern matching allocates 24 B/op for struct state in both paths; the old form allocates 0 B/op. This confirms the allocation concern for that runtime and state shape.
- On .NET Framework 4.8, pattern matching reduces struct-state allocations, but still allocates 24 B/op.
- On .NET 10.0, neither form shows measurable allocation for the struct state.
- Class-state cases show no measurable allocation on any runtime. The nanosecond timings, especially sub-nanosecond results, should be treated as indicative because this was a ShortRun.