```

BenchmarkDotNet v0.15.4, Windows 11 (10.0.26100.6899/24H2/2024Update/HudsonValley)
AMD Ryzen 7 PRO 8840HS w/ Radeon 780M Graphics 3.30GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.100-rc.1.25451.107
  [Host]   : .NET 9.0.6 (9.0.6, 9.0.625.26613), X64 RyuJIT x86-64-v4
  .NET 9.0 : .NET 9.0.6 (9.0.6, 9.0.625.26613), X64 RyuJIT x86-64-v4

Job=.NET 9.0  Runtime=.NET 9.0  MaxIterationCount=20

```
| Method                       | NumberOfInvalidValues | Mean        | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------|---------------------- |------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **DataAnnotation**           | **all**                   | **1,142.29 ns** |  **19.590 ns** |  **18.325 ns** |  **1.00** |    **0.02** | **0.3281** |      **-** |    **2752 B** |        **1.00** |
| Validly (exit early)         | all                   |    60.46 ns |   1.090 ns |   0.966 ns |  0.04 |    0.00 |      - |      - |         - |        0.00 |
| Validly                      | all                   |   116.97 ns |   0.804 ns |   0.752 ns |  0.10 |    0.00 |      - |      - |         - |        0.00 |
| &#39;Validot (IsValid)&#39;  | all                   |    49.11 ns |   2.598 ns |   2.887 ns |  0.04 |    0.00 | 0.0057 |      - |      48 B |        0.02 |
| &#39;Validot (Validate)&#39; | all                   |   851.07 ns |  79.488 ns |  91.538 ns |  0.75 |    0.08 | 0.3948 | 0.0038 |    3304 B |        1.20 |
| FluentValidation             | all                   | 4,987.67 ns | 197.647 ns | 219.684 ns |  4.37 |    0.20 | 1.6632 | 0.0229 |   13928 B |        5.06 |
|                              |                       |             |            |            |       |         |        |        |           |             |
| **DataAnnotation**           | **none**                  | **1,119.20 ns** |  **19.592 ns** |  **17.368 ns** |  **1.00** |    **0.02** | **0.3281** |      **-** |    **2752 B** |        **1.00** |
| Validly (exit early)         | none                  |    95.54 ns |   3.234 ns |   3.177 ns |  0.06 |    0.00 |      - |      - |         - |        0.00 |
| Validly                      | none                  |   102.73 ns |   2.069 ns |   1.935 ns |  0.09 |    0.00 |      - |      - |         - |        0.00 |
| &#39;Validot (IsValid)&#39;  | none                  |   220.75 ns |   4.151 ns |   4.077 ns |  0.20 |    0.00 | 0.0057 |      - |      48 B |        0.02 |
| &#39;Validot (Validate)&#39; | none                  |   585.71 ns |  16.996 ns |  18.185 ns |  0.52 |    0.02 | 0.2842 | 0.0019 |    2384 B |        0.87 |
| FluentValidation             | none                  |   497.71 ns |  23.335 ns |  26.873 ns |  0.44 |    0.02 | 0.0906 |      - |     760 B |        0.28 |
|                              |                       |             |            |            |       |         |        |        |           |             |
| **DataAnnotation**           | **one**                   | **1,106.04 ns** |  **54.234 ns** |  **62.456 ns** |  **1.00** |    **0.08** | **0.2861** |      **-** |    **2408 B** |        **1.00** |
| Validly (exit early)         | one                   |    58.34 ns |   0.954 ns |   0.892 ns |  0.05 |    0.00 |      - |      - |         - |        0.00 |
| Validly                      | one                   |   106.51 ns |   0.563 ns |   0.499 ns |  0.10 |    0.01 |      - |      - |         - |        0.00 |
| &#39;Validot (IsValid)&#39;  | one                   |    45.05 ns |   0.700 ns |   0.585 ns |  0.04 |    0.00 | 0.0057 |      - |      48 B |        0.02 |
| &#39;Validot (Validate)&#39; | one                   |   588.65 ns |   7.792 ns |   6.907 ns |  0.53 |    0.03 | 0.3271 | 0.0029 |    2736 B |        1.14 |
| FluentValidation             | one                   | 1,912.76 ns |  10.243 ns |   9.581 ns |  1.73 |    0.09 | 0.6523 | 0.0057 |    5456 B |        2.27 |
