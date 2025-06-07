using HellishBattle.Enemies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HellishBattle.Level
{
    public class TrapScript : MonoBehaviour
    {
        //public TrapType trapType;
        [Line("Settings")]
        public TriggerType triggerType;

        [Line("Damage Settings")]
        public float Damage;
        public DamageType damageType = DamageType.Normal;

        [Line("Timers")]
        public float delayTime = 0.2f;

        [Line("Transform")]
        public List<GameObject> ObjectToMove;
        public Vector3 StartPosition, EndPosition;

        [Line("Unity Events")]
        public UnityEvent OnActivate;
        public UnityEvent OnDeactivate, OnCountdown;

        private float timer;
        public List<GameObject> targets;
        private bool activate = false;
        bool _objectToMoveState = false;

        private void Start()
        {
            for (int i = 0; i < ObjectToMove.Count; i++) { ObjectToMove[i].transform.localPosition = StartPosition; }
        }

        public void Update()
        {
            timer -= Time.deltaTime;

            if (_objectToMoveState)
            {
                for (int i = 0; i < ObjectToMove.Count; i++) { ObjectToMove[i].transform.localPosition = Vector3.Lerp(ObjectToMove[i].transform.localPosition, EndPosition, 12 * Time.deltaTime); }
            }
            else
            {
                for (int i = 0; i < ObjectToMove.Count; i++) { ObjectToMove[i].transform.localPosition = Vector3.Lerp(ObjectToMove[i].transform.localPosition, StartPosition, 12 * Time.deltaTime); }
            }

        }

        public void OnTriggerEnter(Collider other)
        {
            if (triggerType == TriggerType.Automatic && activate == false) { StartCoroutine(Activating(delayTime)); }
            targets.Add(other.gameObject);
        }
        public void OnTriggerExit(Collider other)
        {
            targets.Remove(other.gameObject);
        }

        IEnumerator Activating(float time)
        {
            OnCountdown.Invoke();
            activate = true;
            yield return new WaitForSeconds(time);
            if (targets.Count != 0)
            {
                for (int i = 0; i < targets.Count; i++) { if (targets[i] != null) targets[i].SendMessage("GetDamage", new DamageClass(Damage, damageType), SendMessageOptions.DontRequireReceiver); }
            }
            OnActivate.Invoke();
        }
        IEnumerator Deactivating(float time)
        {
            yield return new WaitForSeconds(time);
            OnDeactivate.Invoke(); activate = false;
        }


        public void MoveToStartPosition()
        {
            //for (int i = 0; i < ObjectToMove.Count; i++) { ObjectToMove[i].transform.localPosition = StartPosition; }
            _objectToMoveState = false;
        }
        public void MoveToEndPosition()
        {
            //for (int i = 0; i < ObjectToMove.Count; i++) { ObjectToMove[i].transform.localPosition = EndPosition; }
            _objectToMoveState = true;
        }
        public void StartDeactivate(float time)
        {
            StartCoroutine(Deactivating(time));
        }

        public void ManualActivate()
        {
            if (triggerType == TriggerType.Manual)
            {
                StartCoroutine(Activating(delayTime));
            }
        }

        public enum TriggerType
        {
            Manual, Automatic
        }
    }

    public enum TrapType
    {
        Range = 0, Shooting = 1
    }
}

