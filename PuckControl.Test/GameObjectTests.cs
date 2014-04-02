using Microsoft.VisualStudio.TestTools.UnitTesting;
using PuckControl.Domain.Entities;
using System;
using System.Windows.Media.Media3D;

[assembly: CLSCompliant(true)]
namespace PuckControl.Test
{
    [TestClass]
    public class GameObjectTests
    {

        [TestMethod]
        public void TestAveragePositionZero()
        {
            GameObject obj = new GameObject();
            obj.MotionSmoothingSteps = 1;
            obj.Position = new Vector3D(0, 0, 0);
            Assert.AreEqual(new Vector3D(0,0,0), obj.Position);
        }

        [TestMethod]
        public void TestAveragePositionTwoStep()
        {
            GameObject obj = new GameObject();
            obj.MotionSmoothingSteps = 2;
            obj.Position = new Vector3D(0, 0, 0);
            obj.Position = new Vector3D(3, 3, 3);

            Assert.AreEqual(new Vector3D(2, 2, 2), obj.Position);
        }
    }
}
