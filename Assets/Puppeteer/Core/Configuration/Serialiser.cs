using Puppeteer.Core.Debug;
using Puppeteer.Core.Helper;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace Puppeteer.Core.Configuration
{
	using TActionDescList = List<ActionDescription>;

	using TArchetypeDescList = List<ArchetypeDescription>;

	using TGoalDescList = List<GoalDescription>;

	using TSensorDescList = List<SensorDescription>;

	internal class Serialiser
	{
		public static TActionDescList LoadActions()
		{
			return Deserialise<TActionDescList>(Path.Combine(FOLDER, ACTION_FILE));
		}

		public static TArchetypeDescList LoadArchetypes()
		{
			return Deserialise<TArchetypeDescList>(Path.Combine(FOLDER, ARCHETYPE_FILE));
		}

		public static TGoalDescList LoadGoals()
		{
			return Deserialise<TGoalDescList>(Path.Combine(FOLDER, GOAL_FILE));
		}

		public static TSensorDescList LoadSensors()
		{
			return Deserialise<TSensorDescList>(Path.Combine(FOLDER, SENSOR_FILE));
		}

		public static bool SaveActions(TActionDescList _content)
		{
			return Save(_content, ACTION_FILE);
		}

		public static bool SaveArchetypes(TArchetypeDescList _content)
		{
			return Save(_content, ARCHETYPE_FILE);
		}

		public static bool SaveGoals(TGoalDescList _content)
		{
			return Save(_content, GOAL_FILE);
		}

		public static bool SaveSensors(TSensorDescList _content)
		{
			return Save(_content, SENSOR_FILE);
		}

		private static void CreatePathIfNeeded(string _path)
		{
			if (!Directory.Exists(_path))
			{
				Directory.CreateDirectory(_path);
			}
		}

		private static T Deserialise<T>(string _path) where T : new()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(T));
			T deserialized;
			try
			{
				StreamReader reader = new StreamReader(_path);
				deserialized = (T)serializer.Deserialize(reader.BaseStream);
				reader.Close();
			}
			catch (System.Exception e)
			{
				PuppeteerLogger.Log(string.Format("Deserialise: {0}", e.Message));

				deserialized = new T();
			}

			return deserialized;
		}

		private static bool Save<T>(T _content, string _file)
		{
			CreatePathIfNeeded(FOLDER);

			string filePath = Path.Combine(FOLDER, _file);

			if (VersionControl.IsUsable(VersionControl.TryCheckOutOrAdd(filePath)))
			{
				bool successfullySaved = Serialise(_content, filePath);
				VersionControl.TryCheckOutOrAdd(filePath);

				return successfullySaved;
			}
			else
			{
				return false;
			}
		}

		private static bool Serialise(object _item, string _path)
		{
			try
			{
				XmlSerializer serializer = new XmlSerializer(_item.GetType());
				StreamWriter writer = new StreamWriter(_path);
				serializer.Serialize(writer.BaseStream, _item);
				writer.Close();
				return true;
			}
			catch (System.Exception e)
			{
				PuppeteerLogger.Log(string.Format("Serialise {0}", e.Message));
				return false;
			}
		}

		private static readonly string ACTION_FILE = "Actions.pxac";
		private static readonly string ARCHETYPE_FILE = "Archetypes.pxar";
		private static readonly string FOLDER = Path.Combine(Application.streamingAssetsPath, "PuppeteerAI");
		private static readonly string GOAL_FILE = "Goals.pxgo";
		private static readonly string SENSOR_FILE = "Sensors.pxse";
	}
}