using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioClipLocalization : MonoBehaviour
{
    public LocalizedAudio ShowAudio;
    public AudioSource audio;

    private void Update() { audio.clip = ShowAudio.localization.GetAudio(ShowAudio.key); }
    private void OnEnable() { audio.clip = ShowAudio.localization.GetAudio(ShowAudio.key); }
}
