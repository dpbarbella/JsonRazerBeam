``` ini

BenchmarkDotNet=v0.13.1, OS=macOS Monterey 12.0.1 (21A559) [Darwin 21.1.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.1 (6.0.121.56705), Arm64 RyuJIT
  DefaultJob : .NET 6.0.1 (6.0.121.56705), Arm64 RyuJIT


```
|    Method |         FileLocation |          epochStart | numOrders |     Mean |    Error |   StdDev |   Median |
|---------- |--------------------- |-------------------- |---------- |---------:|---------:|---------:|---------:|
| **GetOrders** | **/User(...).json [57]** | **1609750824955201013** |         **1** | **28.90 ms** | **0.577 ms** | **1.425 ms** | **28.35 ms** |
| **GetOrders** | **/User(...).json [57]** | **1609750824955201013** |        **10** | **28.54 ms** | **0.529 ms** | **0.495 ms** | **28.45 ms** |
| **GetOrders** | **/User(...).json [57]** | **1609750824955201013** |       **100** | **32.20 ms** | **0.578 ms** | **1.438 ms** | **31.71 ms** |
| **GetOrders** | **/User(...).json [57]** | **1609750824955201013** |      **1000** | **58.37 ms** | **0.361 ms** | **0.337 ms** | **58.37 ms** |
