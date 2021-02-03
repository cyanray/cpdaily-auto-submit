using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace cpdaily_auto_submit
{
    internal static class CpdailyCrypto
    {
        public const string PrivateKey = "<RSAKeyValue><D>LhZnOjEdaiElzDA5ix4BN294lMZQAUh8ewSjykrSrGsOQ2fYUj1xqlssIcf0+q8QADrgqrbeUiItMVZXRMuxeZz2K4gpsPIdPEaU/nu5ICrxqnig9oeWap4F4drFX9G0bAH1nlmwX230gBqcbMo4M+YW1sQfuErRhFtPo5odQAE=</D><DP>aUTb2SSssbedsuSefwxiFQeKHFLzsYYktmmX2/xQ/GvE19LNOL92K1sItjq9/ZgDtbIpz8nrbCH0n2SCd3/xAQ==</DP><DQ>gOZAMhNrONQdGVzat8acGjpxXROa1qu2qcE2sdGKsNXswpIASU6maSxia2scPNx1smKS0FWlBf61Bst4CEWbyQ==</DQ><Exponent>AQAB</Exponent><InverseQ>ZT69Xm1D4ee8RXxhS3MjSqZ0L3+yg0J6m9C9dfCt6h6mmoL4u01hk1LPby0Nkfw+Ab6TY5x/QbHI5l5ymh+btw==</InverseQ><Modulus>zfnymlUMEUr/QVPB6KspS5Gq4WUPE4+sQ52oRhYDenGXgZK1vIEdmtRmLeFkdoD0ZU4WKgezEFlfK8acDpOXofVvQObQno6UixD9TNK6455W0MV6meyGb3NkpY2yGZaFtUEsscZgED2WfPCVZlGbMzxDpxlH1Xy9sLSIjhyeMYk=</Modulus><P>5fWdxH624ja52ql+yiwo3khetzawPuMeHizrmAjqswNDCmpzPlb6BnpzJ97Kj3UpTldBN7of6f8aGNdA5NfBAQ==</P><Q>5U0Tl0PrYLOZAGVQU0iNJkG/G+RmG3clZs5sb5lbLXTMmnLpNcwI+gI8E+l0rUB3u7ywROEmYCfEnL14nz/oiQ==</Q></RSAKeyValue>";
        public const string PublicKey = "<RSAKeyValue><Exponent>AQAB</Exponent><Modulus>uS9BA5F73PQ6SHw68oVt13htp3UyJiUp1zG5eSNBYPYC9fyZHEC2kp0QgLQrZYHTursebWFcHz80WFUU0k8DfQ7HwiV6HMxMdKpBHPdj/yZAajSX+5xzTZ34zRcgKiwKdfqALYz0/Cr/JC6tkFo7z9dQAeDeGg3wP2YhBSgJEck=</Modulus></RSAKeyValue>";
        public static readonly byte[] IV = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        private const string Chars = "ABCDEFGHJKMNPQRSTWXYZabcdefhijkmnprstwxyz2345678";

        public static byte[] RSADecrpty(byte[] data, string xmlKey)
        {
            RSACryptoServiceProvider rsaPrivate = new RSACryptoServiceProvider();
            rsaPrivate.FromXmlString(xmlKey);
            return rsaPrivate.Decrypt(data, false);
        }

        public static byte[] RSAEncrpty(byte[] data, string xmlKey)
        {
            RSACryptoServiceProvider rsaPrivate = new RSACryptoServiceProvider();
            rsaPrivate.FromXmlString(xmlKey);
            return rsaPrivate.Encrypt(data, false);
        }

        public static byte[] DESDecrypt(byte[] encryptedValue, byte[] key, byte[] iv)
        {
            using DESCryptoServiceProvider sa =
                new DESCryptoServiceProvider
                {
                    Key = key,
                    IV = iv,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7
                };
            using ICryptoTransform ct = sa.CreateDecryptor();
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, ct, CryptoStreamMode.Write))
            {
                cs.Write(encryptedValue, 0, encryptedValue.Length);
                cs.FlushFinalBlock();
            }
            return ms.ToArray();
        }

        public static string DESDecrypt(string encryptedValue, string key, byte[] iv)
        {
            return Encoding.UTF8.GetString(
                DESDecrypt(Convert.FromBase64String(encryptedValue), Encoding.ASCII.GetBytes(key), iv));
        }

        public static byte[] DESEncrypt(byte[] originalValue, byte[] key, byte[] iv)
        {
            using DESCryptoServiceProvider sa
                = new DESCryptoServiceProvider
                {
                    Key = key,
                    IV = iv,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7
                };
            using ICryptoTransform ct = sa.CreateEncryptor();
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, ct, CryptoStreamMode.Write))
            {
                cs.Write(originalValue, 0, originalValue.Length);
                cs.FlushFinalBlock();
            }
            return ms.ToArray();
        }

        public static string DESEncrypt(string originalValue, string key, byte[] iv)
        {
            return Convert.ToBase64String(
                DESEncrypt(Encoding.UTF8.GetBytes(originalValue), Encoding.ASCII.GetBytes(key), iv));
        }

        public static byte[] AESEncrypt(byte[] originalValue, byte[] key, byte[] iv)
        {
            using AesCryptoServiceProvider sa
                = new AesCryptoServiceProvider
                {
                    Key = key,
                    IV = iv,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7
                };
            using ICryptoTransform ct = sa.CreateEncryptor();
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, ct, CryptoStreamMode.Write))
            {
                cs.Write(originalValue, 0, originalValue.Length);
                cs.FlushFinalBlock();
            }
            return ms.ToArray();
        }

        public static string AESEncrypt(string originalValue, string key, byte[] iv)
        {
            return Convert.ToBase64String(
                AESEncrypt(Encoding.UTF8.GetBytes(originalValue), Encoding.ASCII.GetBytes(key), iv));
        }

        public static byte[] DecodeBase64(string str)
        {
            MemoryStream result = new MemoryStream();
            int length = str.Length;
            int i = 0;
            while (true)
            {
                if (i < length && str[i] <= ' ')
                {
                    i++;
                }
                else if (i != length)
                {
                    int i2 = i + 2;
                    int i3 = i + 3;
                    int a = (m53989a(str[i]) << 18) + (m53989a(str[i + 1]) << 12) + (m53989a(str[i2]) << 6) + m53989a(str[i3]);
                    result.WriteByte((byte)((a >> 16) & 255));
                    if (str[i2] != '=')
                    {
                        result.WriteByte((byte)((a >> 8) & 255));
                        if (str[i3] != '=')
                        {
                            result.WriteByte((byte)(a & 255));
                            i += 4;
                        }
                        else
                        {
                            return result.ToArray();
                        }
                    }
                    else
                    {
                        return result.ToArray();
                    }
                }
                else
                {
                    return result.ToArray();
                }
            }
        }

        private static int m53989a(char c)
        {
            int i;
            if (c >= 'A' && c <= 'Z')
            {
                return c - 'A';
            }
            if (c >= 'a' && c <= 'z')
            {
                i = c - 'a';
            }
            else if (c >= '0' && c <= '9')
            {
                i = (c - '0') + 26;
            }
            else if (c == '+')
            {
                return 62;
            }
            else
            {
                if (c == '/')
                {
                    return 63;
                }
                if (c == '=')
                {
                    return 0;
                }
                throw new Exception("unexpected code: " + c);
            }
            return i + 26;
        }

        public static string MD5(string input)
        {
            using MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static string GetOick(string user_id = null)
        {
            if (string.IsNullOrEmpty(user_id)) return "d41d8cd9";
            return MD5(user_id).Substring(0, 8).ToLower();
        }

        /// <summary>
        /// a function from libcipher.so (uint8_t* getckey(void);)
        /// </summary>
        /// <returns></returns>
        public static string GetCKey()
        {
            return "OKXv";
        }

        /// <summary>
        /// a function from libcipher.so (uint8_t* getckey(void);)
        /// </summary>
        /// <returns></returns>
        public static string GetFKey()
        {
            return "b63X";
        }

        /// <summary>
        /// a function from libcipher.so 
        /// (uint8_t* getak(JNIEnv*, uint8_t*, const uint8_t*);)
        /// </summary>
        /// <returns></returns>
        public static string GetAK(string l1, string o2)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Concat(l1.Where((c, i) => i % 2 == 0)));
            sb.Append(string.Concat(o2.Where((c, i) => i % 2 == 0)));
            sb.Append(string.Concat(l1.Where((c, i) => i % 2 == 1)));
            sb.Append(string.Concat(o2.Where((c, i) => i % 2 == 1)));
            return sb.ToString();
        }

        public static string GetDESKey(string chkOrfhk, bool isChk = true)
        {
            string key = isChk ? GetCKey() : GetFKey();
            return !string.IsNullOrEmpty(chkOrfhk) ? GetAK(key, chkOrfhk) : key;
        }

        public static string RandomString(int length)
        {
            StringBuilder sb = new StringBuilder();
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                sb.Append(Chars[random.Next(0, Chars.Length)]);
            }
            return sb.ToString();
        }
    }
}
