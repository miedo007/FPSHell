using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageSpriteLocalization : MonoBehaviour
{
    public LocalizedSprite ShowSprite;
    public Image image;

    private void Update() { image.sprite = ShowSprite.localization.GetSprite(ShowSprite.key); }
    private void OnEnable() { image.sprite = ShowSprite.localization.GetSprite(ShowSprite.key); }
}
