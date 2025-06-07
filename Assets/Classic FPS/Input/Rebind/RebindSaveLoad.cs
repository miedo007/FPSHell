using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class RebindSaveLoad : MonoBehaviour
{
    TinyInput tinyInput;

    //public InputActionAsset actions;
    public GameObject RebindPanel;
    public TMP_Text RebindText;

    public void OnEnable()
    {
        tinyInput = new TinyInput();

        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds)) tinyInput.asset.LoadBindingOverridesFromJson(rebinds); 
    }

    public void OnDisable()
    {
        var rebinds = tinyInput.asset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);
    }
}
