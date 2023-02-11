using System.Text;
using System.Security.Cryptography;
using System;

public class EncryptUtil
{
    public static string ToBase64UTF8(string text)
    {
        var bytesToEncode = Encoding.UTF8.GetBytes(text);
        var encodedText = Convert.ToBase64String(bytesToEncode);

        return encodedText;
    }

    public static string FromBase64UTF8(string text)
    {
        var decodedBytes = Convert.FromBase64String(text);
        var decodedText = Encoding.UTF8.GetString(decodedBytes);

        return decodedText;
    }

    public static string GetSha256Hash(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            var stringBuilder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
                stringBuilder.Append(bytes[i].ToString("x2"));

            return stringBuilder.ToString();
        }
    }

    public static string GetMD5Hash(string rawData)
    {
        using (MD5 md5Hash = MD5.Create())
        {
            var bytes = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            var stringBuilder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
                stringBuilder.Append(bytes[i].ToString("x2"));

            return stringBuilder.ToString();
        }
    }
}
