using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class CryptoAES
{
    public const string DefaultPassword = "01234567890123456789012345678901"; // length : 32

    public enum KeySize 
    {
        Byte128 = 128
    }

    private static RijndaelManaged myRijndael = new RijndaelManaged();

    public static byte[] Encrypt(string plainData, string password = DefaultPassword, KeySize keySize = KeySize.Byte128) 
        => Encrypt(Encoding.UTF8.GetBytes(plainData), password, keySize);

    public static byte[] Encrypt(byte[] plainBytes, string password = DefaultPassword, KeySize keySize = KeySize.Byte128)
    {
        myRijndael.Clear();
        myRijndael.Mode = CipherMode.CBC;
        myRijndael.Padding = PaddingMode.PKCS7;
        myRijndael.KeySize = (int)keySize;

        var key = Encoding.UTF8.GetBytes(password);
        var iv = Encoding.UTF8.GetBytes(password.Substring(0, 16));

        var memoryStream = new MemoryStream();
        var cryptoStream = new CryptoStream(
            memoryStream,
            myRijndael.CreateEncryptor(key, iv),
            CryptoStreamMode.Write);

        cryptoStream.Write(plainBytes, 0, plainBytes.Length);
        cryptoStream.FlushFinalBlock();

        var encryptBytes = memoryStream.ToArray();

        cryptoStream.Close();
        memoryStream.Close();

        return encryptBytes;// Convert.ToBase64String(encryptBytes);
    }

    public static byte[] Decrypt(byte[] encrypt, string password = DefaultPassword, KeySize keySize = KeySize.Byte128)
    {
        var encryptBytes = encrypt;// Convert.FromBase64String(encrypt);

        myRijndael.Clear();
        myRijndael.Mode = CipherMode.CBC;
        myRijndael.Padding = PaddingMode.PKCS7;
        myRijndael.KeySize = (int)keySize;

        var key = Encoding.UTF8.GetBytes(password);
        var iv = Encoding.UTF8.GetBytes(password.Substring(0, 16));

        var memoryStream = new MemoryStream(encryptBytes);
        var cryptoStream = new CryptoStream(
            memoryStream,
            myRijndael.CreateDecryptor(key, iv), 
            CryptoStreamMode.Read);

        var plainBytes = new byte[encryptBytes.Length];
        var plainCount = cryptoStream.Read(plainBytes, 0, plainBytes.Length);

        cryptoStream.Close();
        memoryStream.Close();

        return plainBytes;// Encoding.UTF8.GetString(plainBytes, 0, plainCount);
    }
}
