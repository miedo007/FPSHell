using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.IO;
using System;
using HellishBattle.Weapon;
using HellishBattle.Player;
using HellishBattle.Enemies;
using HellishBattle.SaveSystem;





#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace HellishBattle.Console
{
    public class ConsoleController : MonoBehaviour
    {
        public TMP_InputField commandText;
        public TMP_Text commandInfo;
        public GameObject CommandButtonPrefab;
        public Transform CommandContainer;

        public string currentString = "";
        public float ClearDataAfter;


        bool Timer = false;
        private TinyInput tinyInput;
        string previousText;

        // Private
        public List<CheatCommandBase> FlyCommandList = new List<CheatCommandBase> {
        new CheatCommand("FULLCLIP", "Gives the player the maximum amount of Ammo", "", () =>
        {
            // Add Ammo
            foreach (AmmoType ammoType in Enum.GetValues(typeof(AmmoType)))
            {
                GameManager.Instance.AddAmmo(ammoType, 9999);
            }

            // Update UI
            WeaponSwitch container = Camera.main.transform.parent.GetComponentInChildren(typeof(WeaponSwitch)) as WeaponSwitch;
            if (container != null) { container.actualWeapon.UpdateLeftAmmo(); }
        }),
        new CheatCommand("IWASGOD", "Max Health and Armor", "", () =>
        {
            GameManager.Instance.transform.parent.GetComponent<PlayerHealth>().AddHealth(9999, true);
            GameManager.Instance.transform.parent.GetComponent<PlayerHealth>().AddArmor(9999, true);
        }),
        new CheatCommand("POKLLKOP", "2K Points", "", () =>
        {
            TinySaveSystem.SetInt("unlockable_point", 2000);
        })
    };

        public List<CheatCommandBase> commandList = new List<CheatCommandBase> {
        new CheatCommand("KillEnemy", "Kills all enemies on the map", "KillEnemy", () =>
        {
            Enemy[] buttonObjs = FindObjectsOfType<Enemy>();

            for(int i=0; i< buttonObjs.Length; i++)
            {
                buttonObjs[i].GetDamage(new DamageClass(99999, DamageType.Critical));
            }
        }),
        new CheatCommand("MaxHealth", "Restores the player's maximum amount of life", "MaxHealth", () =>
        {
            GameManager.Instance.transform.parent.GetComponent<PlayerHealth>().AddHealth(9999, true);
        }),
        new CheatCommand("MaxArmor", "Restores the player's maximum amount of armor", "MaxArmor", () =>
        {
            GameManager.Instance.transform.parent.GetComponent<PlayerHealth>().AddArmor(9999, true);
        }),
        new CheatCommand("MaxAmmo", "Gives the player the maximum amount of Ammo", "MaxAmmo", () =>
        {
             foreach (AmmoType ammoType in Enum.GetValues(typeof(AmmoType)))
            {
                GameManager.Instance.AddAmmo(ammoType, 9999);
            }

            // Update UI
            WeaponSwitch container = Camera.main.transform.parent.GetComponentInChildren(typeof(WeaponSwitch)) as WeaponSwitch;
            if (container != null) { container.actualWeapon.UpdateLeftAmmo(); }
        }),
        new CheatCommand("SpawnAllWeapon", "Spawn All Avaiable Weapon", "SpawnAllWeapon", () =>
        {
            GameObject[] prefabs = Resources.LoadAll<GameObject>("Collectable/Weapon");

            foreach (GameObject prefab in prefabs)
            {
                Instantiate(prefab,Camera.main.transform.position,new Quaternion());
            }
        }),

        // Variable

        new CheatCommand<int>("SetHealth", "Allows you to set any number of lives from 0 to the maximum level", "SetHealth <size=12><color=green><Quantity></color></size>", (x) =>
        {
            float currentHealth = GameManager.Instance.transform.parent.GetComponent<PlayerHealth>().health;
            GameManager.Instance.transform.parent.GetComponent<PlayerHealth>().AddHealth(x-currentHealth, true);
        }),
        new CheatCommand<int>("SetArmor", "Allows you to set any number of armor from 0 to the maximum level", "SetArmor <size=12><color=green><Quantity></color></size>", (x) =>
        {
            float currentArmor = GameManager.Instance.transform.parent.GetComponent<PlayerHealth>().armor;
            GameManager.Instance.transform.parent.GetComponent<PlayerHealth>().AddArmor(x-currentArmor, true);
        }),
        new CheatCommand<int>("SetPoints", "Allows you to set any number of armor from 0 to the maximum level", "SetPoints <size=12><color=green><Quantity></color></size>", (x) =>
        {
            TinySaveSystem.SetInt("unlockable_point", x);
        }),
    };

        bool showConsole;

        private void Awake()
        {
            tinyInput = InputManager.Instance.input;
            var rebinds = PlayerPrefs.GetString("rebinds");
            tinyInput.asset.LoadBindingOverridesFromJson(rebinds);

            tinyInput.Menu.Console.performed += ctx =>
            {
                if (GameManager.Instance.UIState == PlayerUIState.None || GameManager.Instance.UIState == PlayerUIState.Console)
                {
                    showConsole = !showConsole;
                    if (showConsole) { GameManager.Instance.UIState = PlayerUIState.Console; }
                    else
                    {
                        GameManager.Instance.UIState = PlayerUIState.None;
                    }
                    GameManager.Instance.paused = showConsole;
                    GameManager.Instance.CheckPauseGame();
                    transform.Find("UI").gameObject.SetActive(showConsole);
                }
            };

            // Reset Command List
            foreach (Transform child in CommandContainer)
            {
                Destroy(child.gameObject);
            }
        }

        public void OnEnable() { tinyInput.Enable(); }
        public void OnDisable() { tinyInput.Disable(); }

        public void Update()
        {

            if (commandText.text != previousText)
            {
                commandInfo.text = "";
                // Reset Command List
                foreach (Transform child in CommandContainer)
                {
                    Destroy(child.gameObject);
                }

                string[] words = commandText.text.Split(' ');

                string tmp = "";

                for (int i = 0; i < commandList.Count; i++)
                {
                    CheatCommandBase commandBase = commandList[i];
                    if (commandList[i].commandID.IndexOf(words[0], System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        GameObject tezt = Instantiate(CommandButtonPrefab, CommandContainer);
                        tezt.GetComponent<TMP_Text>().text = commandList[i].commandFormat + "<br><size=8><color=#969696>" + commandList[i].commandDescription + "</color></size>";
                        int index = i;
                        tezt.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => { commandText.text = commandList[index].commandID; });

                        tmp = "Some Command Found";
                    }
                }
                if (tmp == "") { commandInfo.text += $"<color=#a52a2aff>Cheat <color=red>{words[0]}</color> not exist </color>"; }

                previousText = commandText.text;
            }

            // Fly Cheat
            foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(vKey))
                {
                    currentString += vKey;
                    Reset_Data();
                }
            }


            CheckCheat(currentString);
            if (currentString == "")
            {
                Timer = false;
            }
            else
            {
                Timer = true;
            }
            if (Timer)
            {
                if (ClearDataAfter > 0)
                {
                    ClearDataAfter -= Time.deltaTime;
                }
                else
                {
                    Reset_Data();
                    Timer = false;
                    currentString = "";
                }
            }
        }

        private void Reset_Data()
        {
            ClearDataAfter = 2;
        }
        private bool CheckCheat(string _input)
        {
            foreach (CheatCommand code in FlyCommandList)
            {
                if (_input == code.commandID)
                {
                    if (code.commandID != "")
                    {
                        code.Invoke();
                        Reset_Data();
                        currentString = "";
                        return true;
                    }
                }
            }

            return false;
        }

        public void HandleInput()
        {
            string[] properties = commandText.text.Split(' ');

            for (int i = 0; i < commandList.Count; i++)
            {
                CheatCommandBase commandBase = commandList[i] as CheatCommandBase;
                if (commandText.text.Contains(commandBase.commandID))
                {
                    if (commandList[i] as CheatCommand != null)
                    {
                        (commandList[i] as CheatCommand).Invoke();
                        Debug.Log("Invoke");
                    }
                    else if (commandList[i] as CheatCommand<int> != null && properties.Length > 1)
                    {
                        (commandList[i] as CheatCommand<int>).Invoke(int.Parse(properties[1]));
                        Debug.Log("Invoke Int");
                    }
                }
            }
            commandText.text = "";
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(ConsoleController))]
    public class ConsoleControllerEditor : Editor
    {
        void OnEnable()
        {
            /*
        public TMP_InputField commandText;
        public TMP_Text commandInfo;
        public GameObject CommandButtonPrefab;
        public Transform CommandContainer;
             */
        }

        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            ConsoleController console = (ConsoleController)target;

            EditorGUILayout.BeginHorizontal("helpbox", GUILayout.Height(EditorGUIUtility.singleLineHeight + 6));
            EditorGUILayout.LabelField("Cheat Console Manager Script", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.ObjectField(MonoScript.FromMonoBehaviour((ConsoleController)target), typeof(ConsoleController), false);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Console UI", "Przypisz wymagane elementy konsoli");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("CommandContainer"), new GUIContent("Tiles per Second"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("commandText"), new GUIContent("Tiles per Second"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("commandInfo"), new GUIContent("Tiles per Second"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CommandButtonPrefab"), new GUIContent("Tiles per Second"));

            EditorGUILayout.EndVertical();

            /*
            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Console Cheats List");
            for (int j = 0; j < console.commandList.Count; j++)
            {
                EditorGUILayout.LabelField(new GUIContent(console.commandList[j].commandID), EditorStyles.boldLabel);
                EditorGUILayout.Space(-8); EditorGUILayout.LabelField(new GUIContent(console.commandList[j].commandDescription), EditorStyles.wordWrappedMiniLabel); 
                EditorGUILayout.Space(2); 

                //TinyGUI.Title(console.commandList[j].commandID, console.commandList[j].commandDescription);
                //EditorGUILayout.LabelField(console.commandList[j].commandID + " - " + console.commandList[j].commandDescription);
            }
            EditorGUILayout.EndVertical();*/

            //EditorGUILayout.BeginVertical("box");
            //TinyGUI.EditorTitle("Console Cheats List");

            List<string> CommandList = new List<string>();
            for (int j = 0; j < console.commandList.Count; j++)
            {
                CommandList.Add($"{console.commandList[j].commandID}\n{console.commandList[j].commandDescription}");
            }
            TinyGUI.Guideline("Console Command List", CommandList.ToArray());

            List<string> FlyCommandList = new List<string>();
            for (int j = 0; j < console.FlyCommandList.Count; j++)
            {
                FlyCommandList.Add($"{console.FlyCommandList[j].commandID}\n{console.FlyCommandList[j].commandDescription}");
            }
            TinyGUI.Guideline("Fly Command List", FlyCommandList.ToArray());


            //EditorGUILayout.EndVertical();


            /*ConsoleController loot = (ConsoleController)target;

            serializedObject.Update();
            base.OnInspectorGUI();

            GUILayout.BeginVertical("box");

            GUILayout.BeginVertical("Toolbar");
            GUILayout.Label("Console Cheats");
            GUILayout.EndVertical();
            for (int j = 0; j < loot.commandList.Count; j++)
            {
                GUILayout.Label(loot.commandList[j].commandID + " - " + loot.commandList[j].commandDescription);
            }

            GUILayout.Space(5);

            GUILayout.BeginVertical("Toolbar");
            GUILayout.Label("Fly Cheats");
            GUILayout.EndVertical();
            for (int j = 0; j < loot.FlyCommandList.Count; j++)
            {
                GUILayout.Label(loot.FlyCommandList[j].commandID + " - " + loot.FlyCommandList[j].commandDescription);
            }
            GUILayout.EndVertical();*/

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}