using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon.Encryption
{
    public class EncryptioinKeys
    {
        System.Security.Cryptography.RSACryptoServiceProvider _rsaKey;
        public bool Active { get; private set; }
        byte[] _iv;
        byte[] _encryptedSessionKey;

        public EncryptioinKeys()
        {
            _rsaKey = new System.Security.Cryptography.RSACryptoServiceProvider();
        }

        public System.Security.Cryptography.RSACryptoServiceProvider RsaKey { get => _rsaKey; }
        public byte[] Iv { get => _iv; }
        public byte[] SessionKey { get => _encryptedSessionKey; }

        public void SetIvAndSessionKey(byte[] iv, byte[] encryptedSessionKey)
        {
            _iv = (byte[])iv.Clone();
            _encryptedSessionKey = (byte[])encryptedSessionKey.Clone();
            Active = true;
        }

        public void ImportPublicKey(byte[] publickey)
        {
            _rsaKey.ImportCspBlob(publickey);
        }

        public byte[] ExportPublicKey()
        {
            return _rsaKey.ExportCspBlob(false);
        }
        
    }
}
