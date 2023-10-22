using System.Security.Cryptography;
using System.Text;
using app;


byte[] passwordBytes = Encoding.UTF8.GetBytes("admin");
byte[] saltBytes = Encoding.UTF8.GetBytes("your_salt");
uint iterations = 10000;
int outputLength = 64; // Length of the output in bytes

// Allocate memory for output
byte[] outputArray = new byte[outputLength];


Native.Pbkdf2(passwordBytes,
        (IntPtr)passwordBytes.Length,
        saltBytes,
        (IntPtr)saltBytes.Length,
        iterations,
        outputArray,
        (IntPtr)outputLength);


Rfc2898DeriveBytes.Pbkdf2(passwordBytes, saltBytes, 10000, HashAlgorithmName.SHA512, 64);
