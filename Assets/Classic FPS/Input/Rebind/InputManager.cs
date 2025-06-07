using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    public TinyInput input;
    public static InputManager Instance;

    public void Awake()
    {

        input = new TinyInput();

        var rebinds = PlayerPrefs.GetString("rebinds");
        input.asset.LoadBindingOverridesFromJson(rebinds);

        // Set Static Object
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    public void OnDisable()
    {
        var rebinds = input.asset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);

        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        var rebinds = input.asset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);
    }

}
