using UnityEngine;

namespace HellishBattle
{
    public class DestroyAfter : MonoBehaviour
    {
        public float DestroyTime = 15f;
        void Start()
        {
            Destroy(this.gameObject, DestroyTime);
        }

    }
}
