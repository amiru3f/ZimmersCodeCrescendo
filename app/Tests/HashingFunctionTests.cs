using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace App.Tests;

public class HashingFunctionTests
{
    private const int OutputLength = 64;
    private const int SaltSize = 16;

    private readonly HashAlgorithmName HashAlgorithmName = HashAlgorithmName.SHA512;

    [Theory]
    [InlineData("admin", 100)]
    [InlineData("admin", 10000)]
    [InlineData("A1b@C#d$e^", 10000)]
    [InlineData("A1b@C#d$e^", 100)]
    [InlineData("A1b@C#d$e^", 1)]
    public void GivenAHashedPasswordGeneratedWithLegacyMethod_MustBeVerifiedWithBCLStaticMethod(string password, int iterations)
    {
        byte[] saltBytes;
        byte[] bytesToVerify;

        using (var algorithm = new Rfc2898DeriveBytes(
             password,
             SaltSize,
             iterations,
             HashAlgorithmName))
        {
            bytesToVerify = algorithm.GetBytes(OutputLength);
            saltBytes = algorithm.Salt;
        }

        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        var result = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, saltBytes, iterations, HashAlgorithmName, OutputLength);

        Assert.True(result.SequenceEqual(bytesToVerify));
    }


    [Theory]
    [InlineData("admin", 100)]
    [InlineData("admin", 10000)]
    [InlineData("A1b@C#d$e^", 10000)]
    [InlineData("A1b@C#d$e^", 100)]
    [InlineData("A1b@C#d$e^", 1)]
    public void GivenAHashedPasswordGeneratedWithLegacyMethod_MustBeVerifiedWithNativelyImplemented_CPPHashFunction(string password, int iterations)
    {
        byte[] saltBytes;
        byte[] bytesToVerify;

        using (var algorithm = new Rfc2898DeriveBytes(
             password,
             SaltSize,
             iterations,
             HashAlgorithmName))
        {
            bytesToVerify = algorithm.GetBytes(OutputLength);
            saltBytes = algorithm.Salt;
        }

        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

        // Allocate memory for output
        byte[] outputArray = new byte[OutputLength];

        NativeCall.Pbkdf2(passwordBytes,
                (IntPtr)passwordBytes.Length,
                saltBytes,
                (IntPtr)saltBytes.Length,
                (uint)iterations,
                outputArray,
                (IntPtr)OutputLength);


        Assert.True(bytesToVerify.SequenceEqual(outputArray));
    }
}