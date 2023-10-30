using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace App.Tests;

public class HashingFunctionTests
{
    private const int Iterations = 10000;
    private const int OutputLength = 64;
    private const int SaltSize = 16;
    private readonly HashAlgorithmName HashAlgorithmName = HashAlgorithmName.SHA512;

    [Theory]
    [InlineData("admin")]
    [InlineData("A1b@C#d$e^")]
    public void GivenAHashedPasswordGeneratedWithLegacyMethod_MustBeVerifiedWithBCLStaticMethod(string password)
    {
        byte[] saltBytes;
        byte[] bytesToVerify;

        using (var algorithm = new Rfc2898DeriveBytes(
             password,
             SaltSize,
             Iterations,
             HashAlgorithmName))
        {
            bytesToVerify = algorithm.GetBytes(OutputLength);
            saltBytes = algorithm.Salt;
        }

        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        var result = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, saltBytes, Iterations, HashAlgorithmName, OutputLength);

        Assert.True(result.SequenceEqual(bytesToVerify));
    }
}