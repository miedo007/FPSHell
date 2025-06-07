using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HellishBattle.SaveSystem
{
	public class TinySaveManager : MonoBehaviour
	{
		[Line("Save File Path:","C:\\Users\\NAME\\AppData\\LocalLow\\Tiny Slime Studio\\Hellish Battle")]
		[SerializeField] private string fileName = "HellishBattle.tss"; // file to save with the specified resolution
		[SerializeField] private bool dontDestroyOnLoad; // the object will move from one scene to another (you only need to add it once)

		void Awake()
		{
			TinySaveSystem.Initialize(fileName);
			if (dontDestroyOnLoad) DontDestroyOnLoad(transform.gameObject);
		}

		void OnApplicationQuit()
		{
			TinySaveSystem.SaveToDisk();
		}
	}
}