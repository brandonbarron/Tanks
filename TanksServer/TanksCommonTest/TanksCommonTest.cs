using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using TanksCommon;

namespace TanksTest
{
    [TestClass]
    public class TanksCommonTest
    {

        [TestMethod]
        public void TestMessages()
        {

            using (var stream = new System.IO.MemoryStream())
            {
                var messageStream = new System.IO.MemoryStream();
                TanksCommon.SharedObjects.GameMove theObject = new TanksCommon.SharedObjects.GameMove();
                theObject.GameId = 2;
                theObject.GunType = 5;
                theObject.LocationY = 3;
                theObject.LocationX = 4;
                theObject.MoveId = 7;
                theObject.PlayerId = 15;
                theObject.ShotAngle = 95;
                theObject.ShotPower = 86;

                messageStream = TanksCommon.MessageEncoder.EncodeMessage(stream, theObject);
                messageStream.Seek(0, System.IO.SeekOrigin.Begin);

                var decodedObject = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.GameMove>(messageStream);

                Assert.AreEqual(theObject.GameId, decodedObject.GameId);
                Assert.AreEqual(theObject.GunType, theObject.GunType);
                Assert.AreEqual(theObject.LocationY, theObject.LocationY);
                Assert.AreEqual(theObject.LocationX, theObject.LocationX);
                Assert.AreEqual(theObject.MoveId, theObject.MoveId);
                Assert.AreEqual(theObject.PlayerId, theObject.PlayerId);
                Assert.AreEqual(theObject.ShotAngle, theObject.ShotAngle);
                Assert.AreEqual(theObject.ShotPower, theObject.ShotPower);
                Assert.AreEqual(theObject, theObject);
                Assert.AreEqual(theObject, theObject);
            }

            
        }
    }
}
