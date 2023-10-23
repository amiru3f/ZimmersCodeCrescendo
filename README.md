# ZimmersCodeCrescendo
Zimmer's Code Crescendo

### Introduction

This project aims to enhance the performance of the Password-Based Key Derivation Function (Pbkdf2) in the .NET runtime environment. We achieve this by employing a combination of OpenSSL(1) hash functions and a refined algorithm that minimizes memory and CPU usage. In this README, I provide a comprehensive guide on how to optimize the performance of Pbkdf2 and present the benchmark results to showcase the improvements achieved.

### Problem
Consider having an IdentityServer which supports `client credentials` to verify the clients. Either in `authorization-code` or the other OAuth flows (Which in our case, it's not that important)

Verifing the client `password` is a `cpu intensive` task. So if in case of the high throughput there must be a way to have a high perfomant endpoint.

Results till now:

```

BenchmarkDotNet v0.13.9+228a464e8be6c580ad9408e98f18813f6407fb5a, macOS Ventura 13.6 (22G120) [Darwin 22.6.0]
Apple M2 Pro, 1 CPU, 10 logical and 10 physical cores
.NET SDK 8.0.100-preview.5.23303.2
  [Host]     : .NET 6.0.16 (6.0.1623.17311), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 6.0.16 (6.0.1623.17311), Arm64 RyuJIT AdvSIMD


```
| Method                  | Mean     | Error     | StdDev    |
|------------------------ |---------:|----------:|----------:|
| CppNativeImplementation | 1.778 ms | 0.0028 ms | 0.0025 ms |
| DotnetOptimized         | 3.602 ms | 0.0141 ms | 0.0118 ms |
| DotnetLegacy            | 8.097 ms | 0.0294 ms | 0.0261 ms |


To be continued 🔜 
