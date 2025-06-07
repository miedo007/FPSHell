using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using HellishBattle.SaveSystem;

namespace HellishBattle.Audio
{
    public class AudioVolumeControl : MonoBehaviour
    {
        // Public Value
        public string KeyName;

        // Private Value
        float baseVolume;
        AudioSource source;

        public void Start()
        {
            // Get AudioSource
            source = transform.GetComponent<AudioSource>();

            // Save Base Volume;
            baseVolume = source.volume;
        }

        public void Update()
        {
            // Get Saved Volume Settings
            int MasterVolume = TinySaveSystem.GetInt("Settings_Master_Volume");
            int Volume = TinySaveSystem.GetInt(KeyName);

            // Change Volume
            source.volume = baseVolume * ((float)MasterVolume / 100) * ((float)Volume / 100);
        }

    }

#if UNITY_EDITOR

    [CustomEditor(typeof(AudioVolumeControl))]
    [CanEditMultipleObjects]
    public class AudioVolumeControlEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((AudioVolumeControl)target), typeof(AudioVolumeControl), false);
            GUI.enabled = true;

            EditorUtility.SetDirty(target);
            AudioVolumeControl Control = (AudioVolumeControl)target;

            int ID = 0;
            TinySaveSystem.Initialize("HellishBattle.tss");
            string[] tmp = new string[TinySaveSystem.data.items.Count];

            for (int i = 0; i < TinySaveSystem.data.items.Count; i++)
            {
                tmp[i] = TinySaveSystem.data.items[i].Key;
                if (TinySaveSystem.data.items[i].Key == Control.KeyName)
                {
                    ID = i;
                }
            }

            if (tmp.Length > 0)
            {
                ID = EditorGUILayout.Popup("KeyName", ID, tmp);
                Control.KeyName = tmp[ID];

                TinyGUI.InfoBox($"Base Volume: {Control.GetComponent<AudioSource>().volume * 100}%\nMaster Volume: {TinySaveSystem.GetInt("Settings_Master_Volume")}%, {Control.KeyName} Volume: {TinySaveSystem.GetInt(Control.KeyName)}%\nIn Game Volume: {Control.GetComponent<AudioSource>().volume * ((float)TinySaveSystem.GetInt("Settings_Master_Volume") / 100) * ((float)TinySaveSystem.GetInt(Control.KeyName) / 100) * 100}%");
            }
            else
            {
                TinyGUI.InfoBox("To change these settings, you must fire up the game for the first time, this will save the basic information in TinySaveSystem");
            }
        }
    }

#endif

}
