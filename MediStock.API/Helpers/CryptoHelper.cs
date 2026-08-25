using System.Security.Cryptography;
using System.Text;

namespace MediStock.API.Helpers
{
    public static class CryptoHelper
    {
        public class MediSecurity
        {
            private const string hashProvider = "hashprovider";
            private const string symmProvider = "symprovider";
            private const string symmKeyFileName = "SymmetricKeyFile.txt";

            public interface ICrypto
            {
                int BlockSize();
                int KeySize();
                string Encrypt(string data);
                string Decrypt(string data);
                string Base64Encode(string data);
                string Base64Decode(string data);
            }

            public class CryptoFactory
            {
                public ICrypto MakeCryptographer(string type)
                {
                    switch (type.ToLower())
                    {
                        case "des":
                            return new Rijndael();
                        case "tripledes":
                            return new Rijndael();
                        case "rijndael":
                            return new Rijndael();
                        default:
                            return new Rijndael();
                    }
                }
            }

            public class Rijndael : ICrypto
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

                public int BlockSize()
                {
                    Aes aes = Aes.Create();
                    return aes.BlockSize;
                }

                public int KeySize()
                {
                    Aes aes = Aes.Create();
                    return aes.KeySize;
                }

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

                public string Base64Encode(string plainText)
                {
                    var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                    return Convert.ToBase64String(plainTextBytes);
                }

                public string Base64Decode(string base64EncodedData)
                {
                    var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
                    return Encoding.UTF8.GetString(base64EncodedBytes);
                }
            }
        }
    }
}
