using System;
using System.Security.Cryptography;
using System.Text;
// 해쉬값 구하는 함수

public class HashData
{
    public static string SHA256Hash(string data)
    {
        SHA256 sha256 = new SHA256Managed();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        StringBuilder sb = new StringBuilder();
        foreach (byte b in hash)
        {
            sb.AppendFormat("{0:x2}", b);
        }

        return sb.ToString();
    }
    
    public static string SHA256Base64Hash(string data)
    {
        var sha256 = new SHA256Managed();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        var sb = Convert.ToBase64String(hash);

        return sb.ToString();
    }
    
    public static string MD5Hash(string data)
    {
        MD5 md5 = new MD5CryptoServiceProvider();
        md5.ComputeHash(Encoding.UTF8.GetBytes(data));
        byte[] result = md5.Hash;
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < result.Length; i++)
        {
            sb.AppendFormat("{0:x2}", result[i]);
        }

        return sb.ToString();
    }
}
