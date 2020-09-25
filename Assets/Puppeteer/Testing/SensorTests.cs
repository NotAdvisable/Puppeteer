using NUnit.Framework;
using Puppeteer.Core.Sensor;
using System.Linq;

namespace Puppeteer.Tests
{
	public class SensorTests
	{
		[SetUp]
		public void Setup()
		{
		}

		[TearDown]
		public void Teardown()
		{
		}

		[Test]
		public void SensorListSortByOrder()
		{
			SensorList<string, object> sensorList = new SensorList<string, object>
			{
				new ExecutableSensor<string, object>(5, 20)
			};
			Assert.AreEqual(20, sensorList[0].GetTickRate());
			
			sensorList.AddAndSort(new ExecutableSensor<string, object>(4, 30));
			Assert.AreEqual(30, sensorList[0].GetTickRate());
		}
	}
}

