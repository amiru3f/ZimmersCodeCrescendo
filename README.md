# ZimmersCodeCrescendo
Zimmer's Code Crescendo

### Brief

This project aims to enhance the performance of the Password-Based Key Derivation Function (Pbkdf2) in the .NET runtime environment. We achieve this by employing a combination of OpenSSL(1) hash functions and a refined algorithm that minimizes memory and CPU usage. In this README, I provide a comprehensive guide on how to optimize the performance of Pbkdf2 and present the benchmark results to showcase the improvements achieved.

### Introduction

In our IdentityServer setup, we facilitate client authorization using the `client credentials`, allowing clients to request authorization for various OAuth grant types. One essential aspect of this authorization process is the verification of the client's secret. However, we encountered a challenge in this regard - the client secret verification is a highly CPU-intensive task.

Under a recent performance test, we observed a significant impact on our endpoints' response times due to the resource-intensive nature of client secret verification. The test was conducted in an environment consisting of two AWS Fargate instances, each with limited computational resources—0.5 CPU and 2GB of memory.

The test simulated a load of 70 transactions per second (approximately 8000 per minute). In this high-throughput scenario, the CPU-intensive nature of client secret verification became evident, resulting in performance bottlenecks and increased response times.

Notably, the secret hashing algorithm used for verification is Pbkdf2, which, while secure, adds to the computational workload and contributes to the observed performance challenges.

As a result, we embarked on a journey to address these performance issues and optimize the client credential verification process in our IdentityServer setup. This repository aims to document the steps we took, the optimizations implemented, and the results achieved in our pursuit of a high-performing solution for client credential verification. I intend to share my findings and improvements with the broader developer community to help others facing similar challenges.

### Detection
The setup with 2 fargate tasks with 0.5 CPU and 2Gb ram went well with 65 requests, resulted 300 ms response time.
Increasing the load by 5 resulted in a terrible response time near 4.5 seconds. 
by digging via the profiler I found 100% cpu usage and a kind of thread contention.

The image bellow demonstrates the most expensive path which led us doughting the the starvation but actually it was not the case.
The IO thread pool were well configured and no sync over async detected in the app, however the cpu were stuck on heavy hasing function having no room to take care of the continuation tasks.
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
