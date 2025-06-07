using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace HellishBattle.SaveSystem
{
	[System.Serializable]
	public class TinySaveData
	{
		public string Key { get; set; }
		public string Value { get; set; }

		public TinySaveData() { }

		public TinySaveData(string key, string value)
		{
			this.Key = key;
			this.Value = value;
		}
	}

	[System.Serializable]
	public class TinyData
	{

		public List<TinySaveData> items = new List<TinySaveData>();

		public TinyData() { }

		public void AddItem(TinySaveData item)
		{
			items.Add(item);
		}
	}

	public class SerializatorBinary
	{

		public static void SaveBinary(TinyData state, string path)
		{
			BinaryFormatter _binary = new BinaryFormatter();
			FileStream _fileStream = new FileStream(path, FileMode.Create);
			_binary.Serialize(_fileStream, state);
			_fileStream.Close();
		}

		public static TinyData LoadBinary(string path)
		{
			BinaryFormatter _binary = new BinaryFormatter();
			FileStream _fileStream = new FileStream(path, FileMode.Open);
			TinyData _dataState = (TinyData)_binary.Deserialize(_fileStream);
			_fileStream.Close();
			return _dataState;
		}
	}
}