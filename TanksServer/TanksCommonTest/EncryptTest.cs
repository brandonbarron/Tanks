using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;

namespace TanksCommonTest
{
    [TestClass]
    public class EncryptTest
    {
        [TestMethod]
        public void EncryptDecryptTest()
        {
            byte[] messageBytes = null;
            using (var stream = new System.IO.MemoryStream())
            {
                var theObject = new TanksCommon.SharedObjects.GameMove() { MessageId = 8 };
                var messageStream = TanksCommon.MessageEncoder.EncodeMessage(stream, theObject);
                messageStream.Seek(0, System.IO.SeekOrigin.Begin);
                messageBytes = messageStream.ToArray();
            }

            //can only encrypt messages, only has public key
            TanksCommon.Encryption.EncryptioinKeys clientKey = new TanksCommon.Encryption.EncryptioinKeys();
            //can decrypt messages, has public and private key
            TanksCommon.Encryption.EncryptioinKeys serverKey = new TanksCommon.Encryption.EncryptioinKeys();

            //server provides public RSA keys and client imports them
            clientKey.ImportPublicKey(serverKey.ExportPublicKey());

            //client can encrypt AES keys with the provided RSA keys
            var clientEncryptor = new GameCom.Encrypt(clientKey);

            //server sets public AES keys
            serverKey.SetIvAndSessionKey(clientKey.Iv, clientKey.SessionKey);

            var e1 = clientEncryptor.EncryptBytes(messageBytes);

            var d1 = GameCom.Encrypt.DecryptBytes(serverKey, e1);

            Assert.IsFalse(messageBytes.SequenceEqual(e1));

            Assert.IsTrue(messageBytes.SequenceEqual(d1));

            //serverKey.SetIvAndSessionKey(clientKey.Iv, clientKey.SessionKey);
            /*var serverEncrypter = new GameCom.Encrypt(serverKey);

            var e2 = serverEncrypter.EncryptBytes(messageBytes);

            var d2 = GameCom.Encrypt.DecryptBytes(clientKey, e2);

            Assert.IsTrue(d2.SequenceEqual(messageBytes));*/
        }

    }
}

