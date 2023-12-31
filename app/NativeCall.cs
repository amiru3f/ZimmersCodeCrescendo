using System.Runtime.InteropServices;

namespace App;

public class NativeCall
{
    [DllImport("pbkdf2.so", EntryPoint = "fastpbkdf2_hmac_sha512")]
    public static extern void Pbkdf2(
        byte[] password, 
        IntPtr passwordSize,
        byte[] salt, 
        IntPtr saltSize, 
        uint iterations, 
        byte[] output, 
        IntPtr result);
}