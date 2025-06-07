using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HellishBattle.Level
{
    [RequireComponent(typeof(AudioSource))]
    public class LabolatoryItemSpawner : MonoBehaviour
    {
        public Transform SpawnPoint;
        public GameObject ObjectToSpawn;
        public float TimeToWait = 5;
        public AudioClip spawnSound;

        public UnityEvent OnSpawn;
        public UnityEvent OnDespawn;


        private GameObject _object;
        private bool _spawned = true;
        private float time;
        private AudioSource source;

        private void Start()
        {
            _object = Instantiate(ObjectToSpawn, SpawnPoint);
            source = GetComponent<AudioSource>();
        }

        private void Update()
        {
            bool tmp = _spawned;
            time -= Time.deltaTime;

            if (_object == null && _spawned) { _spawned = false; time = TimeToWait; }


            if (_object == null && time < 0)
            {
                _object = Instantiate(ObjectToSpawn, SpawnPoint);
                _spawned = true;
                source.PlayOneShot(spawnSound);
            }

            if (tmp != _spawned)
            {
                if (_spawned) { OnSpawn.Invoke(); }
                else { OnDespawn.Invoke(); }
            }
        }
    }
}
