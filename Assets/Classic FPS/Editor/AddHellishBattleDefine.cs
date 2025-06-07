using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class AddHellishBattleDefine : Editor
{
    public static readonly string[] Symbols = new string[] {
         "HELLISH_BATTLE"
     };

    static AddHellishBattleDefine()
    {
        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

        List<string> allDefines = definesString.Split(';').ToList();
        allDefines.AddRange(Symbols.Except(allDefines));

        if (!AssetDatabase.IsValidFolder("assets/Meet and Talk"))
        {
            allDefines.Remove("MEET_AND_TALK");
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,string.Join(";", allDefines.ToArray()));

// Remove old Meet and Talk Script
#if MEET_AND_TALK
        if (File.Exists("Assets/Meet and Talk/Resources/LocalizationEnum.cs"))
        {
            File.Delete("Assets/Meet and Talk/Resources/LocalizationEnum.cs");
        }
        if (File.Exists("Assets/Meet and Talk/Resources/Languages.asset"))
        {
            File.Delete("Assets/Meet and Talk/Resources/Languages.asset");
        }
#endif
    }

}