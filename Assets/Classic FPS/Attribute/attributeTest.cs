using HellishBattle;
using HellishBattle.Weapon;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class attributeTest : MonoBehaviour
{
    // Line without Text
    [Line()] public int Count1;

    // Green Line without Text
    [Line(VirtualColor.Green)] public int Count2;

    // Line with Text
    [Line("Line Text")] public int Count3;

    // Line with Multi-Line
    [Line("Line Text", "Line Text", "Line Text")] public int Count4;

    // Purple Line with Text
    [Line(VirtualColor.Purple, "Line Text")] public int Count5;

    // Red Line with Multi-Line
    [Line(VirtualColor.Red, "Line Text", "Line Text", "Line Text")] public int Count6;

    [Space(40)]

    // Example Enum
    public CustomEnum Enums;
    // Example Bool
    public bool enable;
    // Show If Bool
    [ShowIf("enable")] public int enableWhenBool;
    // Show if enums == CustomEnum.B
    [ShowIf("Enums", 1)] public int enableWhenEnumB;
    // Show if enums == CustomEnum.C
    [ShowIf("Enums", (int)CustomEnum.C)] public int enableWhenEnumC;
    // Enable if enums == CustomEnum.D
    [ShowIf("Enums", (int)CustomEnum.D, false)] public int enableWhenEnumD;

    [Space(40)]
    
    // Suffix Ints to Int value
    [Suffix("Ints")] public int suffixInt;
    // Suffix Floats to Float value
    [Suffix("Floats")] public int suffixFloat;
    // Suffix Strings to String value
    [Suffix("Strings")] public int suffixString;

    [Space(40)]

    [HelpBox("Info Help Box", HelpBoxEnum.Info)]
    public int HelpBox_Info;
    [HelpBox("Warning Help Box", HelpBoxEnum.Warning)]
    public int HelpBox_Warning;
    [HelpBox("Error Help Box", HelpBoxEnum.Error)]
    public int HelpBox_Error;
    [HelpBox("Help Box\nWithout\nIcon", HelpBoxEnum.None)]
    public int HelpBox_None;

    [Space(40)]

    [Required()] public GameObject Required;
    [Required()] public Vector3 Required2;
    [Required()] public Color Required3;
    [Required()] public int Required4;
    [Required()] public BaseWeaponScript Required5;

    public enum CustomEnum
    {
        A = 0,
        B = 1,
        C = 2,
        D = 4
    }
}





