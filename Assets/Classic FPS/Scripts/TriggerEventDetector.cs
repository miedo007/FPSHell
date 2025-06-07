using UnityEngine;
using UnityEngine.Events;

namespace HellishBattle
{
    public class TriggerEventDetector : MonoBehaviour
    {
        public string Tag;
        public TriggerStatus status;

        public UnityEvent<Collider> OnEnter, OnStay, OnExit;


        private void Awake()
        {
            status = TriggerStatus.Exit;
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((other.CompareTag(Tag) && Tag != null) || Tag == null)
            {
                status = TriggerStatus.Enter;
                OnEnter.Invoke(other);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if ((other.CompareTag(Tag) && Tag != null) || Tag == null)
            {
                status = TriggerStatus.Stay;
                OnStay.Invoke(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if ((other.CompareTag(Tag) && Tag != null) || Tag == null)
            {
                status = TriggerStatus.Exit;
                OnExit.Invoke(other);
            }
        }

    }

    [System.Serializable]
    public enum TriggerStatus { Enter, Stay, Exit }
}
