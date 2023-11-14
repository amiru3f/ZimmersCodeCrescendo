# ZimmersCodeCrescendo

## Brief

This project aims to enhance the performance of the Password-Based Key Derivation Function (Pbkdf2) in the .NET runtime environment. We achieve this by employing a combination of OpenSSL(1) hash functions and a refined algorithm that minimizes memory and CPU usage. In this README, I provide a comprehensive guide on problem detection steps as well as how to optimize the performance of Pbkdf2 and present the benchmark results to showcase the improvements achieved.

## Introduction

In our `Authentication` setup, we facilitate client authentication using the `client credentials`, allowing clients to request auth for various OAuth grant types. One essential aspect of this process is the verification of the client's secret. However, we encountered a challenge in this regard - the client secret verification is a highly CPU-intensive task.

Under a recent performance test, we observed a significant impact on our endpoints' response times due to the resource-intensive nature of client secret verification. The test was conducted in an environment consisting of 4 AWS `Fargate` instances, each with limited computational resources 0.5 CPU, and 2GB of memory.

The test simulated a load of 140 transactions per second (approximately 8000 per minute). In this high-throughput scenario, the CPU-intensive nature of client secret verification became evident, resulting in performance bottlenecks and increased response times.

Notably, the secret hashing algorithm used for verification is `Pbkdf2`, which, while secure, adds to the computational workload and contributes to the observed performance challenges.

As a result, we embarked on a journey to address these performance issues and optimize the client credential verification process in our authentication solution setup. This repository aims to document the steps we took, the optimizations implemented, and the results achieved in our pursuit of a high-performing solution for client credential verification. I intend to share my findings and improvements with the broader developer community to help others facing similar challenges.

## Detection

The setup with 4 Fargate tasks with 0.5 CPU and 2Gb RAM went acceptable with 65 TPS, resulting in a 300 ms response time.
Increasing the load by 5 resulted in a terrible response time near 4.5 seconds.
By digging via the profiler I found 100% CPU usage and a kind of thread contention.

The image below demonstrates the most expensive path which led us to doubt starvation but actually, it was not the case.

<img width="1343" alt="Profiling Result" src="https://github.com/amiru3f/ZimmersCodeCrescendo/assets/17201404/a23458fc-2f8c-41bf-a128-d0ea249c8661">

The IO thread pool was well configured and no sync over async was detected in the app however, the CPU was stuck on a heavy hashing function having no room to take care of the continuation tasks.

Additionally, By checking the span and traces we could find out the cost of each step down to the response:

<img width="1202" alt="Before Optimization" src="https://github.com/amiru3f/ZimmersCodeCrescendo/assets/17201404/62966ace-8364-4e51-9ba4-06d2997acb92">


## Verify
Since I was in doubt of high CPU usage and lack of CPU for continuations, I needed a way to verify my understanding.
With kind of an easy verification step, I could make sure that the problem was definitely the CPU. I just removed `.Hash()` with `await Task.Delay(COST_OF_HASH_FUNCTION_IN_MILLISECONDS)` and reran the test

The results showed that the Api was resulting wonderfully super fast with 2X capacity with no thread starvation issue. (Toleration of 120 TPS - 7200 TPM)

So I decided to micro-optimize the hash function to lower the CPU usage just as an interesting duty.

# How I started the optimization

## Steps

### Bunch of tests

In any case, that's mandatory to start from a test that ensures that refactoring the logic for any reason, could be performance, clean-up, or re-design, nothing would break.
The test should be like:
Having `verifyPasswordV1`, If changing the code, for each specific test case (input password) the `verifyPasswordV*` function result must be identical. For example: the result of verifyPasswordV1 must be exactly like verifyPasswordV2 and verifyPasswordV3

For implementation details, visit: [Tests](app.tests/HashingFunctionTests.cs)

### `AspnetCore` best practices

At first insight, it was clear to me that checking .Net runtime repo previous issues would help. So a bunch of searching led me to a closed issue '<https://github.com/dotnet/runtime/issues/24897>' which had a good effect on the hashing benchmark. Actually, it improves the legacy .Net implementation of `Rfc2898DeriveBytes` performance nearly 2/2.5 times.

* How does it work? Just by replacing `new Rfc2898DeriveBytes()` with a statically called method `Rfc2898DeriveBytes.Pbkdf2DeriveBytes()`

| Method           | Count | Mean     | Error    | StdDev   | Allocated |
|----------------- |------ |---------:|---------:|---------:|----------:|
| NewDotnetHash    | 1000  |  5.006 s | 0.0170 s | 0.0142 s |  91.89 KB |
| LegacyDotNetHash | 1000  | 11.595 s | 0.0660 s | 0.0617 s | 540.25 KB |

NOTE: you can run Benchmarks on your machine by navigating to ./app and running:
``` sudo dotnet run --configuration Release ```

Definitely, you can see the benchmark was much better, but nothing changed in the result, the same CPU usage with even more response time. So I tried another way to reduce the algorithm time and memory complexity.

## Leveraging OpenSSL to improve the hashing performance in the native playground

After some searching on Github, I could find some native implementations which have focused on `Aggressive inlining`, `Zero allocation`, `Minimal copies`, and `parallelism`.

So started to port the best one in case of benchmarks into C# leveraging ReadonlySpan and safe Pointers. The benchmark shows super fast results:

| Method              | Count | Mean     | Error    | StdDev   | Allocated |
|-----------------    |------ |---------:|---------:|---------:|----------:|
| NewDotnetHash       | 1000  |  4.996 s | 0.0053 s | 0.0041 s |  86.89 KB |
| LegacyDotNetHash    | 1000  | 11.580 s | 0.0471 s | 0.0441 s | 541.99 KB |
| NativeCpp           | 1000  |  2.513 s | 0.0080 s | 0.0075 s |  86.89 KB |
| NativeCppMulThread  | 1000  |  1.941 s | 0.0075 s | 0.0068 s |  86.89 KB |

## Running load tests against new implementation

After deep benchmarking the native implementation and for sure the green tests, I decided to trigger the pipelines to deploy the changes to ECS. 

![After Optimization Traces](https://github.com/amiru3f/ZimmersCodeCrescendo/assets/17201404/fbfcecd0-c6a6-4458-b658-e72aa85e01b7)

To be continued 🔜
