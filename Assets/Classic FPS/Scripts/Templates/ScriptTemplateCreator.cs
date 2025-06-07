using UnityEditor;

namespace HellishBattle
{
    public static class ScriptTemplateCreator
    {
#if UNITY_EDITOR
        [MenuItem("Assets/Create/Hellish Battle/New Ability", priority = 47)]
        public static void NewAbilityScript()
        {
            string script = "Assets/Classic FPS/Scripts/Templates/NewAbility.cs.txt";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(script, "NewAbility.cs");
        }

        [MenuItem("Assets/Create/Hellish Battle/New Effect", priority = 23)]
        public static void NewEffectScript()
        {
            string script = "Assets/Classic FPS/Scripts/Templates/NewEffect.cs.txt";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(script, "NewEffect.cs");
        }
#endif
    }
}