using System.Security.Cryptography;
using System.Text;

namespace MediStock.API.Helpers
{
    public static class CryptoHelper
    {
        public class MediSecurity
        {
            public class Rijndael
            {
                private byte[] _key = {
            132, 42, 53, 124, 75, 56, 87, 38,
            9, 10, 161, 132, 183, 91, 105, 16,
            117, 218, 149, 230, 221, 212, 235, 64
        };

                private byte[] _iv = {
            83, 71, 26, 58, 54, 35, 22, 11,
            83, 71, 26, 58, 54, 35, 22, 11
        };

                public string Decrypt(string data)
                {
                    try
                    {
                        byte[] inBytes = Convert.FromBase64String(data);
                        MemoryStream mStream = new MemoryStream(inBytes, 0, inBytes.Length);

                        Aes aes = Aes.Create();
                        CryptoStream cs = new(mStream, aes.CreateDecryptor(_key, _iv), CryptoStreamMode.Read);

                        StreamReader sr = new(cs);
                        return sr.ReadToEnd();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                public string Encrypt(string data)
                {
                    try
                    {
                        UTF8Encoding utf8 = new();
                        byte[] inBytes = utf8.GetBytes(data);
                        MemoryStream ms = new MemoryStream();

                        Aes aes = Aes.Create();
                        CryptoStream cs = new(ms, aes.CreateEncryptor(_key, _iv), CryptoStreamMode.Write);

                        cs.Write(inBytes, 0, inBytes.Length);
                        cs.FlushFinalBlock();

                        return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }
    }
}