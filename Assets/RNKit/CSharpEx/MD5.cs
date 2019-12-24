using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class MD5
{
    public static string computeHash(string s)
    {
        byte[] result = Encoding.Default.GetBytes(s);
        var md5 = new MD5CryptoServiceProvider();
        byte[] output = md5.ComputeHash(result);
        return System.BitConverter.ToString(output).Replace("-", "");
    }
}
