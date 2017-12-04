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

            RSACryptoServiceProvider rsaKey1 = new RSACryptoServiceProvider();
            RSACryptoServiceProvider rsaKey2 = new RSACryptoServiceProvider();

            rsaKey1.ImportCspBlob(rsaKey2.ExportCspBlob(false));

            var e1 = GameCom.Encrypt.EncryptBytes(rsaKey1, messageBytes, out byte[] iv, out byte[] encryptedSessionKey);

            var d1 = GameCom.Encrypt.DecryptBytes(rsaKey2, iv, encryptedSessionKey, e1);

            Assert.IsTrue(messageBytes.SequenceEqual(d1));
        }

    }
}

