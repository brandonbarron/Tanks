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
        ICryptoTransform _cryptoTransform;
        public Encrypt(TanksCommon.Encryption.EncryptioinKeys key)
        {
            SetKeys(key);
        }

        public void SetKeys(TanksCommon.Encryption.EncryptioinKeys key)
        {
            var aes = new AesCryptoServiceProvider();
            var iv = aes.IV;

            // Encrypt the session key
            RSAOAEPKeyExchangeFormatter keyFormatter = new RSAOAEPKeyExchangeFormatter(key.RsaKey);
            var encryptedSessionKey = keyFormatter.CreateKeyExchange(aes.Key, typeof(Aes));

            key.SetIvAndSessionKey(iv, encryptedSessionKey);

            this._cryptoTransform = aes.CreateEncryptor();
        }

        public byte[] EncryptBytes(byte[] toEncrypt)
        {
            byte[] result = null;

            // Encrypt the message
            using (System.IO.MemoryStream ciphertext = new System.IO.MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ciphertext, _cryptoTransform, CryptoStreamMode.Write))
                {
                    cs.Write(toEncrypt, 0, toEncrypt.Length);
                    cs.Close();
                }
                ciphertext.Close();
                result = ciphertext.ToArray();
            }
            return result;
        }

        public static byte[] DecryptBytes(TanksCommon.Encryption.EncryptioinKeys key, byte[] toDecrypt)
        {
            byte[] result = null;
            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.IV = key.Iv;

                RSAOAEPKeyExchangeDeformatter keyDeformatter = new RSAOAEPKeyExchangeDeformatter(key.RsaKey);
                aes.Key = keyDeformatter.DecryptKeyExchange(key.SessionKey);

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
