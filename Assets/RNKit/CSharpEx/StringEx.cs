using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringEx
{
    public static bool isNullOrEmpty(this string s)
    {
        return string.IsNullOrEmpty(s);
    }
    public static bool notEmpty(this string s)
    {
        return !string.IsNullOrEmpty(s);
    }


    /*public static bool isNullOrWhitespace(this string s)
    {
        return string.IsNullOrEmpty(s) || s.Trim().Length == 0;
    }*/



    public static int getStableHashCode(this string text)
    {
        unchecked
        {
            int hash = 23;
            foreach (char c in text)
                hash = hash * 31 + c;
            return hash;
        }
    }

    /*
    // Copy of Mono string.GetHashCode(), so that we generate same hashes regardless of runtime (mono/MS .NET)
    public static int getHashCode(string s)
    {
        unsafe
        {
            int length = s.Length;
            fixed (char* c = s)
            {
                char* cc = c;
                char* end = cc + length - 1;
                int h = 0;
                for (; cc < end; cc += 2)
                {
                    h = (h << 5) - h + *cc;
                    h = (h << 5) - h + cc[1];
                }
                ++end;
                if (cc < end)
                    h = (h << 5) - h + *cc;
                return h;
            }
        }
    }
    */

}