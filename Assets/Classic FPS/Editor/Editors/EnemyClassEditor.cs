using HellishBattle.Enemies;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnemyScoreClass))]
public class EnemyScoreClassDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var typeRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
        var valueRect = new Rect(position.x + EditorGUIUtility.labelWidth + 5, position.y, position.width - EditorGUIUtility.labelWidth - 5, EditorGUIUtility.singleLineHeight);

        EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("Type"), GUIContent.none);
        EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("Multiplier"), GUIContent.none);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}

[CustomPropertyDrawer(typeof(EnemyFXClass))]
public class EnemyFXClassDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, position.height-5), "", MessageType.None);

        var Type = new Rect(position.x + 5, position.y + 5, position.width - 10, EditorGUIUtility.singleLineHeight);

        var FXChange = new Rect(position.x + 5, position.y + 5 + 1 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width - 10, EditorGUIUtility.singleLineHeight);
        var FXCount = new Rect(position.x + 5, position.y + 5 + 2 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width - 10, EditorGUIUtility.singleLineHeight);
        var FloorFXChange = new Rect(position.x + 5, position.y + 5 + 3 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width - 10, EditorGUIUtility.singleLineHeight);
        var FloorFXCount = new Rect(position.x + 5, position.y + 5 + 4 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width - 10, EditorGUIUtility.singleLineHeight);

        EditorGUI.PropertyField(Type, property.FindPropertyRelative("Type"), new GUIContent("Damage Type"));

        EditorGUI.PropertyField(FXChange, property.FindPropertyRelative("FXChange"), new GUIContent("FX Change"));
        EditorGUI.PropertyField(FXCount, property.FindPropertyRelative("FXCount"), new GUIContent("FX Count"));
        EditorGUI.PropertyField(FloorFXChange, property.FindPropertyRelative("FloorFXChange"), new GUIContent("Floor FX Change"));
        EditorGUI.PropertyField(FloorFXCount, property.FindPropertyRelative("FloorFXCount"), new GUIContent("Floor FX Count"));

        /*
        EditorGUI.LabelField(label2Rect, new GUIContent("Extra FX Modifier"), EditorStyles.boldLabel);
        EditorGUI.PropertyField(ExtraFXChange, property.FindPropertyRelative("ExtraFXChange"), new GUIContent("Extra FX Change [0-100]"));
        EditorGUI.PropertyField(ExtraFXCount, property.FindPropertyRelative("ExtraFXCount"), new GUIContent("Extra FX Count"));
        EditorGUI.PropertyField(ExtraFX, property.FindPropertyRelative("ExtraFX"), new GUIContent("Extra FX List"));

        EditorGUI.PropertyField(ExtraFloorFXChange, property.FindPropertyRelative("ExtraFloorFXChange"), new GUIContent("Extra Floor FX Change [0-100]"));
        EditorGUI.PropertyField(ExtraFloorFXCount, property.FindPropertyRelative("ExtraFloorFXCount"), new GUIContent("Extra Floor FX Count"));
        EditorGUI.PropertyField(ExtraFloorFX, property.FindPropertyRelative("ExtraFloorFX"), new GUIContent("Extra Floor FX List"));*/
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight + 13 + 4 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
    }
}

[CustomPropertyDrawer(typeof(EnemyDeadBodyClass))]
public class EnemyDeadBodyClassDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, position.height - 5), "", MessageType.None);

        var Type = new Rect(position.x + 5, position.y + 5, position.width - 10, EditorGUIUtility.singleLineHeight);

        var SpawnDeathBody = new Rect(position.x + 5, position.y + 5 + 1 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width - 10, EditorGUIUtility.singleLineHeight);
        var DeathBodySprite = new Rect(position.x + 5, position.y + 5 + 2 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width - 10, EditorGUIUtility.singleLineHeight);
        var DeathBodyColliderCenter = new Rect(position.x + 5, position.y + 5 + 3 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width - 10, EditorGUIUtility.singleLineHeight);
        var DeathBodyColliderSize = new Rect(position.x + 5, position.y + 5 + 4 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width - 10, EditorGUIUtility.singleLineHeight);

        EditorGUI.PropertyField(Type, property.FindPropertyRelative("Type"), new GUIContent("Damage Type"));

        EditorGUI.PropertyField(SpawnDeathBody, property.FindPropertyRelative("SpawnDeathBody"), new GUIContent("FX"));
        if (property.FindPropertyRelative("SpawnDeathBody").boolValue)
        {
            EditorGUI.PropertyField(DeathBodySprite, property.FindPropertyRelative("DeathBodySprite"), new GUIContent("Body Sprite"));
            EditorGUI.PropertyField(DeathBodyColliderCenter, property.FindPropertyRelative("DeathBodyColliderCenter"), new GUIContent("Body Collider Center"));
            EditorGUI.PropertyField(DeathBodyColliderSize, property.FindPropertyRelative("DeathBodyColliderSize"), new GUIContent("Body Collider Size"));
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.FindPropertyRelative("SpawnDeathBody").boolValue)
        {
            return EditorGUIUtility.singleLineHeight + 13 + 4 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
        }
        else
        {
            return EditorGUIUtility.singleLineHeight + 13 + 1 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
        }
    }
}

[CustomPropertyDrawer(typeof(EnemySoundClass))]
public class EnemySoundClassDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, position.height - 5), "", MessageType.None);

        var Type = new Rect(position.x + 5, position.y + 5, position.width - 10, EditorGUIUtility.singleLineHeight);

        var PlayOriginal = new Rect(position.x + 5, position.y + 5 + 1 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width - 10, EditorGUIUtility.singleLineHeight);
        var DamageSound = new Rect(position.x + 21, position.y + 5 + 2 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width - 26, EditorGUIUtility.singleLineHeight);
        var DeathSound = new Rect(position.x + 21, position.y + 5 + 2 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("DamageSound"), true), position.width - 26, EditorGUIUtility.singleLineHeight);

        EditorGUI.PropertyField(Type, property.FindPropertyRelative("Type"), new GUIContent("Damage Type"));

        EditorGUI.PropertyField(PlayOriginal, property.FindPropertyRelative("PlayOriginal"), new GUIContent("Play Origin"));
        EditorGUI.PropertyField(DamageSound, property.FindPropertyRelative("DamageSound"), new GUIContent("Damage Sounds"));
        EditorGUI.PropertyField(DeathSound, property.FindPropertyRelative("DeathSound"), new GUIContent("Death Sounds"));

        
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight + 13 + 1 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) + +EditorGUI.GetPropertyHeight(property.FindPropertyRelative("DamageSound"), true) + +EditorGUI.GetPropertyHeight(property.FindPropertyRelative("DeathSound"), true);

    }
}


[CustomPropertyDrawer(typeof(EnemyMelleAttackClass))]
public class EnemyMelleAttackClassDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, position.height - 5), "", MessageType.None);

        // Rect
        var meleeDamage = new Rect(position.x + 5, position.y + 5 + 0 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width - 10, EditorGUIUtility.singleLineHeight);
        var attackRange = new Rect(position.x + 5, position.y + 5 + 1 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width - 10, EditorGUIUtility.singleLineHeight);
        var Delay = new Rect(position.x + 5, position.y + 5 + 2 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width - 10, EditorGUIUtility.singleLineHeight);

        // Property
        EditorGUI.PropertyField(meleeDamage, property.FindPropertyRelative("meleeDamage"), new GUIContent("Damage"));
        EditorGUI.PropertyField(attackRange, property.FindPropertyRelative("attackRange"), new GUIContent("Range"));
        EditorGUI.PropertyField(Delay, property.FindPropertyRelative("Delay"), new GUIContent("Delay"));
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight + 13 + 2 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

    }
}

[CustomPropertyDrawer(typeof(EnemyRangeAttackClass))]
public class EnemyRangeAttackClassDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, position.height - 5), "", MessageType.None);

        // Rect
        var missileDamage = new Rect(position.x + 5, position.y + 5 + 0 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width - 10, EditorGUIUtility.singleLineHeight);
        var missileSpeed = new Rect(position.x + 5, position.y + 5 + 1 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width - 10, EditorGUIUtility.singleLineHeight);
        var shootRange = new Rect(position.x + 5, position.y + 5 + 2 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width - 10, EditorGUIUtility.singleLineHeight);
        var AutoAim = new Rect(position.x + 5, position.y + 5 + 3 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width - 10, EditorGUIUtility.singleLineHeight);
        var rangeDelay = new Rect(position.x + 5, position.y + 5 + 4 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width - 10, EditorGUIUtility.singleLineHeight);
        var missile = new Rect(position.x + 5, position.y + 5 + 5 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width - 10, EditorGUIUtility.singleLineHeight);

        // Property
        EditorGUI.PropertyField(missileDamage, property.FindPropertyRelative("missileDamage"), new GUIContent("Damage"));
        EditorGUI.PropertyField(missileSpeed, property.FindPropertyRelative("missileSpeed"), new GUIContent("Spped"));
        EditorGUI.PropertyField(shootRange, property.FindPropertyRelative("shootRange"), new GUIContent("Range"));
        EditorGUI.PropertyField(AutoAim, property.FindPropertyRelative("AutoAim"), new GUIContent("Auto Aim"));
        EditorGUI.PropertyField(rangeDelay, property.FindPropertyRelative("Delay"), new GUIContent("Delay"));
        EditorGUI.PropertyField(missile, property.FindPropertyRelative("missile"), new GUIContent("Missle Prefab"));
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight + 13 + 5 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

    }
}

[CustomPropertyDrawer(typeof(EnemyDamageMultiplierClass))]
public class EnemyDamageMultiplierClassDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var typeRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
        var valueRect = new Rect(position.x + EditorGUIUtility.labelWidth + 5, position.y, position.width - EditorGUIUtility.labelWidth - 5, EditorGUIUtility.singleLineHeight);

        EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("Type"), GUIContent.none);
        EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("Multiplier"), GUIContent.none);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}







