# Pattern-Matching Benchmark Results

BenchmarkDotNet 0.15.8 `ShortRun` results on Windows 11 with an Intel Core i7-8750H. Each run used one launch, three warmup iterations, and three measured iterations. Allocations are bytes per operation. The benchmark was run on Windows only; results for other operating systems are not available.

The `net462` benchmark executable ran on the installed .NET Framework 4.8 runtime. .NET Framework is an in-place updated runtime, so this does not measure the 4.6.2 CLR specifically.

| Target runtime | Path | State | Old mean (ns) | Pattern mean (ns) | Old alloc (B/op) | Pattern alloc (B/op) |
|---|---|---:|---:|---:|---:|---:|
| .NET Framework 4.8 (`net462` target) | Parser | Class | 54.12 | 53.54 | 0 | 0 |
| .NET Framework 4.8 (`net462` target) | Parser | Struct | 110.81 | 57.74 | 48 | 24 |
| .NET Framework 4.8 (`net462` target) | Logger | Class | 91.41 | 87.90 | 0 | 0 |
| .NET Framework 4.8 (`net462` target) | Logger | Struct | 329.03 | 97.47 | 120 | 24 |
| .NET 8.0 | Parser | Class | 4.05 | 3.94 | 0 | 0 |
| .NET 8.0 | Parser | Struct | 0.74 | 3.76 | 0 | 24 |
| .NET 8.0 | Logger | Class | 28.08 | 32.16 | 0 | 0 |
| .NET 8.0 | Logger | Struct | 19.04 | 22.97 | 0 | 24 |
| .NET 9.0 | Parser | Class | 1.06 | 0.89 | 0 | 0 |
| .NET 9.0 | Parser | Struct | 0.54 | 0.53 | 0 | 0 |
| .NET 9.0 | Logger | Class | 27.06 | 25.57 | 0 | 0 |
| .NET 9.0 | Logger | Struct | 18.14 | 18.52 | 0 | 0 |
| .NET 10.0 | Parser | Class | 1.28 | 0.87 | 0 | 0 |
| .NET 10.0 | Parser | Struct | 0.53 | 0.54 | 0 | 0 |
| .NET 10.0 | Logger | Class | 28.10 | 25.42 | 0 | 0 |
| .NET 10.0 | Logger | Struct | 17.43 | 17.75 | 0 | 0 |

## Takeaways

- On .NET 8.0, pattern matching allocates 24 B/op for struct state in both paths; the old form allocates 0 B/op.
- On .NET Framework 4.8, pattern matching reduces struct-state allocations from 48 to 24 B/op for the parser and from 120 to 24 B/op for the logger.
- On .NET 9.0 and 10.0, neither form shows measurable allocation for the struct state.
- The `net11.0` target is included and builds, but BenchmarkDotNet 0.15.8 cannot run it: despite the .NET 11 SDK/runtime being installed, the target maps to an unrecognized runtime moniker. No .NET 11 timing is reported.
- Class-state cases show no measurable allocation on any runtime. The nanosecond timings, especially sub-nanosecond results, should be treated as indicative because this was a ShortRun.