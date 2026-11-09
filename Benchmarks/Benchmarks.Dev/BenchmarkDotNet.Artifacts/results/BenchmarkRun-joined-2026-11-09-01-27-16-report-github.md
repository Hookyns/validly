```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9457/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 PRO 8840HS w/ Radeon 780M Graphics 3.30GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 11.0.100-rc.2.26504.105
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4

Job=.NET 10.0  Runtime=.NET 10.0  IterationCount=10  
WarmupCount=5  

```
| Type                           | Method                              | Items | TenPercentInvalid | Mean          | Error        | StdDev       | Median        | Gen0   | Gen1   | Allocated |
|------------------------------- |------------------------------------ |------ |------------------ |--------------:|-------------:|-------------:|--------------:|-------:|-------:|----------:|
| **IntegrationBenchmark**           | **SealedFlat_Valid**                    | **?**     | **?**                 |     **118.57 ns** |    **13.066 ns** |     **8.642 ns** |     **115.79 ns** |      **-** |      **-** |         **-** |
| IntegrationBenchmark           | SealedFlat_Invalid                  | ?     | ?                 |     108.56 ns |     2.290 ns |     1.197 ns |     108.43 ns |      - |      - |         - |
| IntegrationBenchmark           | Flat_Valid                          | ?     | ?                 |      83.62 ns |     9.207 ns |     6.090 ns |      81.95 ns |      - |      - |         - |
| IntegrationBenchmark           | Flat_Invalid                        | ?     | ?                 |      86.14 ns |     4.071 ns |     2.422 ns |      85.28 ns |      - |      - |         - |
| IntegrationBenchmark           | Nested_Valid                        | ?     | ?                 |     286.98 ns |    32.070 ns |    21.212 ns |     289.65 ns | 0.0172 |      - |     144 B |
| IntegrationBenchmark           | Nested_Invalid                      | ?     | ?                 |     272.28 ns |     6.900 ns |     4.106 ns |     271.81 ns | 0.0172 |      - |     144 B |
| IntegrationBenchmark           | Nested_Invalid_ProblemDetailsJson   | ?     | ?                 |   1,846.42 ns |   397.321 ns |   262.804 ns |   1,737.70 ns | 1.2646 | 0.0076 |   10584 B |
| **InheritanceCollectionBenchmark** | **Inheritance_SixPropertiesFlat_Valid** | **0**     | **False**             |     **138.24 ns** |    **15.666 ns** |    **10.362 ns** |     **139.51 ns** |      **-** |      **-** |         **-** |
| InheritanceCollectionBenchmark | Inheritance_ThreePlusThree_Valid    | 0     | False             |     307.57 ns |    65.739 ns |    39.120 ns |     306.88 ns |      - |      - |         - |
| InheritanceCollectionBenchmark | Inheritance_ThreePlusThree_Invalid  | 0     | False             |     320.45 ns |    23.789 ns |    14.157 ns |     316.98 ns |      - |      - |         - |
| InheritanceCollectionBenchmark | Collection                          | 0     | False             |      71.88 ns |    34.193 ns |    17.883 ns |      71.83 ns |      - |      - |         - |
| **InheritanceCollectionBenchmark** | **Inheritance_SixPropertiesFlat_Valid** | **0**     | **True**              |      **92.89 ns** |    **12.902 ns** |     **7.678 ns** |      **90.93 ns** |      **-** |      **-** |         **-** |
| InheritanceCollectionBenchmark | Inheritance_ThreePlusThree_Valid    | 0     | True              |     195.32 ns |     5.627 ns |     3.349 ns |     194.49 ns |      - |      - |         - |
| InheritanceCollectionBenchmark | Inheritance_ThreePlusThree_Invalid  | 0     | True              |     283.68 ns |   163.192 ns |    97.113 ns |     214.31 ns |      - |      - |         - |
| InheritanceCollectionBenchmark | Collection                          | 0     | True              |      82.20 ns |     7.893 ns |     5.221 ns |      83.01 ns |      - |      - |         - |
| **InheritanceCollectionBenchmark** | **Inheritance_SixPropertiesFlat_Valid** | **10**    | **False**             |     **113.05 ns** |    **14.373 ns** |     **9.507 ns** |     **110.71 ns** |      **-** |      **-** |         **-** |
| InheritanceCollectionBenchmark | Inheritance_ThreePlusThree_Valid    | 10    | False             |     287.51 ns |    32.372 ns |    21.412 ns |     282.19 ns |      - |      - |         - |
| InheritanceCollectionBenchmark | Inheritance_ThreePlusThree_Invalid  | 10    | False             |     301.36 ns |    23.947 ns |    15.840 ns |     298.04 ns |      - |      - |         - |
| InheritanceCollectionBenchmark | Collection                          | 10    | False             |   1,289.71 ns |    34.534 ns |    20.550 ns |   1,292.34 ns |      - |      - |         - |
| **InheritanceCollectionBenchmark** | **Inheritance_SixPropertiesFlat_Valid** | **10**    | **True**              |     **125.55 ns** |    **25.991 ns** |    **17.192 ns** |     **130.18 ns** |      **-** |      **-** |         **-** |
| InheritanceCollectionBenchmark | Inheritance_ThreePlusThree_Valid    | 10    | True              |     300.75 ns |    45.592 ns |    30.157 ns |     295.73 ns |      - |      - |         - |
| InheritanceCollectionBenchmark | Inheritance_ThreePlusThree_Invalid  | 10    | True              |     341.82 ns |    26.678 ns |    17.646 ns |     336.23 ns |      - |      - |         - |
| InheritanceCollectionBenchmark | Collection                          | 10    | True              |   1,116.73 ns |   146.323 ns |    96.784 ns |   1,069.13 ns | 0.0324 |      - |     272 B |
| **InheritanceCollectionBenchmark** | **Inheritance_SixPropertiesFlat_Valid** | **1000**  | **False**             |     **111.20 ns** |    **11.885 ns** |     **7.861 ns** |     **109.69 ns** |      **-** |      **-** |         **-** |
| InheritanceCollectionBenchmark | Inheritance_ThreePlusThree_Valid    | 1000  | False             |     282.57 ns |    32.100 ns |    21.232 ns |     287.05 ns |      - |      - |         - |
| InheritanceCollectionBenchmark | Inheritance_ThreePlusThree_Invalid  | 1000  | False             |     229.71 ns |    12.066 ns |     7.180 ns |     226.60 ns |      - |      - |         - |
| InheritanceCollectionBenchmark | Collection                          | 1000  | False             |  75,775.48 ns | 2,334.266 ns | 1,543.973 ns |  75,830.33 ns |      - |      - |         - |
| **InheritanceCollectionBenchmark** | **Inheritance_SixPropertiesFlat_Valid** | **1000**  | **True**              |      **87.80 ns** |     **1.163 ns** |     **0.769 ns** |      **87.97 ns** |      **-** |      **-** |         **-** |
| InheritanceCollectionBenchmark | Inheritance_ThreePlusThree_Valid    | 1000  | True              |     237.83 ns |    30.151 ns |    17.942 ns |     239.40 ns |      - |      - |         - |
| InheritanceCollectionBenchmark | Inheritance_ThreePlusThree_Invalid  | 1000  | True              |     261.07 ns |    41.255 ns |    27.287 ns |     256.87 ns |      - |      - |         - |
| InheritanceCollectionBenchmark | Collection                          | 1000  | True              | 120,162.11 ns | 6,871.930 ns | 3,594.152 ns | 121,631.01 ns | 3.6621 | 0.2441 |   31205 B |
