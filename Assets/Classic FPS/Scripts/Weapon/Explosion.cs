using UnityEngine;

namespace HellishBattle.Weapon
{
    public class Explosion : MonoBehaviour
    {
        // Privates
        [HideInInspector] public AudioClip explosionSound;

        AudioSource source;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
        }

        void Start()
        {
            source.PlayOneShot(explosionSound);
        }
    }
}