using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BetterButton : MonoBehaviour
{
    public StringLocalization localization;
    public string keyLocalization;
    public TMP_Text text_tmp;

    private void Update()
    {
        text_tmp.text = localization.GetString(keyLocalization);
    }
}
