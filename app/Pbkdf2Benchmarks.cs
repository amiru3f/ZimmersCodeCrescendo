using System.Security.Cryptography;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace App;

[MemoryDiagnoser]
public class Pbkdf2Benchmarks
{

    [Params(1000)]
    public int Count { get; set; }

    byte[] passwordBytes = Encoding.UTF8.GetBytes("admin");
    byte[] saltBytes = Encoding.UTF8.GetBytes("your_salt");

    [GlobalSetup]
    public void Setup() { }


    [Benchmark]
    public void NewDotnetHash()
    {
        for (int i = 0; i < Count; i++)
            Rfc2898DeriveBytes.Pbkdf2(passwordBytes, saltBytes, Constants.Iterations, HashAlgorithmName.SHA512, Constants.OutputLength);
    }

    [Benchmark]
    public void LegacyDotNetHash()
    {
        for (int i = 0; i < Count; i++)
            using (var algorithm = new Rfc2898DeriveBytes(
              passwordBytes,
              saltBytes,
              Constants.Iterations,
              HashAlgorithmName.SHA512))
            {
                algorithm.GetBytes(Constants.OutputLength);
            }
    }


    [Benchmark]
    public void NativeHash()
    {
        for (int i = 0; i < Count; i++)
        {
            byte[] outputArray = new byte[Constants.OutputLength];

            NativeCall.Pbkdf2(passwordBytes,
                    (IntPtr)passwordBytes.Length,
                    saltBytes,
                    (IntPtr)saltBytes.Length,
                    (uint)Constants.Iterations,
                    outputArray,
                    (IntPtr)Constants.OutputLength);
        }
    }

}