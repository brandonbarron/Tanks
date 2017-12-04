using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameCom
{
    //adapted from https://msdn.microsoft.com/en-us/library/system.security.cryptography.rsaoaepkeyexchangeformatter.aspx
    public class Encrypt
    {
        public Encrypt()
        {
            
        }

        public static byte[] EncryptBytes(RSACryptoServiceProvider key, byte[] toEncrypt, out byte[] iv, out byte[] encryptedSessionKey)
        {
            byte[] result = null;
            using (Aes aes = new AesCryptoServiceProvider())
            {
                iv = aes.IV;

                // Encrypt the session key
                RSAOAEPKeyExchangeFormatter keyFormatter = new RSAOAEPKeyExchangeFormatter(key);
                encryptedSessionKey = keyFormatter.CreateKeyExchange(aes.Key, typeof(Aes));

                // Encrypt the message
                using (System.IO.MemoryStream ciphertext = new System.IO.MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ciphertext, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(toEncrypt, 0, toEncrypt.Length);
                        cs.Close();
                    }
                    ciphertext.Close();
                    result = ciphertext.ToArray();
                }
            }
            return result;
        }

        public static byte[] DecryptBytes(RSACryptoServiceProvider key, byte[] iv, byte[] encryptedSessionKey, byte[] toDecrypt)
        {
            byte[] result = null;
            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.IV = iv;

                RSAOAEPKeyExchangeDeformatter keyDeformatter = new RSAOAEPKeyExchangeDeformatter(key);
                aes.Key = keyDeformatter.DecryptKeyExchange(encryptedSessionKey);

                // Decrypt the message
                using (System.IO.MemoryStream plaintext = new System.IO.MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(plaintext, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(toDecrypt, 0, toDecrypt.Length);
                        cs.Close();
                    }
                    plaintext.Close();
                    result = plaintext.ToArray();
                }
            }
            return result;
        }
        
    }
}
