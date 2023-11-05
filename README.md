# ZimmersCodeCrescendo

Zimmer's Code Crescendo

## Brief

This project aims to enhance the performance of the Password-Based Key Derivation Function (Pbkdf2) in the .NET runtime environment. We achieve this by employing a combination of OpenSSL(1) hash functions and a refined algorithm that minimizes memory and CPU usage. In this README, I provide a comprehensive guide on how to optimize the performance of Pbkdf2 and present the benchmark results to showcase the improvements achieved.

## Introduction

In our IdentityServer setup, we facilitate client authorization using the `client credentials`, allowing clients to request authorization for various OAuth grant types. One essential aspect of this authorization process is the verification of the client's secret. However, we encountered a challenge in this regard - the client secret verification is a highly CPU-intensive task.

Under a recent performance test, we observed a significant impact on our endpoints' response times due to the resource-intensive nature of client secret verification. The test was conducted in an environment consisting of two AWS `Fargate` instances, each with limited computational resources 0.5 CPU, and 2GB of memory.

The test simulated a load of 140 transactions per second (approximately 8000 per minute). In this high-throughput scenario, the CPU-intensive nature of client secret verification became evident, resulting in performance bottlenecks and increased response times.

Notably, the secret hashing algorithm used for verification is `Pbkdf2`, which, while secure, adds to the computational workload and contributes to the observed performance challenges.

As a result, we embarked on a journey to address these performance issues and optimize the client credential verification process in our Auth solution setup. This repository aims to document the steps we took, the optimizations implemented, and the results achieved in our pursuit of a high-performing solution for client credential verification. I intend to share my findings and improvements with the broader developer community to help others facing similar challenges.

## Detection

The setup with 2 Fargate tasks with 0.5 CPU and 2Gb RAM went acceptable with 65 TPS, resulting in a 300 ms response time.
Increasing the load by 5 resulted in a terrible response time near 4.5 seconds.
By digging via the profiler I found 100% CPU usage and a kind of thread contention.

The image below demonstrates the most expensive path which led us to doubt starvation but actually, it was not the case.

<img width="1343" alt="Profiling Result" src="https://github.com/amiru3f/ZimmersCodeCrescendo/assets/17201404/a23458fc-2f8c-41bf-a128-d0ea249c8661">

The IO thread pool was well configured and no sync over async was detected in the app however, the CPU was stuck on a heavy hashing function having no room to take care of the continuation tasks.

Additionally, By checking the span and traces we could find out the cost of each step down to the response:

<img width="1343" alt="Profiling Result" src="https://github.com/amiru3f/ZimmersCodeCrescendo/assets/17201404/a23458fc-2f8c-41bf-a128-d0ea249c8661">

## Verify

With kind of an easy verification step, we could make sure that the problem was related to CPU usage. We just removed `.Hash()` with `await Task.Delay(COST_OF_HASH_FUNCTION_IN_MILLISECONDS)` and reran the test

The result shows that the Api were resulting wonderfully super fast with 2X capacity. (Toleration of 120 TPS)

## Steps

### Bunch of tests

In any case, that's mandatory to start from a test which ensures that refactoring the logic for any reason, could be performance, clean-up, re-design, nothing would break.
The test should be like:
Having `verifyPasswordV1`, If changing the code, for each specific test case (input password) the `verifyPasswordV*` function result must be identical. For example: the result of verifyPasswordV1 must be exactly like verifyPasswordV2 and verifyPasswordV3

For implementation details, visit: [Tests](app.tests/HashingFunctionTests.cs)

### `AspnetCore` best practices

At first insight it was clear to me that checking .Net runtime related issues would help. So a bunch of searching led me to a closed issue '<https://github.com/dotnet/runtime/issues/24897>' which had a good effect on hashing benchmark. Actually it improves the legacy .Net implementation of `Rfc2898DeriveBytes` performance near 2/2.5 times.

* How it works? Just by replacing `new Rfc2898DeriveBytes()` with a statically called method `Rfc2898DeriveBytes.Pbkdf2DeriveBytes()`

| Method           | Count | Mean     | Error    | StdDev   | Allocated |
|----------------- |------ |---------:|---------:|---------:|----------:|
| NewDotnetHash    | 1000  |  5.006 s | 0.0170 s | 0.0142 s |  91.89 KB |
| LegacyDotNetHash | 1000  | 11.595 s | 0.0660 s | 0.0617 s | 540.25 KB |

NOTE: you can run Benchmarks on your machine by navigating to ./app and running:
``` sudo dotnet run --configuration Release ```

## Results after optimization with new hashing alg

![After Optimization Traces](https://github.com/amiru3f/ZimmersCodeCrescendo/assets/17201404/fbfcecd0-c6a6-4458-b658-e72aa85e01b7)

Results till now:

``` Benchmark

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
