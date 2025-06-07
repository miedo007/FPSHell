using HellishBattle;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
#if MeetAndTalk
using MeetAndTalk;
using MeetAndTalk.Settings;
#endif

public class MeetAndTalkIntegration : MonoBehaviour
{
#if MeetAndTalk
    MeetAndTalkSettings meetAndTalk;
    public UnityEvent StartDialog, EndDialog;

    public void Awake()
    {
        meetAndTalk = Resources.Load("MeetAndTalkSettings") as MeetAndTalkSettings;
        //meetAndTalk.DialoguePrefab.GetComponent<>
        GameObject test = Instantiate(meetAndTalk.DialoguePrefab);
        test.transform.Find("Dialogue Manager").GetComponent<DialogueManager>().localizationManager = Resources.Load<LocalizationManager>("Languages");
        test.transform.Find("Dialogue Manager").GetComponent<DialogueManager>().StartDialogueEvent.AddListener(StartDialog.Invoke);
        test.transform.Find("Dialogue Manager").GetComponent<DialogueManager>().EndDialogueEvent.AddListener(EndDialog.Invoke);
    }
#endif

    public void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        GameManager.Instance.paused = false;
    }

    public void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        GameManager.Instance.paused = true;
    }
}
